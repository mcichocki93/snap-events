using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WeddingPhotos.Domain.DTOs;
using WeddingPhotos.Domain.Interfaces;
using WeddingPhotos.Domain.Models;
using WeddingPhotos.Domain.Validation;

namespace WeddingPhotos.Api.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly IClientRepository _clientRepository;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IClientRepository clientRepository,
        IMapper mapper,
        IConfiguration configuration,
        ILogger<AdminController> logger)
    {
        _clientRepository = clientRepository;
        _mapper = mapper;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Login — returns JWT token
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] AdminLoginRequest request)
    {
        var expectedUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME")
            ?? _configuration["Admin:Username"] ?? string.Empty;
        var expectedPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
            ?? _configuration["Admin:Password"] ?? string.Empty;

        var usernameBytes = Encoding.UTF8.GetBytes(request.Username);
        var expectedUsernameBytes = Encoding.UTF8.GetBytes(expectedUsername);
        var passwordBytes = Encoding.UTF8.GetBytes(request.Password);
        var expectedPasswordBytes = Encoding.UTF8.GetBytes(expectedPassword);

        // Constant-time comparison to prevent timing attacks
        var usernameMatch = usernameBytes.Length == expectedUsernameBytes.Length
            && CryptographicOperations.FixedTimeEquals(usernameBytes, expectedUsernameBytes);
        var passwordMatch = passwordBytes.Length == expectedPasswordBytes.Length
            && CryptographicOperations.FixedTimeEquals(passwordBytes, expectedPasswordBytes);

        if (!usernameMatch || !passwordMatch)
        {
            _logger.LogWarning("Failed admin login attempt from {IP}", HttpContext.Connection.RemoteIpAddress);
            return Unauthorized(new { error = "Nieprawidłowe dane logowania" });
        }

        var token = GenerateJwtToken();
        _logger.LogInformation("Admin login successful from {IP}", HttpContext.Connection.RemoteIpAddress);

        return Ok(new { token });
    }

    /// <summary>
    /// Get all clients (with full personal data)
    /// </summary>
    [Authorize]
    [HttpGet("clients")]
    public async Task<ActionResult<List<AdminClientResponse>>> GetClients()
    {
        var clients = await _clientRepository.GetAllAsync();
        var response = clients.Select(MapToAdminResponse).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Get single client with full personal data
    /// </summary>
    [Authorize]
    [HttpGet("clients/{guid}")]
    public async Task<ActionResult<AdminClientResponse>> GetClient(string guid)
    {
        if (!InputValidator.IsValidGuid(guid))
            return BadRequest(new { error = "Invalid GUID format" });

        var client = await _clientRepository.GetByGuidAsync(guid);
        if (client == null)
            return NotFound(new { error = "Galeria nie znaleziona" });

        return Ok(MapToAdminResponse(client));
    }

    /// <summary>
    /// Create new client/gallery
    /// </summary>
    [Authorize]
    [HttpPost("clients")]
    public async Task<ActionResult<AdminClientResponse>> CreateClient([FromBody] CreateClientRequest request)
    {
        if (!InputValidator.IsValidGuid(request.Guid))
            return BadRequest(new { error = "Invalid GUID format" });

        var existing = await _clientRepository.GetByGuidAsync(request.Guid);
        if (existing != null)
            return Conflict(new { error = "Galeria z tym GUID już istnieje" });

        var client = _mapper.Map<Client>(request);
        var created = await _clientRepository.CreateAsync(client);

        _logger.LogInformation("Admin created gallery: {Guid} for {Email}", created.Guid, created.Email);

        return CreatedAtAction(nameof(GetClient), new { guid = created.Guid }, MapToAdminResponse(created));
    }

    /// <summary>
    /// Update client/gallery
    /// </summary>
    [Authorize]
    [HttpPut("clients/{guid}")]
    public async Task<ActionResult<AdminClientResponse>> UpdateClient(string guid, [FromBody] UpdateClientRequest request)
    {
        if (!InputValidator.IsValidGuid(guid))
            return BadRequest(new { error = "Invalid GUID format" });

        var client = await _clientRepository.GetByGuidAsync(guid);
        if (client == null)
            return NotFound(new { error = "Galeria nie znaleziona" });

        if (request.FirstName != null) client.FirstName = request.FirstName;
        if (request.LastName != null) client.LastName = request.LastName;
        if (request.Email != null) client.Email = request.Email;
        if (request.Phone != null) client.Phone = request.Phone;
        if (request.EventName != null) client.EventName = request.EventName;
        if (request.EventType != null) client.EventType = request.EventType;
        if (request.EventDate.HasValue) client.EventDate = request.EventDate;
        if (request.DateTo.HasValue) client.DateTo = request.DateTo.Value;
        if (request.IsActive.HasValue) client.IsActive = request.IsActive.Value;
        if (request.MaxFiles.HasValue) client.MaxFiles = request.MaxFiles.Value;
        if (request.MaxFileSize.HasValue) client.MaxFileSize = request.MaxFileSize.Value;
        if (request.BackgroundColor != null) client.BackgroundColor = request.BackgroundColor;
        if (request.BackgroundColorSecondary != null) client.BackgroundColorSecondary = request.BackgroundColorSecondary;
        if (request.FontColor != null) client.FontColor = request.FontColor;
        if (request.FontType != null) client.FontType = request.FontType;
        if (request.AccentColor != null) client.AccentColor = request.AccentColor;
        if (request.GoogleStorageUrl != null) client.GoogleStorageUrl = request.GoogleStorageUrl;

        await _clientRepository.UpdateAsync(guid, client);
        _logger.LogInformation("Admin updated gallery: {Guid}", guid);

        return Ok(MapToAdminResponse(client));
    }

    /// <summary>
    /// Delete client/gallery permanently
    /// </summary>
    [Authorize]
    [HttpDelete("clients/{guid}")]
    public async Task<IActionResult> DeleteClient(string guid)
    {
        if (!InputValidator.IsValidGuid(guid))
            return BadRequest(new { error = "Invalid GUID format" });

        var client = await _clientRepository.GetByGuidAsync(guid);
        if (client == null)
            return NotFound(new { error = "Galeria nie znaleziona" });

        await _clientRepository.DeleteAsync(guid);
        _logger.LogInformation("Admin deleted gallery: {Guid}, Email: {Email}", guid, client.Email);

        return NoContent();
    }

    private string GenerateJwtToken()
    {
        var jwtSecret = Environment.GetEnvironmentVariable("ADMIN_JWT_SECRET")
            ?? _configuration["Admin:JwtSecret"]!;
        var expirationHours = int.TryParse(_configuration["Admin:JwtExpirationHours"], out var h) ? h : 8;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: new[] { new Claim(ClaimTypes.Role, "Admin") },
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static AdminClientResponse MapToAdminResponse(Client client) => new()
    {
        Guid = client.Guid,
        FirstName = client.FirstName,
        LastName = client.LastName,
        Email = client.Email,
        Phone = client.Phone,
        EventName = client.EventName,
        EventType = client.EventType,
        EventTypeDisplayName = client.GetEventTypeDisplayName(),
        EventTypeEmoji = client.GetEventTypeEmoji(),
        EventDate = client.EventDate,
        DateTo = client.DateTo,
        IsActive = client.IsActive,
        IsExpired = client.DateTo < DateTime.UtcNow,
        MaxFiles = client.MaxFiles,
        UploadedFilesCount = client.UploadedFilesCount,
        CanUploadMore = client.MaxFiles == 0 || client.UploadedFilesCount < client.MaxFiles,
        MaxFileSize = client.MaxFileSize,
        BackgroundColor = client.BackgroundColor,
        BackgroundColorSecondary = client.BackgroundColorSecondary,
        FontColor = client.FontColor,
        FontType = client.FontType,
        AccentColor = client.AccentColor,
        GoogleStorageUrl = client.GoogleStorageUrl,
        CreatedAt = client.CreatedAt,
        UpdatedAt = client.UpdatedAt
    };
}
