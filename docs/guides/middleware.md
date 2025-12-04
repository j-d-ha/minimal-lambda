# Middleware

`aws-lambda-host` uses the same middleware model as ASP.NET Core: each component gets a context object,
runs code before/after the next component, and can short-circuit the pipeline. If you're new to the
pattern, skim the [ASP.NET Core middleware overview](https://learn.microsoft.com/aspnet/core/fundamentals/middleware/)
first. This guide focuses on Lambda-specific behavior: invocation scopes, feature access, and
composition tips that keep middleware and handlers decoupled without extra DI plumbing.

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
- `Features` – ASP.NET-style typed capabilities such as `IEventFeature<T>` and `IResponseFeature<T>` that let middleware collaborate without injecting each other.

## Inline Middleware

`UseMiddleware` currently accepts inline delegates. Class-based middleware activators are on the roadmap,
so for now keep middleware logic inside the lambda or extract helper services (registered in DI) for reuse.
Treat the delegate as the orchestration glue and push heavy lifting into services so the code stays testable.

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
- Custom features are easy to add—register an implementation of `IFeatureProvider` and it becomes available to all middleware.

### Type-Safe Feature Access

The framework provides convenient extension methods on `ILambdaHostContext` for type-safe event and response access, simplifying the feature access pattern shown above:

```csharp title="Program.cs"
lambda.UseMiddleware(async (context, next) =>
{
    // Nullable access - returns null if not found
    var request = context.GetEvent<OrderRequest>();
    if (request is not null)
        Console.WriteLine($"Processing order {request.OrderId}");

    // Try pattern - safe null checking
    if (context.TryGetEvent<OrderRequest>(out var order))
    {
        // Use order safely without additional null checks
        Console.WriteLine($"Order {order.OrderId} has {order.Items.Count} items");
    }

    await next(context);

    // Required access - throws if not found
    var response = context.GetRequiredResponse<OrderResponse>();
    Console.WriteLine($"Status: {response.Status}");
});
```

**Available Methods:**

| Method                     | Description                             | Returns                                  |
|----------------------------|-----------------------------------------|------------------------------------------|
| `GetEvent<T>()`            | Returns event or `null` if not found    | `T?`                                     |
| `GetResponse<T>()`         | Returns response or `null` if not found | `T?`                                     |
| `TryGetEvent<T>(out T)`    | Try-pattern for safe event access       | `bool`                                   |
| `TryGetResponse<T>(out T)` | Try-pattern for safe response access    | `bool`                                   |
| `GetRequiredEvent<T>()`    | Returns event or throws                 | `T` (throws `InvalidOperationException`) |
| `GetRequiredResponse<T>()` | Returns response or throws              | `T` (throws `InvalidOperationException`) |

**When to use each:**

- **Nullable methods** (`GetEvent<T>()`) – When the event/response might not exist and you'll handle null gracefully
- **Try pattern** (`TryGetEvent<T>()`) – When you want explicit null checking without additional conditionals
- **Required methods** (`GetRequiredEvent<T>()`) – When the event/response must exist and missing it is an error condition

These methods are equivalent to calling `context.Features.Get<IEventFeature<T>>()` and accessing the event/response, but provide cleaner syntax and better null-safety annotations.

### Feature Providers in Practice

When `context.Features.Get<T>()` runs, `AwsLambda.Host` walks through every registered `IFeatureProvider`
until one returns the requested feature. Built-in providers handle common cases such as response
serialization. Use the same pattern for your features.

```csharp title="DefaultResponseFeatureProvider.cs" linenums="1"
using Amazon.Lambda.Core;

namespace AwsLambda.Host.Core;

/// <summary>
///     Provides a default implementation of <see cref="IResponseFeature" /> for Lambda response
///     serialization. This provider is instantiated by source-generated code to handle Lambda response
///     processing using the specified <see cref="ILambdaSerializer" />.
/// </summary>
public class DefaultResponseFeatureProvider<T>(ILambdaSerializer lambdaSerializer)
    : IFeatureProvider
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Type FeatureType = typeof(IResponseFeature);

    /// <inheritdoc />
    public bool TryCreate(Type type, out object? feature)
    {
        feature = type == FeatureType ? new DefaultResponseFeature<T>(lambdaSerializer) : null;

        return feature is not null;
    }
}
```

Registering a provider is just another DI call:

```csharp title="Program.cs"
builder.Services.AddSingleton<IFeatureProvider, MyCorrelationFeatureProvider>(); // implements IFeatureProvider
```

Your provider can return singleton instances (for stateless metadata) or create fresh objects per
invocation.

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
- **Make cancellation cooperative.** Honor `context.CancellationToken` in middleware and pass it to downstream I/O.

With these patterns, you can build rich, testable pipelines around your Lambda handlers while keeping
business logic small and focused.
