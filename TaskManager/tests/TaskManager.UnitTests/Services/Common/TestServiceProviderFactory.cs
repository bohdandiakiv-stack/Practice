using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Application.Validation.Facades;
using TaskManager.Application.Validation.Facades.Interfaces;
using TaskManager.Application.Validation.Tasks;

namespace TaskManager.UnitTests.Services.Common
{
    public static class TestServiceProviderFactory
    {
        public static IServiceProvider Create()
        {
            var services = new ServiceCollection();

            services.AddTransient<IValidator<CreateTaskDto>, CreateTaskValidator>();
            services.AddTransient<IValidator<UpdateTaskDto>, UpdateTaskValidator>();
            services.AddTransient<IValidationService, ValidationService>();

            return services.BuildServiceProvider();
        }
    }
}
