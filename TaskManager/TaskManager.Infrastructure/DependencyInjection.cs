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
                var fullConnectionString = configuration.GetConnectionString("couchbase");

                if (string.IsNullOrWhiteSpace(fullConnectionString))
                    throw new InvalidOperationException("Couchbase connection string is missing.");

                var uri = new Uri(fullConnectionString);

                if (string.IsNullOrWhiteSpace(uri.UserInfo))
                    throw new InvalidOperationException("Couchbase connection string must contain credentials.");

                var userInfoParts = uri.UserInfo.Split(':', 2);
                var username = userInfoParts[0];
                var password = userInfoParts.Length > 1 ? userInfoParts[1] : string.Empty;
                var port = uri.Port > 0 ? uri.Port : 11210;

                options.ConnectionString = $"couchbase://{uri.Host}:{port}";
                options.UserName = username;
                options.Password = password;
            });

            services.AddCouchbaseBucket<INamedBucketProvider>("Tasks");
            services.AddScoped<ITaskRepository>(sp =>
            {
                var bucketProvider = sp.GetRequiredService<INamedBucketProvider>();
                var bucket = bucketProvider.GetBucketAsync().GetAwaiter().GetResult();
                var scope = bucket.Scope("1l");
                var collection = scope.Collection("tasks");
                var cluster = bucket.Cluster;
                return new TaskRepository(collection, cluster);
            });

            return services;
        }
    }
}
