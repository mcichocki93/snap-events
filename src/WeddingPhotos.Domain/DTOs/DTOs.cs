using WeddingPhotos.Domain.Models;

namespace WeddingPhotos.Domain.DTOs;

public class GalleryResponse
{
    public List<PhotoInfo> Photos { get; set; } = new();
    public int TotalCount { get; set; }
    public bool HasMore { get; set; }
    public string? NextPageToken { get; set; }
}

public class ClientResponse
{
    public string Guid { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;

    // ⭐ PERSONAL DATA (opcjonalne - nie zawsze wysyłane do frontend)
    // Te pola mogą być null jeśli nie chcesz ich wysyłać do galerii publicznej
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // EVENT TYPE
    public string EventType { get; set; } = "Wedding";
    public string EventTypeDisplayName { get; set; } = string.Empty;
    public string EventTypeEmoji { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }

    public DateTime DateTo { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public int MaxFiles { get; set; }
    public int UploadedFilesCount { get; set; }
    public bool CanUploadMore { get; set; }
    public long MaxFileSize { get; set; }

    // Theme settings
    public string BackgroundColor { get; set; } = string.Empty;
    public string BackgroundColorSecondary { get; set; } = string.Empty;
    public string FontColor { get; set; } = string.Empty;
    public string FontType { get; set; } = string.Empty;
    public string AccentColor { get; set; } = string.Empty;
}

public class UploadPhotoResponse
{
    public bool Success { get; set; }
    public string? PhotoId { get; set; }
    public string? Message { get; set; }
    public int RemainingUploads { get; set; }
}

// ⭐ For admin/panel to create clients
public class CreateClientRequest
{
    // PERSONAL DATA
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // EVENT INFO
    public string Guid { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventType { get; set; } = "Wedding";
    public DateTime? EventDate { get; set; }
    public DateTime DateTo { get; set; }

    // LIMITS
    public int MaxFiles { get; set; } = 300;
    public long MaxFileSize { get; set; } = 10485760;

    // THEME (optional)
    public string? BackgroundColor { get; set; }
    public string? BackgroundColorSecondary { get; set; }
    public string? FontColor { get; set; }
    public string? FontType { get; set; }
    public string? AccentColor { get; set; }

    // STORAGE
    public string GoogleStorageUrl { get; set; } = string.Empty;
}

public class UpdateClientRequest
{
    // PERSONAL DATA
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // EVENT INFO
    public string? EventName { get; set; }
    public string? EventType { get; set; }
    public DateTime? EventDate { get; set; }
    public DateTime? DateTo { get; set; }
    public bool? IsActive { get; set; }

    // LIMITS
    public int? MaxFiles { get; set; }
    public long? MaxFileSize { get; set; }

    // THEME
    public string? BackgroundColor { get; set; }
    public string? BackgroundColorSecondary { get; set; }
    public string? FontColor { get; set; }
    public string? FontType { get; set; }
    public string? AccentColor { get; set; }

    // STORAGE
    public string? GoogleStorageUrl { get; set; }
}