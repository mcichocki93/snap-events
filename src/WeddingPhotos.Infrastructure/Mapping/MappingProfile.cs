using AutoMapper;
using WeddingPhotos.Domain.DTOs;
using WeddingPhotos.Domain.Models;

namespace WeddingPhotos.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // PhotoInfo mappings (from storage model to DTO)
        CreateMap<PhotoInfo, PhotoInfo>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.ThumbnailUrl))
            .ForMember(dest => dest.FullUrl, opt => opt.MapFrom(src => src.FullUrl))
            .ForMember(dest => dest.DateAdded, opt => opt.MapFrom(src => src.DateAdded))
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size))
            .ForMember(dest => dest.MimeType, opt => opt.MapFrom(src => src.MimeType));
            // Computed properties (IsNew, FormattedSize, FileExtension) will be auto-calculated

        // Client to ClientResponse mapping
        CreateMap<Client, ClientResponse>()
            .ForMember(dest => dest.Guid, opt => opt.MapFrom(src => src.Guid))
            .ForMember(dest => dest.EventName, opt => opt.MapFrom(src => src.EventName))
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType))
            .ForMember(dest => dest.EventTypeDisplayName, opt => opt.MapFrom(src => src.GetEventTypeDisplayName()))
            .ForMember(dest => dest.EventTypeEmoji, opt => opt.MapFrom(src => src.GetEventTypeEmoji()))
            .ForMember(dest => dest.EventDate, opt => opt.MapFrom(src => src.EventDate))
            .ForMember(dest => dest.DateTo, opt => opt.MapFrom(src => src.DateTo))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.DateTo < DateTime.UtcNow))
            .ForMember(dest => dest.MaxFiles, opt => opt.MapFrom(src => src.MaxFiles))
            .ForMember(dest => dest.UploadedFilesCount, opt => opt.MapFrom(src => src.UploadedFilesCount))
            .ForMember(dest => dest.CanUploadMore, opt => opt.MapFrom(src => src.UploadedFilesCount < src.MaxFiles))
            .ForMember(dest => dest.MaxFileSize, opt => opt.MapFrom(src => src.MaxFileSize))
            .ForMember(dest => dest.BackgroundColor, opt => opt.MapFrom(src => src.BackgroundColor))
            .ForMember(dest => dest.BackgroundColorSecondary, opt => opt.MapFrom(src => src.BackgroundColorSecondary))
            .ForMember(dest => dest.FontColor, opt => opt.MapFrom(src => src.FontColor))
            .ForMember(dest => dest.FontType, opt => opt.MapFrom(src => src.FontType))
            .ForMember(dest => dest.AccentColor, opt => opt.MapFrom(src => src.AccentColor))
            // Personal data excluded by default (FirstName, LastName, Email, Phone are null)
            .ForMember(dest => dest.FirstName, opt => opt.Ignore())
            .ForMember(dest => dest.LastName, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.Phone, opt => opt.Ignore());

        // CreateClientRequest to Client
        CreateMap<CreateClientRequest, Client>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())  // MongoDB auto-generates
            .ForMember(dest => dest.Guid, opt => opt.MapFrom(src => src.Guid))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.EventName, opt => opt.MapFrom(src => src.EventName))
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType))
            .ForMember(dest => dest.EventDate, opt => opt.MapFrom(src => src.EventDate))
            .ForMember(dest => dest.DateTo, opt => opt.MapFrom(src => src.DateTo))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.MaxFiles, opt => opt.MapFrom(src => src.MaxFiles))
            .ForMember(dest => dest.UploadedFilesCount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.MaxFileSize, opt => opt.MapFrom(src => src.MaxFileSize))
            .ForMember(dest => dest.BackgroundColor, opt => opt.MapFrom(src => src.BackgroundColor ?? "#667eea"))
            .ForMember(dest => dest.BackgroundColorSecondary, opt => opt.MapFrom(src => src.BackgroundColorSecondary ?? "#764ba2"))
            .ForMember(dest => dest.FontColor, opt => opt.MapFrom(src => src.FontColor ?? "#ffffff"))
            .ForMember(dest => dest.FontType, opt => opt.MapFrom(src => src.FontType ?? "Roboto"))
            .ForMember(dest => dest.AccentColor, opt => opt.MapFrom(src => src.AccentColor ?? "#3b82f6"))
            .ForMember(dest => dest.GoogleStorageUrl, opt => opt.MapFrom(src => src.GoogleStorageUrl))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}
