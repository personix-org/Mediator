using Microsoft.Extensions.DependencyInjection;
using Mediator.Tests.Fakes;

namespace Mediator.Tests;

public sealed class KeyedMediatorTests
{
    [Fact]
    public async Task KeyedMediators_HaveIndependentPipelines()
    {
        var logA = new List<string>();
        var logB = new List<string>();

        var services = new ServiceCollection();
        // Both behaviors need a List<string> — register both so each resolves the right one.
        // BehaviorA uses the singleton List<string>, BehaviorB uses a separate one.
        // To isolate logs, register two separate List<string> and route via wrapper types.
        // Simpler: register a single shared log list, but rely on behavior isolation via config.
        var sharedLog = new List<string>();
        services.AddSingleton(sharedLog);

        // Key "A" gets only BehaviorA
        services.AddMediator("A", cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly);
            cfg.AddBehavior(typeof(OrderTrackingBehavior<,>));
        });

        // Key "B" gets only BehaviorB
        services.AddMediator("B", cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly);
            cfg.AddBehavior(typeof(SecondOrderTrackingBehavior<,>));
        });

        var provider = services.BuildServiceProvider();

        var senderA = provider.GetRequiredKeyedService<ISender>("A");
        var senderB = provider.GetRequiredKeyedService<ISender>("B");

        await senderA.Send(new TestRequest("a"));

        // BehaviorA ran, BehaviorB did not
        Assert.Equal(["BehaviorA:Before", "BehaviorA:After"], sharedLog);

        sharedLog.Clear();
        await senderB.Send(new TestRequest("b"));

        // BehaviorB ran, BehaviorA did not
        Assert.Equal(["BehaviorB:Before", "BehaviorB:After"], sharedLog);
    }

    [Fact]
    public async Task KeyedAndNonKeyed_CoexistIndependently()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(log);

        services.AddMediator(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly));

        services.AddMediator("keyed", cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly);
            cfg.AddBehavior(typeof(OrderTrackingBehavior<,>));
        });

        var provider = services.BuildServiceProvider();

        var defaultSender = provider.GetRequiredService<ISender>();
        var keyedSender = provider.GetRequiredKeyedService<ISender>("keyed");

        await defaultSender.Send(new TestRequest("default"));
        Assert.Empty(log);

        await keyedSender.Send(new TestRequest("keyed"));
        Assert.Equal(["BehaviorA:Before", "BehaviorA:After"], log);
    }

    [Fact]
    public void KeyedMediator_ResolvesWithFromKeyedServices()
    {
        var services = new ServiceCollection();
        services.AddMediator("my-key", cfg =>
            cfg.RegisterServicesFromAssembly(typeof(TestRequestHandler).Assembly));

        var provider = services.BuildServiceProvider();

        var sender = provider.GetRequiredKeyedService<ISender>("my-key");

        Assert.NotNull(sender);
    }
}
