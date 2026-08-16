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

        // No "must be in the future" rule here, unlike on create: an expired
        // gallery has a past DateTo, and the edit form resends it, so requiring
        // a future date made expired galleries impossible to edit - including
        // impossible to extend, which is the main reason to edit one.

        // Limits
        RuleFor(x => x.MaxFiles)
            .GreaterThanOrEqualTo(0).WithMessage("Maksymalna liczba plików nie może być ujemna")
            .LessThanOrEqualTo(ClientValidationRules.MaxFilesLimit)
            .WithMessage($"Maksymalna liczba plików nie może przekraczać {ClientValidationRules.MaxFilesLimit} (użyj 0 dla braku limitu)")
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

        // Storage — accepts bare folder ID or full Google Drive URL, same as create
        RuleFor(x => x.GoogleStorageUrl)
            .Must(ClientValidationRules.IsValidGoogleDriveInput)
            .WithMessage("Podaj ID folderu Google Drive lub pełny URL folderu")
            .When(x => !string.IsNullOrEmpty(x.GoogleStorageUrl));
    }

    private bool BeValidEventType(string? eventType)
    {
        return ClientValidationRules.IsValidEventType(eventType);
    }
}
