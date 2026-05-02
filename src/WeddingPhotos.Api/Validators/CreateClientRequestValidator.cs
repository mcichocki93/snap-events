using FluentValidation;
using WeddingPhotos.Domain.Constants;
using WeddingPhotos.Domain.DTOs;

namespace WeddingPhotos.Api.Validators;

public class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        // Personal Data
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Imię jest wymagane")
            .MaximumLength(100).WithMessage("Imię nie może być dłuższe niż 100 znaków");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Nazwisko jest wymagane")
            .MaximumLength(100).WithMessage("Nazwisko nie może być dłuższe niż 100 znaków");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany")
            .EmailAddress().WithMessage("Nieprawidłowy format email")
            .MaximumLength(255).WithMessage("Email nie może być dłuższy niż 255 znaków");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{6,14}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Nieprawidłowy format numeru telefonu (użyj formatu: +48123456789)");

        // Event Info
        RuleFor(x => x.Guid)
            .NotEmpty().WithMessage("GUID jest wymagany")
            .Must(BeValidGuid).WithMessage("GUID musi być prawidłowym identyfikatorem UUID");

        RuleFor(x => x.EventName)
            .NotEmpty().WithMessage("Nazwa wydarzenia jest wymagana")
            .MaximumLength(200).WithMessage("Nazwa wydarzenia nie może być dłuższa niż 200 znaków");

        RuleFor(x => x.EventType)
            .NotEmpty().WithMessage("Typ wydarzenia jest wymagany")
            .Must(BeValidEventType).WithMessage($"Nieprawidłowy typ wydarzenia. Dozwolone: {string.Join(", ", ApplicationConstants.EventTypes.All)}");

        RuleFor(x => x.EventDate)
            .LessThanOrEqualTo(x => x.DateTo)
            .When(x => x.EventDate.HasValue)
            .WithMessage("Data wydarzenia musi być wcześniejsza niż data wygaśnięcia");

        RuleFor(x => x.DateTo)
            .NotEmpty().WithMessage("Data wygaśnięcia jest wymagana")
            .GreaterThan(DateTime.UtcNow).WithMessage("Data wygaśnięcia musi być w przyszłości");

        // Limits
        RuleFor(x => x.MaxFiles)
            .GreaterThanOrEqualTo(0).WithMessage("Maksymalna liczba plików nie może być ujemna")
            .LessThanOrEqualTo(10000).WithMessage("Maksymalna liczba plików nie może przekraczać 10000 (użyj 0 dla braku limitu)");

        RuleFor(x => x.MaxFileSize)
            .GreaterThan(0).WithMessage("Maksymalny rozmiar pliku musi być większy niż 0")
            .LessThanOrEqualTo(ApplicationConstants.FileUpload.MaxFileSizeBytes)
            .WithMessage($"Maksymalny rozmiar pliku nie może przekraczać {ApplicationConstants.FileUpload.MaxFileSizeBytes / (1024 * 1024)}MB");

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

        // Storage — accepts bare folder ID or full Google Drive URL
        RuleFor(x => x.GoogleStorageUrl)
            .NotEmpty().WithMessage("ID lub URL folderu Google Drive jest wymagany")
            .Must(BeValidGoogleDriveInput).WithMessage("Podaj ID folderu Google Drive lub pełny URL folderu");
    }

    private bool BeValidGuid(string guid)
    {
        return Guid.TryParse(guid, out _);
    }

    private bool BeValidEventType(string eventType)
    {
        return ApplicationConstants.EventTypes.All.Contains(eventType);
    }

    private bool BeValidGoogleDriveInput(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        // Bare folder ID: alphanumeric + dash + underscore
        if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z0-9_-]{10,}$")) return true;
        // Full Google Drive URL
        return value.Contains("drive.google.com");
    }
}
