using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;

namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordRequest, bool>
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogRepository _auditLogRepository;

        public ChangePasswordHandler(
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<bool> Handle(ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdClaim, out var userId))
                return false;

            var succeeded = await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

            if (succeeded)
            {
                await _auditLogRepository.LogAsync(new AuditLogs
                {
                    Action = AuditActions.PasswordChanged,
                    UserId = userId,
                    Ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress,
                    UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString(),
                    Data = JsonSerializer.Serialize(new { success = true }),
                    LoggedAt = DateTime.UtcNow
                });
            }

            return succeeded;
        }
    }
}
