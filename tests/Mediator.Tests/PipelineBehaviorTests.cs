using Microsoft.Extensions.DependencyInjection;
using Mediator.Tests.Fakes;

namespace Mediator.Tests;

public sealed class PipelineBehaviorTests
{
    [Fact]
    public async Task Behaviors_ExecuteInConfiguredOrder()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(log);
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly);
            cfg.AddBehavior(typeof(OrderTrackingBehavior<,>));
            cfg.AddBehavior(typeof(SecondOrderTrackingBehavior<,>));
        });

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        await sender.Send(new TestRequest("test"));

        Assert.Equal(
            ["BehaviorA:Before", "BehaviorB:Before", "BehaviorB:After", "BehaviorA:After"],
            log);
    }

    [Fact]
    public async Task Behavior_CanShortCircuit()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(log);
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly);
            cfg.AddBehavior(typeof(ShortCircuitBehavior<,>));
            cfg.AddBehavior(typeof(OrderTrackingBehavior<,>));
        });

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var result = await sender.Send(new TestRequest("test"));

        Assert.Null(result);
        Assert.Empty(log);
    }

    [Fact]
    public async Task NoBehaviors_HandlerExecutesDirectly()
    {
        var services = new ServiceCollection();
        services.AddMediator(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly));

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var result = await sender.Send(new TestRequest("direct"));

        Assert.Equal("Handled: direct", result.ProcessedValue);
    }

    [Fact]
    public async Task Behavior_ReceivesCorrectRequest()
    {
        var capturingBehavior = new RequestCapturingBehavior<TestRequest, TestResult>();
        var services = new ServiceCollection();
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly);
            cfg.AddBehavior(typeof(RequestCapturingBehavior<,>));
        });

        // Replace with our instance
        services.AddTransient<RequestCapturingBehavior<TestRequest, TestResult>>(_ => capturingBehavior);

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var request = new TestRequest("capture-me");
        await sender.Send(request);

        Assert.Equal(request, capturingBehavior.CapturedRequest);
    }
}
