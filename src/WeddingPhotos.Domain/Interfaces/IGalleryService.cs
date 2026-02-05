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

    Task<(bool Success, string? DownloadUrl, string? ErrorMessage)> GetPhotoDownloadUrlAsync(
        string photoId);
}
