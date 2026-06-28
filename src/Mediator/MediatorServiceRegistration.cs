using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Personix.Mediator;

public static class MediatorServiceRegistration
{
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        Action<MediatorConfiguration> configure)
    {
        var config = new MediatorConfiguration();
        configure(config);

        RegisterHandlers(services, config.HandlerAssemblies);
        RegisterBehaviors(services, config.BehaviorTypes);

        var behaviorTypes = config.BehaviorTypes.AsReadOnly();
        services.AddTransient<ISender>(sp => new Mediator(sp, behaviorTypes));

        return services;
    }

    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        string key,
        Action<MediatorConfiguration> configure)
    {
        var config = new MediatorConfiguration();
        configure(config);

        RegisterHandlers(services, config.HandlerAssemblies);
        RegisterBehaviors(services, config.BehaviorTypes);

        var behaviorTypes = config.BehaviorTypes.AsReadOnly();
        services.AddKeyedTransient<ISender>(key, (sp, _) => new Mediator(sp, behaviorTypes));

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, List<Assembly> assemblies)
    {
        var handlerInterfaceType = typeof(IRequestHandler<,>);

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false })
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                    .Select(i => new { Implementation = t, ServiceType = i }));

            foreach (var handler in handlerTypes)
            {
                services.AddTransient(handler.ServiceType, handler.Implementation);
            }
        }
    }

    private static void RegisterBehaviors(IServiceCollection services, List<Type> behaviorTypes)
    {
        foreach (var behaviorType in behaviorTypes)
        {
            services.AddTransient(behaviorType, behaviorType);
        }
    }
}
