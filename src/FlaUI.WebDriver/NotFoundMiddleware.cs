using FlaUI.WebDriver.Models;

namespace FlaUI.WebDriver
{
    public class NotFoundMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public NotFoundMiddleware(RequestDelegate next, ILogger<NotFoundMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next(httpContext);
            if (!httpContext.Response.HasStarted)
            {
                if (httpContext.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    _logger.LogError("Unknown endpoint {Path}", SanitizeForLog(httpContext.Request.Path.ToString()));
                    await httpContext.Response.WriteAsJsonAsync(new ResponseWithValue<ErrorResponse>(new ErrorResponse
                    {
                        ErrorCode = "unknown command",
                        Message = "Unknown command"
                    }));
                }
                else if (httpContext.Response.StatusCode == StatusCodes.Status405MethodNotAllowed)
                {
                    _logger.LogError("Unknown method {Method} for endpoint {Path}", SanitizeForLog(httpContext.Request.Method), SanitizeForLog(httpContext.Request.Path.ToString()));
                    await httpContext.Response.WriteAsJsonAsync(new ResponseWithValue<ErrorResponse>(new ErrorResponse
                    {
                        ErrorCode = "unknown method",
                        Message = "Unknown method for this endpoint"
                    }));
                }
            }
        }

        private static string SanitizeForLog(string str)
        {
            return str.Replace("\r", "").Replace("\n", "");
        }
    }
}
