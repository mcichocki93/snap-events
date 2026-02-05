using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Cryptography;
using WeddingPhotos.Domain.Interfaces;
using WeddingPhotos.Domain.Models;
using WeddingPhotos.Infrastructure.Configuration;

namespace WeddingPhotos.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly IMongoCollection<Client> _clientsCollection;
    private readonly ILogger<ClientRepository> _logger;

    public ClientRepository(
        IOptions<MongoDbSettings> settings,
        ILogger<ClientRepository> logger)
    {
        _logger = logger;

        try
        {
            var mongoSettings = settings.Value;

            if (string.IsNullOrEmpty(mongoSettings.ConnectionString) || 
                string.IsNullOrEmpty(mongoSettings.DatabaseName))
            {
                throw new InvalidOperationException(
                    "MongoDB connection string or database name is not configured.");
            }

            var client = new MongoClient(mongoSettings.ConnectionString);
            var database = client.GetDatabase(mongoSettings.DatabaseName);
            _clientsCollection = database.GetCollection<Client>(mongoSettings.ClientsCollectionName);

            CreateIndexes();
            
            _logger.LogInformation("ClientRepository initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MongoDB connection");
            throw;
        }
    }

    private void CreateIndexes()
    {
        try
        {
            var guidIndexModel = new CreateIndexModel<Client>(
                Builders<Client>.IndexKeys.Ascending(x => x.Guid));

            var dateToIndexModel = new CreateIndexModel<Client>(
                Builders<Client>.IndexKeys.Ascending(x => x.DateTo));

            _clientsCollection.Indexes.CreateMany(new[] { guidIndexModel, dateToIndexModel });
            
            _logger.LogDebug("Database indexes created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create database indexes");
        }
    }

    public async Task<Client?> GetByGuidAsync(string guid)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                _logger.LogWarning("GetByGuidAsync called with empty guid");
                return null;
            }

            guid = guid.Trim();

            var filter = Builders<Client>.Filter.And(
                Builders<Client>.Filter.Eq(x => x.Guid, guid),
                Builders<Client>.Filter.Eq(x => x.IsActive, true)
            );

            var client = await _clientsCollection.Find(filter).FirstOrDefaultAsync();

            if (client == null)
            {
                _logger.LogWarning("Client not found for guid: {Guid}", guid);
                return null;
            }

            if (client.DateTo < DateTime.Now)
            {
                _logger.LogInformation("Client access expired for guid: {Guid}", guid);
            }

            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving client with guid: {Guid}", guid);
            throw new ApplicationException("Błąd podczas pobierania danych klienta", ex);
        }
    }

    public async Task<Client> CreateAsync(Client client)
    {
        try
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            ValidateClient(client);

            if (string.IsNullOrEmpty(client.Guid))
            {
                client.Guid = GenerateUniqueGuid();
            }

            client.CreatedAt = DateTime.Now;
            client.IsActive = true;
            client.UploadedFilesCount = 0;

            await _clientsCollection.InsertOneAsync(client);

            _logger.LogInformation(
                "Created new client: {Name} {LastName} with guid: {Guid}", 
                client.FirstName, 
                client.LastName, 
                client.Guid);

            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client");
            throw new ApplicationException("Błąd podczas tworzenia klienta", ex);
        }
    }

    public async Task UpdateUploadedFilesCountAsync(string guid, int additionalFiles)
    {
        try
        {
            var filter = Builders<Client>.Filter.Eq(x => x.Guid, guid);
            var update = Builders<Client>.Update.Inc(x => x.UploadedFilesCount, additionalFiles);

            await _clientsCollection.UpdateOneAsync(filter, update);

            _logger.LogInformation(
                "Updated uploaded files count for guid: {Guid}, added: {Count}", 
                guid, 
                additionalFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating uploaded files count for guid: {Guid}", guid);
            throw;
        }
    }

    public async Task<bool> DeactivateAsync(string guid)
    {
        try
        {
            var filter = Builders<Client>.Filter.Eq(x => x.Guid, guid);
            var update = Builders<Client>.Update.Set(x => x.IsActive, false);

            var result = await _clientsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                _logger.LogInformation("Deactivated client with guid: {Guid}", guid);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating client with guid: {Guid}", guid);
            throw;
        }
    }

    public async Task<List<Client>> GetExpiredClientsAsync()
    {
        try
        {
            var filter = Builders<Client>.Filter.And(
                Builders<Client>.Filter.Lt(x => x.DateTo, DateTime.Now),
                Builders<Client>.Filter.Eq(x => x.IsActive, true)
            );

            return await _clientsCollection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expired clients");
            throw;
        }
    }

    private void ValidateClient(Client client)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(client.FirstName))
            errors.Add("Imię jest wymagane");

        if (string.IsNullOrWhiteSpace(client.LastName))
            errors.Add("Nazwisko jest wymagane");

        if (string.IsNullOrWhiteSpace(client.Email) || !IsValidEmail(client.Email))
            errors.Add("Poprawny email jest wymagany");

        if (client.DateTo <= DateTime.Now)
            errors.Add("Data ważności musi być w przyszłości");

        if (string.IsNullOrWhiteSpace(client.GoogleStorageUrl))
            errors.Add("URL Google Storage jest wymagany");

        if (errors.Any())
        {
            throw new ArgumentException($"Błędy walidacji: {string.Join(", ", errors)}");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static string GenerateUniqueGuid()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "")
            [..12];
    }
}
