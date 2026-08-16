using WeddingPhotos.Domain.DTOs;
using WeddingPhotos.Domain.Models;

namespace WeddingPhotos.Domain.Interfaces;

public interface IGalleryService
{
    Task<(bool Success, GalleryResponse? Response, string? ErrorMessage)> GetGalleryAsync(
        string guid,
        int page,
        int pageSize);

    Task<(bool Success, UploadPhotoResponse Response, string? ErrorMessage)> UploadPhotoAsync(
        string guid,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize);

    /// <summary>
    /// Validates the gallery and the file, reserves a quota slot, and opens a
    /// Drive session the browser uploads to directly.
    /// </summary>
    Task<(bool Success, CreateUploadSessionResponse Response, string? ErrorMessage)> CreateUploadSessionAsync(
        string guid,
        string fileName,
        string mimeType,
        long fileSize,
        string? origin);

    /// <summary>
    /// Called once the browser has finished writing to the session. Keeps the
    /// reserved slot and drops the cached gallery pages.
    /// </summary>
    Task<(bool Success, UploadPhotoResponse Response, string? ErrorMessage)> CompleteUploadSessionAsync(
        string guid,
        string photoId);

    /// <summary>
    /// Called when a direct upload is abandoned, so the reserved slot goes back.
    /// </summary>
    Task CancelUploadSessionAsync(string guid);

    Task<(bool Success, PhotoStreamResult? Photo, string? ErrorMessage)> GetPhotoStreamAsync(
        string photoId,
        int? thumbnailSize = null,
        string? rangeHeader = null);
}
