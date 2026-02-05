using WeddingPhotos.Domain.Models;

namespace WeddingPhotos.Domain.Interfaces;

public interface IGoogleStorageService
{
    Task<string?> UploadPhotoAsync(Stream fileStream, string fileName, string folderId);
    Task<List<PhotoInfo>> GetPhotosFromFolderAsync(string folderUrl, int page = 1, int pageSize = 100);
    Task<string> GetPhotoDownloadUrlAsync(string photoId);
    Task<bool> DeletePhotoAsync(string photoId);
    Task<bool> VerifyFolderExistsAsync(string folderId);
    Task<long> GetFolderSizeAsync(string folderId);
}
