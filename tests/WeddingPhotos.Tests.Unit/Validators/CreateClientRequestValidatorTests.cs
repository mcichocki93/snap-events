using FluentAssertions;
using WeddingPhotos.Api.Validators;
using WeddingPhotos.Domain.Constants;
using WeddingPhotos.Domain.DTOs;

namespace WeddingPhotos.Tests.Unit.Validators;

public class CreateClientRequestValidatorTests
{
    private readonly CreateClientRequestValidator _validator;

    public CreateClientRequestValidatorTests()
    {
        _validator = new CreateClientRequestValidator();
    }

    [Fact]
    public async Task Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan.kowalski@example.com",
            Phone = "+48123456789",
            Guid = Guid.NewGuid().ToString(),
            EventName = "Wesele Jana i Anny",
            EventType = ApplicationConstants.EventTypes.Wedding,
            EventDate = DateTime.UtcNow.AddDays(30),
            DateTo = DateTime.UtcNow.AddDays(60),
            MaxFiles = 100,
            MaxFileSize = 10485760,
            GoogleStorageUrl = "https://drive.google.com/drive/folders/abc123"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithEmptyFirstName_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.FirstName = "";

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientRequest.FirstName));
    }

    [Fact]
    public async Task Validate_WithTooLongFirstName_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.FirstName = new string('A', 101);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientRequest.FirstName));
    }

    [Fact]
    public async Task Validate_WithInvalidEmail_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = "invalid-email";

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientRequest.Email));
    }

    [Fact]
    public async Task Validate_WithInvalidPhoneFormat_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Phone = "123"; // Too short

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientRequest.Phone));
    }

    [Fact]
    public async Task Validate_WithValidPhoneFormats_ShouldNotHaveValidationError()
    {
        // Arrange
        var validPhones = new[] { "+48123456789", "+1234567890", "123456789" };

        foreach (var phone in validPhones)
        {
            var request = CreateValidRequest();
            request.Phone = phone;

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.Errors.Where(e => e.PropertyName == nameof(CreateClientRequest.Phone))
                .Should().BeEmpty($"phone {phone} should be valid");
        }
    }

    [Fact]
    public async Task Validate_WithInvalidGuid_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Guid = "not-a-valid-guid";

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientRequest.Guid));
    }

    [Fact]
    public async Task Validate_WithInvalidEventType_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.EventType = "InvalidEventType";

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientRequest.EventType));
    }

    [Fact]
    public async Task Validate_WithEventDateAfterDateTo_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.EventDate = DateTime.UtcNow.AddDays(60);
        request.DateTo = DateTime.UtcNow.AddDays(30);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientRequest.EventDate));
    }

    [Fact]
    public async Task Validate_WithDateToInPast_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DateTo = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientRequest.DateTo));
    }

    [Fact]
    public async Task Validate_WithZeroMaxFiles_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MaxFiles = 0;

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientRequest.MaxFiles));
    }

    [Fact]
    public async Task Validate_WithTooLargeMaxFileSize_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MaxFileSize = ApplicationConstants.FileUpload.MaxFileSizeBytes + 1;

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientRequest.MaxFileSize));
    }

    [Fact]
    public async Task Validate_WithInvalidHexColor_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.BackgroundColor = "not-a-color";

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientRequest.BackgroundColor));
    }

    [Fact]
    public async Task Validate_WithValidHexColors_ShouldNotHaveValidationError()
    {
        // Arrange
        var validColors = new[] { "#fff", "#FFF", "#ffffff", "#FFFFFF", "#667eea" };

        foreach (var color in validColors)
        {
            var request = CreateValidRequest();
            request.BackgroundColor = color;

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.Errors.Where(e => e.PropertyName == nameof(CreateClientRequest.BackgroundColor))
                .Should().BeEmpty($"color {color} should be valid");
        }
    }

    [Fact]
    public async Task Validate_WithInvalidUrl_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.GoogleStorageUrl = "not-a-valid-url";

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientRequest.GoogleStorageUrl));
    }

    private CreateClientRequest CreateValidRequest()
    {
        return new CreateClientRequest
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan.kowalski@example.com",
            Phone = "+48123456789",
            Guid = Guid.NewGuid().ToString(),
            EventName = "Wesele Jana i Anny",
            EventType = ApplicationConstants.EventTypes.Wedding,
            EventDate = DateTime.UtcNow.AddDays(30),
            DateTo = DateTime.UtcNow.AddDays(60),
            MaxFiles = 100,
            MaxFileSize = 10485760,
            GoogleStorageUrl = "https://drive.google.com/drive/folders/abc123"
        };
    }
}
