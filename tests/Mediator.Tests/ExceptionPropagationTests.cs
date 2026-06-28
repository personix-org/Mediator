using Microsoft.Extensions.DependencyInjection;
using Mediator.Tests.Fakes;

namespace Mediator.Tests;

public sealed class ExceptionPropagationTests
{
    // Send uses method.Invoke() via reflection — a synchronous throw from SendInternal arrives
    // wrapped in TargetInvocationException. The implementation explicitly unwraps it so callers
    // always see the original exception type. These tests cover that contract.

    [Fact]
    public async Task Send_HandlerThrows_OriginalExceptionReachesCallerUnwrapped()
    {
        var services = new ServiceCollection();
        services.AddMediator(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ThrowingRequestHandler).Assembly));

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        // Assert.ThrowsAsync<T> performs an exact type check — passes only if the exception is
        // InvalidOperationException, not a TargetInvocationException wrapper around it.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sender.Send(new ThrowingRequest()));

        Assert.Equal("Handler failed", ex.Message);
    }

    [Fact]
    public async Task Send_HandlerThrows_BehaviorAfterIsSkipped()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(log);
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ThrowingRequestHandler).Assembly);
            cfg.AddBehavior(typeof(OrderTrackingBehavior<,>));
        });

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sender.Send(new ThrowingRequest()));

        Assert.Contains("BehaviorA:Before", log);
        Assert.DoesNotContain("BehaviorA:After", log);
    }

    [Fact]
    public async Task Send_BehaviorThrowsBeforeNext_ExceptionReachesCaller()
    {
        var services = new ServiceCollection();
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly);
            cfg.AddBehavior(typeof(ThrowingBeforeNextBehavior<,>));
        });

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sender.Send(new TestRequest("test")));

        Assert.Equal("Behavior failed before next", ex.Message);
    }

    [Fact]
    public async Task Send_BehaviorThrowsAfterNext_ExceptionReachesCaller()
    {
        var services = new ServiceCollection();
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly);
            cfg.AddBehavior(typeof(ThrowingAfterNextBehavior<,>));
        });

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sender.Send(new TestRequest("test")));

        Assert.Equal("Behavior failed after next", ex.Message);
    }
}
