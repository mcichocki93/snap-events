using Microsoft.AspNetCore.Mvc;
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
    }
}
