using System.Net;
using System.Text.Json;
using UniformWMS.Application.Common.Exceptions;
using UniformWMS.Application.Common.Models;

namespace UniformWMS.API.Middlewares;

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
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        (HttpStatusCode statusCode, IEnumerable<string> errors) result = ex switch
        //var (statusCode, errors) = ex switch
        {
            NotFoundException => (HttpStatusCode.NotFound, new[] { ex.Message }),
            UnauthorizedException => (HttpStatusCode.Unauthorized, new[] { ex.Message }),
            ForbiddenException => (HttpStatusCode.Forbidden, new[] { ex.Message }),
            //Exceptions.ValidationException ve => (HttpStatusCode.BadRequest,
            //    ve.Errors.SelectMany(e => e.Value.Select(m => $"{e.Key}: {m}")).ToArray()),
            BusinessException => (HttpStatusCode.UnprocessableEntity, new[] { ex.Message }),
            ConflictException => (HttpStatusCode.Conflict, new[] { ex.Message }),
            _ => (HttpStatusCode.InternalServerError, new[] { "Đã xảy ra lỗi. Vui lòng thử lại sau." })
        };

        context.Response.StatusCode = (int)result.statusCode;

        var response = ApiResponse<object>.Fail(result.errors.ToList());
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

// Alias for the validation exception from Application layer
//namespace UniformWMS.API.Middlewares.Exceptions
//{
//    using AppValidation = UniformWMS.Application.Common.Exceptions.ValidationException;
//    internal class ValidationException : AppValidation
//    {
//        public ValidationException(IDictionary<string, string[]> errors) : base(errors) { }
//    }
//}
