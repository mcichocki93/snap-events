using WeddingPhotos.Domain.DTOs;
using WeddingPhotos.Domain.Models;

namespace WeddingPhotos.Domain.Interfaces;

public interface INotificationService
{
    Task SendContactFormNotificationAsync(ContactFormRequest request);
    Task SendGalleryExpiryReminderAsync(Client client);
}
