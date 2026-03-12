using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AuthService.ApplicationApi.Application.Command.Auth;
using AuthService.ApplicationApi.Application.Command.User;
using AuthService.ApplicationApi.Application.Query.UsersQuery;
using AuthService.Domain.SeedWork;
using System.Net;

namespace AuthService.ApplicationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAppLogger<UserController> _logger;

        public UserController(IMediator mediator, IAppLogger<UserController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegisterUserResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            _logger.LogInformation("Register endpoint called");
            var result = await _mediator.Send(request);
            if (result == null)
                return Conflict("Email already registered or role not found.");
            return CreatedAtAction(nameof(Register), new { userId = result.UserId }, result);
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        //[EnableRateLimiting("authenticate")]
        [ProducesResponseType(typeof(AuthenticateResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
        {
            _logger.LogInformation("Authenticate endpoint called");
            var result = await _mediator.Send(request);
            if (result == null)
            {
                _logger.LogWarning("Authentication failed");
                return Unauthorized(new
                {
                    status = 401,
                    title = "Credenciales incorrectas",
                    detail = "El email o contraseña son incorrectos"
                });
            }
            return Ok(result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticateResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            _logger.LogInformation("Refresh token endpoint called");
            var result = await _mediator.Send(request);
            if (result == null)
            {
                _logger.LogWarning("Refresh token failed");
                return Unauthorized();
            }
            return Ok(result);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            _logger.LogInformation("Logout endpoint called");
            var success = await _mediator.Send(request);
            if (!success)
                return BadRequest("Invalid or already revoked token.");
            return Ok();
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(GetUsersListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _mediator.Send(new GetUserByIdRequest { Id = userId });
            if (result == null) return Unauthorized();
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(PagedResponse<GetUsersListResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetUsersListRequest request)
        {
            _logger.LogInformation("GetAll users endpoint called");
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpGet("{id:long}")]
        [Authorize]
        [ProducesResponseType(typeof(GetUsersListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById(long id)
        {
            _logger.LogInformation("GetById user endpoint called");
            var result = await _mediator.Send(new GetUserByIdRequest { Id = id });
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id:long}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UpdateUserRequest request)
        {
            _logger.LogInformation("UpdateUser endpoint called");
            request.Id = id;
            var success = await _mediator.Send(request);
            if (!success) return NotFound();
            return Ok();
        }

        [HttpPut("{id:long}/unlock")]
        [Authorize(Roles = "SUPERADMIN")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> UnlockUser(long id)
        {
            _logger.LogInformation("UnlockUser endpoint called");
            var success = await _mediator.Send(new UnlockUserRequest { Id = id });
            if (!success) return NotFound();
            return Ok();
        }

        [HttpDelete("{id:long}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeactivateUser(long id)
        {
            _logger.LogInformation("DeactivateUser endpoint called");
            var success = await _mediator.Send(new DeactivateUserRequest { Id = id });
            if (!success) return NotFound();
            return Ok();
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        //[EnableRateLimiting("forgot-password")]
        [ProducesResponseType(typeof(ForgotPasswordResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            _logger.LogInformation("ForgotPassword endpoint called");
            var result = await _mediator.Send(request);
            if (result == null)
                return NotFound("Email not found.");
            return Ok(result);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            _logger.LogInformation("ResetPassword endpoint called");
            var success = await _mediator.Send(request);
            if (!success)
                return BadRequest("Invalid, expired, or already used reset token.");
            return Ok();
        }

        [HttpPut("password")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            _logger.LogInformation("ChangePassword endpoint called");
            var success = await _mediator.Send(request);
            if (!success)
                return BadRequest("Current password is incorrect or user not found.");
            return Ok();
        }
    }
}
