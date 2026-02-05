namespace WeddingPhotos.Domain.Models;

public class PhotoInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string FullUrl { get; set; } = string.Empty;
    public DateTime DateAdded { get; set; }
    public long Size { get; set; }
    public string MimeType { get; set; } = string.Empty;

    // Helper properties
    public bool IsNew => (DateTime.Now - DateAdded).TotalDays < 1;
    public string FormattedSize => FormatFileSize(Size);
    public string FileExtension => Path.GetExtension(Name).ToUpperInvariant().TrimStart('.');
    public bool IsImage => MimeType.StartsWith("image/");

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size = size / 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}
