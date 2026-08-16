using Microsoft.Extensions.Logging;
using WeddingPhotos.Domain.Constants;
using WeddingPhotos.Domain.DTOs;
using WeddingPhotos.Domain.Interfaces;
using WeddingPhotos.Domain.Models;
using WeddingPhotos.Domain.Validation;

namespace WeddingPhotos.Infrastructure.Services;

public class GalleryService : IGalleryService
{
    private readonly IClientRepository _clientRepository;
    private readonly IGoogleStorageService _storageService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GalleryService> _logger;

    public GalleryService(
        IClientRepository clientRepository,
        IGoogleStorageService storageService,
        ICacheService cacheService,
        ILogger<GalleryService> logger)
    {
        _clientRepository = clientRepository;
        _storageService = storageService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<(bool Success, GalleryResponse? Response, string? ErrorMessage)> GetGalleryAsync(
        string guid,
        int page,
        int pageSize)
    {
        try
        {
            // Validate pagination parameters
            if (page < ApplicationConstants.Pagination.MinPage ||
                page > ApplicationConstants.Pagination.MaxPage)
            {
                return (false, null, ApplicationConstants.ErrorMessages.InvalidPageNumber);
            }

            if (pageSize < ApplicationConstants.Pagination.MinPageSize ||
                pageSize > ApplicationConstants.Pagination.MaxPageSize)
            {
                return (false, null, ApplicationConstants.ErrorMessages.InvalidPageSize);
            }

            // Get client
            var client = await _clientRepository.GetByGuidAsync(guid);

            if (client == null)
            {
                _logger.LogWarning("Gallery access attempt with non-existent GUID: {Guid}", guid);
                return (false, null, ApplicationConstants.ErrorMessages.GalleryNotFound);
            }

            // Check if active
            if (!client.IsActive)
            {
                _logger.LogWarning("Access attempt to inactive gallery: {Guid}", guid);
                return (false, null, ApplicationConstants.ErrorMessages.GalleryDeactivated);
            }

            // Log if expired but allow viewing
            if (client.DateTo < DateTime.UtcNow)
            {
                _logger.LogInformation(
                    "Accessing expired gallery: {Guid}, expired on: {DateTo}",
                    guid, client.DateTo);
            }

            // Try to get from cache first
            var cacheKey = $"gallery:{guid}:page:{page}:size:{pageSize}";
            var cachedResponse = await _cacheService.GetAsync<GalleryResponse>(cacheKey);

            if (cachedResponse != null)
            {
                _logger.LogInformation(
                    "Gallery loaded from cache: {Guid}, Page: {Page}",
                    guid, page);
                return (true, cachedResponse, null);
            }

            // Get photos from storage
            var photos = await _storageService.GetPhotosFromFolderAsync(
                client.GoogleStorageUrl,
                page,
                pageSize);

            // Map to DTOs
            var photoDtos = photos.Select(p => new PhotoInfo
            {
                Id = p.Id,
                Name = p.Name,
                ThumbnailUrl = p.ThumbnailUrl,
                FullUrl = p.FullUrl,
                DateAdded = p.DateAdded,
                Size = p.Size,
                MimeType = p.MimeType
            }).ToList();

            // Drive has no count endpoint, so this is a second query. It is worth
            // it: the gallery shows "loaded of total", and knowing the total also
            // makes HasMore exact. Inferring it from "was the page full" offered
            // a Load more button that led to an empty page whenever the total was
            // an exact multiple of pageSize.
            var totalCount = await _storageService.GetPhotoCountAsync(client.GoogleStorageUrl);

            var response = new GalleryResponse
            {
                Photos = photoDtos,
                TotalCount = totalCount,
                HasMore = page * pageSize < totalCount,
                NextPageToken = page * pageSize < totalCount ? (page + 1).ToString() : null
            };

            // Cache the response (15 minutes)
            await _cacheService.SetAsync(
                cacheKey,
                response,
                TimeSpan.FromMinutes(15));

            _logger.LogInformation(
                "Gallery loaded from storage and cached: {Guid}, Page: {Page}, Count: {Count}",
                guid, page, photoDtos.Count);

            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading gallery: {Guid}", guid);
            return (false, null, ApplicationConstants.ErrorMessages.LoadingGalleryError);
        }
    }

    public async Task<(bool Success, UploadPhotoResponse Response, string? ErrorMessage)> UploadPhotoAsync(
        string guid,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize)
    {
        try
        {
            // Get client
            var client = await _clientRepository.GetByGuidAsync(guid);

            if (client == null)
            {
                _logger.LogWarning("Upload attempt to non-existent gallery: {Guid}", guid);
                return (false, new UploadPhotoResponse
                {
                    Success = false,
                    Message = ApplicationConstants.ErrorMessages.GalleryNotFound
                }, ApplicationConstants.ErrorMessages.GalleryNotFound);
            }

            // Validate client status
            if (!client.IsActive)
            {
                return (false, new UploadPhotoResponse
                {
                    Success = false,
                    Message = ApplicationConstants.ErrorMessages.GalleryDeactivated
                }, ApplicationConstants.ErrorMessages.GalleryDeactivated);
            }

            if (client.DateTo < DateTime.UtcNow)
            {
                return (false, new UploadPhotoResponse
                {
                    Success = false,
                    Message = ApplicationConstants.ErrorMessages.GalleryExpired
                }, ApplicationConstants.ErrorMessages.GalleryExpired);
            }

            // Validate file
            if (fileSize == 0)
            {
                return (false, new UploadPhotoResponse
                {
                    Success = false,
                    Message = ApplicationConstants.ErrorMessages.NoFileSelected
                }, ApplicationConstants.ErrorMessages.NoFileSelected);
            }

            // Validate file size
            if (!InputValidator.IsValidFileSize(fileSize, client.MaxFileSize))
            {
                var maxSizeMB = client.MaxFileSize / (1024 * 1024);
                _logger.LogWarning(
                    "Upload rejected - file too large: {Size}MB, Max: {MaxSize}MB",
                    fileSize / (1024 * 1024),
                    maxSizeMB);

                var errorMessage = string.Format(ApplicationConstants.ErrorMessages.FileTooBig, maxSizeMB);
                return (false, new UploadPhotoResponse
                {
                    Success = false,
                    Message = errorMessage
                }, errorMessage);
            }

            // Validate file type
            if (!InputValidator.IsValidImageFile(fileName, contentType))
            {
                _logger.LogWarning(
                    "Upload rejected - invalid file type: {FileName}, {ContentType}",
                    fileName,
                    contentType);

                return (false, new UploadPhotoResponse
                {
                    Success = false,
                    Message = ApplicationConstants.ErrorMessages.InvalidFileType
                }, ApplicationConstants.ErrorMessages.InvalidFileType);
            }

            // Sanitize filename
            var sanitizedFileName = InputValidator.SanitizeFileName(fileName);

            // Atomically reserve an upload slot. This both enforces the quota
            // (race-condition-safe across concurrent guests) and tracks the
            // count for unlimited galleries too. Returns null if quota exhausted.
            var reservedClient = await _clientRepository.TryReserveUploadSlotAsync(guid);
            if (reservedClient == null)
            {
                _logger.LogWarning(
                    "Upload rejected - quota exceeded for GUID={Guid}: {Max} limit reached",
                    guid, client.MaxFiles);
                return (false, new UploadPhotoResponse
                {
                    Success = false,
                    Message = "Osiągnięto limit zdjęć dla tej galerii"
                }, "Osiągnięto limit zdjęć dla tej galerii");
            }

            _logger.LogInformation(
                "Upload started: GUID={Guid}, File={FileName}, Size={Size}MB",
                guid,
                sanitizedFileName,
                fileSize / (1024.0 * 1024.0));

            // Extract folder ID from URL
            var folderId = GoogleDriveHelper.ExtractFolderId(client.GoogleStorageUrl);

            // Upload file. If it fails, release the reserved slot so the count
            // stays accurate.
            string? photoId;
            try
            {
                photoId = await _storageService.UploadPhotoAsync(
                    fileStream,
                    sanitizedFileName,
                    folderId);
            }
            catch
            {
                await _clientRepository.ReleaseUploadSlotAsync(guid);
                throw;
            }

            if (string.IsNullOrEmpty(photoId))
            {
                await _clientRepository.ReleaseUploadSlotAsync(guid);
                _logger.LogError("Upload failed - no photoId returned for {Guid}", guid);
                return (false, new UploadPhotoResponse
                {
                    Success = false,
                    Message = ApplicationConstants.ErrorMessages.UploadError
                }, ApplicationConstants.ErrorMessages.UploadError);
            }

            // Invalidate gallery cache for this client (all pages)
            await _cacheService.RemoveByPrefixAsync($"gallery:{guid}:");

            // RemainingUploads: -1 signals "unlimited" (MaxFiles == 0)
            var remainingUploads = reservedClient.MaxFiles == 0
                ? -1
                : Math.Max(0, reservedClient.MaxFiles - reservedClient.UploadedFilesCount);

            _logger.LogInformation(
                "Upload successful and cache invalidated: GUID={Guid}, PhotoId={PhotoId}, Remaining={Remaining}",
                guid,
                photoId,
                remainingUploads);

            return (true, new UploadPhotoResponse
            {
                Success = true,
                PhotoId = photoId,
                Message = ApplicationConstants.SuccessMessages.PhotoUploadedSuccessfully,
                RemainingUploads = remainingUploads
            }, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo for client: {Guid}", guid);
            return (false, new UploadPhotoResponse
            {
                Success = false,
                Message = ApplicationConstants.ErrorMessages.GeneralUploadError
            }, ApplicationConstants.ErrorMessages.GeneralUploadError);
        }
    }

    public async Task<(bool Success, CreateUploadSessionResponse Response, string? ErrorMessage)> CreateUploadSessionAsync(
        string guid,
        string fileName,
        string mimeType,
        long fileSize,
        string? origin)
    {
        CreateUploadSessionResponse Failure(string message) =>
            new() { Success = false, Message = message };

        try
        {
            var client = await _clientRepository.GetByGuidAsync(guid);

            if (client == null)
                return (false, Failure(ApplicationConstants.ErrorMessages.GalleryNotFound),
                    ApplicationConstants.ErrorMessages.GalleryNotFound);

            if (!client.IsActive)
                return (false, Failure(ApplicationConstants.ErrorMessages.GalleryDeactivated),
                    ApplicationConstants.ErrorMessages.GalleryDeactivated);

            if (client.DateTo < DateTime.UtcNow)
                return (false, Failure(ApplicationConstants.ErrorMessages.GalleryExpired),
                    ApplicationConstants.ErrorMessages.GalleryExpired);

            if (fileSize == 0)
                return (false, Failure(ApplicationConstants.ErrorMessages.NoFileSelected),
                    ApplicationConstants.ErrorMessages.NoFileSelected);

            if (!InputValidator.IsValidFileSize(fileSize, client.MaxFileSize))
            {
                var maxSizeMB = client.MaxFileSize / (1024 * 1024);
                var tooBig = string.Format(ApplicationConstants.ErrorMessages.FileTooBig, maxSizeMB);
                return (false, Failure(tooBig), tooBig);
            }

            // The type is judged from the declared filename. That is no weaker
            // than the buffered path: it never inspected the bytes either, and
            // the stored name is generated server-side regardless.
            if (!InputValidator.IsValidImageFile(fileName, mimeType))
                return (false, Failure(ApplicationConstants.ErrorMessages.InvalidFileType),
                    ApplicationConstants.ErrorMessages.InvalidFileType);

            // Reserved up front so concurrent guests cannot overshoot the quota.
            // If the guest walks away mid-upload the slot is only returned when
            // the client calls cancel, so an abandoned session can hold one.
            var reservedClient = await _clientRepository.TryReserveUploadSlotAsync(guid);
            if (reservedClient == null)
            {
                const string quotaMessage = "Osiągnięto limit zdjęć dla tej galerii";
                _logger.LogWarning("Upload session rejected - quota exceeded for {Guid}", guid);
                return (false, Failure(quotaMessage), quotaMessage);
            }

            try
            {
                var folderId = GoogleDriveHelper.ExtractFolderId(client.GoogleStorageUrl);

                var uploadUrl = await _storageService.CreateResumableUploadSessionAsync(
                    fileName, fileSize, folderId, origin);

                _logger.LogInformation(
                    "Upload session opened: GUID={Guid}, File={FileName}, Size={Size}MB",
                    guid, fileName, fileSize / (1024.0 * 1024.0));

                return (true, new CreateUploadSessionResponse
                {
                    Success = true,
                    UploadUrl = uploadUrl
                }, null);
            }
            catch
            {
                await _clientRepository.ReleaseUploadSlotAsync(guid);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening upload session for {Guid}", guid);
            return (false, Failure(ApplicationConstants.ErrorMessages.GeneralUploadError),
                ApplicationConstants.ErrorMessages.GeneralUploadError);
        }
    }

    public async Task<(bool Success, UploadPhotoResponse Response, string? ErrorMessage)> CompleteUploadSessionAsync(
        string guid,
        string photoId)
    {
        try
        {
            var client = await _clientRepository.GetByGuidAsync(guid);

            if (client == null)
            {
                return (false, new UploadPhotoResponse
                {
                    Success = false,
                    Message = ApplicationConstants.ErrorMessages.GalleryNotFound
                }, ApplicationConstants.ErrorMessages.GalleryNotFound);
            }

            await _cacheService.RemoveByPrefixAsync($"gallery:{guid}:");

            var remainingUploads = client.MaxFiles == 0
                ? -1
                : Math.Max(0, client.MaxFiles - client.UploadedFilesCount);

            _logger.LogInformation(
                "Direct upload completed: GUID={Guid}, PhotoId={PhotoId}, Remaining={Remaining}",
                guid, photoId, remainingUploads);

            return (true, new UploadPhotoResponse
            {
                Success = true,
                PhotoId = photoId,
                Message = ApplicationConstants.SuccessMessages.PhotoUploadedSuccessfully,
                RemainingUploads = remainingUploads
            }, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing upload session for {Guid}", guid);
            return (false, new UploadPhotoResponse
            {
                Success = false,
                Message = ApplicationConstants.ErrorMessages.GeneralUploadError
            }, ApplicationConstants.ErrorMessages.GeneralUploadError);
        }
    }

    public async Task CancelUploadSessionAsync(string guid)
    {
        try
        {
            await _clientRepository.ReleaseUploadSlotAsync(guid);
            _logger.LogInformation("Upload session cancelled, slot released: {Guid}", guid);
        }
        catch (Exception ex)
        {
            // Losing a slot is far better than failing the guest's request here.
            _logger.LogError(ex, "Failed to release upload slot for {Guid}", guid);
        }
    }

    public async Task<(bool Success, PhotoStreamResult? Photo, string? ErrorMessage)> GetPhotoStreamAsync(
        string photoId,
        int? thumbnailSize = null,
        string? rangeHeader = null)
    {
        try
        {
            var photo = await _storageService.GetPhotoStreamAsync(photoId, thumbnailSize, rangeHeader);
            return (true, photo, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting photo stream: {PhotoId}", photoId);
            return (false, null, ApplicationConstants.ErrorMessages.PhotoNotFound);
        }
    }
}
