namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class ForgotPasswordResponse
    {
        public string ResetToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
