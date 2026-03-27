using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using WeddingPhotos.Domain.Interfaces;
using WeddingPhotos.Infrastructure.Configuration;
using WeddingPhotos.Infrastructure.Repositories;
using WeddingPhotos.Infrastructure.Services;

namespace WeddingPhotos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Note: MongoDbSettings and GoogleCloudSettings are configured in Program.cs
        // with environment variable overrides — do not re-configure here.

        // Configure Redis with environment variable overrides
        var redisEnabled = bool.TryParse(
            Environment.GetEnvironmentVariable("REDIS_ENABLED")
            ?? configuration["RedisCache:Enabled"],
            out var enabled) && enabled;

        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
            ?? configuration["RedisCache:ConnectionString"];

        services.Configure<RedisCacheSettings>(options =>
        {
            options.Enabled = redisEnabled;
            options.ConnectionString = redisConnectionString ?? "localhost:6379";
            options.DefaultExpirationMinutes = int.TryParse(
                Environment.GetEnvironmentVariable("REDIS_DEFAULT_EXPIRATION_MINUTES")
                ?? configuration["RedisCache:DefaultExpirationMinutes"],
                out var defaultExp) ? defaultExp : 30;
            options.GalleryCacheExpirationMinutes = int.TryParse(
                Environment.GetEnvironmentVariable("REDIS_GALLERY_CACHE_EXPIRATION_MINUTES")
                ?? configuration["RedisCache:GalleryCacheExpirationMinutes"],
                out var galleryExp) ? galleryExp : 15;
        });

        // Register HttpClient for GoogleStorageService
        services.AddHttpClient();

        // Register Memory Cache
        services.AddMemoryCache();

        // Register Redis (optional - will fallback to memory cache if disabled)
        if (redisEnabled && !string.IsNullOrEmpty(redisConnectionString))
        {
            try
            {
                var redis = ConnectionMultiplexer.Connect(redisConnectionString);
                services.AddSingleton<IConnectionMultiplexer>(redis);
            }
            catch (Exception)
            {
                // Redis not available, will use memory cache only
                services.AddSingleton<IConnectionMultiplexer>(provider => null!);
            }
        }
        else
        {
            services.AddSingleton<IConnectionMultiplexer>(provider => null!);
        }

        // Register AutoMapper
        services.AddAutoMapper(typeof(Mapping.MappingProfile));

        // Register cache service
        services.AddSingleton<ICacheService, CacheService>();

        // Register repositories
        services.AddSingleton<IClientRepository, ClientRepository>();
        services.AddSingleton<IContactMessageRepository, ContactMessageRepository>();

        // Register services
        services.AddSingleton<IGoogleStorageService, GoogleStorageService>();
        services.AddScoped<IGalleryService, GalleryService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
