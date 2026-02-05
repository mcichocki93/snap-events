using FluentValidation;
using WeddingPhotos.Domain.DTOs;

namespace WeddingPhotos.Api.Validators;

public class ContactFormRequestValidator : AbstractValidator<ContactFormRequest>
{
    public ContactFormRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Imię i nazwisko są wymagane")
            .MaximumLength(100).WithMessage("Imię i nazwisko nie może być dłuższe niż 100 znaków")
            .Matches(@"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ\s\-']+$")
            .WithMessage("Imię i nazwisko może zawierać tylko litery, spacje, myślniki i apostrofy");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany")
            .EmailAddress().WithMessage("Nieprawidłowy format adresu email")
            .MaximumLength(255).WithMessage("Email nie może być dłuższy niż 255 znaków");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{6,14}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Nieprawidłowy format numeru telefonu (użyj formatu: +48123456789 lub 123456789)");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Wiadomość jest wymagana")
            .MinimumLength(10).WithMessage("Wiadomość musi mieć co najmniej 10 znaków")
            .MaximumLength(2000).WithMessage("Wiadomość nie może być dłuższa niż 2000 znaków");

        RuleFor(x => x.Subject)
            .MaximumLength(200).WithMessage("Temat nie może być dłuższy niż 200 znaków")
            .When(x => !string.IsNullOrEmpty(x.Subject));
    }
}
