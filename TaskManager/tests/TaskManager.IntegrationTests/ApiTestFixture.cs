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
        var timeout = TimeSpan.FromSeconds(30);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                var response = await Client.GetAsync("/health");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("API is ready!");
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(500);
        }

        throw new TimeoutException(
            $"API did not become ready within {timeout.TotalSeconds} seconds");
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }
}