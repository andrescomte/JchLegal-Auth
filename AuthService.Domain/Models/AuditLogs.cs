using AuthService.Domain.SeedWork;
using System.Net;

namespace AuthService.Domain.Models;

public partial class AuditLogs : Entity
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string Action { get; set; } = null!;

    public IPAddress? Ip { get; set; }

    public string? UserAgent { get; set; }

    public string? Data { get; set; }

    public DateTime LoggedAt { get; set; }

    public virtual Users? User { get; set; }
}
