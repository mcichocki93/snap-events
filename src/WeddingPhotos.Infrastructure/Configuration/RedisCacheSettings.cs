namespace WeddingPhotos.Infrastructure.Configuration;

public class RedisCacheSettings
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public bool Enabled { get; set; } = false; // Disabled by default, fallback to memory cache
    public int DefaultExpirationMinutes { get; set; } = 30;
    public int GalleryCacheExpirationMinutes { get; set; } = 15;
}
