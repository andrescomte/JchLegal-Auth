using AuthService.ApplicationApi.Filters;
using AuthService.ApplicationApi.Middleware;
using AuthService.ApplicationApi.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using AuthService.Domain.Repository;
using AuthService.Domain.SeedWork;
using AuthService.Domain.Services;
using AuthService.Domain.Helpers;
using AuthService.Infrastructure.Context;
using AuthService.Infrastructure.Logging;
using AuthService.Infrastructure.Repository;
using AuthService.Infrastructure.Services;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

var logger = NLog.LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
logger.Debug("init main");
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.ClearProviders();
    //builder.Logging.AddConsole();
    builder.Host.UseNLog();

    // Add services to the container.
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendPolicy", policy =>
        {
            policy.WithOrigins("http://localhost:8082")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
    builder.Services.AddControllers();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthService API", Version = "v1" });

        // Botón Authorize para JWT
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Ingresa tu JWT token. Ejemplo: eyJhbGci..."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });

        // Header X-Tenant-Id en todos los endpoints
        options.OperationFilter<TenantHeaderOperationFilter>();
    });

    var services = builder.Services;
    builder.Services.AddDbContext<AuthDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("MainConnectionString")));

    services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
        ?? throw new InvalidOperationException("JWT configuration is missing from appsettings.");

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var tenantIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
                if (!int.TryParse(tenantIdClaim, out var tenantId))
                    return [];
                var baseKey = Encoding.UTF8.GetBytes(jwtSettings.Key);
                using var hmac = new System.Security.Cryptography.HMACSHA256(baseKey);
                var derivedKey = hmac.ComputeHash(Encoding.UTF8.GetBytes(tenantId.ToString()));
                return [new SymmetricSecurityKey(derivedKey)];
            }
        };
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var tenantCtx = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
                var tokenTenantIdClaim = context.Principal?.FindFirst("tenant_id")?.Value;
                if (!int.TryParse(tokenTenantIdClaim, out var tokenTenantId) || tokenTenantId != tenantCtx.TenantId)
                    context.Fail("El token no pertenece al tenant actual.");
                return Task.CompletedTask;
            }
        };
    });
    services.AddAuthorization();
    services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblyContaining(typeof(Program));
    });

    services.AddHttpContextAccessor();

    services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddPolicy("authenticate", httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(15),
                    SegmentsPerWindow = 3,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

        options.AddPolicy("forgot-password", httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(15),
                    SegmentsPerWindow = 3,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
    });

    services.AddHealthChecks()
        .AddDbContextCheck<AuthDbContext>("database");

    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped<IRolesRepository, RolesRepository>();
    services.AddScoped<IUsersRepository, UsersRepository>();
    services.AddScoped<ITenantsRepository, TenantsRepository>();
    services.AddScoped<ITokensRepository, TokensRepository>();
    services.AddScoped<ILoginAttemptsRepository, LoginAttemptsRepository>();
    services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));

    services.AddScoped<IJwtTokenService, JwtTokenService>();
    services.AddScoped<IPasswordHasher, PasswordHasher>();
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ITenantContext, TenantContext>();
    services.AddScoped<TenantMiddleware>();
    services.AddScoped<ExceptionMiddleware>();

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

    //app.UseHttpsRedirection();

    app.UseCors("FrontendPolicy");

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<TenantMiddleware>();

    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health");
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}

public partial class Program { }
