using MediatR;

namespace AuthService.ApplicationApi.Application.Command.User
{
    public class UnlockUserRequest : IRequest<bool>
    {
        public long Id { get; set; }
    }
}
