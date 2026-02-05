namespace WeddingPhotos.Infrastructure.Configuration;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ClientsCollectionName { get; set; } = "Clients";
    public string ContactMessagesCollectionName { get; set; } = "ContactMessages";
}
