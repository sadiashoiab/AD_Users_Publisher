using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Azure_AD_Users_Extract.Filters
{
    public class ExceptionActionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger _logger;

        public ExceptionActionFilter(ILogger<ExceptionActionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unexpected error");

            context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.HttpContext.Response.ContentType = "application/json";
            context.Result = new JsonResult(context.Exception);
            context.ExceptionHandled = true;

            base.OnException(context);
        }
    }
}
