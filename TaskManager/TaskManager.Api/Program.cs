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