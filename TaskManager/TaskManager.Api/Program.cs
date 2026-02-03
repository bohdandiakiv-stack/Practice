using TaskManager.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TaskManager API", Version = "v1" });
});

builder.Services.AddApplicationWithInfrastructure(builder.Configuration);

var app = builder.Build();


app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        if (exceptionHandlerFeature?.Error != null)
        {
            var exception = exceptionHandlerFeature.Error;

            context.Response.ContentType = "application/json";

            if (exception is FluentValidation.ValidationException validationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                var errors = validationException.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Error = e.ErrorMessage
                });

                await context.Response.WriteAsJsonAsync(new
                {
                    Message = "Validation failed",
                    Errors = errors
                });
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new
                {
                    Message = "An unexpected error occurred.",
                    Detail = exception.Message
                });
            }
        }
    });
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskManager API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors(builder =>
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());

//app.UseHttpsRedirection();

app.MapControllers();

app.Run();