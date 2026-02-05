using FluentValidation;
using WeddingPhotos.Domain.Constants;
using WeddingPhotos.Domain.DTOs;

namespace WeddingPhotos.Api.Validators;

public class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequest>
{
    public UpdateClientRequestValidator()
    {
        // Personal Data (optional but must be valid if provided)
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("Imię nie może być dłuższe niż 100 znaków")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Nazwisko nie może być dłuższe niż 100 znaków")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Nieprawidłowy format email")
            .MaximumLength(255).WithMessage("Email nie może być dłuższy niż 255 znaków")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{6,14}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Nieprawidłowy format numeru telefonu (użyj formatu: +48123456789)");

        // Event Info
        RuleFor(x => x.EventName)
            .MaximumLength(200).WithMessage("Nazwa wydarzenia nie może być dłuższa niż 200 znaków")
            .When(x => !string.IsNullOrEmpty(x.EventName));

        RuleFor(x => x.EventType)
            .Must(BeValidEventType).WithMessage($"Nieprawidłowy typ wydarzenia. Dozwolone: {string.Join(", ", ApplicationConstants.EventTypes.All)}")
            .When(x => !string.IsNullOrEmpty(x.EventType));

        RuleFor(x => x.DateTo)
            .GreaterThan(DateTime.UtcNow).WithMessage("Data wygaśnięcia musi być w przyszłości")
            .When(x => x.DateTo.HasValue);

        // Limits
        RuleFor(x => x.MaxFiles)
            .GreaterThan(0).WithMessage("Maksymalna liczba plików musi być większa niż 0")
            .LessThanOrEqualTo(10000).WithMessage("Maksymalna liczba plików nie może przekraczać 10000")
            .When(x => x.MaxFiles.HasValue);

        RuleFor(x => x.MaxFileSize)
            .GreaterThan(0).WithMessage("Maksymalny rozmiar pliku musi być większy niż 0")
            .LessThanOrEqualTo(ApplicationConstants.FileUpload.MaxFileSizeBytes)
            .WithMessage($"Maksymalny rozmiar pliku nie może przekraczać {ApplicationConstants.FileUpload.MaxFileSizeBytes / (1024 * 1024)}MB")
            .When(x => x.MaxFileSize.HasValue);

        // Theme Colors (optional but must be valid hex if provided)
        RuleFor(x => x.BackgroundColor)
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .When(x => !string.IsNullOrEmpty(x.BackgroundColor))
            .WithMessage("Kolor tła musi być w formacie HEX (np. #667eea)");

        RuleFor(x => x.BackgroundColorSecondary)
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .When(x => !string.IsNullOrEmpty(x.BackgroundColorSecondary))
            .WithMessage("Drugi kolor tła musi być w formacie HEX (np. #764ba2)");

        RuleFor(x => x.FontColor)
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .When(x => !string.IsNullOrEmpty(x.FontColor))
            .WithMessage("Kolor czcionki musi być w formacie HEX (np. #ffffff)");

        RuleFor(x => x.AccentColor)
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .When(x => !string.IsNullOrEmpty(x.AccentColor))
            .WithMessage("Kolor akcentu musi być w formacie HEX (np. #3b82f6)");

        // Storage
        RuleFor(x => x.GoogleStorageUrl)
            .Must(BeValidUrl).WithMessage("Nieprawidłowy format URL storage")
            .When(x => !string.IsNullOrEmpty(x.GoogleStorageUrl));
    }

    private bool BeValidEventType(string? eventType)
    {
        return !string.IsNullOrEmpty(eventType) && ApplicationConstants.EventTypes.All.Contains(eventType);
    }

    private bool BeValidUrl(string? url)
    {
        return !string.IsNullOrEmpty(url)
            && Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
