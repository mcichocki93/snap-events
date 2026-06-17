using System.Text.RegularExpressions;

namespace WeddingPhotos.Domain.Validation;

/// <summary>
/// Helpers for working with Google Drive folder references. Accepts either a
/// full Drive URL or a bare folder ID and normalises it to the folder ID.
/// </summary>
public static class GoogleDriveHelper
{
    private static readonly string[] Patterns =
    {
        @"/folders/([a-zA-Z0-9_-]+)",
        @"[?&]id=([a-zA-Z0-9_-]+)",
        @"/file/d/([a-zA-Z0-9_-]+)"
    };

    /// <summary>
    /// Extracts the folder ID from a Google Drive URL, or returns the input
    /// unchanged if it is already a bare folder ID. Returns the original input
    /// as a best-effort fallback when no pattern matches (never throws).
    /// </summary>
    public static string ExtractFolderId(string folderUrlOrId)
    {
        if (string.IsNullOrWhiteSpace(folderUrlOrId))
            return string.Empty;

        foreach (var pattern in Patterns)
        {
            var match = Regex.Match(folderUrlOrId, pattern);
            if (match.Success)
                return match.Groups[1].Value;
        }

        return folderUrlOrId;
    }
}
