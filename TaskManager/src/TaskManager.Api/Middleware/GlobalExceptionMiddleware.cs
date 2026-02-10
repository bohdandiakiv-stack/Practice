using System.Net;

namespace TaskManager.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(
    HttpContext context,
    Exception exception)
    {
        context.Response.ContentType = "application/json";

        if (exception is FluentValidation.ValidationException validationException)
        {
            if (!validationException.Errors.Any())
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                await context.Response.WriteAsJsonAsync(new
                {
                    Message = validationException.Message
                });

                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var validationErrors = validationException.Errors.Select(error => new
            {
                Field = error.PropertyName,
                Error = error.ErrorMessage
            });

            await context.Response.WriteAsJsonAsync(new
            {
                Message = "Validation failed",
                Errors = validationErrors
            });

            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        _logger.LogError(exception, "Unhandled exception occurred!");

        await context.Response.WriteAsJsonAsync(new
        {
            Message = "An unexpected error occurred.",
            Detail = exception.Message
        });
    }
}
