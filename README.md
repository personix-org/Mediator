# Personix.Mediator

[![NuGet](https://img.shields.io/nuget/v/Personix.Mediator.svg)](https://www.nuget.org/packages/Personix.Mediator/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A lightweight mediator for .NET with **keyed dependency-injection support** and **configurable pipeline behaviors** — a free, MIT-licensed take on request/response dispatch and pipeline middleware.

Unlike most mediator libraries, it supports **multiple isolated (keyed) mediator instances** in the same container, each with its own handlers and behavior pipeline.

## Install

```sh
dotnet add package Personix.Mediator
```

## Usage

Define a request and its handler:

```csharp
public sealed record Ping(string Message) : IRequest<string>;

public sealed class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken ct)
        => Task.FromResult($"pong: {request.Message}");
}
```

Register and send:

```csharp
services.AddMediator(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(PingHandler).Assembly);
    cfg.AddBehavior(typeof(LoggingBehavior<,>));
});

var sender = provider.GetRequiredService<ISender>();
var reply = await sender.Send(new Ping("hi"));
```

## Pipeline behaviors

Behaviors wrap a request like middleware — pre/post logic, and they can short-circuit the pipeline. Registration order is execution order (first added = outermost).

```csharp
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // before
        var response = await next(ct);
        // after
        return response;
    }
}
```

## Keyed instances

Register several independent mediators, each with its own pipeline, and resolve them by key:

```csharp
services.AddMediator("admin", cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AdminHandler).Assembly);
    cfg.AddBehavior(typeof(AuditBehavior<,>));
});

var adminSender = provider.GetRequiredKeyedService<ISender>("admin");
```

Keyed and non-keyed mediators coexist; each key has a fully isolated behavior pipeline.

## License

[MIT](LICENSE) © Personix
