using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WeddingPhotos.Domain.DTOs;
using WeddingPhotos.Domain.Interfaces;
using WeddingPhotos.Domain.Models;

namespace WeddingPhotos.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IContactMessageRepository _contactMessageRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string? _discordWebhookUrl;

    public NotificationService(
        ILogger<NotificationService> logger,
        IHttpClientFactory httpClientFactory,
        IContactMessageRepository contactMessageRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _contactMessageRepository = contactMessageRepository;
        _httpContextAccessor = httpContextAccessor;
        _discordWebhookUrl = Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL");
    }

    public async Task SendContactFormNotificationAsync(ContactFormRequest request)
    {
        string? messageId = null;

        try
        {
            // 1. Save message to database first
            var contactMessage = new ContactMessage
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Subject = request.Subject,
                Message = request.Message,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                CreatedAt = DateTime.UtcNow
            };

            var savedMessage = await _contactMessageRepository.CreateAsync(contactMessage);
            messageId = savedMessage.Id;

            _logger.LogInformation(
                "Contact message saved to database: {Name} ({Email}), ID: {Id}",
                request.Name,
                request.Email,
                messageId);

            // 2. Send Discord notification if configured
            if (!string.IsNullOrEmpty(_discordWebhookUrl))
            {
                await SendDiscordNotificationAsync(request);

                // Update notification status
                await _contactMessageRepository.UpdateNotificationStatusAsync(messageId, true);

                _logger.LogInformation(
                    "Discord notification sent successfully for message ID: {Id}",
                    messageId);
            }
            else
            {
                _logger.LogWarning(
                    "Discord webhook URL is not configured. Message saved but notification not sent. ID: {Id}",
                    messageId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing contact form for {Name} ({Email}). Message ID: {Id}",
                request.Name,
                request.Email,
                messageId ?? "not saved");
            throw;
        }
    }

    private string? GetClientIpAddress()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Check for forwarded IP (behind proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').FirstOrDefault()?.Trim();
            }

            // Check for real IP
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fallback to remote IP
            return context.Connection.RemoteIpAddress?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private string? GetUserAgent()
    {
        try
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private async Task SendDiscordNotificationAsync(ContactFormRequest request)
    {
        var subject = !string.IsNullOrEmpty(request.Subject)
            ? request.Subject
            : "Nowa wiadomość kontaktowa";

        var payload = new
        {
            content = "📬 **Nowa wiadomość z formularza kontaktowego!**",
            embeds = new object[]
            {
                new
                {
                    title = subject,
                    color = 5814783, // Blue color
                    fields = new object[]
                    {
                        new
                        {
                            name = "👤 Imię i nazwisko",
                            value = request.Name,
                            inline = true
                        },
                        new
                        {
                            name = "📧 Email",
                            value = request.Email,
                            inline = true
                        },
                        new
                        {
                            name = "📱 Telefon",
                            value = string.IsNullOrEmpty(request.Phone) ? "Nie podano" : request.Phone,
                            inline = true
                        },
                        new
                        {
                            name = "💬 Wiadomość",
                            value = TruncateMessage(request.Message, 1024) // Discord limit: 1024 chars per field
                        }
                    },
                    timestamp = DateTime.UtcNow.ToString("o"),
                    footer = new
                    {
                        text = "Wedding Photos - Contact Form"
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_discordWebhookUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Discord webhook failed with status {response.StatusCode}: {errorContent}");
        }
    }

    private static string TruncateMessage(string message, int maxLength)
    {
        if (string.IsNullOrEmpty(message) || message.Length <= maxLength)
            return message;

        return message[..(maxLength - 3)] + "...";
    }
}
