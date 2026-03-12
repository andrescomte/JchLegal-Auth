using MediatR;

namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class ResetPasswordRequest : IRequest<bool>
    {
        public string ResetToken { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
