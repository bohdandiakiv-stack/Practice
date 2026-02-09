using Microsoft.Extensions.Configuration;

namespace TaskManager.IntegrationTests;

public class ApiTestFixture : IAsyncLifetime
{
    public HttpClient Client { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var baseUrl = config["Api:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Api:BaseUrl is not configured in appsettings.json");

        Client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        await WaitForApiAsync();
    }

    private async Task WaitForApiAsync()
    {
        for (int i = 0; i < 20; i++)
        {
            try
            {
                var response = await Client.PostAsync("/api/tasks/test", null);
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch { }

            await Task.Delay(1000);
        }

        throw new TimeoutException("API not ready");
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }
}