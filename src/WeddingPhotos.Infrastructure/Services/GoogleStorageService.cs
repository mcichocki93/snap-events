using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using WeddingPhotos.Domain.Interfaces;
using WeddingPhotos.Domain.Models;
using WeddingPhotos.Infrastructure.Configuration;

namespace WeddingPhotos.Infrastructure.Services;

public class GoogleStorageService : IGoogleStorageService
{
    private readonly GoogleCredential _googleCredential;
    private readonly DriveService _driveService;
    private readonly ILogger<GoogleStorageService> _logger;
    private readonly HttpClient _httpClient;

    public GoogleStorageService(
        IOptions<GoogleCloudSettings> settings,
        ILogger<GoogleStorageService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();

        try
        {
            var googleSettings = settings.Value;
            var serviceAccountKeyPath = googleSettings.ServiceAccountKeyPath;

            if (string.IsNullOrEmpty(serviceAccountKeyPath) || !File.Exists(serviceAccountKeyPath))
            {
                throw new FileNotFoundException(
                    $"Service account key file not found: {serviceAccountKeyPath}");
            }

            // Use ServiceAccountCredential (recommended approach, no deprecation warnings)
            var json = File.ReadAllText(serviceAccountKeyPath);
            var credentialParameters = JsonConvert.DeserializeObject<JsonCredentialParameters>(json);

            if (credentialParameters == null)
            {
                throw new InvalidOperationException("Failed to parse service account credentials");
            }

            var initializer = new ServiceAccountCredential.Initializer(credentialParameters.ClientEmail)
            {
                Scopes = new[] { DriveService.Scope.DriveFile }
            }.FromPrivateKey(credentialParameters.PrivateKey);

            var serviceAccountCredential = new ServiceAccountCredential(initializer);
            _googleCredential = GoogleCredential.FromServiceAccountCredential(serviceAccountCredential);

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _googleCredential,
                ApplicationName = "WeddingPhotos"
            });

            _logger.LogInformation("Google Storage Service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Google Storage Service");
            throw;
        }
    }

    public async Task<string?> UploadPhotoAsync(Stream fileStream, string fileName, string folderId)
    {
        try
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                _logger.LogWarning("Attempted to upload empty or null file stream");
                return null;
            }

            if (string.IsNullOrWhiteSpace(folderId))
            {
                throw new ArgumentException("Folder ID cannot be empty", nameof(folderId));
            }

            var secureFileName = GenerateSecureFileName(Path.GetExtension(fileName));

            if (!IsAllowedFileType(secureFileName))
            {
                throw new ArgumentException(
                    $"File type not allowed: {Path.GetExtension(secureFileName)}");
            }

            var fileMetaData = new Google.Apis.Drive.v3.Data.File
            {
                Name = secureFileName,
                Parents = new List<string> { folderId },
                Description = $"Uploaded on {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
            };

            var mimeType = GetMimeType(secureFileName);

            fileStream.Position = 0;

            var request = _driveService.Files.Create(fileMetaData, fileStream, mimeType);
            request.Fields = "id,name,size,createdTime";

            var progress = await request.UploadAsync();

            if (progress.Status == Google.Apis.Upload.UploadStatus.Completed)
            {
                var uploadedFile = request.ResponseBody;

                _logger.LogInformation(
                    "Successfully uploaded file: {FileName} (ID: {FileId})",
                    secureFileName,
                    uploadedFile?.Id);

                return uploadedFile?.Id;
            }
            else if (progress.Exception != null)
            {
                _logger.LogError(progress.Exception, "Upload failed for file: {FileName}", fileName);
                throw new ApplicationException(
                    $"Upload failed: {progress.Exception.Message}",
                    progress.Exception);
            }
            else
            {
                _logger.LogWarning(
                    "Upload incomplete for file: {FileName}, Status: {Status}",
                    fileName,
                    progress.Status);
                throw new ApplicationException($"Upload incomplete, Status: {progress.Status}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Google Drive, folder: {FolderId}", folderId);
            throw new ApplicationException("Błąd podczas przesyłania pliku do Google Drive", ex);
        }
    }

    public async Task<List<PhotoInfo>> GetPhotosFromFolderAsync(
        string folderUrl,
        int page = 1,
        int pageSize = 100)
    {
        try
        {
            var folderId = ExtractFolderIdFromUrl(folderUrl);
            var photos = new List<PhotoInfo>();

            var request = _driveService.Files.List();
            request.Q = $"'{folderId}' in parents and mimeType contains 'image/' and trashed=false";
            request.Fields = "nextPageToken,files(id,name,size,createdTime,mimeType,thumbnailLink,webViewLink,webContentLink)";
            request.OrderBy = "createdTime desc";
            request.PageSize = pageSize;

            if (page > 1)
            {
                var totalToSkip = (page - 1) * pageSize;
                request.PageSize = totalToSkip + pageSize;
            }

            var result = await request.ExecuteAsync();

            if (result.Files != null)
            {
                var filesToProcess = page > 1
                    ? result.Files.Skip((page - 1) * pageSize).Take(pageSize)
                    : result.Files;

                foreach (var file in filesToProcess)
                {
                    var photo = new PhotoInfo
                    {
                        Id = file.Id,
                        Name = file.Name,
                        ThumbnailUrl = GetOptimizedThumbnailUrl(file.Id, file.ThumbnailLink),
                        FullUrl = GetOptimizedImageUrl(file.Id),
                        DateAdded = file.CreatedTime ?? DateTime.Now,
                        Size = file.Size ?? 0,
                        MimeType = file.MimeType ?? "image/jpeg"
                    };

                    photos.Add(photo);
                }
            }

            _logger.LogInformation(
                "Retrieved {Count} photos from folder {FolderId}, page {Page}",
                photos.Count,
                folderId,
                page);

            return photos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get photos from folder {FolderUrl}", folderUrl);
            throw;
        }
    }

    public async Task<string> GetPhotoDownloadUrlAsync(string photoId)
    {
        try
        {
            var request = _driveService.Files.Get(photoId);
            request.Fields = "webContentLink,id,name";
            var file = await request.ExecuteAsync();

            if (!string.IsNullOrEmpty(file.WebContentLink))
            {
                return file.WebContentLink;
            }

            return $"https://drive.google.com/uc?export=download&id={photoId}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get download URL for photo {PhotoId}", photoId);
            return $"https://drive.google.com/file/d/{photoId}/view";
        }
    }

    public async Task<bool> DeletePhotoAsync(string photoId)
    {
        try
        {
            await _driveService.Files.Delete(photoId).ExecuteAsync();
            _logger.LogInformation("Successfully deleted photo {PhotoId}", photoId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete photo {PhotoId}", photoId);
            return false;
        }
    }

    public async Task<bool> VerifyFolderExistsAsync(string folderId)
    {
        try
        {
            var request = _driveService.Files.Get(folderId);
            request.Fields = "id,name,mimeType";

            var folder = await request.ExecuteAsync();

            return folder != null && folder.MimeType == "application/vnd.google-apps.folder";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not verify folder exists: {FolderId}", folderId);
            return false;
        }
    }

    public async Task<long> GetFolderSizeAsync(string folderId)
    {
        try
        {
            var request = _driveService.Files.List();
            request.Q = $"'{folderId}' in parents and trashed=false";
            request.Fields = "files(id,name,size)";

            var result = await request.ExecuteAsync();

            long totalSize = 0;
            if (result.Files != null)
            {
                foreach (var file in result.Files)
                {
                    if (file.Size.HasValue)
                        totalSize += file.Size.Value;
                }
            }

            return totalSize;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating folder size for: {FolderId}", folderId);
            return 0;
        }
    }

    private static string GenerateSecureFileName(string extension)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var randomSuffix = GenerateRandomString(8);
        return $"photo_{timestamp}_{randomSuffix}{extension}".ToLower();
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var bytes = RandomNumberGenerator.GetBytes(length);
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }

    private static bool IsAllowedFileType(string fileName)
    {
        var allowedExtensions = new[]
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp",
            ".webp", ".heic", ".tiff"
        };
        var extension = Path.GetExtension(fileName).ToLower();
        return allowedExtensions.Contains(extension);
    }

    private static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".heic" => "image/heic",
            ".tiff" or ".tif" => "image/tiff",
            _ => "application/octet-stream"
        };
    }

    private string ExtractFolderIdFromUrl(string folderUrl)
    {
        try
        {
            var patterns = new[]
            {
                @"/folders/([a-zA-Z0-9_-]+)",
                @"[?&]id=([a-zA-Z0-9_-]+)",
                @"/file/d/([a-zA-Z0-9_-]+)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(folderUrl, pattern);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            if (Regex.IsMatch(folderUrl, @"^[a-zA-Z0-9_-]+$"))
            {
                return folderUrl;
            }

            throw new ArgumentException($"Invalid Google Drive folder URL: {folderUrl}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract folder ID from URL: {FolderUrl}", folderUrl);
            throw;
        }
    }

    private static string GetOptimizedThumbnailUrl(string fileId, string? originalThumbnailLink)
    {
        if (!string.IsNullOrEmpty(originalThumbnailLink))
        {
            return originalThumbnailLink;
        }

        return $"https://drive.google.com/thumbnail?id={fileId}&sz=w400-h300";
    }

    private static string GetOptimizedImageUrl(string fileId)
    {
        return $"https://drive.google.com/uc?id={fileId}&export=view";
    }

    public void Dispose()
    {
        _driveService?.Dispose();
    }

    // Helper class for deserializing Google service account JSON
    private class JsonCredentialParameters
    {
        [JsonProperty("client_email")]
        public string ClientEmail { get; set; } = string.Empty;

        [JsonProperty("private_key")]
        public string PrivateKey { get; set; } = string.Empty;
    }
}