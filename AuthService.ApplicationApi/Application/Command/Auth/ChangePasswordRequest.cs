using MediatR;

namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class ChangePasswordRequest : IRequest<bool>
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
