using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Collections.Concurrent;
using WeddingPhotos.Domain.Interfaces;
using WeddingPhotos.Infrastructure.Configuration;

namespace WeddingPhotos.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IConnectionMultiplexer? _redisConnection;
    private readonly IDatabase? _redisDatabase;
    private readonly ILogger<CacheService> _logger;
    private readonly RedisCacheSettings _settings;
    private readonly bool _isRedisAvailable;
    private readonly ConcurrentDictionary<string, byte> _memoryKeys = new();

    public CacheService(
        IMemoryCache memoryCache,
        ILogger<CacheService> logger,
        IOptions<RedisCacheSettings> settings,
        IConnectionMultiplexer? redisConnection = null)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _settings = settings.Value;
        _redisConnection = redisConnection;

        if (_settings.Enabled && _redisConnection != null)
        {
            try
            {
                _redisDatabase = _redisConnection.GetDatabase();
                _isRedisAvailable = _redisConnection.IsConnected;

                if (_isRedisAvailable)
                {
                    _logger.LogInformation("Redis cache is enabled and connected");
                }
                else
                {
                    _logger.LogWarning("Redis is configured but not connected, falling back to memory cache");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Redis, falling back to memory cache");
                _isRedisAvailable = false;
            }
        }
        else
        {
            _logger.LogInformation("Redis cache is disabled, using memory cache only");
            _isRedisAvailable = false;
        }
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            // Try Redis first if available
            if (_isRedisAvailable && _redisDatabase != null)
            {
                var redisValue = await _redisDatabase.StringGetAsync(key);
                if (redisValue.HasValue)
                {
                    var value = JsonConvert.DeserializeObject<T>(redisValue.ToString());

                    // Also cache in memory for faster subsequent access
                    _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));

                    _logger.LogDebug("Cache HIT from Redis: {Key}", key);
                    return value;
                }
            }

            // Fallback to memory cache
            if (_memoryCache.TryGetValue<T>(key, out var memoryCachedValue))
            {
                _logger.LogDebug("Cache HIT from Memory: {Key}", key);
                return memoryCachedValue;
            }

            _logger.LogDebug("Cache MISS: {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading from cache: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var expirationTime = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);

            // Set in Redis if available
            if (_isRedisAvailable && _redisDatabase != null)
            {
                var serialized = JsonConvert.SerializeObject(value);
                await _redisDatabase.StringSetAsync(key, serialized, expirationTime);
                _logger.LogDebug("Cache SET in Redis: {Key}, Expiration: {Expiration}min", key, expirationTime.TotalMinutes);
            }

            // Always set in memory cache as fallback
            _memoryCache.Set(key, value, expirationTime);
            _memoryKeys.TryAdd(key, 0);
            _logger.LogDebug("Cache SET in Memory: {Key}, Expiration: {Expiration}min", key, expirationTime.TotalMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing to cache: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            // Remove from Redis
            if (_isRedisAvailable && _redisDatabase != null)
            {
                await _redisDatabase.KeyDeleteAsync(key);
                _logger.LogDebug("Cache REMOVED from Redis: {Key}", key);
            }

            // Remove from memory cache
            _memoryCache.Remove(key);
            _memoryKeys.TryRemove(key, out _);
            _logger.LogDebug("Cache REMOVED from Memory: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from cache: {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            // Remove from Redis (scan and delete keys with prefix)
            if (_isRedisAvailable && _redisConnection != null)
            {
                var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());
                var keys = server.Keys(pattern: $"{prefix}*").ToArray();

                if (keys.Length > 0 && _redisDatabase != null)
                {
                    await _redisDatabase.KeyDeleteAsync(keys);
                    _logger.LogDebug("Cache REMOVED {Count} keys from Redis with prefix: {Prefix}", keys.Length, prefix);
                }
            }

            // Remove matching keys from memory cache using the tracked key set
            var keysToRemove = _memoryKeys.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList();
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _memoryKeys.TryRemove(key, out _);
            }
            _logger.LogDebug("Cache REMOVED {Count} keys from Memory with prefix: {Prefix}", keysToRemove.Count, prefix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache by prefix: {Prefix}", prefix);
        }
    }
}
