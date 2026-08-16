using WeddingPhotos.Domain.Models;

namespace WeddingPhotos.Domain.Interfaces;

public interface IGoogleStorageService
{
    Task<string?> UploadPhotoAsync(Stream fileStream, string fileName, string folderId);
    Task<List<PhotoInfo>> GetPhotosFromFolderAsync(string folderUrl, int page = 1, int pageSize = 100);
    /// <param name="thumbnailSize">
    /// When set, serves Drive's thumbnail rendered at roughly this many pixels
    /// on its longest edge instead of the original file. Falls back to the
    /// original if Drive has not generated a thumbnail yet.
    /// </param>
    Task<(Stream stream, string mimeType, string fileName, long? length)> GetPhotoStreamAsync(
        string photoId,
        int? thumbnailSize = null);
    Task<bool> DeletePhotoAsync(string photoId);
    Task<bool> VerifyFolderExistsAsync(string folderId);
    Task<long> GetFolderSizeAsync(string folderId);
}
