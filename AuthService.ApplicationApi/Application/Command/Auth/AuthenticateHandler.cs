using MediatR;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;

namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class AuthenticateHandler : IRequestHandler<AuthenticateRequest, AuthenticateResponse?>
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogRepository _auditLogRepository;

        public AuthenticateHandler(
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<AuthenticateResponse?> Handle(AuthenticateRequest request, CancellationToken cancellationToken)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

            var result = await _userService.LoginAsync(request.Email, request.Password, ip, userAgent);

            await _auditLogRepository.LogAsync(new AuditLogs
            {
                Action = result != null ? AuditActions.LoginSuccess : AuditActions.LoginFailed,
                UserId = result?.UserId,
                Ip = ip,
                UserAgent = userAgent,
                Data = JsonSerializer.Serialize(new { email = request.Email }),
                LoggedAt = DateTime.UtcNow
            });

            if (result == null)
                return null;

            return new AuthenticateResponse { Token = result.JwtToken, RefreshToken = result.RefreshToken };
        }
    }
}
