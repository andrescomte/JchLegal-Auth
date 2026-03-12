using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;
using AuthService.UnitTest.Mocks;
using AuthService.Infrastructure.Services;

namespace AuthService.FunctionalTest;

public class IntegrationTestBase
{
    private class TestApplication : WebApplicationFactory<Program>
    {
        public TestServer CreateServer()
        {
            return Server;
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(c =>
            {
                var directory = Path.GetDirectoryName(typeof(IntegrationTestBase).Assembly.Location)!;
                c.AddJsonFile(Path.Combine(directory, "appsettings.Test.json"), optional: false);
            });

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IStartupFilter, AuthStartupFilter>();
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

    public TestServer CreateServer()
    {
        var factory = new TestApplication();
        return factory.CreateServer();
    }

    private class AuthStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseMiddleware<AutoTenantMiddleware>();
                app.UseMiddleware<AutoAuthorizeMiddleware>();
                next(app);
            };
        }
    }
}
