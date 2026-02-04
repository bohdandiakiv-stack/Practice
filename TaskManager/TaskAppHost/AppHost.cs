using Aspire.Hosting;
using Couchbase.Aspire.Hosting;
using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);
var config = builder.Configuration;

IResourceBuilder<CouchbaseClusterResource> couchbasedb = null;
#region Legacy
//// Couchbase config
//var couchbaseSection = config.GetSection("Couchbase");
//var cbImage = couchbaseSection.GetValue<string>("Image");
//var cbTag = couchbaseSection.GetValue<string>("Tag");
//var cbAdminUser = couchbaseSection.GetValue<string>("AdminUser");
//var cbAdminPassword = couchbaseSection.GetValue<string>("AdminPassword");
//var cbDataVolume = couchbaseSection.GetValue<string>("DataVolume");
//var cbDataPath = couchbaseSection.GetValue<string>("DataPath");
//var cbPorts = couchbaseSection.GetSection("Ports");
//var cbConsolePort = cbPorts.GetValue<int>("Console");
//var cbMemcachedPort = cbPorts.GetValue<int>("Memcached");
//var cbQueryPort = cbPorts.GetValue<int>("Query");

//// Couchbase Init config
//var cbInitSection = config.GetSection("CouchbaseInit");
//var cbInitImage = cbInitSection.GetValue<string>("Image");
//var cbInitScriptHostPath = cbInitSection.GetValue<string>("ScriptHostPath");
//var cbInitScriptContainerPath = cbInitSection.GetValue<string>("ScriptContainerPath");

//// API config
//var apiSection = config.GetSection("TaskManagerApi");
//var apiDockerfilePath = apiSection.GetValue<string>("DockerfilePath");
//var apiDockerfileName = apiSection.GetValue<string>("DockerfileName");
//var apiPort = apiSection.GetValue<int>("Port");
//var apiTargetPort = apiSection.GetValue<int>("TargetPort");
//var apiConnectionString = apiSection.GetSection("ConnectionStrings").GetValue<string>("Couchbase");
//var apiCbSection = apiSection.GetSection("Couchbase");
//var apiCbUsername = apiCbSection.GetValue<string>("Username");
//var apiCbPassword = apiCbSection.GetValue<string>("Password");
//var apiCbBucketName = apiCbSection.GetValue<string>("BucketName");

//// Couchbase container
//var couchbase = builder.AddContainer("couchbase", cbImage, cbTag)
//    .WithHttpEndpoint(port: cbConsolePort, targetPort: cbConsolePort, name: "console")
//    .WithEndpoint(port: cbMemcachedPort, targetPort: cbMemcachedPort, name: "memcached")
//    .WithEndpoint(port: cbQueryPort, targetPort: cbQueryPort, name: "query")
//    .WithEnvironment("COUCHBASE_ADMINISTRATOR_USERNAME", cbAdminUser)
//    .WithEnvironment("COUCHBASE_ADMINISTRATOR_PASSWORD", cbAdminPassword)
//    .WithVolume(cbDataVolume, cbDataPath);

//// Couchbase init container
//var couchbaseInit = builder.AddContainer("couchbase-init", cbInitImage)
//    .WithBindMount(cbInitScriptHostPath, cbInitScriptContainerPath)
//    .WithArgs("sh", cbInitScriptContainerPath)
//    .WaitFor(couchbase);

//// API container
//var api = builder.AddDockerfile("taskmanager-api", apiDockerfilePath, apiDockerfileName)
//    .WithHttpEndpoint(port: apiPort, targetPort: apiTargetPort, name: "api-http")
//    .WithEnvironment("ConnectionStrings__couchbase", apiConnectionString)
//    .WithEnvironment("Couchbase__Username", apiCbUsername)
//    .WithEnvironment("Couchbase__Password", apiCbPassword)
//    .WithEnvironment("Couchbase__BucketName", apiCbBucketName)
//    .WaitFor(couchbaseInit);
#endregion

#region Couchbase Setup  

couchbasedb = builder
    .AddCouchbase("couchbase")
    .WithManagementPort(8091);


var bucket = couchbasedb.AddBucket("tasks");


couchbasedb.ApplicationBuilder.Eventing.Subscribe<ConnectionStringAvailableEvent>(
    couchbasedb.Resource,
    async (@event, ct) =>
    {
        try
        {
            var connectionString = await couchbasedb
                        .Resource.ConnectionStringExpression.GetValueAsync(ct)
                        .ConfigureAwait(false);
            Console.WriteLine($"Connecting to Couchbase with connection string: {connectionString}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obtaining Couchbase connection string: {ex.Message}");
        }
    }
);


#endregion

 
var taskApiProject = builder.AddProject<Projects.TaskManager_Api>("tasks-application-api")
       .WithUrlForEndpoint("http", url =>
       {
           url.DisplayText = "Swagger";
           url.Url = "/swagger";
       });
taskApiProject.WithReference(couchbasedb).WaitFor(couchbasedb);

builder.Build().Run();
