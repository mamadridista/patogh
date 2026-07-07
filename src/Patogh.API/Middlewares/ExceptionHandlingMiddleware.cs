using FluentValidation;
using Patogh.Domain.Exceptions;
using System.Net;

namespace Patogh.API.Middlewares;

/// <summary>
/// Global exception handler that converts all domain exceptions to
/// RFC 7807 Problem Details responses (https://tools.ietf.org/html/rfc7807).
/// This ensures the frontend always receives a consistent error shape
/// regardless of which layer threw the exception.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblem(context, HttpStatusCode.BadRequest,
                title: "Validation Failed",
                detail: "یک یا چند فیلد ورودی نامعتبر است.",
                extensions: new Dictionary<string, object?>
                {
                    ["errors"] = ex.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });
        }
        catch (NotFoundException ex)
        {
            await WriteProblem(context, HttpStatusCode.NotFound,
                title: "Not Found",
                detail: ex.Message);
        }
        catch (UnauthorizedDomainException ex)
        {
            await WriteProblem(context, HttpStatusCode.Unauthorized,
                title: "Unauthorized",
                detail: ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteProblem(context, HttpStatusCode.Forbidden,
                title: "Forbidden",
                detail: ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteProblem(context, HttpStatusCode.Conflict,
                title: "Conflict",
                detail: ex.Message);
        }
        catch (DomainValidationException ex)
        {
            await WriteProblem(context, HttpStatusCode.UnprocessableEntity,
                title: "Business Rule Violation",
                detail: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception on {Method} {Path}: {Message}",
                context.Request.Method,
                context.Request.Path,
                ex.Message);

            await WriteProblem(context, HttpStatusCode.InternalServerError,
                title: "Internal Server Error",
                detail: _env.IsDevelopment()
                    ? ex.Message
                    : "خطای داخلی سرور. لطفاً دوباره تلاش کنید.",
                extensions: _env.IsDevelopment()
                    ? new Dictionary<string, object?> { ["stackTrace"] = ex.StackTrace }
                    : null);
        }
    }

    private static async Task WriteProblem(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        string detail,
        Dictionary<string, object?>? extensions = null)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail,
            instance = context.Request.Path.Value,
            traceId = context.TraceIdentifier,
            extensions
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}