using FluentValidation;
using AuthService.ApplicationApi.Application.Command.Tenant;

namespace AuthService.ApplicationApi.Validators
{
    public class UpdateTenantValidator : AbstractValidator<UpdateTenantRequest>
    {
        public UpdateTenantValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del tenant es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");
        }
    }
}
