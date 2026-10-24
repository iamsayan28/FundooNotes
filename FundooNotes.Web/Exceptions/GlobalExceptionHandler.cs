using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FundooNotes.Web.Exceptions
{
    public class GlobalExceptionHandler :  IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception, 
                "An unhandled exception occured while processing {HttpMethod} {RequestPath}}", 
                httpContext.Request.Method, 
                httpContext.Request.Path, 
                httpContext.TraceIdentifier);

            var statusCode = exception switch
            {
                KeyNotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode switch
                {
                    404 => "Resource not found",
                    403 => "Forbidden",
                    400 => "Invalid request",
                    _ => "An unexpected error occurred"
                },
                Detail = statusCode == 500 ? "An internal server error occured." : exception.Message
            };

            errorResponse.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;

        }
    }
}
