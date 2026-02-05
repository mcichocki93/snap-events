using AutoMapper;
using Microsoft.Extensions.Logging;
using WeddingPhotos.Domain.Constants;
using WeddingPhotos.Domain.DTOs;
using WeddingPhotos.Domain.Interfaces;

namespace WeddingPhotos.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ClientService> _logger;

    public ClientService(
        IClientRepository clientRepository,
        IMapper mapper,
        ILogger<ClientService> logger)
    {
        _clientRepository = clientRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<(bool Success, ClientResponse? Response, string? ErrorMessage)> GetClientByGuidAsync(string guid)
    {
        try
        {
            var client = await _clientRepository.GetByGuidAsync(guid);

            if (client == null)
            {
                _logger.LogInformation("Client not found: {Guid}", guid);
                return (false, null, "Gallery not found");
            }

            // Check if gallery is active
            if (!client.IsActive)
            {
                _logger.LogInformation("Inactive gallery accessed: {Guid}", guid);
                return (false, null, "Gallery is no longer active");
            }

            // Check if gallery has expired
            var isExpired = client.DateTo < DateTime.UtcNow;
            if (isExpired)
            {
                _logger.LogInformation("Expired gallery accessed: {Guid}, ExpiredOn: {DateTo}",
                    guid, client.DateTo);
                return (false, null, "Gallery has expired");
            }

            // Map to response DTO using AutoMapper (personal data excluded by mapping profile)
            var response = _mapper.Map<ClientResponse>(client);

            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client: {Guid}", guid);
            return (false, null, "An error occurred while fetching gallery information");
        }
    }
}
