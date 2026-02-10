using TaskManager.Api.Mappers.Tasks;

namespace TaskManager.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(
        this IServiceCollection services)
        {
            services.AddControllers();

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<TaskRequestMappingProfile>();
            });

            return services;
        }
    }
}
