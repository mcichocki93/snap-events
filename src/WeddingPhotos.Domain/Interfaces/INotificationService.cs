using WeddingPhotos.Domain.DTOs;

namespace WeddingPhotos.Domain.Interfaces;

public interface INotificationService
{
    Task SendContactFormNotificationAsync(ContactFormRequest request);
}
