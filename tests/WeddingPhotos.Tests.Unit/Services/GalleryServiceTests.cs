using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WeddingPhotos.Domain.Constants;
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

        // TotalCount is the whole folder, not the page: 42 photos exist, this
        // page holds 1 of them.
        _mockStorageService
            .Setup(x => x.GetPhotoCountAsync(client.GoogleStorageUrl))
            .ReturnsAsync(42);

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
        response.TotalCount.Should().Be(42);
        response.HasMore.Should().BeTrue();
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

        // Uploads go through an atomic slot reservation; without this the quota
        // check reads it as exhausted and rejects the upload.
        _mockClientRepository
            .Setup(x => x.TryReserveUploadSlotAsync(guid))
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

    [Fact]
    public async Task GetPhotoStreamAsync_WhenPhotoBelongsToGallery_ServesIt()
    {
        // Arrange
        var guid = "gallery-a";
        var photoId = "photo-in-a";
        var folderUrl = "https://drive.google.com/drive/folders/folderA";

        var client = new Client { Guid = guid, IsActive = true, GoogleStorageUrl = folderUrl };
        _mockClientRepository.Setup(x => x.GetByGuidAsync(guid)).ReturnsAsync(client);

        _mockCacheService
            .Setup(x => x.GetAsync<HashSet<string>>($"photoIds:{guid}"))
            .ReturnsAsync(new HashSet<string> { photoId });

        _mockStorageService
            .Setup(x => x.GetPhotoStreamAsync(photoId, null, null))
            .ReturnsAsync(new PhotoStreamResult
            {
                Stream = new MemoryStream([1, 2, 3]),
                MimeType = "image/jpeg",
                FileName = "photo.jpg"
            });

        // Act
        var (success, photo, _) = await _galleryService.GetPhotoStreamAsync(guid, photoId);

        // Assert
        success.Should().BeTrue();
        photo.Should().NotBeNull();
    }

    /// <summary>
    /// Photo IDs are not secret - they sit in the JSON of every gallery page -
    /// so holding one must not open a photo from somebody else's event.
    /// </summary>
    [Fact]
    public async Task GetPhotoStreamAsync_WhenPhotoBelongsToAnotherGallery_Refuses()
    {
        // Arrange
        var guid = "gallery-b";
        var foreignPhotoId = "photo-in-a";
        var folderUrl = "https://drive.google.com/drive/folders/folderB";

        var client = new Client { Guid = guid, IsActive = true, GoogleStorageUrl = folderUrl };
        _mockClientRepository.Setup(x => x.GetByGuidAsync(guid)).ReturnsAsync(client);

        _mockCacheService
            .Setup(x => x.GetAsync<HashSet<string>>($"photoIds:{guid}"))
            .ReturnsAsync((HashSet<string>?)null);

        // Gallery B holds its own photos, and the requested one is not among them.
        _mockStorageService
            .Setup(x => x.GetPhotoIdsAsync(folderUrl))
            .ReturnsAsync(new HashSet<string> { "photo-in-b" });

        // Act
        var (success, photo, errorMessage) = await _galleryService.GetPhotoStreamAsync(guid, foreignPhotoId);

        // Assert
        success.Should().BeFalse();
        photo.Should().BeNull();
        errorMessage.Should().NotBeNull();

        _mockStorageService.Verify(
            x => x.GetPhotoStreamAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    /// A photo uploaded seconds ago is legitimately missing from a list fetched
    /// minutes earlier. Treating that as a refusal would show the guest a broken
    /// tile for the photo they just sent.
    /// </summary>
    [Fact]
    public async Task GetPhotoStreamAsync_WhenPhotoIsNewerThanCachedList_RechecksDrive()
    {
        // Arrange
        var guid = "gallery-a";
        var justUploaded = "brand-new-photo";
        var folderUrl = "https://drive.google.com/drive/folders/folderA";

        var client = new Client { Guid = guid, IsActive = true, GoogleStorageUrl = folderUrl };
        _mockClientRepository.Setup(x => x.GetByGuidAsync(guid)).ReturnsAsync(client);

        // Stale cache: predates the upload.
        _mockCacheService
            .Setup(x => x.GetAsync<HashSet<string>>($"photoIds:{guid}"))
            .ReturnsAsync(new HashSet<string> { "older-photo" });

        // Drive knows better.
        _mockStorageService
            .Setup(x => x.GetPhotoIdsAsync(folderUrl))
            .ReturnsAsync(new HashSet<string> { "older-photo", justUploaded });

        _mockStorageService
            .Setup(x => x.GetPhotoStreamAsync(justUploaded, It.IsAny<int?>(), It.IsAny<string>()))
            .ReturnsAsync(new PhotoStreamResult
            {
                Stream = new MemoryStream([1, 2, 3]),
                MimeType = "image/jpeg",
                FileName = "new.jpg"
            });

        // Act
        var (success, _, _) = await _galleryService.GetPhotoStreamAsync(guid, justUploaded);

        // Assert
        success.Should().BeTrue();
        _mockStorageService.Verify(x => x.GetPhotoIdsAsync(folderUrl), Times.Once);
    }

    /// <summary>
    /// The counter drifts upward - abandoned sessions keep their slot, and
    /// photos deleted straight from Drive are never noticed - so a gallery can
    /// look full while it is not. The guest must not pay for that.
    /// </summary>
    [Fact]
    public async Task UploadPhotoAsync_WhenCounterOvershotReality_ReconcilesAndAccepts()
    {
        // Arrange
        var guid = "test-guid";
        var folderUrl = "https://drive.google.com/drive/folders/folder123";

        // Counter says the 150-photo package is used up; Drive holds 140.
        var client = new Client
        {
            Guid = guid,
            IsActive = true,
            DateTo = DateTime.UtcNow.AddDays(30),
            MaxFiles = 150,
            UploadedFilesCount = 150,
            MaxFileSize = 10485760,
            GoogleStorageUrl = folderUrl
        };

        _mockClientRepository.Setup(x => x.GetByGuidAsync(guid)).ReturnsAsync(client);

        _mockStorageService
            .Setup(x => x.GetPhotoCountAsync(folderUrl))
            .ReturnsAsync(140);

        // Refuses while the counter is wrong, succeeds once it has been fixed.
        _mockClientRepository
            .SetupSequence(x => x.TryReserveUploadSlotAsync(guid))
            .ReturnsAsync((Client?)null)
            .ReturnsAsync(client);

        _mockClientRepository
            .Setup(x => x.ReconcileUploadedFilesCountAsync(guid, 150, 140))
            .ReturnsAsync(true);

        _mockStorageService
            .Setup(x => x.UploadPhotoAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("photo123");

        _mockCacheService
            .Setup(x => x.RemoveByPrefixAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var (success, _, errorMessage) = await _galleryService.UploadPhotoAsync(
            guid, new MemoryStream(new byte[] { 1, 2, 3 }), "test.jpg", "image/jpeg", 1024);

        // Assert
        success.Should().BeTrue();
        errorMessage.Should().BeNull();
        _mockClientRepository.Verify(
            x => x.ReconcileUploadedFilesCountAsync(guid, 150, 140), Times.Once);
    }

    [Fact]
    public async Task UploadPhotoAsync_WhenGalleryGenuinelyFull_StillRejects()
    {
        // Arrange
        var guid = "test-guid";
        var folderUrl = "https://drive.google.com/drive/folders/folder123";

        // Counter and Drive agree: the package really is used up.
        var client = new Client
        {
            Guid = guid,
            IsActive = true,
            DateTo = DateTime.UtcNow.AddDays(30),
            MaxFiles = 150,
            UploadedFilesCount = 150,
            MaxFileSize = 10485760,
            GoogleStorageUrl = folderUrl
        };

        _mockClientRepository.Setup(x => x.GetByGuidAsync(guid)).ReturnsAsync(client);

        _mockStorageService
            .Setup(x => x.GetPhotoCountAsync(folderUrl))
            .ReturnsAsync(150);

        _mockClientRepository
            .Setup(x => x.TryReserveUploadSlotAsync(guid))
            .ReturnsAsync((Client?)null);

        // Act
        var (success, _, errorMessage) = await _galleryService.UploadPhotoAsync(
            guid, new MemoryStream(new byte[] { 1, 2, 3 }), "test.jpg", "image/jpeg", 1024);

        // Assert
        success.Should().BeFalse();
        errorMessage.Should().NotBeNull();

        _mockClientRepository.Verify(
            x => x.ReconcileUploadedFilesCountAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _mockStorageService.Verify(
            x => x.UploadPhotoAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    /// Galleries without a limit never fail the reservation on quota, so a
    /// refusal there means something else - and must not cost a Drive listing.
    /// </summary>
    [Fact]
    public async Task UploadPhotoAsync_WithUnlimitedGallery_DoesNotCountDrive()
    {
        // Arrange
        var guid = "test-guid";
        var client = new Client
        {
            Guid = guid,
            IsActive = true,
            DateTo = DateTime.UtcNow.AddDays(30),
            MaxFiles = 0, // unlimited
            UploadedFilesCount = 500,
            MaxFileSize = 10485760,
            GoogleStorageUrl = "https://drive.google.com/drive/folders/folder123"
        };

        _mockClientRepository.Setup(x => x.GetByGuidAsync(guid)).ReturnsAsync(client);
        _mockClientRepository
            .Setup(x => x.TryReserveUploadSlotAsync(guid))
            .ReturnsAsync((Client?)null);

        // Act
        var (success, _, _) = await _galleryService.UploadPhotoAsync(
            guid, new MemoryStream(new byte[] { 1, 2, 3 }), "test.jpg", "image/jpeg", 1024);

        // Assert
        success.Should().BeFalse();
        _mockStorageService.Verify(x => x.GetPhotoCountAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// A gallery that does not take videos must refuse one before a Drive
    /// session is ever opened, and without spending a slot from the package.
    /// </summary>
    [Fact]
    public async Task CreateUploadSessionAsync_WithVideoWhenNotAllowed_IsRefused()
    {
        // Arrange
        var guid = "test-guid";
        var client = VideoTestClient(guid, allowVideos: false);

        _mockClientRepository.Setup(x => x.GetByGuidAsync(guid)).ReturnsAsync(client);

        // Act
        var (success, response, _) = await _galleryService.CreateUploadSessionAsync(
            guid, "wishes.mp4", "video/mp4", 20_000_000, null);

        // Assert
        success.Should().BeFalse();
        response.Message.Should().Be(ApplicationConstants.ErrorMessages.VideosNotAllowed);

        _mockClientRepository.Verify(x => x.TryReserveUploadSlotAsync(It.IsAny<string>()), Times.Never);
        _mockStorageService.Verify(
            x => x.CreateResumableUploadSessionAsync(
                It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    /// A video is judged against the video ceiling, not the gallery's photo size
    /// limit. Photos here are capped at 10MB; a 60-second film is far past that
    /// and must still go through.
    /// </summary>
    [Fact]
    public async Task CreateUploadSessionAsync_WithAllowedVideo_IgnoresPhotoSizeLimit()
    {
        // Arrange
        var guid = "test-guid";
        var client = VideoTestClient(guid, allowVideos: true);

        _mockClientRepository.Setup(x => x.GetByGuidAsync(guid)).ReturnsAsync(client);
        _mockClientRepository.Setup(x => x.TryReserveUploadSlotAsync(guid)).ReturnsAsync(client);
        _mockStorageService
            .Setup(x => x.CreateResumableUploadSessionAsync(
                It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync("https://upload.example.com/session");

        // Act - 200MB, twenty times the photo limit on this gallery
        var (success, response, _) = await _galleryService.CreateUploadSessionAsync(
            guid, "first-dance.mov", "video/quicktime", 209_715_200, null);

        // Assert
        success.Should().BeTrue();
        response.UploadUrl.Should().Be("https://upload.example.com/session");
    }

    /// <summary>
    /// The size ceiling is the server's stand-in for the sixty-second limit,
    /// since duration cannot be measured from bytes. It is what stops a browser
    /// that skipped the length check.
    /// </summary>
    [Fact]
    public async Task CreateUploadSessionAsync_WithVideoOverSizeCeiling_IsRefused()
    {
        // Arrange
        var guid = "test-guid";
        var client = VideoTestClient(guid, allowVideos: true);

        _mockClientRepository.Setup(x => x.GetByGuidAsync(guid)).ReturnsAsync(client);

        // Act
        var (success, response, _) = await _galleryService.CreateUploadSessionAsync(
            guid, "whole-reception.mp4", "video/mp4",
            ApplicationConstants.FileUpload.MaxVideoSizeBytes + 1, null);

        // Assert
        success.Should().BeFalse();
        response.Message.Should().Contain(
            ApplicationConstants.FileUpload.MaxVideoDurationSeconds.ToString());

        _mockClientRepository.Verify(x => x.TryReserveUploadSlotAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// A container we do not serve back is refused even when videos are on -
    /// better than accepting a file the gallery could never play.
    /// </summary>
    [Fact]
    public async Task CreateUploadSessionAsync_WithUnsupportedVideoFormat_IsRefused()
    {
        // Arrange
        var guid = "test-guid";
        var client = VideoTestClient(guid, allowVideos: true);

        _mockClientRepository.Setup(x => x.GetByGuidAsync(guid)).ReturnsAsync(client);

        // Act
        var (success, response, _) = await _galleryService.CreateUploadSessionAsync(
            guid, "clip.avi", "video/x-msvideo", 5_000_000, null);

        // Assert
        success.Should().BeFalse();
        response.Message.Should().Be(ApplicationConstants.ErrorMessages.InvalidFileType);
    }

    /// <summary>
    /// Turning videos on must not loosen anything for photos: the gallery's own
    /// MaxFileSize still applies to them.
    /// </summary>
    [Fact]
    public async Task CreateUploadSessionAsync_WithOversizedPhotoInVideoGallery_IsRefused()
    {
        // Arrange
        var guid = "test-guid";
        var client = VideoTestClient(guid, allowVideos: true);

        _mockClientRepository.Setup(x => x.GetByGuidAsync(guid)).ReturnsAsync(client);

        // Act - 50MB photo against this gallery's 10MB limit
        var (success, _, _) = await _galleryService.CreateUploadSessionAsync(
            guid, "huge.jpg", "image/jpeg", 52_428_800, null);

        // Assert
        success.Should().BeFalse();
        _mockClientRepository.Verify(x => x.TryReserveUploadSlotAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// The buffered path answers to the same rules as the resumable one - both
    /// go through the same validation, so a video cannot slip in that way.
    /// </summary>
    [Fact]
    public async Task UploadPhotoAsync_WithVideoWhenNotAllowed_IsRefused()
    {
        // Arrange
        var guid = "test-guid";
        var client = VideoTestClient(guid, allowVideos: false);

        _mockClientRepository.Setup(x => x.GetByGuidAsync(guid)).ReturnsAsync(client);

        // Act
        var (success, response, _) = await _galleryService.UploadPhotoAsync(
            guid, new MemoryStream(new byte[] { 1, 2, 3 }), "wishes.mp4", "video/mp4", 20_000_000);

        // Assert
        success.Should().BeFalse();
        response.Message.Should().Be(ApplicationConstants.ErrorMessages.VideosNotAllowed);

        _mockStorageService.Verify(
            x => x.UploadPhotoAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    private static Client VideoTestClient(string guid, bool allowVideos) => new()
    {
        Guid = guid,
        IsActive = true,
        DateTo = DateTime.UtcNow.AddDays(30),
        MaxFiles = 0,
        MaxFileSize = 10485760, // 10MB, photos only
        AllowVideos = allowVideos,
        GoogleStorageUrl = "https://drive.google.com/drive/folders/folder123"
    };
}
