using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AuthService.FunctionalTest;

class AutoAuthorizeMiddleware
{
    public const string IDENTITY_ID = "1";

    private readonly RequestDelegate _next;

    public AutoAuthorizeMiddleware(RequestDelegate rd)
    {
        _next = rd;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var identity = new ClaimsIdentity("TestAuth");

        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, IDENTITY_ID));
        identity.AddClaim(new Claim(ClaimTypes.Name, IDENTITY_ID));
        identity.AddClaim(new Claim(ClaimTypes.Role, "ADMIN"));
        identity.AddClaim(new Claim(ClaimTypes.Role, "USER"));
        identity.AddClaim(new Claim(ClaimTypes.Role, "SUPERADMIN"));

        httpContext.User = new System.Security.Principal.GenericPrincipal(identity, new[] { "ADMIN", "USER", "SUPERADMIN" });

        await _next.Invoke(httpContext);
    }
}
