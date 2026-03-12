using AuthService.Domain.Services;

namespace AuthService.Infrastructure.Services
{
    public class TenantContext : ITenantContext
    {
        public int TenantId { get; private set; }
        public string TenantCode { get; private set; } = string.Empty;
        public bool IsResolved { get; private set; }

        public void SetTenant(int id, string code)
        {
            TenantId = id;
            TenantCode = code;
            IsResolved = true;
        }
    }
}
