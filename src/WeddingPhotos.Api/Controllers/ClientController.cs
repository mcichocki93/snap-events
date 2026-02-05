using Microsoft.AspNetCore.Mvc;
using WeddingPhotos.Domain.Constants;
using WeddingPhotos.Domain.DTOs;
using WeddingPhotos.Domain.Interfaces;
using WeddingPhotos.Domain.Validation;

namespace WeddingPhotos.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientService clientService, ILogger<ClientController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        /// <summary>
        /// Get client (gallery) information by GUID
        /// NOTE: Personal data (FirstName, LastName, Email) is NOT sent to public gallery for privacy
        /// </summary>
        [HttpGet("{guid}")]
        public async Task<ActionResult<ClientResponse>> GetClient(string guid)
        {
            // Validate GUID format
            if (!InputValidator.IsValidGuid(guid))
            {
                _logger.LogWarning("Invalid GUID format attempted from {IP}: {Guid}",
                    HttpContext.Connection.RemoteIpAddress, guid);
                return BadRequest(new { error = "Invalid GUID format" });
            }

            // Check for injection attempts
            if (InputValidator.ContainsSuspiciousPatterns(guid))
            {
                _logger.LogWarning("Suspicious pattern detected in GUID from {IP}: {Guid}",
                    HttpContext.Connection.RemoteIpAddress, guid);
                return BadRequest(new { error = "Invalid input" });
            }

            // Delegate business logic to service
            var (success, response, errorMessage) = await _clientService.GetClientByGuidAsync(guid);

            if (!success)
            {
                return NotFound(new { error = errorMessage });
            }

            return Ok(response);
        }

        /// <summary>
        /// Get client with personal data (admin only - should be protected in production)
        /// This endpoint returns full client info including personal data
        /// </summary>
        //[HttpGet("{guid}/admin")]
        //public async Task<ActionResult<ClientResponse>> GetClientAdmin(string guid)
        //{
        //    // TODO: Add authentication/authorization here
        //    // [Authorize(Roles = "Admin")]

        //    if (!InputValidator.IsValidGuid(guid))
        //    {
        //        return BadRequest(new { error = "Invalid GUID format" });
        //    }

        //    var client = await _clientRepository.GetByGuidAsync(guid);

        //    if (client == null)
        //    {
        //        return NotFound(new { error = "Gallery not found" });
        //    }

        //    // Map to response DTO WITH personal data
        //    var response = new ClientResponse
        //    {
        //        Guid = client.Guid,
        //        EventName = client.BrideGroom,

        //        // ⭐ Personal data INCLUDED for admin
        //        FirstName = client.FirstName,
        //        LastName = client.LastName,
        //        Email = client.Email,
        //        Phone = client.Phone,

        //        EventType = client.EventType,
        //        EventTypeDisplayName = client.GetEventTypeDisplayName(),
        //        EventTypeEmoji = client.GetEventTypeEmoji(),
        //        EventDate = client.EventDate,
        //        DateTo = client.DateTo,
        //        IsActive = client.IsActive,
        //        IsExpired = client.DateTo < DateTime.UtcNow,
        //        MaxFiles = client.MaxFiles,
        //        UploadedFilesCount = client.UploadedFilesCount,
        //        CanUploadMore = client.UploadedFilesCount < client.MaxFiles,
        //        MaxFileSize = client.MaxFileSize,
        //        BackgroundColor = client.BackgroundColor,
        //        BackgroundColorSecondary = client.BackgroundColorSecondary,
        //        FontColor = client.FontColor,
        //        FontType = client.FontType,
        //        AccentColor = client.AccentColor
        //    };

        //    return Ok(response);
        //}

        /// <summary>
        /// Create new client (admin only - should be protected in production)
        /// </summary>
        //    [HttpPost]
        //    public async Task<ActionResult<ClientResponse>> CreateClient([FromBody] CreateClientRequest request)
        //    {
        //        // TODO: Add authentication/authorization
        //        // [Authorize(Roles = "Admin")]

        //        // Validate GUID
        //        if (!InputValidator.IsValidGuid(request.Guid))
        //        {
        //            return BadRequest(new { error = "Invalid GUID format" });
        //        }

        //        // Validate Email
        //        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
        //        {
        //            return BadRequest(new { error = "Valid email is required" });
        //        }

        //        // Check if GUID already exists
        //        var existing = await _clientRepository.GetByGuidAsync(request.Guid);
        //        if (existing != null)
        //        {
        //            return Conflict(new { error = "Gallery with this GUID already exists" });
        //        }

        //        // Validate EventType
        //        if (!InputValidator.IsValidEventType(request.EventType))
        //        {
        //            return BadRequest(new { error = "Invalid event type" });
        //        }

        //        var client = new Client
        //        {
        //            // Personal data
        //            FirstName = request.FirstName,
        //            LastName = request.LastName,
        //            Email = request.Email,
        //            Phone = request.Phone,

        //            // Event info
        //            Guid = request.Guid,
        //            BrideGroom = request.EventName,
        //            EventType = request.EventType,
        //            EventDate = request.EventDate,
        //            DateTo = request.DateTo,
        //            IsActive = true,

        //            // Limits
        //            MaxFiles = request.MaxFiles,
        //            UploadedFilesCount = 0,
        //            MaxFileSize = request.MaxFileSize,

        //            // Theme
        //            BackgroundColor = request.BackgroundColor ?? "#667eea",
        //            BackgroundColorSecondary = request.BackgroundColorSecondary ?? "#764ba2",
        //            FontColor = request.FontColor ?? "#ffffff",
        //            FontType = request.FontType ?? "Roboto",
        //            AccentColor = request.AccentColor ?? "#3b82f6",

        //            // Storage
        //            GoogleStorageUrl = request.GoogleStorageUrl,

        //            // Timestamps
        //            CreatedAt = DateTime.UtcNow,
        //            UpdatedAt = DateTime.UtcNow
        //        };

        //        await _clientRepository.CreateAsync(client);

        //        _logger.LogInformation("New gallery created: {Guid}, EventType: {EventType}, Email: {Email}",
        //            client.Guid, client.EventType, client.Email);

        //        var response = new ClientResponse
        //        {
        //            Guid = client.Guid,
        //            EventName = client.BrideGroom,
        //            FirstName = client.FirstName,
        //            LastName = client.LastName,
        //            Email = client.Email,
        //            Phone = client.Phone,
        //            EventType = client.EventType,
        //            EventTypeDisplayName = client.GetEventTypeDisplayName(),
        //            EventTypeEmoji = client.GetEventTypeEmoji(),
        //            EventDate = client.EventDate,
        //            DateTo = client.DateTo,
        //            IsActive = client.IsActive,
        //            IsExpired = false,
        //            MaxFiles = client.MaxFiles,
        //            UploadedFilesCount = client.UploadedFilesCount,
        //            CanUploadMore = true,
        //            MaxFileSize = client.MaxFileSize,
        //            BackgroundColor = client.BackgroundColor,
        //            BackgroundColorSecondary = client.BackgroundColorSecondary,
        //            FontColor = client.FontColor,
        //            FontType = client.FontType,
        //            AccentColor = client.AccentColor
        //        };

        //        return CreatedAtAction(nameof(GetClient), new { guid = client.Guid }, response);
        //    }

        //    /// <summary>
        //    /// Update client (admin only - should be protected in production)
        //    /// </summary>
        //    [HttpPut("{guid}")]
        //    public async Task<ActionResult<ClientResponse>> UpdateClient(string guid, [FromBody] UpdateClientRequest request)
        //    {
        //        // TODO: Add authentication/authorization

        //        if (!InputValidator.IsValidGuid(guid))
        //        {
        //            return BadRequest(new { error = "Invalid GUID format" });
        //        }

        //        var client = await _clientRepository.GetByGuidAsync(guid);
        //        if (client == null)
        //        {
        //            return NotFound(new { error = "Gallery not found" });
        //        }

        //        // Update personal data
        //        if (request.FirstName != null)
        //            client.FirstName = request.FirstName;

        //        if (request.LastName != null)
        //            client.LastName = request.LastName;

        //        if (request.Email != null)
        //        {
        //            if (!request.Email.Contains("@"))
        //                return BadRequest(new { error = "Invalid email format" });
        //            client.Email = request.Email;
        //        }

        //        if (request.Phone != null)
        //            client.Phone = request.Phone;

        //        // Update event info
        //        if (request.EventName != null)
        //            client.BrideGroom = request.EventName;

        //        if (request.EventType != null)
        //        {
        //            if (!InputValidator.IsValidEventType(request.EventType))
        //                return BadRequest(new { error = "Invalid event type" });
        //            client.EventType = request.EventType;
        //        }

        //        if (request.EventDate.HasValue)
        //            client.EventDate = request.EventDate;

        //        if (request.DateTo.HasValue)
        //            client.DateTo = request.DateTo.Value;

        //        if (request.IsActive.HasValue)
        //            client.IsActive = request.IsActive.Value;

        //        // Update limits
        //        if (request.MaxFiles.HasValue)
        //            client.MaxFiles = request.MaxFiles.Value;

        //        if (request.MaxFileSize.HasValue)
        //            client.MaxFileSize = request.MaxFileSize.Value;

        //        // Update theme
        //        if (request.BackgroundColor != null)
        //            client.BackgroundColor = request.BackgroundColor;

        //        if (request.BackgroundColorSecondary != null)
        //            client.BackgroundColorSecondary = request.BackgroundColorSecondary;

        //        if (request.FontColor != null)
        //            client.FontColor = request.FontColor;

        //        if (request.FontType != null)
        //            client.FontType = request.FontType;

        //        if (request.AccentColor != null)
        //            client.AccentColor = request.AccentColor;

        //        if (request.GoogleStorageUrl != null)
        //            client.GoogleStorageUrl = request.GoogleStorageUrl;

        //        client.UpdatedAt = DateTime.UtcNow;

        //        await _clientRepository.UpdateAsync(guid, client);

        //        _logger.LogInformation("Gallery updated: {Guid}", guid);

        //        var response = new ClientResponse
        //        {
        //            Guid = client.Guid,
        //            EventName = client.BrideGroom,
        //            FirstName = client.FirstName,
        //            LastName = client.LastName,
        //            Email = client.Email,
        //            Phone = client.Phone,
        //            EventType = client.EventType,
        //            EventTypeDisplayName = client.GetEventTypeDisplayName(),
        //            EventTypeEmoji = client.GetEventTypeEmoji(),
        //            EventDate = client.EventDate,
        //            DateTo = client.DateTo,
        //            IsActive = client.IsActive,
        //            IsExpired = client.DateTo < DateTime.UtcNow,
        //            MaxFiles = client.MaxFiles,
        //            UploadedFilesCount = client.UploadedFilesCount,
        //            CanUploadMore = client.UploadedFilesCount < client.MaxFiles,
        //            MaxFileSize = client.MaxFileSize,
        //            BackgroundColor = client.BackgroundColor,
        //            BackgroundColorSecondary = client.BackgroundColorSecondary,
        //            FontColor = client.FontColor,
        //            FontType = client.FontType,
        //            AccentColor = client.AccentColor
        //        };

        //        return Ok(response);
        //    }

        //    /// <summary>
        //    /// Delete client (admin only - should be protected in production)
        //    /// </summary>
        //    [HttpDelete("{guid}")]
        //    public async Task<ActionResult> DeleteClient(string guid)
        //    {
        //        // TODO: Add authentication/authorization

        //        if (!InputValidator.IsValidGuid(guid))
        //        {
        //            return BadRequest(new { error = "Invalid GUID format" });
        //        }

        //        var client = await _clientRepository.GetByGuidAsync(guid);
        //        if (client == null)
        //        {
        //            return NotFound(new { error = "Gallery not found" });
        //        }

        //        await _clientRepository.DeleteAsync(guid);

        //        _logger.LogInformation("Gallery deleted: {Guid}, Email: {Email}", guid, client.Email);

        //        return NoContent();
        //    }

        //    /// <summary>
        //    /// Increment uploaded files count (called by PhotoController after successful upload)
        //    /// </summary>
        //    [HttpPost("{guid}/increment-count")]
        //    public async Task<ActionResult> IncrementUploadCount(string guid)
        //    {
        //        if (!InputValidator.IsValidGuid(guid))
        //        {
        //            return BadRequest(new { error = "Invalid GUID format" });
        //        }

        //        var client = await _clientRepository.GetByGuidAsync(guid);
        //        if (client == null)
        //        {
        //            return NotFound(new { error = "Gallery not found" });
        //        }

        //        client.UploadedFilesCount++;
        //        client.UpdatedAt = DateTime.UtcNow;

        //        await _clientRepository.UpdateAsync(guid, client);

        //        _logger.LogInformation("Upload count incremented for gallery: {Guid}, NewCount: {Count}",
        //            guid, client.UploadedFilesCount);

        //        return Ok(new { uploadedFilesCount = client.UploadedFilesCount });
        //    }
        //}
    }
}