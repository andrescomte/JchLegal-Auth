using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AuthService.ApplicationApi.Application.Command.Role;
using AuthService.ApplicationApi.Application.Query.RolesQuery;
using AuthService.Domain.SeedWork;
using System.Net;

namespace AuthService.ApplicationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAppLogger<RolesController> _logger;

        public RolesController(IMediator mediator, IAppLogger<RolesController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(typeof(RolesListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetAll([FromQuery] RolesListRequest request)
        {
            _logger.LogInformation("GetAll roles endpoint called");
            var result = await _mediator.Send(request);
            if (result == null)
                return NotFound("No roles found.");
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(GetRoleByIdResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("GetById role endpoint called");
            var result = await _mediator.Send(new GetRoleByIdRequest { Id = id });
            if (result == null)
                return NotFound($"Role {id} not found.");
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(UpdateRoleResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
        {
            _logger.LogInformation("UpdateRole endpoint called");
            request.Id = id;
            var result = await _mediator.Send(request);
            if (result == null)
                return NotFound($"Role {id} not found.");
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(CreateRoleResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            _logger.LogInformation("CreateRole endpoint called");
            var result = await _mediator.Send(request);
            if (result == null)
                return Conflict("A role with this code already exists.");
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("{roleId:int}/users/{userId:long}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> AssignRole(int roleId, long userId)
        {
            _logger.LogInformation("AssignRole endpoint called");
            await _mediator.Send(new AssignRoleRequest { UserId = userId, RoleId = roleId });
            return Ok();
        }

        [HttpDelete("{roleId:int}/users/{userId:long}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> RevokeRole(int roleId, long userId)
        {
            _logger.LogInformation("RevokeRole endpoint called");
            await _mediator.Send(new RevokeRoleRequest { UserId = userId, RoleId = roleId });
            return NoContent();
        }
    }
}
