using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Models;

public partial class UserStatus : Entity
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<UserStatusHistory> UserStatusHistory { get; set; } = new List<UserStatusHistory>();
}
