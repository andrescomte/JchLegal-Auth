namespace AuthService.Domain.Models
{
    public class AuthResult
    {
        public long UserId { get; set; }
        public string JwtToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

