using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using WeddingPhotos.Domain.Constants;

namespace WeddingPhotos.Domain.Models
{
    /// <summary>
    /// Tolerate fields the running code does not know about. Without this the
    /// driver throws on any element with no matching property, which makes a
    /// rollback dangerous rather than cheap: deploy a version that adds a field,
    /// let one gallery be saved, roll back, and reading that document fails -
    /// taking the gallery and the admin list with it.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Client
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Guid")]
        [BsonRequired]
        public string Guid { get; set; } = string.Empty;

        // ⭐ PERSONAL DATA
        [BsonElement("Name")]
        public string FirstName { get; set; } = string.Empty;

        [BsonElement("LastName")]
        public string LastName { get; set; } = string.Empty;

        [BsonElement("Email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("Phone")]
        public string? Phone { get; set; }

        // EVENT INFO
        [BsonElement("EventName")]
        public string EventName { get; set; } = string.Empty;

        [BsonElement("EventType")]
        [BsonRequired]
        public string EventType { get; set; } = "Wedding"; // Default

        [BsonElement("EventDate")]
        public DateTime? EventDate { get; set; }

        [BsonElement("DateTo")]
        [BsonRequired]
        public DateTime DateTo { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; } = true;

        // UPLOAD LIMITS
        [BsonElement("MaxFiles")]
        public int MaxFiles { get; set; } = 300;

        [BsonElement("UploadedFilesCount")]
        public int UploadedFilesCount { get; set; } = 0;

        [BsonElement("MaxFileSize")]
        public long MaxFileSize { get; set; } = 20971520; // 20MB

        // Videos are opt-in per gallery rather than per package, because packages
        // are not an entity in the database - just a helper in the admin panel. The
        // panel turns this on for Premium; nothing stops turning it on elsewhere.
        [BsonElement("AllowVideos")]
        public bool AllowVideos { get; set; } = false;

        // THEME CUSTOMIZATION
        [BsonElement("BackgroundColor")]
        public string BackgroundColor { get; set; } = "#667eea";

        [BsonElement("BackgroundColorSecondary")]
        public string BackgroundColorSecondary { get; set; } = "#764ba2";

        [BsonElement("FontColor")]
        public string FontColor { get; set; } = "#ffffff";

        [BsonElement("FontType")]
        public string FontType { get; set; } = "Roboto";

        [BsonElement("AccentColor")]
        public string AccentColor { get; set; } = "#3b82f6";

        // STORAGE
        [BsonElement("GoogleStorageUrl")]
        public string GoogleStorageUrl { get; set; } = string.Empty;

        // TIMESTAMPS
        [BsonElement("CreateDate")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Helper method to get event type display name
        public string GetEventTypeDisplayName()
        {
            return EventType switch
            {
                ApplicationConstants.EventTypes.Wedding => ApplicationConstants.EventTypeDisplayNames.Wedding,
                ApplicationConstants.EventTypes.Birthday => ApplicationConstants.EventTypeDisplayNames.Birthday,
                ApplicationConstants.EventTypes.Baptism => ApplicationConstants.EventTypeDisplayNames.Baptism,
                ApplicationConstants.EventTypes.Communion => ApplicationConstants.EventTypeDisplayNames.Communion,
                ApplicationConstants.EventTypes.Corporate => ApplicationConstants.EventTypeDisplayNames.Corporate,
                ApplicationConstants.EventTypes.Conference => ApplicationConstants.EventTypeDisplayNames.Conference,
                ApplicationConstants.EventTypes.Other => ApplicationConstants.EventTypeDisplayNames.Other,
                _ => ApplicationConstants.EventTypeDisplayNames.Default
            };
        }

        // Helper method to get appropriate emoji
        public string GetEventTypeEmoji()
        {
            return EventType switch
            {
                ApplicationConstants.EventTypes.Wedding => ApplicationConstants.EventTypeEmojis.Wedding,
                ApplicationConstants.EventTypes.Birthday => ApplicationConstants.EventTypeEmojis.Birthday,
                ApplicationConstants.EventTypes.Baptism => ApplicationConstants.EventTypeEmojis.Baptism,
                ApplicationConstants.EventTypes.Communion => ApplicationConstants.EventTypeEmojis.Communion,
                ApplicationConstants.EventTypes.Corporate => ApplicationConstants.EventTypeEmojis.Corporate,
                ApplicationConstants.EventTypes.Conference => ApplicationConstants.EventTypeEmojis.Conference,
                ApplicationConstants.EventTypes.Other => ApplicationConstants.EventTypeEmojis.Other,
                _ => ApplicationConstants.EventTypeEmojis.Default
            };
        }

        // Helper to get full name
        public string GetFullName()
        {
            if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
                return $"{FirstName} {LastName}";

            if (!string.IsNullOrWhiteSpace(FirstName))
                return FirstName;

            if (!string.IsNullOrWhiteSpace(LastName))
                return LastName;

            return "Klient";
        }
    }
}