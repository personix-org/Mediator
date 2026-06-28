namespace Personix.Mediator.Tests.Fakes;

public sealed record ThrowingRequest : IRequest<ThrowingResult>;

public sealed record ThrowingResult;

public sealed class ThrowingRequestHandler : IRequestHandler<ThrowingRequest, ThrowingResult>
{
    public Task<ThrowingResult> Handle(ThrowingRequest request, CancellationToken cancellationToken)
        => throw new InvalidOperationException("Handler failed");
}
