using FoodOrderingCore.Exceptions;
using FoodOrderingCore.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace FoodOrderingPRM392.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;
        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }
        public void OnException(ExceptionContext context)
        {
            ParentResponse response = new ParentResponse();
            switch(context.Exception)
            {
                case DbUpdateException:
                    response.Message = "Bad request";
                    context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;
                case DbException:
                    _logger.LogDebug(context.Exception.Message + context.Exception.StackTrace);
                    response.Message = context.Exception.Message;
                    context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;
                case BadRequestException:
                    response.Message = "Bad request";
                    context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;
                case OutOfWalletAmountException:
                    response.Message = "Out of wallet amount";
                    context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;
                default:
                    _logger.LogError(context.Exception.Message + context.Exception.StackTrace);
                    response.Message = context.Exception.Message;
                    context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }
            context.Result = new JsonResult(response);
        }
    }
}
