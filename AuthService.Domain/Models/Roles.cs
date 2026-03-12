using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Models;

public class Roles : Entity
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public long? HomeMenuId { get; set; }

    public virtual Menus? HomeMenu { get; set; }

    public virtual ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();

    public virtual ICollection<Menus> Menu { get; set; } = new List<Menus>();

    public static Roles Create(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("El código del rol es obligatorio.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del rol es obligatorio.");

        return new Roles
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim()
        };
    }
}
