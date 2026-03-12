using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Models;

public partial class UserStatusHistory : Entity
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public int StatusId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public virtual UserStatus Status { get; set; } = null!;

    public virtual Users User { get; set; } = null!;
}
