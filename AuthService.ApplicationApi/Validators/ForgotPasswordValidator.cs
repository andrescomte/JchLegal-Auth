using FluentValidation;
using AuthService.ApplicationApi.Application.Command.Auth;

namespace AuthService.ApplicationApi.Validators
{
    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es obligatorio.")
                .EmailAddress().WithMessage("El formato del email no es válido.");
        }
    }
}
