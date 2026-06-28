using System.Collections.Concurrent;
using System.Reflection;

namespace Mediator;

internal sealed class Mediator(IServiceProvider serviceProvider, IReadOnlyList<Type> behaviorTypes) : ISender
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> MethodCache = new();

    private static readonly MethodInfo SendInternalMethod =
        typeof(Mediator).GetMethod(nameof(SendInternal), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        var method = MethodCache.GetOrAdd(
            requestType,
            static (rt, resp) => SendInternalMethod.MakeGenericMethod(rt, resp),
            responseType);

        try
        {
            return (Task<TResponse>)method.Invoke(this, [request, cancellationToken])!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw; // unreachable
        }
    }

    private Task<TResponse> SendInternal<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        var handler = serviceProvider.GetService(typeof(IRequestHandler<TRequest, TResponse>))
            as IRequestHandler<TRequest, TResponse>
            ?? throw new InvalidOperationException(
                $"No handler registered for {typeof(TRequest).Name}. " +
                $"Ensure an IRequestHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}> is registered in the service collection.");

        RequestHandlerDelegate<TResponse> handlerDelegate = () => handler.Handle(request, cancellationToken);

        var pipeline = handlerDelegate;

        for (var i = behaviorTypes.Count - 1; i >= 0; i--)
        {
            var closedBehaviorType = behaviorTypes[i].MakeGenericType(typeof(TRequest), typeof(TResponse));
            var behavior = serviceProvider.GetService(closedBehaviorType);

            if (behavior is not IPipelineBehavior<TRequest, TResponse> typedBehavior)
                continue;

            var next = pipeline;
            pipeline = () => typedBehavior.Handle(request, next, cancellationToken);
        }

        return pipeline();
    }
}
