using Couchbase;
using Couchbase.Management.Collections;
using Microsoft.Extensions.Logging;
namespace TaskAppHost.Helpers
{
    public static class CouchbaseInitializer
    {
        private const int WaitTimeoutMs = 10000;
        private const int WaitIntervalMs = 500;

        public static async Task InitializeCouchbaseAsync(
            string connectionString,
            string bucketName,
            string scopeName,
            string collectionName,
            ILogger? logger = null,
            CancellationToken cancellationToken = default)
        {
            var uri = new Uri(connectionString);

            if (string.IsNullOrWhiteSpace(uri.UserInfo))
                throw new ArgumentException("Connection string must contain user credentials.");

            var userInfo = uri.UserInfo.Split(':');
            var userName = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
            var port = uri.Port > 0 ? uri.Port : 8091;
            var cleanConnectionString = $"couchbase://{uri.Host}:{port}";

            var options = new ClusterOptions
            {
                ConnectionString = cleanConnectionString,
                UserName = userName,
                Password = password
            };

            ICluster cluster = null;
            IBucket bucket = null;

            try
            {
                cluster = await Cluster.ConnectAsync(options);
                bucket = await cluster.BucketAsync(bucketName);

                var collectionManager = bucket.Collections;

                await CreateScopeIfNotExistsAsync(collectionManager, scopeName, logger, cancellationToken);
                await CreateCollectionIfNotExistsAsync(collectionManager, scopeName, collectionName, logger, cancellationToken);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during Couchbase initialization: {Message}", ex.Message);
                throw;
            }
            finally
            {
                if (cluster != null)
                {
                    await cluster.DisposeAsync();
                }
            }
        }

        private static async Task CreateScopeIfNotExistsAsync(
            ICouchbaseCollectionManager collectionManager,
            string scopeName,
            ILogger? logger,
            CancellationToken cancellationToken)
        {
            var scopes = await collectionManager.GetAllScopesAsync();
            if (scopes.Any(s => s.Name == scopeName))
                return;

            try
            {
                await collectionManager.CreateScopeAsync(scopeName);
                logger?.LogInformation("Scope '{ScopeName}' created.", scopeName);

                await WaitForConditionAsync(async () =>
                {
                    var updatedScopes = await collectionManager.GetAllScopesAsync();
                    return updatedScopes.Any(s => s.Name == scopeName);
                }, WaitTimeoutMs, WaitIntervalMs, cancellationToken);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error creating scope: {Message}", ex.Message);
                throw;
            }
        }

        private static async Task CreateCollectionIfNotExistsAsync(
            ICouchbaseCollectionManager collectionManager,
            string scopeName,
            string collectionName,
            ILogger? logger,
            CancellationToken cancellationToken)
        {
            var scopes = await collectionManager.GetAllScopesAsync();
            var scope = scopes.FirstOrDefault(s => s.Name == scopeName);
            if (scope?.Collections.Any(c => c.Name == collectionName) == true)
                return;

            try
            {
                var collectionSpec = new CollectionSpec(scopeName, collectionName);
                await collectionManager.CreateCollectionAsync(collectionSpec);
                logger?.LogInformation("Collection '{CollectionName}' created in scope '{ScopeName}'.", collectionName, scopeName);

                await WaitForConditionAsync(async () =>
                {
                    var updatedScopes = await collectionManager.GetAllScopesAsync();
                    var updatedScope = updatedScopes.FirstOrDefault(s => s.Name == scopeName);
                    return updatedScope?.Collections.Any(c => c.Name == collectionName) == true;
                }, WaitTimeoutMs, WaitIntervalMs, cancellationToken);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error creating collection: {Message}", ex.Message);
                throw;
            }
        }

        private static async Task WaitForConditionAsync(
            Func<Task<bool>> condition,
            int timeoutMs,
            int intervalMs,
            CancellationToken cancellationToken)
        {
            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
            {
                if (await condition())
                    return;
                await Task.Delay(intervalMs, cancellationToken);
            }
            throw new TimeoutException("Timeout waiting for Couchbase resource to become available.");
        }
    }
}