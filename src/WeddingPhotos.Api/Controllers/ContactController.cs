using Hangfire;
using Microsoft.AspNetCore.Mvc;
using WeddingPhotos.Domain.DTOs;
using WeddingPhotos.Domain.Interfaces;

namespace WeddingPhotos.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ContactController : ControllerBase
{
    private readonly ILogger<ContactController> _logger;

    public ContactController(ILogger<ContactController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Send contact form message
    /// </summary>
    /// <param name="request">Contact form data</param>
    /// <returns>Success response</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult SendContactForm([FromBody] ContactFormRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid contact form submission from {Email}", request?.Email ?? "(unknown)");
            return BadRequest(new
            {
                error = "Nieprawidłowe dane formularza",
                details = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        try
        {
            // Queue notification to be sent asynchronously with Hangfire
            // This ensures the user gets immediate response and we have automatic retry on failure
            BackgroundJob.Enqueue<INotificationService>(
                service => service.SendContactFormNotificationAsync(request));

            _logger.LogInformation(
                "Contact form queued for {Name} ({Email})",
                request.Name,
                request.Email);

            return Ok(new
            {
                message = "Dziękujemy za wiadomość! Odpowiemy najszybciej jak to możliwe.",
                success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue contact form notification");
            return StatusCode(500, new
            {
                error = "Wystąpił błąd podczas wysyłania wiadomości. Spróbuj ponownie później.",
                success = false
            });
        }
    }

    /// <summary>
    /// Health check endpoint for contact service
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        // Do not expose configuration details (information disclosure risk)
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        });
    }
}
