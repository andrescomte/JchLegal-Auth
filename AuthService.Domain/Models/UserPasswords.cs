using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Models;

public partial class UserPasswords :  Entity
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int FailedAttempts { get; set; }

    public virtual Users User { get; set; } = null!;
}
