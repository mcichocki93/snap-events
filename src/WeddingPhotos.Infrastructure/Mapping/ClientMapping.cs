using WeddingPhotos.Domain.DTOs;
using WeddingPhotos.Domain.Models;

namespace WeddingPhotos.Infrastructure.Mapping;

/// <summary>
/// Translations between the stored client and the shapes the API exposes.
///
/// Written out by hand rather than configured in a mapping library: every field
/// was already spelled out one by one in the old profile, so the library was
/// adding a dependency and a layer of indirection without doing any of the
/// work. Plain methods also mean the compiler notices when a field is added.
/// </summary>
public static class ClientMapping
{
    /// <summary>
    /// The public view of a gallery. Personal details are deliberately left
    /// unset - this response is served to every guest holding the link.
    /// </summary>
    public static ClientResponse ToResponse(this Client client)
    {
        return new ClientResponse
        {
            Guid = client.Guid,
            EventName = client.EventName,
            EventType = client.EventType,
            EventTypeDisplayName = client.GetEventTypeDisplayName(),
            EventTypeEmoji = client.GetEventTypeEmoji(),
            EventDate = client.EventDate,
            DateTo = client.DateTo,
            IsActive = client.IsActive,
            IsExpired = client.DateTo < DateTime.UtcNow,
            MaxFiles = client.MaxFiles,
            UploadedFilesCount = client.UploadedFilesCount,
            // MaxFiles == 0 means no limit
            CanUploadMore = client.MaxFiles == 0 || client.UploadedFilesCount < client.MaxFiles,
            MaxFileSize = client.MaxFileSize,
            BackgroundColor = client.BackgroundColor,
            BackgroundColorSecondary = client.BackgroundColorSecondary,
            FontColor = client.FontColor,
            FontType = client.FontType,
            AccentColor = client.AccentColor
        };
    }

    /// <summary>
    /// A new gallery from an admin's create request. Id is left unset for
    /// MongoDB to generate.
    /// </summary>
    public static Client ToClient(this CreateClientRequest request)
    {
        var now = DateTime.UtcNow;

        return new Client
        {
            Guid = request.Guid,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            EventName = request.EventName,
            EventType = request.EventType,
            EventDate = request.EventDate,
            DateTo = request.DateTo,
            IsActive = true,
            MaxFiles = request.MaxFiles,
            UploadedFilesCount = 0,
            MaxFileSize = request.MaxFileSize,
            BackgroundColor = request.BackgroundColor ?? "#667eea",
            BackgroundColorSecondary = request.BackgroundColorSecondary ?? "#764ba2",
            FontColor = request.FontColor ?? "#ffffff",
            FontType = request.FontType ?? "Roboto",
            AccentColor = request.AccentColor ?? "#3b82f6",
            GoogleStorageUrl = request.GoogleStorageUrl,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
