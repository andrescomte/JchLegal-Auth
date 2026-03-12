using FluentValidation;
using AuthService.ApplicationApi.Application.Command.Role;

namespace AuthService.ApplicationApi.Validators
{
    public class CreateRoleValidator : AbstractValidator<CreateRoleRequest>
    {
        public CreateRoleValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("El código del rol es obligatorio.")
                .MaximumLength(20).WithMessage("El código no puede superar 20 caracteres.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del rol es obligatorio.")
                .MaximumLength(50).WithMessage("El nombre no puede superar 50 caracteres.");
        }
    }
}
