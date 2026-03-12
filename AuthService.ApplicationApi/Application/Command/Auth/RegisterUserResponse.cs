namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class RegisterUserResponse
    {
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
