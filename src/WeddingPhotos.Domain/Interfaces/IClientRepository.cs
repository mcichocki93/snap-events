using WeddingPhotos.Domain.Models;

namespace WeddingPhotos.Domain.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetByGuidAsync(string guid);
    Task<List<Client>> GetAllAsync();
    Task<Client> CreateAsync(Client client);
    Task<bool> UpdateAsync(string guid, Client client);
    Task UpdateUploadedFilesCountAsync(string guid, int additionalFiles);
    Task<bool> DeactivateAsync(string guid);
    Task<bool> DeleteAsync(string guid);
    Task<List<Client>> GetExpiredClientsAsync();
}
