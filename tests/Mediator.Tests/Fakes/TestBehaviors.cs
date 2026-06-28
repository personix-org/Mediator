namespace Mediator.Tests.Fakes;

public sealed class OrderTrackingBehavior<TRequest, TResponse>(List<string> log)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        log.Add("BehaviorA:Before");
        var response = await next();
        log.Add("BehaviorA:After");
        return response;
    }
}

public sealed class SecondOrderTrackingBehavior<TRequest, TResponse>(List<string> log)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        log.Add("BehaviorB:Before");
        var response = await next();
        log.Add("BehaviorB:After");
        return response;
    }
}

public sealed class ShortCircuitBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return Task.FromResult(default(TResponse)!);
    }
}

public sealed class RequestCapturingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public TRequest? CapturedRequest { get; private set; }

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        CapturedRequest = request;
        return next();
    }
}

public sealed class ThrowingBeforeNextBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        => throw new InvalidOperationException("Behavior failed before next");
}

public sealed class ThrowingAfterNextBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await next();
        throw new InvalidOperationException("Behavior failed after next");
    }
}
