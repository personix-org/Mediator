namespace Mediator.Tests.Fakes;

public sealed class TestRequestHandler : IRequestHandler<TestRequest, TestResult>
{
    public CancellationToken ReceivedToken { get; private set; }

    public Task<TestResult> Handle(TestRequest request, CancellationToken cancellationToken)
    {
        ReceivedToken = cancellationToken;
        return Task.FromResult(new TestResult($"Handled: {request.Value}"));
    }
}
