using AuthService.Domain.Services;
using Microsoft.AspNetCore.Http;

namespace AuthService.FunctionalTest;

class AutoTenantMiddleware
{
    private readonly RequestDelegate _next;

    public AutoTenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ITenantContext tenantContext)
    {
        tenantContext.SetTenant(1, "default");
        await _next(context);
    }
}
