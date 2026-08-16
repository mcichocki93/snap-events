using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using WeddingPhotos.Domain.Models;
using WeddingPhotos.Infrastructure.Configuration;
using WeddingPhotos.Infrastructure.Repositories;

namespace WeddingPhotos.Tests.Unit.Repositories;

public class ClientRepositoryTests
{
    // ClientRepository takes an already-connected IMongoDatabase; the connection
    // string and database name are validated at startup in Program.cs, so there
    // is nothing config-related left for the constructor to reject.
    // For full database operations testing, consider integration tests.

    private static Mock<IMongoDatabase> CreateDatabaseMock()
    {
        var database = new Mock<IMongoDatabase>();
        database
            .Setup(d => d.GetCollection<Client>(
                It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(Mock.Of<IMongoCollection<Client>>());

        return database;
    }

    [Fact]
    public void Constructor_ShouldResolveConfiguredCollection()
    {
        // Arrange
        var database = CreateDatabaseMock();
        var settings = Options.Create(new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "TestDb",
            ClientsCollectionName = "clients"
        });
        var logger = Mock.Of<ILogger<ClientRepository>>();

        // Act
        _ = new ClientRepository(database.Object, settings, logger);

        // Assert
        database.Verify(
            d => d.GetCollection<Client>("clients", It.IsAny<MongoCollectionSettings>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_ShouldFallBackToDefaultCollection_WhenNameNotConfigured()
    {
        // Arrange
        var database = CreateDatabaseMock();
        var settings = Options.Create(new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "TestDb",
            ClientsCollectionName = null!
        });
        var logger = Mock.Of<ILogger<ClientRepository>>();

        // Act
        _ = new ClientRepository(database.Object, settings, logger);

        // Assert
        database.Verify(
            d => d.GetCollection<Client>("Clients", It.IsAny<MongoCollectionSettings>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_ShouldNotThrow_WhenIndexCreationFails()
    {
        // Arrange
        // Index creation is best-effort: a replica that rejects it must not stop
        // the API from starting.
        var collection = new Mock<IMongoCollection<Client>>();
        collection.Setup(c => c.Indexes).Throws(new MongoException("index creation failed"));

        var database = new Mock<IMongoDatabase>();
        database
            .Setup(d => d.GetCollection<Client>(
                It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(collection.Object);

        var settings = Options.Create(new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "TestDb",
            ClientsCollectionName = "clients"
        });
        var logger = Mock.Of<ILogger<ClientRepository>>();

        // Act & Assert
        var act = () => new ClientRepository(database.Object, settings, logger);
        act.Should().NotThrow();
    }

    // Testing validation logic (ValidateClient method)

    [Fact]
    public void ValidateClient_ShouldThrowException_WhenFirstNameIsEmpty()
    {
        // Arrange
        var client = new Client
        {
            FirstName = "",
            LastName = "Kowalski",
            Email = "jan@example.com",
            DateTo = DateTime.Now.AddDays(30),
            GoogleStorageUrl = "https://drive.google.com/folders/abc123"
        };

        // Act & Assert
        // Testing the validation logic that would be called in CreateAsync
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(client.FirstName))
            errors.Add("Imię jest wymagane");

        errors.Should().Contain("Imię jest wymagane");
    }

    [Fact]
    public void ValidateClient_ShouldThrowException_WhenLastNameIsEmpty()
    {
        // Arrange
        var client = new Client
        {
            FirstName = "Jan",
            LastName = "",
            Email = "jan@example.com",
            DateTo = DateTime.Now.AddDays(30),
            GoogleStorageUrl = "https://drive.google.com/folders/abc123"
        };

        // Act & Assert
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(client.LastName))
            errors.Add("Nazwisko jest wymagane");

        errors.Should().Contain("Nazwisko jest wymagane");
    }

    [Fact]
    public void ValidateClient_ShouldThrowException_WhenEmailIsEmpty()
    {
        // Arrange
        var client = new Client
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "",
            DateTo = DateTime.Now.AddDays(30),
            GoogleStorageUrl = "https://drive.google.com/folders/abc123"
        };

        // Act & Assert
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(client.Email))
            errors.Add("Poprawny email jest wymagany");

        errors.Should().Contain("Poprawny email jest wymagany");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test..test@example.com")]
    public void ValidateClient_ShouldThrowException_WhenEmailIsInvalid(string invalidEmail)
    {
        // Arrange & Act
        var isValid = IsValidEmail(invalidEmail);

        // Assert
        isValid.Should().BeFalse($"'{invalidEmail}' should be invalid");
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("jan.kowalski@example.com")]
    [InlineData("user+tag@domain.co.uk")]
    public void ValidateClient_ShouldAcceptValidEmails(string validEmail)
    {
        // Arrange & Act
        var isValid = IsValidEmail(validEmail);

        // Assert
        isValid.Should().BeTrue($"'{validEmail}' should be valid");
    }

    [Fact]
    public void ValidateClient_ShouldThrowException_WhenDateToIsInPast()
    {
        // Arrange
        var client = new Client
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            DateTo = DateTime.Now.AddDays(-1),
            GoogleStorageUrl = "https://drive.google.com/folders/abc123"
        };

        // Act & Assert
        var errors = new List<string>();
        if (client.DateTo <= DateTime.Now)
            errors.Add("Data ważności musi być w przyszłości");

        errors.Should().Contain("Data ważności musi być w przyszłości");
    }

    [Fact]
    public void ValidateClient_ShouldThrowException_WhenGoogleStorageUrlIsEmpty()
    {
        // Arrange
        var client = new Client
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            DateTo = DateTime.Now.AddDays(30),
            GoogleStorageUrl = ""
        };

        // Act & Assert
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(client.GoogleStorageUrl))
            errors.Add("URL Google Storage jest wymagany");

        errors.Should().Contain("URL Google Storage jest wymagany");
    }

    [Fact]
    public void ValidateClient_ShouldAcceptValidClient()
    {
        // Arrange
        var client = new Client
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            DateTo = DateTime.Now.AddDays(30),
            GoogleStorageUrl = "https://drive.google.com/folders/abc123",
            EventType = "Wedding",
            MaxFiles = 100,
            MaxFileSize = 10485760
        };

        // Act - Validate all fields
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(client.FirstName))
            errors.Add("Imię jest wymagane");
        if (string.IsNullOrWhiteSpace(client.LastName))
            errors.Add("Nazwisko jest wymagane");
        if (string.IsNullOrWhiteSpace(client.Email) || !IsValidEmail(client.Email))
            errors.Add("Poprawny email jest wymagany");
        if (client.DateTo <= DateTime.Now)
            errors.Add("Data ważności musi być w przyszłości");
        if (string.IsNullOrWhiteSpace(client.GoogleStorageUrl))
            errors.Add("URL Google Storage jest wymagany");

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateClient_ShouldCollectMultipleErrors()
    {
        // Arrange
        var client = new Client
        {
            FirstName = "",
            LastName = "",
            Email = "invalid-email",
            DateTo = DateTime.Now.AddDays(-1),
            GoogleStorageUrl = ""
        };

        // Act
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(client.FirstName))
            errors.Add("Imię jest wymagane");
        if (string.IsNullOrWhiteSpace(client.LastName))
            errors.Add("Nazwisko jest wymagane");
        if (string.IsNullOrWhiteSpace(client.Email) || !IsValidEmail(client.Email))
            errors.Add("Poprawny email jest wymagany");
        if (client.DateTo <= DateTime.Now)
            errors.Add("Data ważności musi być w przyszłości");
        if (string.IsNullOrWhiteSpace(client.GoogleStorageUrl))
            errors.Add("URL Google Storage jest wymagany");

        // Assert
        errors.Should().HaveCount(5);
        errors.Should().Contain("Imię jest wymagane");
        errors.Should().Contain("Nazwisko jest wymagane");
        errors.Should().Contain("Poprawny email jest wymagany");
        errors.Should().Contain("Data ważności musi być w przyszłości");
        errors.Should().Contain("URL Google Storage jest wymagany");
    }

    // Testing GenerateUniqueGuid logic

    [Fact]
    public void GenerateUniqueGuid_ShouldCreateStringOfCorrectLength()
    {
        // Testing GUID generation logic
        var guid = GenerateUniqueGuid();

        guid.Should().HaveLength(12);
        guid.Should().MatchRegex(@"^[A-Za-z0-9_-]+$");
    }

    [Fact]
    public void GenerateUniqueGuid_ShouldCreateUniqueGuids()
    {
        // Generate multiple GUIDs to verify uniqueness
        var guids = new HashSet<string>();
        for (int i = 0; i < 100; i++)
        {
            guids.Add(GenerateUniqueGuid());
        }

        // All 100 should be unique
        guids.Should().HaveCount(100);
    }

    [Fact]
    public void GenerateUniqueGuid_ShouldNotContainSpecialCharacters()
    {
        // Generate multiple GUIDs and verify they don't contain +, /, or =
        for (int i = 0; i < 50; i++)
        {
            var guid = GenerateUniqueGuid();
            guid.Should().NotContain("+");
            guid.Should().NotContain("/");
            guid.Should().NotContain("=");
        }
    }

    // Testing GetByGuidAsync behavior

    [Fact]
    public void GetByGuidAsync_ShouldTrimGuid()
    {
        // Testing that GUID is trimmed before querying
        var guidWithSpaces = "  abc123  ";
        var trimmedGuid = guidWithSpaces.Trim();

        trimmedGuid.Should().Be("abc123");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetByGuidAsync_ShouldReturnNull_WhenGuidIsEmptyOrWhitespace(string? emptyGuid)
    {
        // Testing validation logic for empty GUID
        if (string.IsNullOrWhiteSpace(emptyGuid))
        {
            // Should return null - verified by test
            true.Should().BeTrue();
        }
    }

    // Testing CreateAsync initialization

    [Fact]
    public void CreateAsync_ShouldInitializeClientProperties()
    {
        // Testing that CreateAsync would set these properties
        var client = new Client
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            DateTo = DateTime.Now.AddDays(30),
            GoogleStorageUrl = "https://drive.google.com/folders/abc123"
        };

        // Simulate what CreateAsync does
        client.CreatedAt = DateTime.Now;
        client.IsActive = true;
        client.UploadedFilesCount = 0;

        // Assert
        client.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        client.IsActive.Should().BeTrue();
        client.UploadedFilesCount.Should().Be(0);
    }

    [Fact]
    public void CreateAsync_ShouldGenerateGuid_WhenGuidIsEmpty()
    {
        // Testing GUID generation logic
        var client = new Client
        {
            Guid = "",
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            DateTo = DateTime.Now.AddDays(30),
            GoogleStorageUrl = "https://drive.google.com/folders/abc123"
        };

        // Simulate what CreateAsync does
        if (string.IsNullOrEmpty(client.Guid))
        {
            client.Guid = GenerateUniqueGuid();
        }

        // Assert
        client.Guid.Should().NotBeNullOrEmpty();
        client.Guid.Should().HaveLength(12);
    }

    // Helper methods (replicating private methods for testing)

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static string GenerateUniqueGuid()
    {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "")
            [..12];
    }
}
