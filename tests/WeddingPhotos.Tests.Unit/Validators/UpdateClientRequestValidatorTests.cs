using FluentAssertions;
using WeddingPhotos.Api.Validators;
using WeddingPhotos.Domain.Constants;
using WeddingPhotos.Domain.DTOs;

namespace WeddingPhotos.Tests.Unit.Validators;

public class UpdateClientRequestValidatorTests
{
    private readonly UpdateClientRequestValidator _validator;

    public UpdateClientRequestValidatorTests()
    {
        _validator = new UpdateClientRequestValidator();
    }

    [Fact]
    public async Task Validate_WithEmptyRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange - all fields optional, empty request is valid
        var request = new UpdateClientRequest();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithValidPartialUpdate_ShouldNotHaveValidationErrors()
    {
        // Arrange - update only some fields
        var request = new UpdateClientRequest
        {
            FirstName = "Jan",
            EventName = "Updated Event Name",
            MaxFiles = 200
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithTooLongFirstName_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            FirstName = new string('A', 101)
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateClientRequest.FirstName));
    }

    [Fact]
    public async Task Validate_WithTooLongLastName_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            LastName = new string('B', 101)
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateClientRequest.LastName));
    }

    [Fact]
    public async Task Validate_WithInvalidEmail_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            Email = "not-a-valid-email"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateClientRequest.Email));
    }

    [Fact]
    public async Task Validate_WithValidEmail_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            Email = "jan.kowalski@example.com"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithInvalidPhoneFormat_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            Phone = "123" // Too short
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateClientRequest.Phone));
    }

    [Fact]
    public async Task Validate_WithValidPhoneFormats_ShouldNotHaveValidationError()
    {
        // Arrange
        var validPhones = new[] { "+48123456789", "+1234567890", "123456789" };

        foreach (var phone in validPhones)
        {
            var request = new UpdateClientRequest
            {
                Phone = phone
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.Errors.Where(e => e.PropertyName == nameof(UpdateClientRequest.Phone))
                .Should().BeEmpty($"phone {phone} should be valid");
        }
    }

    [Fact]
    public async Task Validate_WithTooLongEventName_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            EventName = new string('E', 201)
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateClientRequest.EventName));
    }

    [Fact]
    public async Task Validate_WithInvalidEventType_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            EventType = "InvalidEventType"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateClientRequest.EventType));
    }

    [Fact]
    public async Task Validate_WithValidEventType_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            EventType = ApplicationConstants.EventTypes.Wedding
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithDateToInPast_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            DateTo = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateClientRequest.DateTo));
    }

    [Fact]
    public async Task Validate_WithDateToInFuture_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            DateTo = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithZeroMaxFiles_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            MaxFiles = 0
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateClientRequest.MaxFiles));
    }

    [Fact]
    public async Task Validate_WithTooManyMaxFiles_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            MaxFiles = 10001
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateClientRequest.MaxFiles));
    }

    [Fact]
    public async Task Validate_WithValidMaxFiles_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            MaxFiles = 500
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithZeroMaxFileSize_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            MaxFileSize = 0
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateClientRequest.MaxFileSize));
    }

    [Fact]
    public async Task Validate_WithTooLargeMaxFileSize_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            MaxFileSize = ApplicationConstants.FileUpload.MaxFileSizeBytes + 1
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateClientRequest.MaxFileSize));
    }

    [Fact]
    public async Task Validate_WithValidMaxFileSize_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            MaxFileSize = 10485760 // 10MB
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithInvalidHexColor_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            BackgroundColor = "not-a-color"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateClientRequest.BackgroundColor));
    }

    [Fact]
    public async Task Validate_WithValidHexColors_ShouldNotHaveValidationError()
    {
        // Arrange
        var validColors = new[] { "#fff", "#FFF", "#ffffff", "#FFFFFF", "#667eea" };

        foreach (var color in validColors)
        {
            var request = new UpdateClientRequest
            {
                BackgroundColor = color,
                BackgroundColorSecondary = color,
                FontColor = color,
                AccentColor = color
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.Errors.Should().BeEmpty($"color {color} should be valid for all color fields");
        }
    }

    [Fact]
    public async Task Validate_WithInvalidUrl_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            GoogleStorageUrl = "not-a-valid-url"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateClientRequest.GoogleStorageUrl));
    }

    [Fact]
    public async Task Validate_WithValidUrl_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            GoogleStorageUrl = "https://drive.google.com/drive/folders/abc123"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithMultipleValidFields_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan.kowalski@example.com",
            Phone = "+48123456789",
            EventName = "Updated Wedding",
            EventType = ApplicationConstants.EventTypes.Wedding,
            DateTo = DateTime.UtcNow.AddDays(60),
            MaxFiles = 200,
            MaxFileSize = 20971520, // 20MB
            BackgroundColor = "#667eea",
            BackgroundColorSecondary = "#764ba2",
            FontColor = "#ffffff",
            AccentColor = "#3b82f6",
            GoogleStorageUrl = "https://storage.googleapis.com/bucket/folder"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithMultipleInvalidFields_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            FirstName = new string('A', 101), // Too long
            Email = "invalid-email", // Invalid format
            Phone = "123", // Too short
            EventType = "InvalidType", // Invalid event type
            DateTo = DateTime.UtcNow.AddDays(-1), // In the past
            MaxFiles = 0, // Zero
            BackgroundColor = "not-a-color", // Invalid hex
            GoogleStorageUrl = "not-a-url" // Invalid URL
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(8);
    }
}
