using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WeddingPhotos.Domain.Interfaces;
using WeddingPhotos.Domain.Models;
using WeddingPhotos.Infrastructure.Configuration;

namespace WeddingPhotos.Infrastructure.Repositories;

public class ContactMessageRepository : IContactMessageRepository
{
    private readonly IMongoCollection<ContactMessage> _contactMessagesCollection;
    private readonly ILogger<ContactMessageRepository> _logger;

    public ContactMessageRepository(
        IMongoDatabase database,
        IOptions<MongoDbSettings> settings,
        ILogger<ContactMessageRepository> logger)
    {
        _logger = logger;

        try
        {
            var mongoSettings = settings.Value;
            var collectionName = mongoSettings.ContactMessagesCollectionName ?? "ContactMessages";
            _contactMessagesCollection = database.GetCollection<ContactMessage>(collectionName);

            CreateIndexes();

            _logger.LogInformation("ContactMessageRepository initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ContactMessageRepository");
            throw;
        }
    }

    private void CreateIndexes()
    {
        try
        {
            // Index for email (for searching)
            var emailIndexModel = new CreateIndexModel<ContactMessage>(
                Builders<ContactMessage>.IndexKeys.Ascending(x => x.Email));

            // Index for createdAt (for sorting by date)
            var createdAtIndexModel = new CreateIndexModel<ContactMessage>(
                Builders<ContactMessage>.IndexKeys.Descending(x => x.CreatedAt));

            // Index for isRead (for filtering unread messages)
            var isReadIndexModel = new CreateIndexModel<ContactMessage>(
                Builders<ContactMessage>.IndexKeys.Ascending(x => x.IsRead));

            _contactMessagesCollection.Indexes.CreateMany(new[]
            {
                emailIndexModel,
                createdAtIndexModel,
                isReadIndexModel
            });

            _logger.LogDebug("ContactMessage indexes created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create ContactMessage indexes");
        }
    }

    public async Task<ContactMessage> CreateAsync(ContactMessage message)
    {
        try
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message.CreatedAt = DateTime.UtcNow;
            message.IsRead = false;

            await _contactMessagesCollection.InsertOneAsync(message);

            _logger.LogInformation(
                "Contact message created: {Name} ({Email}), ID: {Id}",
                message.Name,
                message.Email,
                message.Id);

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contact message");
            throw new ApplicationException("Błąd podczas zapisywania wiadomości", ex);
        }
    }

    public async Task<ContactMessage?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("GetByIdAsync called with empty id");
                return null;
            }

            var filter = Builders<ContactMessage>.Filter.Eq(x => x.Id, id);
            return await _contactMessagesCollection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contact message with id: {Id}", id);
            throw;
        }
    }

    public async Task<List<ContactMessage>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        try
        {
            var skip = (page - 1) * pageSize;

            return await _contactMessagesCollection
                .Find(_ => true)
                .SortByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contact messages");
            throw;
        }
    }

    public async Task<List<ContactMessage>> GetUnreadAsync()
    {
        try
        {
            var filter = Builders<ContactMessage>.Filter.Eq(x => x.IsRead, false);

            return await _contactMessagesCollection
                .Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unread contact messages");
            throw;
        }
    }

    public async Task<bool> MarkAsReadAsync(string id)
    {
        try
        {
            var filter = Builders<ContactMessage>.Filter.Eq(x => x.Id, id);
            var update = Builders<ContactMessage>.Update
                .Set(x => x.IsRead, true)
                .Set(x => x.ReadAt, DateTime.UtcNow);

            var result = await _contactMessagesCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                _logger.LogInformation("Contact message marked as read: {Id}", id);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking contact message as read: {Id}", id);
            throw;
        }
    }

    public async Task<bool> UpdateNotificationStatusAsync(string id, bool sent)
    {
        try
        {
            var filter = Builders<ContactMessage>.Filter.Eq(x => x.Id, id);
            var update = Builders<ContactMessage>.Update
                .Set(x => x.NotificationSent, sent)
                .Set(x => x.NotificationSentAt, sent ? DateTime.UtcNow : null);

            var result = await _contactMessagesCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification status for: {Id}", id);
            throw;
        }
    }

    public async Task<long> GetTotalCountAsync()
    {
        try
        {
            return await _contactMessagesCollection.CountDocumentsAsync(_ => true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total contact messages count");
            throw;
        }
    }

    public async Task<long> GetUnreadCountAsync()
    {
        try
        {
            var filter = Builders<ContactMessage>.Filter.Eq(x => x.IsRead, false);
            return await _contactMessagesCollection.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread contact messages count");
            throw;
        }
    }
}
