using WeddingPhotos.Domain.DTOs;

namespace WeddingPhotos.Domain.Interfaces;

public interface IClientService
{
    Task<(bool Success, ClientResponse? Response, string? ErrorMessage)> GetClientByGuidAsync(string guid);
}
