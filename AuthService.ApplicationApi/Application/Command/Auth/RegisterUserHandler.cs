using MediatR;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;

namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserRequest, RegisterUserResponse?>
    {
        private readonly IUserService _userService;
        private readonly IRolesRepository _rolesRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogRepository _auditLogRepository;

        public RegisterUserHandler(
            IUserService userService,
            IRolesRepository rolesRepository,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository)
        {
            _userService = userService;
            _rolesRepository = rolesRepository;
            _httpContextAccessor = httpContextAccessor;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<RegisterUserResponse?> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
        {
            var role = await _rolesRepository.GetByCodeAsync(request.RoleCode.ToUpperInvariant());
            if (role == null)
                return null;

            try
            {
                var user = await _userService.RegisterAsync(
                    request.Username,
                    request.Email,
                    request.Password,
                    role.Id);

                await _auditLogRepository.LogAsync(new AuditLogs
                {
                    Action = AuditActions.UserRegistered,
                    UserId = user.Id,
                    Ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress,
                    UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString(),
                    Data = JsonSerializer.Serialize(new { email = request.Email, username = request.Username, roleCode = request.RoleCode }),
                    LoggedAt = DateTime.UtcNow
                });

                return new RegisterUserResponse
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email
                };
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
