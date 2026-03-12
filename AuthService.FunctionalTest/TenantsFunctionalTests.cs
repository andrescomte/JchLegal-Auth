using System.Net;
using System.Text;
using System.Text.Json;
using AuthService.UnitTest.Mocks;

namespace AuthService.FunctionalTest
{
    public class TenantsFunctionalTests : IntegrationTestBase
    {
        private static StringContent Json(object body) =>
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        // ─── GET /api/Tenants ──────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_Returns_Ok_With_Tenants()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().GetAsync("/api/Tenants");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ─── GET /api/Tenants/{id} ─────────────────────────────────────────────

        [Fact]
        public async Task GetById_Returns_Ok_When_Tenant_Exists()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().GetAsync($"/api/Tenants/{TenantsRepositoryFake.ExistingId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_Returns_NotFound_When_Tenant_Does_Not_Exist()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().GetAsync("/api/Tenants/9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ─── POST /api/Tenants ─────────────────────────────────────────────────

        [Fact]
        public async Task Create_Returns_Created_With_New_Code()
        {
            using var server = CreateServer();
            var content = Json(new { Code = "newcode", Name = "New Tenant" });

            var response = await server.CreateClient().PostAsync("/api/Tenants", content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Create_Returns_Conflict_With_Existing_Code()
        {
            using var server = CreateServer();
            var content = Json(new { Code = TenantsRepositoryFake.ExistingCode, Name = "Duplicate" });

            var response = await server.CreateClient().PostAsync("/api/Tenants", content);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // ─── PUT /api/Tenants/{id} ─────────────────────────────────────────────

        [Fact]
        public async Task Update_Returns_Ok_When_Tenant_Exists()
        {
            using var server = CreateServer();
            var content = Json(new { Name = "Updated Name" });

            var response = await server.CreateClient().PutAsync($"/api/Tenants/{TenantsRepositoryFake.ExistingId}", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Update_Returns_NotFound_When_Tenant_Does_Not_Exist()
        {
            using var server = CreateServer();
            var content = Json(new { Name = "Ghost Tenant" });

            var response = await server.CreateClient().PutAsync("/api/Tenants/9999", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ─── DELETE /api/Tenants/{id} ──────────────────────────────────────────

        [Fact]
        public async Task Deactivate_Returns_NoContent_When_Tenant_Exists()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().DeleteAsync($"/api/Tenants/{TenantsRepositoryFake.ExistingId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Deactivate_Returns_NotFound_When_Tenant_Does_Not_Exist()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().DeleteAsync("/api/Tenants/9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
