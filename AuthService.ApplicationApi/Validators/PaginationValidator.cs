using FluentValidation;
using AuthService.ApplicationApi.Application.Query.UsersQuery;
using AuthService.ApplicationApi.Application.Query.TenantsQuery;
using AuthService.ApplicationApi.Application.Query.RolesQuery;

namespace AuthService.ApplicationApi.Validators
{
    public class GetUsersListValidator : AbstractValidator<GetUsersListRequest>
    {
        public GetUsersListValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("La página debe ser mayor o igual a 1.");
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100.");
        }
    }

    public class GetTenantsListValidator : AbstractValidator<GetTenantsListRequest>
    {
        public GetTenantsListValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("La página debe ser mayor o igual a 1.");
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100.");
        }
    }

    public class RolesListValidator : AbstractValidator<RolesListRequest>
    {
        public RolesListValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("La página debe ser mayor o igual a 1.");
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100.");
        }
    }
}
