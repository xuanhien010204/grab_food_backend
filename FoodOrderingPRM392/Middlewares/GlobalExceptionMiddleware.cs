using FoodOrderingCore.Exceptions;
using FoodOrderingCore.Extensions;
using FoodOrderingCore.Response;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Net;
using System.Text.Json;

namespace FoodOrderingPRM392.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ParentResponse();
            var statusCode = HttpStatusCode.InternalServerError;

            switch (exception)
            {
                // Custom Business Logic Exceptions
                case BadRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Message = exception.Message ?? "Bad request";
                    _logger.LogWarning("Bad request: {Message}", exception.Message);
                    break;

                case OutOfWalletAmountException:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Message = exception.Message ?? "Insufficient wallet balance";
                    _logger.LogWarning("Wallet amount insufficient: {Message}", exception.Message);
                    break;

                // Database Exceptions
                case DbUpdateException dbUpdateEx:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Message = GetDatabaseErrorMessage(dbUpdateEx);
                    _logger.LogError(dbUpdateEx, "Database update error: {Message}", dbUpdateEx.Message);
                    break;

                case DbException dbEx:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Message = GetDatabaseErrorMessage(dbEx);
                    _logger.LogError(dbEx, "Database error: {Message}", dbEx.Message);
                    break;

                // Validation Exceptions
                case ArgumentNullException argNullEx:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Message = $"Required parameter is missing: {argNullEx.ParamName}";
                    _logger.LogWarning("Argument null: {ParamName}", argNullEx.ParamName);
                    break;

                case ArgumentException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Message = argEx.Message;
                    _logger.LogWarning("Invalid argument: {Message}", argEx.Message);
                    break;

                case InvalidOperationException invalidOpEx:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Message = invalidOpEx.Message;
                    _logger.LogWarning("Invalid operation: {Message}", invalidOpEx.Message);
                    break;

                // Not Found Exception
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    response.Message = exception.Message ?? "Resource not found";
                    _logger.LogWarning("Resource not found: {Message}", exception.Message);
                    break;

                // Generic Exceptions
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    response.Message = "An unexpected error occurred. Please try again later.";
                    _logger.LogError(exception, "Unhandled exception: {Message}\nStackTrace: {StackTrace}", 
                        exception.Message, exception.StackTrace);
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var result = JsonConvertExtension.ToJsonString(response);
            return context.Response.WriteAsync(result);
        }

        private string GetDatabaseErrorMessage(Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;

            // Handle common SQL Server errors
            if (message.Contains("UNIQUE KEY", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
            {
                return "A record with the same value already exists.";
            }

            if (message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
            {
                return "Cannot perform this operation due to related records.";
            }

            if (message.Contains("DELETE", StringComparison.OrdinalIgnoreCase) &&
                message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
            {
                return "Cannot delete this record as it is referenced by other data.";
            }

            return "A database error occurred. Please check your data and try again.";
        }
    }
}