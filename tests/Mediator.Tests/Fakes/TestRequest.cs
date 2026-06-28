namespace Personix.Mediator.Tests.Fakes;

public sealed record TestRequest(string Value) : IRequest<TestResult>;

public sealed record TestResult(string ProcessedValue);
