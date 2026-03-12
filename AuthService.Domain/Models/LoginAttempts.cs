using AuthService.Domain.SeedWork;
using System.Net;

namespace AuthService.Domain.Models;

public partial class LoginAttempts : Entity
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string? Email { get; set; }

    public bool Succeeded { get; set; }

    public IPAddress? Ip { get; set; }

    public string? UserAgent { get; set; }

    public DateTime AttemptedAt { get; set; }

    public virtual Users? User { get; set; }
}
