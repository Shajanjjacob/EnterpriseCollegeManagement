using EnterpriseCollegeManagement.IdentityService.Common;
using EnterpriseCollegeManagement.IdentityService.Exceptions;
using System.Net;
using System.Text.Json;

namespace EnterpriseCollegeManagement.IdentityService.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger, RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context,Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                NotFoundException => HttpStatusCode.NotFound,

                BadRequestException => HttpStatusCode.BadRequest,

                UnauthorizedException => HttpStatusCode.Unauthorized,

                ConflictException => HttpStatusCode.Conflict,

                _ => HttpStatusCode.InternalServerError

            };

            context.Response.StatusCode = (int)statusCode;

            var response = new ApiErrorResponse
            {
                Success = false,
                StatusCode = context.Response.StatusCode,
                Message = exception.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));

        }
    }
}
