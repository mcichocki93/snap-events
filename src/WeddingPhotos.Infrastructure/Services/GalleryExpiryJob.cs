using Microsoft.Extensions.Logging;
using WeddingPhotos.Domain.Interfaces;

namespace WeddingPhotos.Infrastructure.Services;

public class GalleryExpiryJob
{
    private readonly IClientRepository _clientRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<GalleryExpiryJob> _logger;

    public GalleryExpiryJob(
        IClientRepository clientRepository,
        INotificationService notificationService,
        ILogger<GalleryExpiryJob> logger)
    {
        _clientRepository = clientRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task SendExpiryRemindersAsync()
    {
        _logger.LogInformation("Running gallery expiry reminder job");

        var clients = await _clientRepository.GetExpiringInDaysAsync(7);

        if (clients.Count == 0)
        {
            _logger.LogInformation("No galleries expiring in 7 days");
            return;
        }

        _logger.LogInformation("Found {Count} galleries expiring in 7 days", clients.Count);

        foreach (var client in clients)
        {
            try
            {
                await _notificationService.SendGalleryExpiryReminderAsync(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send expiry reminder for client {Guid}", client.Guid);
            }
        }
    }
}
