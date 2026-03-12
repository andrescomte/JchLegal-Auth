using FluentValidation;
using AuthService.ApplicationApi.Application.Command.Role;

namespace AuthService.ApplicationApi.Validators
{
    public class UpdateRoleValidator : AbstractValidator<UpdateRoleRequest>
    {
        public UpdateRoleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del rol es obligatorio.")
                .MaximumLength(50).WithMessage("El nombre no puede superar 50 caracteres.");
        }
    }
}
