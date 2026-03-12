using System.Net;
using System.Text;
using System.Text.Json;
using AuthService.UnitTest.Mocks;

namespace AuthService.FunctionalTest
{
    public class UserFunctionalTests : IntegrationTestBase
    {
        private static StringContent Json(object body) =>
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        // ─── Me ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Me_Returns_Ok_With_Current_User()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().GetAsync("/api/User/me");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ─── Authenticate ─────────────────────────────────────────────────────

        [Fact]
        public async Task Authenticate_Returns_Ok_With_Valid_Credentials()
        {
            using var server = CreateServer();
            var content = Json(new { Email = UserServiceFake.ValidEmail, Password = UserServiceFake.ValidPassword });

            var response = await server.CreateClient().PostAsync("/api/User/authenticate", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Authenticate_Returns_Unauthorized_With_Invalid_Credentials()
        {
            using var server = CreateServer();
            var content = Json(new { Email = "wrong@test.com", Password = "wrong" });

            var response = await server.CreateClient().PostAsync("/api/User/authenticate", content);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ─── Register ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Register_Returns_Created_With_New_Email()
        {
            using var server = CreateServer();
            var content = Json(new { Username = "newuser", Email = "brand-new@test.com", Password = "Password1", RoleCode = "USER" });

            var response = await server.CreateClient().PostAsync("/api/User/register", content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_Returns_Conflict_With_Existing_Email()
        {
            using var server = CreateServer();
            var content = Json(new { Username = "existing", Email = UserServiceFake.ValidEmail, Password = "Password1", RoleCode = "USER" });

            var response = await server.CreateClient().PostAsync("/api/User/register", content);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // ─── Refresh ──────────────────────────────────────────────────────────

        [Fact]
        public async Task Refresh_Returns_Ok_With_Valid_Token()
        {
            using var server = CreateServer();
            var content = Json(new { RefreshToken = UserServiceFake.ValidRefreshToken });

            var response = await server.CreateClient().PostAsync("/api/User/refresh", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Refresh_Returns_Unauthorized_With_Invalid_Token()
        {
            using var server = CreateServer();
            var content = Json(new { RefreshToken = "invalid-token" });

            var response = await server.CreateClient().PostAsync("/api/User/refresh", content);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ─── Logout ───────────────────────────────────────────────────────────

        [Fact]
        public async Task Logout_Returns_Ok_With_Valid_Token()
        {
            using var server = CreateServer();
            var content = Json(new { RefreshToken = UserServiceFake.ValidRefreshToken });

            var response = await server.CreateClient().PostAsync("/api/User/logout", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Logout_Returns_BadRequest_With_Invalid_Token()
        {
            using var server = CreateServer();
            var content = Json(new { RefreshToken = "invalid-token" });

            var response = await server.CreateClient().PostAsync("/api/User/logout", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ─── ForgotPassword ───────────────────────────────────────────────────

        [Fact]
        public async Task ForgotPassword_Returns_Ok_When_Email_Found()
        {
            using var server = CreateServer();
            var content = Json(new { Email = UserServiceFake.ValidEmail });

            var response = await server.CreateClient().PostAsync("/api/User/forgot-password", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ForgotPassword_Returns_NotFound_When_Email_Not_Found()
        {
            using var server = CreateServer();
            var content = Json(new { Email = "unknown@test.com" });

            var response = await server.CreateClient().PostAsync("/api/User/forgot-password", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ─── ResetPassword ────────────────────────────────────────────────────

        [Fact]
        public async Task ResetPassword_Returns_Ok_With_Valid_Token()
        {
            using var server = CreateServer();
            var content = Json(new { ResetToken = "fake-reset-token", NewPassword = "NewPass1!" });

            var response = await server.CreateClient().PostAsync("/api/User/reset-password", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ResetPassword_Returns_BadRequest_With_Invalid_Token()
        {
            using var server = CreateServer();
            var content = Json(new { ResetToken = "invalid-token", NewPassword = "NewPass1!" });

            var response = await server.CreateClient().PostAsync("/api/User/reset-password", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ─── ChangePassword ───────────────────────────────────────────────────

        [Fact]
        public async Task ChangePassword_Returns_Ok_With_Valid_Current_Password()
        {
            using var server = CreateServer();
            var content = Json(new { CurrentPassword = UserServiceFake.ValidPassword, NewPassword = "NewPass1!" });

            var response = await server.CreateClient().PutAsync("/api/User/password", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_Returns_BadRequest_With_Wrong_Current_Password()
        {
            using var server = CreateServer();
            var content = Json(new { CurrentPassword = "wrong-password", NewPassword = "NewPass1!" });

            var response = await server.CreateClient().PutAsync("/api/User/password", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ─── CRUD Usuarios ────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_Returns_Ok()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().GetAsync("/api/User");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_Returns_Ok_When_User_Exists()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().GetAsync("/api/User/1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_Returns_NotFound_When_User_Does_Not_Exist()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().GetAsync("/api/User/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_Returns_Ok_When_User_Exists()
        {
            using var server = CreateServer();
            var content = Json(new { Username = "updated", Email = "updated@test.com" });

            var response = await server.CreateClient().PutAsync("/api/User/1", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_Returns_NotFound_When_User_Does_Not_Exist()
        {
            using var server = CreateServer();
            var content = Json(new { Username = "nope", Email = "nope@test.com" });

            var response = await server.CreateClient().PutAsync("/api/User/999", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeactivateUser_Returns_Ok_When_User_Exists()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().DeleteAsync("/api/User/1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeactivateUser_Returns_NotFound_When_User_Does_Not_Exist()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().DeleteAsync("/api/User/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ─── UnlockUser ───────────────────────────────────────────────────────

        [Fact]
        public async Task UnlockUser_Returns_Ok_When_User_Exists()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().PutAsync("/api/User/1/unlock", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UnlockUser_Returns_NotFound_When_User_Does_Not_Exist()
        {
            using var server = CreateServer();

            var response = await server.CreateClient().PutAsync("/api/User/999/unlock", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
