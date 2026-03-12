using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthService.ApplicationApi.Application.Command.Tenant;
using AuthService.ApplicationApi.Application.Query.TenantsQuery;
using AuthService.Domain.SeedWork;
using System.Net;

namespace AuthService.ApplicationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TenantsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAppLogger<TenantsController> _logger;

        public TenantsController(IMediator mediator, IAppLogger<TenantsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<GetTenantsListResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetTenantsListRequest request)
        {
            _logger.LogInformation("GetAll tenants endpoint called");
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(GetTenantsListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("GetById tenant endpoint called");
            var result = await _mediator.Send(new GetTenantByIdRequest { Id = id });
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(GetTenantsListResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
        {
            _logger.LogInformation("Create tenant endpoint called");
            var result = await _mediator.Send(request);
            if (result == null)
                return Conflict("Tenant code already exists.");
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(GetTenantsListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTenantRequest request)
        {
            _logger.LogInformation("Update tenant endpoint called");
            request.Id = id;
            var result = await _mediator.Send(request);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Deactivate(int id)
        {
            _logger.LogInformation("Deactivate tenant endpoint called");
            var success = await _mediator.Send(new DeactivateTenantRequest { Id = id });
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
