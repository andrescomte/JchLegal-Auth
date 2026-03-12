using MediatR;
using AuthService.Domain.Services;

namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, AuthenticateResponse?>
    {
        private readonly IUserService _userService;

        public RefreshTokenHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<AuthenticateResponse?> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.RefreshTokenAsync(request.RefreshToken);
            if (result == null)
            {
                return null;
            }

            return new AuthenticateResponse { Token = result.JwtToken, RefreshToken = result.RefreshToken };
        }
    }
}
