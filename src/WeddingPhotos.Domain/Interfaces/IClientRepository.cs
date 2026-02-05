using WeddingPhotos.Domain.Models;

namespace WeddingPhotos.Domain.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetByGuidAsync(string guid);
    Task<Client> CreateAsync(Client client);
    Task UpdateUploadedFilesCountAsync(string guid, int additionalFiles);
    Task<bool> DeactivateAsync(string guid);
    Task<List<Client>> GetExpiredClientsAsync();
}
