using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Mappers.Tasks;
using TaskManager.Application.Services;
using TaskManager.Application.Services.Implementations;
using TaskManager.Application.Validation.Facades;
using TaskManager.Application.Validation.Facades.Interfaces;
using TaskManager.Application.Validation.Tasks;
using TaskManager.Infrastructure;

namespace TaskManager.Application;

public static class DependencyInjection
{

    public static IServiceCollection AddApplicationWithInfrastructure(
       this IServiceCollection services,
       IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services;
    }

    private static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITaskService, TaskService>();

        services.AddAutoMapper(cfg => cfg.AddProfile<TaskMappingProfile>());
        services.AddValidatorsFromAssemblyContaining<CreateTaskValidator>(includeInternalTypes: true);

        services.AddScoped<IValidationService, ValidationService>();

        return services;
    }
}
