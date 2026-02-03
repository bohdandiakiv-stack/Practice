using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            services.AddCouchbase(options =>
            {
                var connectionString = configuration.GetConnectionString("couchbase")
                    ?? "couchbase://localhost";

                options.ConnectionString = connectionString;
                options.UserName = configuration["Couchbase:UserName"];
                options.Password = configuration["Couchbase:Password"];
                options.Buckets = configuration.GetSection("Couchbase:Buckets").Get<List<string>>();
            });

            services.AddScoped<ITaskRepository, TaskRepository>(sp =>
            {
                var provider = sp.GetRequiredService<IBucketProvider>();
                var bucket = provider.GetBucketAsync("TaskManager").GetAwaiter().GetResult();

                var scope = bucket.DefaultScope();
                var collection = bucket.DefaultCollection();

                return new TaskRepository(bucket, scope, collection);
            });

            return services;
        }
    }
}
