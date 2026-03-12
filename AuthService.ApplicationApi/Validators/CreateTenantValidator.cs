using FluentValidation;
using AuthService.ApplicationApi.Application.Command.Tenant;

namespace AuthService.ApplicationApi.Validators
{
    public class CreateTenantValidator : AbstractValidator<CreateTenantRequest>
    {
        public CreateTenantValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("El código del tenant es obligatorio.")
                .MaximumLength(20).WithMessage("El código no puede superar 20 caracteres.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del tenant es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");
        }
    }
}
