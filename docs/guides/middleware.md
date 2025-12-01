# Middleware

`aws-lambda-host` uses the same middleware model as ASP.NET Core: each component gets a context object,
runs code before/after the next component, and can short-circuit the pipeline. If you're new to the
pattern, skim the [ASP.NET Core middleware overview](https://learn.microsoft.com/aspnet/core/fundamentals/middleware/)
first. This guide focuses on Lambda-specific behavior: invocation scopes, feature access, and
composition tips.

## Pipeline Basics

Register middleware before calling `MapHandler`. Components execute in registration order and unwind in
reverse order:

```csharp title="Program.cs"
var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

lambda.UseMiddleware(async (context, next) =>
{
    Console.WriteLine("[Logging] Before handler");
    await next(context);
    Console.WriteLine("[Logging] After handler");
});

lambda.UseMiddleware(async (context, next) =>
{
    Console.WriteLine("[Metrics] Before handler");
    await next(context);
    Console.WriteLine("[Metrics] After handler");
});

lambda.MapHandler(([Event] Request request) => new Response("ok"));
await lambda.RunAsync();
```

Output:

```
[Logging] Before handler
[Metrics] Before handler
[Metrics] After handler
[Logging] After handler
```

## `ILambdaHostContext`

Every middleware receives the same `ILambdaHostContext`, which is scoped to the invocation.

```csharp title="Program.cs"
lambda.UseMiddleware(async (context, next) =>
{
    var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();

    if (context.CancellationToken.IsCancellationRequested)
    {
        logger.LogWarning("Invocation cancelled before handler");
        return;
    }

    context.Items["RequestId"] = Guid.NewGuid().ToString();
    context.Properties["Version"] ??= "1.0.0"; // safe cross-invocation value

    await next(context);

    var started = (DateTimeOffset)context.Items["Start"];
    logger.LogInformation("Completed in {Duration}ms", (DateTimeOffset.UtcNow - started).TotalMilliseconds);
});
```

Key members:

- `ServiceProvider` – resolve scoped services for the invocation.
- `CancellationToken` – fires before Lambda termination (buffer controlled by
  `LambdaHostOptions.InvocationCancellationBuffer`). Pass it to downstream async work.
- `Items` – per-invocation storage shared by middleware/handler.
- `Properties` – cross-invocation storage.
- `Features` – type-keyed feature objects such as `IEventFeature<T>`
  and `IResponseFeature<T>`.

## Inline Middleware Only

`UseMiddleware` currently accepts inline delegates. Class-based middleware activators are on the roadmap,
so for now keep middleware logic inside the lambda or extract helper services for reuse.

## Working with Features

Features are type-keyed adapters stored inside `ILambdaHostContext.Features` (an
`IFeatureCollection`). They decouple middleware from handlers: a handler (or the framework) populates a
feature, middleware reads or mutates it, and nobody needs to inject each other through DI. The
collection lazily creates features by asking every registered `IFeatureProvider` to build them when
first requested.

```csharp title="Program.cs"
using AwsLambda.Host.Abstractions.Features;

lambda.UseMiddleware(async (context, next) =>
{
    var eventFeature = context.Features.Get<IEventFeature<OrderRequest>>();
    if (eventFeature is { Event: { } request })
        Console.WriteLine($"Processing {request.OrderId}");

    await next(context);

    var responseFeature = context.Features.Get<IResponseFeature<OrderResponse>>();
    if (responseFeature?.Response is { } response)
        Console.WriteLine($"Result: {response.Status}");
});
```

Common features:

| Feature                       | Purpose                                                      |
|-------------------------------|--------------------------------------------------------------|
| `IEventFeature<TEvent>`       | Access the deserialized event payload                        |
| `IResponseFeature<TResponse>` | Inspect or replace the handler response before serialization |
| `IInvocationDataFeature`      | Access raw event/response streams for envelopes              |

**Why features matter:**

- Middleware can extract values set by handlers (or other middleware) without DI fan-out.
- Handlers remain free of middleware-specific dependencies; they just work with the event/response types.
- Custom features are easy to add—Just add an implementation of `IFeatureProvider` and register it. It will then be available to all middleware.

```csharp title="Custom feature"
public interface ICorrelationFeature
{
    string CorrelationId { get; set; }
}

public sealed class CorrelationFeature : ICorrelationFeature
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

lambda.UseMiddleware(async (context, next) =>
{
    var feature = context.Features.Get<ICorrelationFeature>() ?? new CorrelationFeature();
    
    context.Items["CorrelationId"] = feature.CorrelationId;
    await next(context);
});

public sealed class CorrelationFeatureProvider : IFeatureProvider
{
    private static readonly Type FeatureType = typeof(ICorrelationFeature);

    public bool TryCreate(Type type, out object? feature)
    {
        feature = type == FeatureType ? new CorrelationFeature() : null;
        return feature is not null;
    }
}

builder.Services.AddSingleton<IFeatureProvider, CorrelationFeatureProvider>();
```

## Short-Circuiting and Error Handling

Middleware can stop the pipeline early:

```csharp title="Caching"
lambda.UseMiddleware(async (context, next) =>
{
    var cache = context.ServiceProvider.GetRequiredService<ICache>();
    var request = context.Features.Get<IEventFeature<OrderRequest>>()?.Event;

    if (request is not null && cache.TryGet(request.OrderId, out OrderResponse cached))
    {
        context.Features.Get<IResponseFeature<OrderResponse>>()!.Response = cached;
        return; // skip handler
    }

    await next(context);
});
```

Wrap the pipeline to catch and translate exceptions:

```csharp title="Error Handling"
lambda.UseMiddleware(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (ValidationException ex)
    {
        var response = context.Features.Get<IResponseFeature<OrderResponse>>();
        if (response is not null)
            response.Response = new("invalid", ex.Message);
        return; // handled
    }
});
```

## Ordering Strategy

Register middleware from outermost to innermost:

```csharp title="Order"
lambda.UseMiddleware<ErrorHandlingMiddleware>();   // catches everything
lambda.UseMiddleware<LoggingMiddleware>();         // logs every request
lambda.UseMiddleware<MetricsMiddleware>();         // records durations
lambda.UseMiddleware<AuthenticationMiddleware>();  // auth first
lambda.UseMiddleware<AuthorizationMiddleware>();   // then authorization
lambda.UseMiddleware<ValidationMiddleware>();      // validate payloads
lambda.MapHandler(/* handler */);
```

Guidelines:

- Error/diagnostics (logging, metrics) go first so they see every request.
- Authentication/authorization should wrap validation and business logic.
- Response caching happens late so only valid, authorized responses are stored.

## Configuration and Options

Even though middleware delegates are inline, they still run inside the invocation scope. Resolve options
or services via `context.ServiceProvider` the same way you would inside a handler.

## Best Practices

- **Keep middleware focused.** One responsibility per component (logging, metrics, caching, etc.).
- **Always call `await next(context)`** unless you intentionally short-circuit; forgetting it prevents the
  handler from running.
- **Never swallow exceptions silently.** If you handle an error, set a response or log it so Lambda doesn’t
  report success unintentionally.
- **Use per-invocation state wisely.** `Items` is cleared after each request; `Properties` live for the life
  of the container and must be thread-safe.
- **Prefer class-based middleware** for anything reusable or needing DI/Options.
- **Make cancellation cooperative.** Honor `context.CancellationToken` in middleware and pass it to downstream I/O.

With these patterns, you can build rich, testable pipelines around your Lambda handlers while keeping
business logic small and focused.
