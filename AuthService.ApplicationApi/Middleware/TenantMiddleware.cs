using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Services;
using AuthService.Infrastructure.Context;

namespace AuthService.ApplicationApi.Middleware
{
    public class TenantMiddleware : IMiddleware
    {
        private readonly AuthDbContext _context;
        private readonly ITenantContext _tenantContext;

        public TenantMiddleware(AuthDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/health") ||
                _tenantContext.IsResolved)
            {
                await next(context);
                return;
            }

            var tenantCode = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

            if (string.IsNullOrEmpty(tenantCode))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Missing X-Tenant-Id header.");
                return;
            }

            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Code == tenantCode && t.IsActive);

            if (tenant == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or inactive tenant.");
                return;
            }

            _tenantContext.SetTenant(tenant.Id, tenant.Code);

            await next(context);
        }
    }
}
