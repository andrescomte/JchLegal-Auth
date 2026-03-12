using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Models;

public partial class Tokens : Entity
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public string TokenType { get; set; } = null!;

    public DateTime IssuedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool Used { get; set; }

    public virtual Users User { get; set; } = null!;
}
