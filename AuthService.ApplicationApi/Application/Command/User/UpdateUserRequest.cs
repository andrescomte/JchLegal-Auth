using MediatR;

namespace AuthService.ApplicationApi.Application.Command.User
{
    public class UpdateUserRequest : IRequest<bool>
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
