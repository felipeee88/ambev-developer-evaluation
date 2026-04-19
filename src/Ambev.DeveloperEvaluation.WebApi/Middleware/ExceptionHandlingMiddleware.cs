using System.Text.Json;
using Ambev.DeveloperEvaluation.Application.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

/// <summary>
/// Global exception handler. Maps domain/application exceptions to
/// { type, error, detail } payloads with the appropriate HTTP status.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, new
            {
                type = "ValidationError",
                error = "Validation failed",
                detail = ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            });
        }
        catch (NotFoundException ex)
        {
            await WriteAsync(context, StatusCodes.Status404NotFound, new
            {
                type = "ResourceNotFound",
                error = "Resource not found",
                detail = ex.Message
            });
        }
        catch (DomainException ex)
        {
            await WriteAsync(context, StatusCodes.Status422UnprocessableEntity, new
            {
                type = "DomainError",
                error = "Business rule violation",
                detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteAsync(context, StatusCodes.Status500InternalServerError, new
            {
                type = "InternalError",
                error = "Unexpected error",
                detail = "An unexpected error occurred."
            });
        }
    }

    private static Task WriteAsync(HttpContext context, int status, object payload)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
