using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Models;

public partial class UserRoles : Entity
{
    public long UserId { get; set; }

    public int RoleId { get; set; }

    public bool IsPrimary { get; set; }

    public virtual Roles Role { get; set; } = null!;

    public virtual Users User { get; set; } = null!;
}
