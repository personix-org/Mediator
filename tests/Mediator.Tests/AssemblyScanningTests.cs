using Microsoft.Extensions.DependencyInjection;
using Personix.Mediator.Tests.Fakes;

namespace Personix.Mediator.Tests;

public sealed class AssemblyScanningTests
{
    [Fact]
    public void RegisterServicesFromAssembly_DiscoversHandlers()
    {
        var services = new ServiceCollection();
        services.AddMediator(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly));

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<IRequestHandler<TestRequest, TestResult>>();

        Assert.NotNull(handler);
        Assert.IsType<TestRequestHandler>(handler);
    }
}
