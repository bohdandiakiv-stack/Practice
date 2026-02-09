using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Mappers.Tasks;

namespace TaskManager.UnitTests.Services.Common;

public abstract class BaseTestFixture : IDisposable
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly IMapper Mapper;

    protected BaseTestFixture()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<TaskMappingProfile>();
        });

        ServiceProvider = services.BuildServiceProvider();

        Mapper = ServiceProvider.GetRequiredService<IMapper>();
    }

    public void Dispose()
    {
        ServiceProvider.Dispose();
    }
}