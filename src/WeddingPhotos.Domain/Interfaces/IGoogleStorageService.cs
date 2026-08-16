using WeddingPhotos.Domain.Models;

namespace WeddingPhotos.Domain.Interfaces;

public interface IGoogleStorageService
{
    Task<string?> UploadPhotoAsync(Stream fileStream, string fileName, string folderId);
    Task<List<PhotoInfo>> GetPhotosFromFolderAsync(string folderUrl, int page = 1, int pageSize = 100);

    /// <summary>
    /// Total number of photos in the folder, across all pages.
    /// </summary>
    Task<int> GetPhotoCountAsync(string folderUrl);
    /// <param name="thumbnailSize">
    /// When set, serves Drive's thumbnail rendered at roughly this many pixels
    /// on its longest edge instead of the original file. Falls back to the
    /// original if Drive has not generated a thumbnail yet.
    /// </param>
    /// <param name="rangeHeader">
    /// A raw HTTP Range header to forward to storage, letting an interrupted
    /// download resume. Ignored for thumbnails, which are small enough that
    /// resuming them is pointless.
    /// </param>
    Task<PhotoStreamResult> GetPhotoStreamAsync(
        string photoId,
        int? thumbnailSize = null,
        string? rangeHeader = null);
    /// <summary>
    /// Opens a Drive resumable upload session and returns its session URL, which
    /// the browser then writes to directly in chunks.
    ///
    /// The file's name and parent folder are fixed here, server-side, so the
    /// client only ever supplies bytes - it cannot choose where the photo lands
    /// or what it is called.
    /// </summary>
    /// <param name="origin">
    /// The browser's Origin. Google echoes it onto the session so cross-origin
    /// chunk uploads are allowed; without it the browser is refused.
    /// </param>
    Task<string> CreateResumableUploadSessionAsync(
        string fileName,
        long fileSize,
        string folderId,
        string? origin);

    Task<bool> DeletePhotoAsync(string photoId);
    Task<bool> VerifyFolderExistsAsync(string folderId);
    Task<long> GetFolderSizeAsync(string folderId);
}
