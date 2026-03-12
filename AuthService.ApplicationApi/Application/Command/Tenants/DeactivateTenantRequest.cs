using MediatR;

namespace AuthService.ApplicationApi.Application.Command.Tenant
{
    public class DeactivateTenantRequest : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
