using WeddingPhotos.Domain.Models;

namespace WeddingPhotos.Domain.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetByGuidAsync(string guid);
    Task<List<Client>> GetAllAsync();
    Task<Client> CreateAsync(Client client);
    Task<bool> UpdateAsync(string guid, Client client);
    Task UpdateUploadedFilesCountAsync(string guid, int additionalFiles);

    /// <summary>
    /// Atomically reserves an upload slot: increments UploadedFilesCount by 1,
    /// but only if the gallery is unlimited (MaxFiles == 0) or still under its
    /// limit. Returns the updated client, or null if the quota is exhausted.
    /// This prevents concurrent uploads from exceeding MaxFiles (race condition).
    /// </summary>
    Task<Client?> TryReserveUploadSlotAsync(string guid);

    /// <summary>
    /// Releases a previously reserved slot (decrements the counter, floored at 0).
    /// Called when an upload fails after the slot was reserved.
    /// </summary>
    Task ReleaseUploadSlotAsync(string guid);
    Task<bool> DeactivateAsync(string guid);
    Task<bool> DeleteAsync(string guid);
    Task<List<Client>> GetExpiredClientsAsync();
    Task<List<Client>> GetExpiringInDaysAsync(int days);
}
