using AuthService.ApplicationApi.Application.Command.Tenant;
using AuthService.ApplicationApi.Application.Query.TenantsQuery;
using AuthService.UnitTest.Mocks;

namespace AuthService.UnitTest.Handlers
{
    public class TenantHandlerTests
    {
        private readonly TenantsRepositoryFake _repo = new();

        // ─── GetTenantsListHandler ─────────────────────────────────────────────

        [Fact]
        public async Task GetAll_Returns_All_Tenants()
        {
            var handler = new GetTenantsListHandler(_repo);

            var result = await handler.Handle(new GetTenantsListRequest(), CancellationToken.None);

            Assert.NotEmpty(result.Data);
        }

        // ─── GetTenantByIdHandler ──────────────────────────────────────────────

        [Fact]
        public async Task GetById_Returns_Tenant_When_Exists()
        {
            var handler = new GetTenantByIdHandler(_repo);

            var result = await handler.Handle(new GetTenantByIdRequest { Id = TenantsRepositoryFake.ExistingId }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(TenantsRepositoryFake.ExistingCode, result.Code);
        }

        [Fact]
        public async Task GetById_Returns_Null_When_Not_Found()
        {
            var handler = new GetTenantByIdHandler(_repo);

            var result = await handler.Handle(new GetTenantByIdRequest { Id = 9999 }, CancellationToken.None);

            Assert.Null(result);
        }

        // ─── CreateTenantHandler ───────────────────────────────────────────────

        [Fact]
        public async Task Create_Returns_Tenant_With_New_Code()
        {
            var handler = new CreateTenantHandler(_repo);

            var result = await handler.Handle(new CreateTenantRequest { Code = "newcode", Name = "New Tenant" }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("NEWCODE", result.Code);
            Assert.Equal("New Tenant", result.Name);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task Create_Returns_Null_When_Code_Already_Exists()
        {
            var handler = new CreateTenantHandler(_repo);

            var result = await handler.Handle(new CreateTenantRequest { Code = TenantsRepositoryFake.ExistingCode, Name = "Duplicate" }, CancellationToken.None);

            Assert.Null(result);
        }

        // ─── UpdateTenantHandler ───────────────────────────────────────────────

        [Fact]
        public async Task Update_Returns_Updated_Tenant_When_Exists()
        {
            var handler = new UpdateTenantHandler(_repo);

            var result = await handler.Handle(new UpdateTenantRequest { Id = TenantsRepositoryFake.ExistingId, Name = "Updated" }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Updated", result.Name);
        }

        [Fact]
        public async Task Update_Returns_Null_When_Not_Found()
        {
            var handler = new UpdateTenantHandler(_repo);

            var result = await handler.Handle(new UpdateTenantRequest { Id = 9999, Name = "Ghost" }, CancellationToken.None);

            Assert.Null(result);
        }

        // ─── DeactivateTenantHandler ───────────────────────────────────────────

        [Fact]
        public async Task Deactivate_Returns_True_When_Tenant_Exists()
        {
            var handler = new DeactivateTenantHandler(_repo);

            var result = await handler.Handle(new DeactivateTenantRequest { Id = TenantsRepositoryFake.ExistingId }, CancellationToken.None);

            Assert.True(result);
        }

        [Fact]
        public async Task Deactivate_Returns_False_When_Tenant_Not_Found()
        {
            var handler = new DeactivateTenantHandler(_repo);

            var result = await handler.Handle(new DeactivateTenantRequest { Id = 9999 }, CancellationToken.None);

            Assert.False(result);
        }
    }
}
