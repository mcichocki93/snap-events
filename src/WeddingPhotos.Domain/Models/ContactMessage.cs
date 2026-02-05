using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WeddingPhotos.Domain.Models;

public class ContactMessage
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("phone")]
    public string? Phone { get; set; }

    [BsonElement("subject")]
    public string? Subject { get; set; }

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    [BsonElement("isRead")]
    public bool IsRead { get; set; } = false;

    [BsonElement("readAt")]
    public DateTime? ReadAt { get; set; }

    [BsonElement("notificationSent")]
    public bool NotificationSent { get; set; } = false;

    [BsonElement("notificationSentAt")]
    public DateTime? NotificationSentAt { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }
}
