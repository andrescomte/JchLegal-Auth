namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class AuthenticateResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
