using MediatR;

namespace AuthService.ApplicationApi.Application.Command.User
{
    public class DeactivateUserRequest : IRequest<bool>
    {
        public long Id { get; set; }
    }
}
