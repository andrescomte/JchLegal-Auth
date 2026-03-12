using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Models;

public partial class Users : Entity
{
    public long Id { get; set; }

    public int TenantId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual Tenants Tenant { get; set; } = null!;

    public virtual ICollection<AuditLogs> AuditLogs { get; set; } = new List<AuditLogs>();

    public virtual ICollection<LoginAttempts> LoginAttempts { get; set; } = new List<LoginAttempts>();

    public virtual ICollection<Tokens> Tokens { get; set; } = new List<Tokens>();

    public virtual UserPasswords? UserPasswords { get; set; }

    public virtual ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();

    public virtual ICollection<UserStatusHistory> UserStatusHistory { get; set; } = new List<UserStatusHistory>();
}
