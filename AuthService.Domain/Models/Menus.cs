using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Models;

public partial class Menus : Entity
{
    public long Id { get; set; }

    public long? ParentId { get; set; }

    public string Path { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? IconClass { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Menus> InverseParent { get; set; } = new List<Menus>();

    public virtual Menus? Parent { get; set; }

    public virtual ICollection<Roles> Roles { get; set; } = new List<Roles>();

    public virtual ICollection<Roles> Role { get; set; } = new List<Roles>();
}
