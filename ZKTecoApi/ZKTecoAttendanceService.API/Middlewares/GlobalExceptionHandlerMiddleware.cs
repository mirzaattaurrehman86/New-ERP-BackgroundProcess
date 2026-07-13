using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using ZKTecoAttendanceService.Dto;

namespace ZKTecoAttendanceService.API.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                NotFoundException notFoundEx => new ApiResponse<object>(notFoundEx.Message, (int)HttpStatusCode.NotFound),

                ValidationException validationEx => new ApiResponse<object>(
                    validationEx.Errors.SelectMany(e => e.Value).ToList(),
                    (int)HttpStatusCode.BadRequest),

                BadRequestException badRequestEx => new ApiResponse<object>(badRequestEx.Message, (int)HttpStatusCode.BadRequest),

                UnauthorizedException unauthorizedEx => new ApiResponse<object>(unauthorizedEx.Message, (int)HttpStatusCode.Unauthorized),

                ForbiddenException forbiddenEx => new ApiResponse<object>(forbiddenEx.Message, (int)HttpStatusCode.Forbidden),

                ConflictException conflictEx => new ApiResponse<object>(conflictEx.Message, (int)HttpStatusCode.Conflict),

                DbUpdateException dbEx => HandleDbUpdateException(dbEx),

                _ => new ApiResponse<object>("An error occurred while processing your request.", (int)HttpStatusCode.InternalServerError)
            };

            context.Response.StatusCode = response.StatusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }

        /// <summary>
        /// Handles DbUpdateException and provides user-friendly error messages.
        /// </summary>
        private static ApiResponse<object> HandleDbUpdateException(DbUpdateException ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;

            if (innerMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                innerMessage.Contains("unique", StringComparison.OrdinalIgnoreCase))
            {
                if (innerMessage.Contains("EmergencyContacts", StringComparison.OrdinalIgnoreCase))
                {
                    return new ApiResponse<object>(
                        "Emergency contact already exists for this employee. Each employee can have only one emergency contact record.",
                        (int)HttpStatusCode.Conflict);
                }

                if (innerMessage.Contains("EmployeeBankAccounts", StringComparison.OrdinalIgnoreCase))
                {
                    return new ApiResponse<object>(
                        "Bank account already exists for this employee. Each employee can have only one bank account record.",
                        (int)HttpStatusCode.Conflict);
                }

                if (innerMessage.Contains("EmployeeAddresses", StringComparison.OrdinalIgnoreCase))
                {
                    return new ApiResponse<object>(
                        "Employee address already exists. Each employee can have only one address record.",
                        (int)HttpStatusCode.Conflict);
                }

                if (innerMessage.Contains("EmploymentInfos", StringComparison.OrdinalIgnoreCase))
                {
                    return new ApiResponse<object>(
                        "Employment information already exists for this employee.",
                        (int)HttpStatusCode.Conflict);
                }

                return new ApiResponse<object>(
                    "A unique constraint violation occurred. Please ensure all related data is unique.",
                    (int)HttpStatusCode.Conflict);
            }

            if (innerMessage.Contains("foreign key", StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<object>(
                    "Cannot perform this operation. The record references data that does not exist or cannot be deleted.",
                    (int)HttpStatusCode.BadRequest);
            }

            if (innerMessage.Contains("constraint", StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<object>(
                    "A constraint violation occurred. Please check your data and try again.",
                    (int)HttpStatusCode.BadRequest);
            }

            return new ApiResponse<object>(
                "An error occurred while updating the database. Please try again.",
                (int)HttpStatusCode.InternalServerError);
        }
    }
}
