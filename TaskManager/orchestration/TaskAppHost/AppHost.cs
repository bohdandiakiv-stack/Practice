using Couchbase.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
var config = builder.Configuration;

IResourceBuilder<CouchbaseClusterResource> couchbase = null;

#region Couchbase Setup  

var couchbasePassword = builder.AddParameter(
    "couchbase-password",
    value: "password",
    secret: true);

couchbase = builder
    .AddCouchbase("couchbase", password: couchbasePassword)
    .WithManagementPort(8091)
    .WithDataVolumes();

var tasksBucket = couchbase
    .AddBucket("Tasks")
    .WithScope(
        scopeName: "1l",
        collections: ["tasks"]);

//builder.Eventing.Subscribe<AfterResourcesCreatedEvent>(async (@event, ct) =>
//{
//    try
//    {
//        var connectionString = await couchbase
//                    .Resource.ConnectionStringExpression.GetValueAsync(ct)
//                    .ConfigureAwait(false);
//        Console.WriteLine($"Connecting to Couchbase with connection string: {connectionString}");

//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error obtaining Couchbase connection string: {ex.Message}");
//    }
//});

#endregion

var taskApiProject = builder
    .AddProject<Projects.TaskManager_Api>("tasks-application-api")
    .WithReference(couchbase)
    .WithReference(tasksBucket)
    .WaitFor(tasksBucket);

builder.Build().Run();