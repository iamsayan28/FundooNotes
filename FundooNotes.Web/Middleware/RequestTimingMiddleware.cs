using System.Diagnostics;

namespace FundooNotes.Web.Middleware
{
    public class RequestTimingMiddleware
    {
        private readonly ILogger<RequestTimingMiddleware> _logger;
        private readonly RequestDelegate _next;
        public RequestTimingMiddleware(ILogger<RequestTimingMiddleware> logger, RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
            }

            _logger.LogInformation(
                "Http {Method} {Path} responded with {StatusCode} in {ElapsedTime} ms. Trace Id: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                context.TraceIdentifier
                );
        }
    }
}
