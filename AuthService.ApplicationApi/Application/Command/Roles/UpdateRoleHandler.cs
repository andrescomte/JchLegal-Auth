using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;

namespace AuthService.ApplicationApi.Application.Command.Role
{
    public class UpdateRoleHandler : IRequestHandler<UpdateRoleRequest, UpdateRoleResponse?>
    {
        private readonly IRolesRepository _rolesRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogRepository _auditLogRepository;

        public UpdateRoleHandler(
            IRolesRepository rolesRepository,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository)
        {
            _rolesRepository = rolesRepository;
            _httpContextAccessor = httpContextAccessor;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<UpdateRoleResponse?> Handle(UpdateRoleRequest request, CancellationToken cancellationToken)
        {
            var updated = await _rolesRepository.UpdateRoleAsync(request.Id, request.Name);
            if (updated == null)
                return null;

            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            long.TryParse(userIdClaim, out var currentUserId);

            await _auditLogRepository.LogAsync(new AuditLogs
            {
                Action = AuditActions.RoleUpdated,
                UserId = currentUserId > 0 ? currentUserId : null,
                Ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress,
                UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString(),
                Data = JsonSerializer.Serialize(new { roleId = updated.Id, roleCode = updated.Code, newName = updated.Name }),
                LoggedAt = DateTime.UtcNow
            });

            return new UpdateRoleResponse { Id = updated.Id, Code = updated.Code, Name = updated.Name };
        }
    }
}
