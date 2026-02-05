using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WeddingPhotos.Domain.DTOs;
using WeddingPhotos.Domain.Interfaces;
using WeddingPhotos.Domain.Models;
using WeddingPhotos.Infrastructure.Services;

namespace WeddingPhotos.Tests.Unit.Services;

public class GalleryServiceTests
{
    private readonly Mock<IGoogleStorageService> _mockStorageService;
    private readonly Mock<IClientRepository> _mockClientRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<GalleryService>> _mockLogger;
    private readonly GalleryService _galleryService;

    public GalleryServiceTests()
    {
        _mockStorageService = new Mock<IGoogleStorageService>();
        _mockClientRepository = new Mock<IClientRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<GalleryService>>();

        _galleryService = new GalleryService(
            _mockClientRepository.Object,
            _mockStorageService.Object,
            _mockCacheService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetGalleryAsync_WithCachedData_ReturnsCachedResponse()
    {
        // Arrange
        var guid = "test-guid";
        var page = 1;
        var pageSize = 20;
        var cacheKey = $"gallery:{guid}:page:{page}:size:{pageSize}";

        var client = new Client
        {
            Guid = guid,
            IsActive = true,
            DateTo = DateTime.UtcNow.AddDays(30),
            GoogleStorageUrl = "https://storage.googleapis.com/bucket/folder"
        };

        var cachedResponse = new GalleryResponse
        {
            Photos = new List<PhotoInfo>
            {
                new PhotoInfo
                {
                    Id = "photo1",
                    Name = "test.jpg",
                    ThumbnailUrl = "https://example.com/thumb.jpg",
                    FullUrl = "https://example.com/full.jpg",
                    DateAdded = DateTime.UtcNow,
                    Size = 1024,
                    MimeType = "image/jpeg"
                }
            },
            TotalCount = 1,
            HasMore = false,
            NextPageToken = null
        };

        _mockClientRepository
            .Setup(x => x.GetByGuidAsync(guid))
            .ReturnsAsync(client);

        _mockCacheService
            .Setup(x => x.GetAsync<GalleryResponse>(cacheKey))
            .ReturnsAsync(cachedResponse);

        // Act
        var (success, response, errorMessage) = await _galleryService.GetGalleryAsync(guid, page, pageSize);

        // Assert
        success.Should().BeTrue();
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(cachedResponse);
        errorMessage.Should().BeNull();

        _mockClientRepository.Verify(x => x.GetByGuidAsync(guid), Times.Once);
        _mockCacheService.Verify(x => x.GetAsync<GalleryResponse>(cacheKey), Times.Once);
        _mockStorageService.Verify(x => x.GetPhotosFromFolderAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetGalleryAsync_WithoutCache_FetchesFromStorageAndCaches()
    {
        // Arrange
        var guid = "test-guid";
        var page = 1;
        var pageSize = 20;
        var cacheKey = $"gallery:{guid}:page:{page}:size:{pageSize}";

        var client = new Client
        {
            Guid = guid,
            IsActive = true,
            GoogleStorageUrl = "https://storage.googleapis.com/bucket/folder"
        };

        var photos = new List<PhotoInfo>
        {
            new PhotoInfo
            {
                Id = "photo1",
                Name = "test.jpg",
                ThumbnailUrl = "https://example.com/thumb.jpg",
                FullUrl = "https://example.com/full.jpg",
                DateAdded = DateTime.UtcNow,
                Size = 1024,
                MimeType = "image/jpeg"
            }
        };

        _mockCacheService
            .Setup(x => x.GetAsync<GalleryResponse>(cacheKey))
            .ReturnsAsync((GalleryResponse?)null);

        _mockClientRepository
            .Setup(x => x.GetByGuidAsync(guid))
            .ReturnsAsync(client);

        _mockStorageService
            .Setup(x => x.GetPhotosFromFolderAsync(client.GoogleStorageUrl, page, pageSize))
            .ReturnsAsync(photos);

        _mockCacheService
            .Setup(x => x.SetAsync(cacheKey, It.IsAny<GalleryResponse>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var (success, response, errorMessage) = await _galleryService.GetGalleryAsync(guid, page, pageSize);

        // Assert
        success.Should().BeTrue();
        response.Should().NotBeNull();
        response!.Photos.Should().HaveCount(1);
        response.Photos[0].Id.Should().Be("photo1");
        response.TotalCount.Should().Be(1);
        errorMessage.Should().BeNull();

        _mockCacheService.Verify(x => x.GetAsync<GalleryResponse>(cacheKey), Times.Once);
        _mockClientRepository.Verify(x => x.GetByGuidAsync(guid), Times.Once);
        _mockStorageService.Verify(x => x.GetPhotosFromFolderAsync(client.GoogleStorageUrl, page, pageSize), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(cacheKey, It.IsAny<GalleryResponse>(), TimeSpan.FromMinutes(15)), Times.Once);
    }

    [Fact]
    public async Task GetGalleryAsync_WithInvalidPage_ReturnsError()
    {
        // Arrange
        var guid = "test-guid";
        var invalidPage = 0;
        var pageSize = 20;

        // Act
        var (success, response, errorMessage) = await _galleryService.GetGalleryAsync(guid, invalidPage, pageSize);

        // Assert
        success.Should().BeFalse();
        response.Should().BeNull();
        errorMessage.Should().NotBeNullOrEmpty();

        _mockCacheService.Verify(x => x.GetAsync<GalleryResponse>(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetGalleryAsync_WithInvalidPageSize_ReturnsError()
    {
        // Arrange
        var guid = "test-guid";
        var page = 1;
        var invalidPageSize = 0;

        // Act
        var (success, response, errorMessage) = await _galleryService.GetGalleryAsync(guid, page, invalidPageSize);

        // Assert
        success.Should().BeFalse();
        response.Should().BeNull();
        errorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetGalleryAsync_WithNonExistentClient_ReturnsNotFound()
    {
        // Arrange
        var guid = "non-existent-guid";
        var page = 1;
        var pageSize = 20;
        var cacheKey = $"gallery:{guid}:page:{page}:size:{pageSize}";

        _mockCacheService
            .Setup(x => x.GetAsync<GalleryResponse>(cacheKey))
            .ReturnsAsync((GalleryResponse?)null);

        _mockClientRepository
            .Setup(x => x.GetByGuidAsync(guid))
            .ReturnsAsync((Client?)null);

        // Act
        var (success, response, errorMessage) = await _galleryService.GetGalleryAsync(guid, page, pageSize);

        // Assert
        success.Should().BeFalse();
        response.Should().BeNull();
        errorMessage.Should().Contain("nie została znaleziona");

        _mockStorageService.Verify(x => x.GetPhotosFromFolderAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetGalleryAsync_WithInactiveClient_ReturnsError()
    {
        // Arrange
        var guid = "test-guid";
        var page = 1;
        var pageSize = 20;
        var cacheKey = $"gallery:{guid}:page:{page}:size:{pageSize}";

        var inactiveClient = new Client
        {
            Guid = guid,
            IsActive = false
        };

        _mockCacheService
            .Setup(x => x.GetAsync<GalleryResponse>(cacheKey))
            .ReturnsAsync((GalleryResponse?)null);

        _mockClientRepository
            .Setup(x => x.GetByGuidAsync(guid))
            .ReturnsAsync(inactiveClient);

        // Act
        var (success, response, errorMessage) = await _galleryService.GetGalleryAsync(guid, page, pageSize);

        // Assert
        success.Should().BeFalse();
        response.Should().BeNull();
        errorMessage.Should().Contain("dezaktywowana");
    }

    [Fact]
    public async Task UploadPhotoAsync_Success_InvalidatesCache()
    {
        // Arrange
        var guid = "test-guid";
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var fileSize = 1024L;
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        var folderId = "folder123";

        var client = new Client
        {
            Guid = guid,
            IsActive = true,
            DateTo = DateTime.UtcNow.AddDays(30),
            MaxFiles = 100,
            MaxFileSize = 10485760,
            GoogleStorageUrl = $"https://drive.google.com/drive/folders/{folderId}"
        };

        _mockClientRepository
            .Setup(x => x.GetByGuidAsync(guid))
            .ReturnsAsync(client);

        _mockStorageService
            .Setup(x => x.UploadPhotoAsync(fileStream, fileName, folderId))
            .ReturnsAsync("photo123");

        _mockCacheService
            .Setup(x => x.RemoveByPrefixAsync($"gallery:{guid}:"))
            .Returns(Task.CompletedTask);

        // Act
        var (success, response, errorMessage) = await _galleryService.UploadPhotoAsync(guid, fileStream, fileName, contentType, fileSize);

        // Assert
        success.Should().BeTrue();
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.PhotoId.Should().Be("photo123");
        errorMessage.Should().BeNull();

        _mockStorageService.Verify(x => x.UploadPhotoAsync(fileStream, fileName, folderId), Times.Once);
        _mockCacheService.Verify(x => x.RemoveByPrefixAsync($"gallery:{guid}:"), Times.Once);
    }

    [Fact]
    public async Task UploadPhotoAsync_WithExpiredClient_ReturnsError()
    {
        // Arrange
        var guid = "test-guid";
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var fileSize = 1024L;
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4 });

        var client = new Client
        {
            Guid = guid,
            IsActive = true,
            DateTo = DateTime.UtcNow.AddDays(-1), // Expired
            MaxFiles = 100,
            MaxFileSize = 10485760
        };

        _mockClientRepository
            .Setup(x => x.GetByGuidAsync(guid))
            .ReturnsAsync(client);

        // Act
        var (success, response, errorMessage) = await _galleryService.UploadPhotoAsync(guid, fileStream, fileName, contentType, fileSize);

        // Assert
        success.Should().BeFalse();
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        errorMessage.Should().NotBeNullOrEmpty();

        _mockStorageService.Verify(x => x.UploadPhotoAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
