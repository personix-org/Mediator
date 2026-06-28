# Pipelines

Lightweight mediator with keyed DI support and configurable pipeline behaviors.

## Usage

```csharp
services.AddMediator(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(MyHandler).Assembly);
    cfg.AddBehavior(typeof(LoggingBehavior<,>));
});
```

### Keyed instances

```csharp
services.AddMediator("admin", cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AdminHandler).Assembly);
    cfg.AddBehavior(typeof(AuditBehavior<,>));
});

// Resolve
var sender = provider.GetRequiredKeyedService<ISender>("admin");
```
