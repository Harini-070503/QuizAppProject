using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace QuizAppProject.Middleware
{
    /// <summary>
    /// Global exception handling middleware.
    /// Catches all unhandled exceptions from the pipeline and returns
    /// a consistent JSON error response — no stack traces leak to the client.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Map exception types to HTTP status codes
            var (statusCode, title) = exception switch
            {
                KeyNotFoundException => (HttpStatusCode.NotFound, "Resource Not Found"),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
                InvalidOperationException => (HttpStatusCode.Conflict, "Operation Failed"),
                ArgumentException => (HttpStatusCode.BadRequest, "Bad Request"),
                NotSupportedException => (HttpStatusCode.BadRequest, "Not Supported"),
                TimeoutException => (HttpStatusCode.GatewayTimeout, "Request Timeout"),
                _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
            };

            // In development, expose the real exception message.
            // In production, show a generic message for 500s.
            var message = statusCode == HttpStatusCode.InternalServerError && !_env.IsDevelopment()
                ? "An unexpected error occurred. Please try again later."
                : exception.Message;

            var response = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = message,
                Instance = context.Request.Path
            };

            // Only include stack trace in Development
            if (_env.IsDevelopment() && exception.StackTrace is not null)
            {
                response.Extensions["stackTrace"] = exception.StackTrace;
                if (exception.InnerException is not null)
                    response.Extensions["innerException"] = exception.InnerException.Message;
            }

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/problem+json";

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }

    /// <summary>
    /// Extension method — register in Program.cs with one line.
    /// </summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
            => app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}