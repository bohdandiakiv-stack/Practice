using Couchbase.Aspire.Hosting;
using TaskAppHost.Helpers;

var builder = DistributedApplication.CreateBuilder(args);
var config = builder.Configuration;

IResourceBuilder<CouchbaseClusterResource> couchbasedb = null;

#region Couchbase Setup  

couchbasedb = builder
    .AddCouchbase("couchbase")
    .WithManagementPort(8091)
    .WithDataVolumes();

var bucket = couchbasedb.AddBucket("Tasks");

builder.Eventing.Subscribe<AfterResourcesCreatedEvent>(async (@event, ct) =>
{
    try
    {
        var connectionString = await couchbasedb
                    .Resource.ConnectionStringExpression.GetValueAsync(ct)
                    .ConfigureAwait(false);
        Console.WriteLine($"Connecting to Couchbase with connection string: {connectionString}");

        await CouchbaseInitializer.InitializeCouchbaseAsync(
            connectionString: connectionString,
            bucketName: "Tasks",
            scopeName: "1l",
            collectionName: "tasks",
            cancellationToken: ct
        );
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error obtaining Couchbase connection string: {ex.Message}");
    }
});

#endregion

var taskApiProject = builder
    .AddProject<Projects.TaskManager_Api>("tasks-application-api")
    .WithReference(couchbasedb)
    .WithReference(bucket)
    .WaitFor(bucket);

builder.Build().Run();