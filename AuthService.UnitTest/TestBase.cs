using Microsoft.Extensions.DependencyInjection;
using AuthService.Domain.Repository;
using AuthService.UnitTest.Mocks;

namespace AuthService.UnitTest
{
    public class TestBase
    {
        protected readonly ServiceProvider ServiceProvider;

        public TestBase()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IRolesRepository, RolesRepositoryMock>();
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
