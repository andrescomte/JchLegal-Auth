using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Models;

public class Tenants : Entity
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();

    public static Tenants Create(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("El código del tenant es obligatorio.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del tenant es obligatorio.");

        return new Tenants
        {
            Code = code.Trim().ToUpper(),
            Name = name.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
