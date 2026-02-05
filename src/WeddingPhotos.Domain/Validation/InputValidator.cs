using System.Text.RegularExpressions;
using WeddingPhotos.Domain.Constants;

namespace WeddingPhotos.Domain.Validation;

public static class InputValidator
{
    // GUID validation - alphanumeric, dash, underscore (6-50 chars for flexibility)
    private static readonly Regex GuidRegex = new Regex(
        @"^[a-zA-Z0-9_-]{6,50}$",
        RegexOptions.Compiled
    );

    // Suspicious patterns for MongoDB injection and path traversal
    private static readonly string[] SuspiciousPatterns = new[]
    {
        "$where",
        "$ne",
        "$gt",
        "$lt",
        "$regex",
        "$exists",
        "$in",
        "$nin",
        "javascript:",
        "<script",
        "../",
        "..\\",
        ":",
        "//",
        "\\\\",
        "%2e%2e",
        "%252e"
    };

    // File name validation - no path traversal characters
    private static readonly Regex FileNameRegex = new Regex(
        @"^[a-zA-Z0-9_\-\.\s()]+$",
        RegexOptions.Compiled
    );

    // Allowed image MIME types - using constants from ApplicationConstants
    private static readonly HashSet<string> AllowedImageMimeTypes = ApplicationConstants.AllowedMimeTypes.Images;

    // Allowed image extensions - using constants from ApplicationConstants
    private static readonly HashSet<string> AllowedImageExtensions = ApplicationConstants.AllowedFileExtensions.Images;

    /// <summary>
    /// Validates GUID format
    /// </summary>
    public static bool IsValidGuid(string guid)
    {
        if (string.IsNullOrWhiteSpace(guid))
            return false;

        return GuidRegex.IsMatch(guid);
    }

    /// <summary>
    /// Checks for suspicious patterns (MongoDB injection, path traversal, XSS)
    /// </summary>
    public static bool ContainsSuspiciousPatterns(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var lowerInput = input.ToLowerInvariant();

        foreach (var pattern in SuspiciousPatterns)
        {
            if (lowerInput.Contains(pattern.ToLowerInvariant()))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates file name
    /// </summary>
    public static bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        // Check for path traversal patterns
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\") || fileName.Contains(":"))
            return false;

        return FileNameRegex.IsMatch(fileName);
    }

    /// <summary>
    /// Validates image file by MIME type and extension
    /// </summary>
    public static bool IsValidImageFile(string fileName, string mimeType)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(mimeType))
            return false;

        // Check MIME type
        if (!AllowedImageMimeTypes.Contains(mimeType.ToLowerInvariant()))
            return false;

        // Check extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
            return false;

        return true;
    }

    /// <summary>
    /// Validates file size
    /// </summary>
    public static bool IsValidFileSize(long fileSize, long maxFileSize)
    {
        return fileSize > 0 && fileSize <= maxFileSize;
    }

    /// <summary>
    /// Sanitizes file name (removes dangerous characters)
    /// </summary>
    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        // Remove path traversal characters
        fileName = fileName.Replace("..", "");
        fileName = fileName.Replace("/", "");
        fileName = fileName.Replace("\\", "");
        fileName = fileName.Replace(":", "");

        // Remove any other potentially dangerous characters
        fileName = Regex.Replace(fileName, @"[^\w\s\-\.]", "");

        // Limit length
        if (fileName.Length > ApplicationConstants.FileUpload.MaxFileNameLength)
            fileName = fileName.Substring(0, ApplicationConstants.FileUpload.MaxFileNameLength);

        return fileName;
    }

    /// <summary>
    /// Validates EventType
    /// </summary>
    public static bool IsValidEventType(string eventType)
    {
        return ApplicationConstants.EventTypes.All.Contains(eventType);
    }

    /// <summary>
    /// Validates hex color code
    /// </summary>
    public static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        // Matches #RGB, #RRGGBB, #RRGGBBAA
        var hexColorRegex = new Regex(@"^#([A-Fa-f0-9]{3}|[A-Fa-f0-9]{6}|[A-Fa-f0-9]{8})$");
        return hexColorRegex.IsMatch(color);
    }

    /// <summary>
    /// Validates font name (alphanumeric + spaces)
    /// </summary>
    public static bool IsValidFontName(string fontName)
    {
        if (string.IsNullOrWhiteSpace(fontName))
            return false;

        var fontNameRegex = new Regex(@"^[a-zA-Z0-9\s\-]+$");
        return fontNameRegex.IsMatch(fontName);
    }
}
