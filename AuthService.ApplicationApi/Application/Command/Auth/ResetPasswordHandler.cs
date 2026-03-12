using MediatR;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;

namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordRequest, bool>
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogRepository _auditLogRepository;

        public ResetPasswordHandler(
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<bool> Handle(ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            var succeeded = await _userService.ResetPasswordAsync(request.ResetToken, request.NewPassword);

            if (succeeded)
            {
                await _auditLogRepository.LogAsync(new AuditLogs
                {
                    Action = AuditActions.PasswordResetCompleted,
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
