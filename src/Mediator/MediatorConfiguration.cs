using System.Reflection;

namespace Mediator;

public sealed class MediatorConfiguration
{
    internal List<Type> BehaviorTypes { get; } = [];
    internal List<Assembly> HandlerAssemblies { get; } = [];

    public MediatorConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        HandlerAssemblies.Add(assembly);
        return this;
    }

    public MediatorConfiguration AddBehavior(Type openGenericBehaviorType)
    {
        ArgumentNullException.ThrowIfNull(openGenericBehaviorType);

        if (!openGenericBehaviorType.IsGenericTypeDefinition)
            throw new ArgumentException(
                $"Type '{openGenericBehaviorType.Name}' must be an open generic type definition.",
                nameof(openGenericBehaviorType));

        BehaviorTypes.Add(openGenericBehaviorType);
        return this;
    }
}
