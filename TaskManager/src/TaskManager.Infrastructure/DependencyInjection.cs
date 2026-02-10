using Couchbase.Aspire.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Domain.Repositories;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddScoped<ITaskRepository>(sp =>
        {
            var clientProvider = sp.GetRequiredService<ICouchbaseClientProvider>();

            var bucket = clientProvider.GetBucketAsync().GetAwaiter().GetResult();

            var scope = bucket.Scope("1l");
            var collection = scope.Collection("tasks");

            return new TaskRepository(collection, bucket.Cluster);
        });

        return services;
    }
}
