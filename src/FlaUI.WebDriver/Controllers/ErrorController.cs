using FlaUI.WebDriver.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FlaUI.WebDriver.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;


        public ErrorController(ILogger<ErrorController> logger) {
            _logger = logger;
        }

        [Route("/error")]
        public IActionResult HandleError() {
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;
            
            _logger.LogError(exceptionHandlerFeature.Error, "Returning WebDriver error response with error code 'unknown error'");

            return new ObjectResult(new ResponseWithValue<ErrorResponse>(new ErrorResponse { 
                ErrorCode = "unknown error", 
                Message = exceptionHandlerFeature.Error.Message, 
                StackTrace = exceptionHandlerFeature.Error.StackTrace ?? "" }))
            {
                StatusCode = 500
            };
        }
    }
}
