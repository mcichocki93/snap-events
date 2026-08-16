using System.Text.RegularExpressions;
using WeddingPhotos.Domain.Constants;

namespace WeddingPhotos.Api.Validators;

/// <summary>
/// Rules shared by the create and update client validators.
///
/// These live in one place because the two copies drifted apart: create
/// accepted a bare Drive folder ID and treated MaxFiles = 0 as "no limit",
/// while update rejected both. Since the Standard and Premium packages set
/// MaxFiles = 0 and the form's placeholder recommends the bare folder ID,
/// nearly every gallery became impossible to edit.
/// </summary>
internal static class ClientValidationRules
{
    public const int MaxFilesLimit = 10000;

    public static bool IsValidEventType(string? eventType)
    {
        return !string.IsNullOrEmpty(eventType)
            && ApplicationConstants.EventTypes.All.Contains(eventType);
    }

    /// <summary>
    /// Accepts either a bare Drive folder ID or a full Drive URL, because the
    /// admin form asks for the ID but pasting the whole URL is the obvious thing
    /// to do.
    /// </summary>
    public static bool IsValidGoogleDriveInput(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        // Bare folder ID: alphanumeric + dash + underscore
        if (Regex.IsMatch(value, @"^[a-zA-Z0-9_-]{10,}$")) return true;

        // Full Google Drive URL
        return value.Contains("drive.google.com");
    }
}
