using AuthService.Domain.SeedWork;
using System.Text.Json;

namespace AuthService.ApplicationApi.Middleware
{
    public class ExceptionMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (DomainException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                var body = JsonSerializer.Serialize(new { error = ex.Message });
                await context.Response.WriteAsync(body);
            }
        }
    }
}
