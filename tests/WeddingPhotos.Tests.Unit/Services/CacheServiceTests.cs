using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using StackExchange.Redis;
using WeddingPhotos.Infrastructure.Configuration;
using WeddingPhotos.Infrastructure.Services;

namespace WeddingPhotos.Tests.Unit.Services;

public class CacheServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _mockRedis;
    private readonly Mock<IDatabase> _mockRedisDatabase;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<CacheService>> _mockLogger;
    private readonly IOptions<RedisCacheSettings> _cacheSettings;

    public CacheServiceTests()
    {
        _mockRedis = new Mock<IConnectionMultiplexer>();
        _mockRedisDatabase = new Mock<IDatabase>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<CacheService>>();

        _cacheSettings = Options.Create(new RedisCacheSettings
        {
            Enabled = true,
            ConnectionString = "localhost:6379",
            DefaultExpirationMinutes = 30
        });

        _mockRedis.Setup(x => x.IsConnected).Returns(true);
        _mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_mockRedisDatabase.Object);
    }

    [Fact]
    public async Task GetAsync_WithRedisAvailable_ReturnsValueFromRedis()
    {
        // Arrange
        var key = "test-key";
        var testObject = new TestDto { Id = 1, Name = "Test" };
        var serialized = JsonConvert.SerializeObject(testObject);

        _mockRedisDatabase
            .Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(serialized));

        var cacheService = new CacheService(
            _memoryCache,
            _mockLogger.Object,
            _cacheSettings,
            _mockRedis.Object);

        // Act
        var result = await cacheService.GetAsync<TestDto>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testObject.Id);
        result.Name.Should().Be(testObject.Name);

        _mockRedisDatabase.Verify(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WithRedisUnavailable_FallsBackToMemoryCache()
    {
        // Arrange
        var key = "test-key";
        var testObject = new TestDto { Id = 1, Name = "Test" };

        var mockRedisDisconnected = new Mock<IConnectionMultiplexer>();
        mockRedisDisconnected.Setup(x => x.IsConnected).Returns(false);

        var cacheService = new CacheService(
            _memoryCache,
            _mockLogger.Object,
            _cacheSettings,
            mockRedisDisconnected.Object);

        // Pre-populate memory cache
        _memoryCache.Set(key, testObject);

        // Act
        var result = await cacheService.GetAsync<TestDto>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testObject.Id);
        result.Name.Should().Be(testObject.Name);
    }

    [Fact]
    public async Task GetAsync_WithNoCache_ReturnsNull()
    {
        // Arrange
        var key = "non-existent-key";

        _mockRedisDatabase
            .Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var cacheService = new CacheService(
            _memoryCache,
            _mockLogger.Object,
            _cacheSettings,
            _mockRedis.Object);

        // Act
        var result = await cacheService.GetAsync<TestDto>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithRedisAvailable_SetsInBothCaches()
    {
        // Arrange
        var key = "test-key";
        var testObject = new TestDto { Id = 1, Name = "Test" };
        var expiration = TimeSpan.FromMinutes(15);

        _mockRedisDatabase
            .Setup(x => x.StringSetAsync(key, It.IsAny<RedisValue>(), expiration, It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var cacheService = new CacheService(
            _memoryCache,
            _mockLogger.Object,
            _cacheSettings,
            _mockRedis.Object);

        // Act
        await cacheService.SetAsync(key, testObject, expiration);

        // Assert
        _mockRedisDatabase.Verify(
            x => x.StringSetAsync(
                key,
                It.Is<RedisValue>(v => v.ToString().Contains("\"Id\":1")),
                expiration,
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()),
            Times.Once);

        // Verify memory cache also has the value
        var memoryCachedValue = _memoryCache.Get<TestDto>(key);
        memoryCachedValue.Should().NotBeNull();
        memoryCachedValue!.Id.Should().Be(testObject.Id);
    }

    [Fact]
    public async Task SetAsync_WithRedisUnavailable_SetsInMemoryCacheOnly()
    {
        // Arrange
        var key = "test-key";
        var testObject = new TestDto { Id = 1, Name = "Test" };
        var expiration = TimeSpan.FromMinutes(15);

        var mockRedisDisconnected = new Mock<IConnectionMultiplexer>();
        mockRedisDisconnected.Setup(x => x.IsConnected).Returns(false);

        var cacheService = new CacheService(
            _memoryCache,
            _mockLogger.Object,
            _cacheSettings,
            mockRedisDisconnected.Object);

        // Act
        await cacheService.SetAsync(key, testObject, expiration);

        // Assert
        var memoryCachedValue = _memoryCache.Get<TestDto>(key);
        memoryCachedValue.Should().NotBeNull();
        memoryCachedValue!.Id.Should().Be(testObject.Id);
    }

    [Fact]
    public async Task RemoveAsync_WithRedisAvailable_RemovesFromBothCaches()
    {
        // Arrange
        var key = "test-key";
        var testObject = new TestDto { Id = 1, Name = "Test" };

        // Pre-populate both caches
        _memoryCache.Set(key, testObject);

        _mockRedisDatabase
            .Setup(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var cacheService = new CacheService(
            _memoryCache,
            _mockLogger.Object,
            _cacheSettings,
            _mockRedis.Object);

        // Act
        await cacheService.RemoveAsync(key);

        // Assert
        _mockRedisDatabase.Verify(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once);
        _memoryCache.TryGetValue(key, out var _).Should().BeFalse();
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
