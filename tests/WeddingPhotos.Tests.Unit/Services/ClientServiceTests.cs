using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WeddingPhotos.Domain.Constants;
using WeddingPhotos.Domain.DTOs;
using WeddingPhotos.Domain.Interfaces;
using WeddingPhotos.Domain.Models;
using WeddingPhotos.Infrastructure.Mapping;
using WeddingPhotos.Infrastructure.Services;

namespace WeddingPhotos.Tests.Unit.Services;

public class ClientServiceTests
{
    private readonly Mock<IClientRepository> _mockClientRepository;
    private readonly Mock<ILogger<ClientService>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly ClientService _clientService;

    public ClientServiceTests()
    {
        _mockClientRepository = new Mock<IClientRepository>();
        _mockLogger = new Mock<ILogger<ClientService>>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = config.CreateMapper();

        _clientService = new ClientService(
            _mockClientRepository.Object,
            _mapper,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetClientByGuidAsync_WithValidActiveClient_ReturnsClientResponse()
    {
        // Arrange
        var guid = "test-guid";
        var client = new Client
        {
            Id = "client123",
            Guid = guid,
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            EventName = "Wesele Jana i Anny",
            EventType = ApplicationConstants.EventTypes.Wedding,
            EventDate = DateTime.UtcNow.AddDays(30),
            DateTo = DateTime.UtcNow.AddDays(60),
            IsActive = true,
            MaxFiles = 100,
            UploadedFilesCount = 25,
            MaxFileSize = 10485760,
            BackgroundColor = "#667eea",
            BackgroundColorSecondary = "#764ba2",
            FontColor = "#ffffff",
            FontType = "Roboto",
            AccentColor = "#3b82f6",
            GoogleStorageUrl = "https://storage.googleapis.com/bucket/folder"
        };

        _mockClientRepository
            .Setup(x => x.GetByGuidAsync(guid))
            .ReturnsAsync(client);

        // Act
        var (success, response, errorMessage) = await _clientService.GetClientByGuidAsync(guid);

        // Assert
        success.Should().BeTrue();
        response.Should().NotBeNull();
        errorMessage.Should().BeNull();

        response!.Guid.Should().Be(guid);
        response.EventName.Should().Be(client.EventName);
        response.EventType.Should().Be(client.EventType);
        response.EventTypeDisplayName.Should().NotBeNullOrEmpty();
        response.EventTypeEmoji.Should().NotBeNullOrEmpty();
        response.IsActive.Should().BeTrue();
        response.IsExpired.Should().BeFalse();
        response.MaxFiles.Should().Be(100);
        response.UploadedFilesCount.Should().Be(25);
        response.CanUploadMore.Should().BeTrue();

        // Personal data should be excluded
        response.FirstName.Should().BeNull();
        response.LastName.Should().BeNull();
        response.Email.Should().BeNull();
        response.Phone.Should().BeNull();

        _mockClientRepository.Verify(x => x.GetByGuidAsync(guid), Times.Once);
    }

    [Fact]
    public async Task GetClientByGuidAsync_WithNonExistentClient_ReturnsNotFound()
    {
        // Arrange
        var guid = "non-existent-guid";

        _mockClientRepository
            .Setup(x => x.GetByGuidAsync(guid))
            .ReturnsAsync((Client?)null);

        // Act
        var (success, response, errorMessage) = await _clientService.GetClientByGuidAsync(guid);

        // Assert
        success.Should().BeFalse();
        response.Should().BeNull();
        errorMessage.Should().Contain("not found");

        _mockClientRepository.Verify(x => x.GetByGuidAsync(guid), Times.Once);
    }

    [Fact]
    public async Task GetClientByGuidAsync_WithInactiveClient_ReturnsError()
    {
        // Arrange
        var guid = "test-guid";
        var inactiveClient = new Client
        {
            Guid = guid,
            IsActive = false,
            EventName = "Inactive Event",
            EventType = ApplicationConstants.EventTypes.Wedding
        };

        _mockClientRepository
            .Setup(x => x.GetByGuidAsync(guid))
            .ReturnsAsync(inactiveClient);

        // Act
        var (success, response, errorMessage) = await _clientService.GetClientByGuidAsync(guid);

        // Assert
        success.Should().BeFalse();
        response.Should().BeNull();
        errorMessage.Should().Contain("no longer active");
    }

    [Fact]
    public async Task GetClientByGuidAsync_WithExpiredClient_ReturnsError()
    {
        // Arrange
        var guid = "test-guid";
        var expiredClient = new Client
        {
            Guid = guid,
            IsActive = true,
            EventName = "Past Event",
            EventType = ApplicationConstants.EventTypes.Wedding,
            EventDate = DateTime.UtcNow.AddDays(-30),
            DateTo = DateTime.UtcNow.AddDays(-1), // Expired yesterday
            MaxFiles = 100,
            UploadedFilesCount = 50
        };

        _mockClientRepository
            .Setup(x => x.GetByGuidAsync(guid))
            .ReturnsAsync(expiredClient);

        // Act
        var (success, response, errorMessage) = await _clientService.GetClientByGuidAsync(guid);

        // Assert
        success.Should().BeFalse();
        response.Should().BeNull();
        errorMessage.Should().Contain("expired");
    }
}
