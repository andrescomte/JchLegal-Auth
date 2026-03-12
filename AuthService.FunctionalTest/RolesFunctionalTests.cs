using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthService.FunctionalTest
{
    public class RolesFunctionalTests : IntegrationTestBase
    {
        private static StringContent Json(object body) =>
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        // ─── GetAll ───────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_Returns_Ok_With_Roles()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().GetAsync("/api/Roles");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ─── GetById ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetById_Returns_Ok_When_Role_Exists()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().GetAsync("/api/Roles/1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_Returns_NotFound_When_Role_Does_Not_Exist()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().GetAsync("/api/Roles/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ─── CreateRole ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateRole_Returns_Created_With_New_Code()
        {
            using var server = CreateServer();
            var content = Json(new { Code = "SUPERVISOR", Name = "Supervisor" });

            var response = await server.CreateClient().PostAsync("/api/Roles", content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateRole_Returns_Conflict_When_Code_Already_Exists()
        {
            using var server = CreateServer();
            var content = Json(new { Code = "ADMIN", Name = "Administrador" });

            var response = await server.CreateClient().PostAsync("/api/Roles", content);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // ─── UpdateRole ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateRole_Returns_Ok_When_Role_Exists()
        {
            using var server = CreateServer();
            var content = Json(new { Name = "Administrador General" });

            var response = await server.CreateClient().PutAsync("/api/Roles/1", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdateRole_Returns_NotFound_When_Role_Does_Not_Exist()
        {
            using var server = CreateServer();
            var content = Json(new { Name = "No existe" });

            var response = await server.CreateClient().PutAsync("/api/Roles/999", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ─── AssignRole ───────────────────────────────────────────────────────

        [Fact]
        public async Task AssignRole_Returns_Ok_With_Valid_Data()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().PostAsync("/api/Roles/1/users/1", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ─── RevokeRole ───────────────────────────────────────────────────────

        [Fact]
        public async Task RevokeRole_Returns_NoContent_With_Valid_Data()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().DeleteAsync("/api/Roles/1/users/1");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
