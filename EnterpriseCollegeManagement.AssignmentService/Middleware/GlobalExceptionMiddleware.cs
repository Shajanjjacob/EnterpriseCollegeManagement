using EnterpriseCollegeManagement.AssignmentService.Common;
using EnterpriseCollegeManagement.AssignmentService.Exceptions;
using System.Net;
using System.Text.Json;

namespace EnterpriseCollegeManagement.AssignmentService.Middleware
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

        public static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statuscode = exception switch
            {
                NotFoundException => HttpStatusCode.NotFound,
                BadRequestException => HttpStatusCode.BadRequest,
                UnauthorizedException => HttpStatusCode.Unauthorized,

                ConflictException => HttpStatusCode.Conflict,
                _ => HttpStatusCode.InternalServerError

            };

            context.Response.StatusCode = (int)statuscode;
            var response = new ApiErrorResponse
            {
                Success = true,
                StatusCode = context.Response.StatusCode,
                Message = exception.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

    }
}
