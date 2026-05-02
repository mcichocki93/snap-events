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

            // TotalCount is the number of photos returned in this page (not the global total,
            // which is not provided by Google Drive without a separate count query).
            // HasMore is inferred from whether a full page was returned.
            var response = new GalleryResponse
            {
                Photos = photoDtos,
                TotalCount = photoDtos.Count,
                HasMore = photoDtos.Count == pageSize,
                NextPageToken = photoDtos.Count == pageSize ? (page + 1).ToString() : null
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

            // Check upload quota before uploading (MaxFiles == 0 means unlimited)
            if (client.MaxFiles > 0 && client.UploadedFilesCount >= client.MaxFiles)
            {
                _logger.LogWarning(
                    "Upload rejected - quota exceeded for GUID={Guid}: {Count}/{Max}",
                    guid, client.UploadedFilesCount, client.MaxFiles);
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
            var folderId = ExtractFolderIdFromUrl(client.GoogleStorageUrl);

            // Upload file
            var photoId = await _storageService.UploadPhotoAsync(
                fileStream,
                sanitizedFileName,
                folderId);

            if (string.IsNullOrEmpty(photoId))
            {
                _logger.LogError("Upload failed - no photoId returned for {Guid}", guid);
                return (false, new UploadPhotoResponse
                {
                    Success = false,
                    Message = ApplicationConstants.ErrorMessages.UploadError
                }, ApplicationConstants.ErrorMessages.UploadError);
            }

            // Increment uploaded files counter (only if limit is enabled)
            if (client.MaxFiles > 0)
            {
                await _clientRepository.UpdateUploadedFilesCountAsync(guid, 1);
            }

            // Invalidate gallery cache for this client (all pages)
            await _cacheService.RemoveByPrefixAsync($"gallery:{guid}:");

            _logger.LogInformation(
                "Upload successful and cache invalidated: GUID={Guid}, PhotoId={PhotoId}",
                guid,
                photoId);

            return (true, new UploadPhotoResponse
            {
                Success = true,
                PhotoId = photoId,
                Message = ApplicationConstants.SuccessMessages.PhotoUploadedSuccessfully
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

    public async Task<(bool Success, Stream? Stream, string? MimeType, string? FileName, string? ErrorMessage)> GetPhotoStreamAsync(
        string photoId)
    {
        try
        {
            var (stream, mimeType, fileName) = await _storageService.GetPhotoStreamAsync(photoId);
            return (true, stream, mimeType, fileName, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting photo stream: {PhotoId}", photoId);
            return (false, null, null, null, ApplicationConstants.ErrorMessages.PhotoNotFound);
        }
    }

    private string ExtractFolderIdFromUrl(string url)
    {
        // Extract folder ID from Google Drive URL or return as-is
        if (url.Contains("drive.google.com/drive/folders/"))
        {
            var parts = url.Split('/');
            var folderIndex = Array.IndexOf(parts, "folders");
            if (folderIndex >= 0 && folderIndex < parts.Length - 1)
            {
                return parts[folderIndex + 1].Split('?')[0];
            }
        }

        return url;
    }
}
