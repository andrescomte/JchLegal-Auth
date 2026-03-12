using MediatR;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;

namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordRequest, ForgotPasswordResponse?>
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogRepository _auditLogRepository;

        public ForgotPasswordHandler(
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<ForgotPasswordResponse?> Handle(ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.ForgotPasswordAsync(request.Email);
            if (result == null)
                return null;

            var (rawToken, expiresAt) = result.Value;

            await _auditLogRepository.LogAsync(new AuditLogs
            {
                Action = AuditActions.PasswordResetRequested,
                Ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress,
                UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString(),
                Data = JsonSerializer.Serialize(new { email = request.Email }),
                LoggedAt = DateTime.UtcNow
            });

            return new ForgotPasswordResponse
            {
                ResetToken = rawToken,
                ExpiresAt = expiresAt
            };
        }
    }
}
