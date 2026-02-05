using WeddingPhotos.Domain.Models;

namespace WeddingPhotos.Domain.Interfaces;

public interface IContactMessageRepository
{
    Task<ContactMessage> CreateAsync(ContactMessage message);
    Task<ContactMessage?> GetByIdAsync(string id);
    Task<List<ContactMessage>> GetAllAsync(int page = 1, int pageSize = 50);
    Task<List<ContactMessage>> GetUnreadAsync();
    Task<bool> MarkAsReadAsync(string id);
    Task<bool> UpdateNotificationStatusAsync(string id, bool sent);
    Task<long> GetTotalCountAsync();
    Task<long> GetUnreadCountAsync();
}
