namespace AuthService.Domain.Services
{
    public interface ITenantContext
    {
        int TenantId { get; }
        string TenantCode { get; }
        bool IsResolved { get; }
        void SetTenant(int id, string code);
    }
}
