using Microsoft.Extensions.DependencyInjection;
using Personix.Mediator.Tests.Fakes;

namespace Personix.Mediator.Tests;

public sealed class MediatorTests
{
    [Fact]
    public async Task Send_ResolvesHandler_ReturnsResult()
    {
        var services = new ServiceCollection();
        services.AddMediator(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly));

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var result = await sender.Send(new TestRequest("hello"));

        Assert.Equal("Handled: hello", result.ProcessedValue);
    }

    [Fact]
    public async Task Send_ThrowsWhenHandlerNotFound()
    {
        var services = new ServiceCollection();
        services.AddMediator(_ => { });

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sender.Send(new TestRequest("hello")));
    }

    [Fact]
    public async Task Send_PropagatesCancellationToken()
    {
        var handler = new TestRequestHandler();
        var services = new ServiceCollection();
        services.AddMediator(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly));

        // Replace the scanned handler with our instance so we can inspect the token
        services.AddTransient<IRequestHandler<TestRequest, TestResult>>(_ => handler);

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        using var cts = new CancellationTokenSource();
        await sender.Send(new TestRequest("test"), cts.Token);

        Assert.Equal(cts.Token, handler.ReceivedToken);
    }

    [Fact]
    public async Task Send_NullRequest_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        services.AddMediator(_ => { });

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sender.Send<TestResult>(null!));
    }
}
