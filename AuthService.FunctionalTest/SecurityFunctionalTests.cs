using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;
using AuthService.Infrastructure.Services;
using AuthService.UnitTest.Mocks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.FunctionalTest;

public class SecurityFunctionalTests
{
    private const string TestJwtKey = "test-secret-key-for-functional-tests-minimum-32-chars";
    private const string TestIssuer = "AuthService";
    private const string TestAudience = "AuthServiceUsers";

    // ─── Factory ──────────────────────────────────────────────────────────────

    private class SecurityTestApplication : WebApplicationFactory<Program>
    {
        private readonly int _tenantId;
        private readonly string _tenantCode;

        public SecurityTestApplication(int tenantId, string tenantCode)
        {
            _tenantId = tenantId;
            _tenantCode = tenantCode;
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var tenantId = _tenantId;
            var tenantCode = _tenantCode;

            builder.ConfigureAppConfiguration(c =>
            {
                var directory = Path.GetDirectoryName(typeof(SecurityFunctionalTests).Assembly.Location)!;
                c.AddJsonFile(Path.Combine(directory, "appsettings.Test.json"), optional: false);
            });

            builder.ConfigureServices(services =>
            {
                // Fija el tenant en el contexto sin pasar por TenantMiddleware (que usa AuthDbContext)
                services.AddSingleton<IStartupFilter>(new FixedTenantStartupFilter(tenantId, tenantCode));

                // Fakes de repositorios para no depender de la DB
                services.AddScoped<IRolesRepository, RolesRepositoryMock>();
                services.AddScoped<IUsersRepository, UsersRepositoryFake>();
                services.AddScoped<ITenantsRepository, TenantsRepositoryFake>();
                services.AddScoped<IUserService, UserServiceFake>();
                services.AddScoped<IAuditLogRepository, AuditLogRepositoryFake>();
                services.AddScoped<ITenantContext, TenantContext>();
            });

            return base.CreateHost(builder);
        }
    }

    // ─── Middleware auxiliar ───────────────────────────────────────────────────

    // No hay AutoAuthorizeMiddleware: el JWT bearer real valida el token.
    // FixedTenantStartupFilter reemplaza a TenantMiddleware (que usa AuthDbContext)
    // fijando el tenant directamente, haciendo que TenantMiddleware lo saltee via IsResolved.
    private class FixedTenantStartupFilter : IStartupFilter
    {
        private readonly int _tenantId;
        private readonly string _tenantCode;

        public FixedTenantStartupFilter(int tenantId, string tenantCode)
        {
            _tenantId = tenantId;
            _tenantCode = tenantCode;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Use(async (ctx, nextMiddleware) =>
                {
                    ctx.RequestServices.GetRequiredService<ITenantContext>().SetTenant(_tenantId, _tenantCode);
                    await nextMiddleware(ctx);
                });
                next(app);
            };
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static TestServer CreateServer(int tenantId, string tenantCode)
        => new SecurityTestApplication(tenantId, tenantCode).Server;

    private static string GenerateJwt(int tenantId, string tenantCode)
    {
        var baseKey = Encoding.UTF8.GetBytes(TestJwtKey);
        using var hmac = new HMACSHA256(baseKey);
        var derivedKey = hmac.ComputeHash(Encoding.UTF8.GetBytes(tenantId.ToString()));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim("tenant", tenantCode),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim(ClaimTypes.Role, "ADMIN")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = TestIssuer,
            Audience = TestAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(derivedKey),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(tokenDescriptor));
    }

    // ─── Tests ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task JWT_With_Correct_Tenant_Returns_Ok()
    {
        var jwt = GenerateJwt(tenantId: 1, tenantCode: "default");
        using var server = CreateServer(tenantId: 1, tenantCode: "default");
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var response = await client.GetAsync("/api/roles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task JWT_From_Tenant1_Is_Rejected_When_Accessing_Tenant2()
    {
        // JWT firmado para tenant 1, pero el contexto del request es tenant 2
        var jwt = GenerateJwt(tenantId: 1, tenantCode: "default");
        using var server = CreateServer(tenantId: 2, tenantCode: "other");
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var response = await client.GetAsync("/api/roles");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task JWT_With_Tampered_Signature_Is_Rejected()
    {
        var jwt = GenerateJwt(tenantId: 1, tenantCode: "default");

        // Modifica los últimos caracteres de la firma
        var parts = jwt.Split('.');
        parts[2] = parts[2][..^4] + "XXXX";
        var tamperedJwt = string.Join('.', parts);

        using var server = CreateServer(tenantId: 1, tenantCode: "default");
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tamperedJwt);

        var response = await client.GetAsync("/api/roles");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Request_Without_JWT_Is_Rejected()
    {
        using var server = CreateServer(tenantId: 1, tenantCode: "default");

        var response = await server.CreateClient().GetAsync("/api/roles");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task JWT_Signed_With_Wrong_Tenant_Key_Is_Rejected()
    {
        // JWT generado con la clave derivada de tenant 2, pero claims dicen tenant_id=1
        // Simula un atacante que conoce el tenant_id pero no el secret correcto
        var baseKey = Encoding.UTF8.GetBytes(TestJwtKey);
        using var hmac = new HMACSHA256(baseKey);
        var wrongDerivedKey = hmac.ComputeHash(Encoding.UTF8.GetBytes("2")); // clave de tenant 2

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("tenant", "default"),
            new Claim("tenant_id", "1"),           // afirma ser tenant 1
            new Claim(ClaimTypes.Role, "ADMIN")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = TestIssuer,
            Audience = TestAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(wrongDerivedKey), // pero firmado con clave de tenant 2
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.WriteToken(handler.CreateToken(tokenDescriptor));

        using var server = CreateServer(tenantId: 1, tenantCode: "default");
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var response = await client.GetAsync("/api/roles");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
