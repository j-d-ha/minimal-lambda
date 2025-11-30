# Middleware

Middleware provides a powerful mechanism for building composable pipelines around your Lambda handlers. Inspired by ASP.NET Core middleware, aws-lambda-host middleware enables cross-cutting concerns like logging, validation, metrics, error handling, and more—all without cluttering your handler code.

## Introduction

Middleware components execute in sequence, forming a pipeline around your handler:

```
Request → Middleware 1 → Middleware 2 → Handler → Middleware 2 → Middleware 1 → Response
```

Each middleware can:

- Inspect the request before the handler executes
- Short-circuit the pipeline (skip the handler)
- Execute logic after the handler completes
- Handle exceptions from downstream components

## Basic Middleware

### Inline Middleware

The simplest form of middleware is an inline lambda function:

```csharp title="Program.cs"
using AwsLambda.Host;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

lambda.UseMiddleware(async (context, next) =>
{
    Console.WriteLine("Before handler");
    await next(context);
    Console.WriteLine("After handler");
});

lambda.MapHandler(([Event] Request request) =>
{
    Console.WriteLine("Handler executing");
    return new Response("Success");
});

await lambda.RunAsync();
```

**Output:**
```
Before handler
Handler executing
After handler
```

### Middleware Pipeline Composition

Multiple middleware components execute in the order they're registered:

```csharp title="Program.cs"
lambda.UseMiddleware(async (context, next) =>
{
    Console.WriteLine("[Middleware 1]: Before");
    await next(context);
    Console.WriteLine("[Middleware 1]: After");
});

lambda.UseMiddleware(async (context, next) =>
{
    Console.WriteLine("[Middleware 2]: Before");
    await next(context);
    Console.WriteLine("[Middleware 2]: After");
});

lambda.MapHandler(([Event] Request request) =>
{
    Console.WriteLine("[Handler]: Executing");
    return new Response("Success");
});
```

**Output:**
```
[Middleware 1]: Before
[Middleware 2]: Before
[Handler]: Executing
[Middleware 2]: After
[Middleware 1]: After
```

## ILambdaHostContext

Middleware receives an `ILambdaHostContext` which provides access to invocation details and services.

### Context Properties

```csharp title="Program.cs"
lambda.UseMiddleware(async (context, next) =>
{
    // Access DI container
    var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Access cancellation token
    if (context.CancellationToken.IsCancellationRequested)
    {
        logger.LogWarning("Request cancelled before handler");
        return;
    }

    // Store invocation-scoped data
    context.Items["RequestId"] = Guid.NewGuid().ToString();
    context.Items["StartTime"] = DateTimeOffset.UtcNow;

    await next(context);

    // Access stored data
    var startTime = (DateTimeOffset)context.Items["StartTime"];
    var duration = DateTimeOffset.UtcNow - startTime;
    logger.LogInformation("Request completed in {DurationMs}ms", duration.TotalMilliseconds);
});
```

### Accessing Event and Response

Use the `Features` collection to access typed event and response data:

```csharp title="Program.cs"
using AwsLambda.Host.Abstractions.Features;

lambda.UseMiddleware(async (context, next) =>
{
    // Access the deserialized event
    var eventFeature = context.Features.Get<IEventFeature<Request>>();
    if (eventFeature != null)
    {
        var request = eventFeature.Event;
        Console.WriteLine($"Processing request: {request.Name}");
    }

    await next(context);

    // Access the response
    var responseFeature = context.Features.Get<IResponseFeature<Response>>();
    if (responseFeature != null)
    {
        var response = responseFeature.Response;
        Console.WriteLine($"Response: {response.Message}");
    }
});
```

## Common Middleware Patterns

### Logging Middleware

```csharp title="Program.cs"
using System.Diagnostics;

lambda.UseMiddleware(async (context, next) =>
{
    var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var stopwatch = Stopwatch.StartNew();

    logger.LogInformation("Request starting");

    try
    {
        await next(context);
        stopwatch.Stop();
        logger.LogInformation("Request completed successfully in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        logger.LogError(ex, "Request failed after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        throw;
    }
});
```

### Error Handling Middleware

```csharp title="Program.cs"
lambda.UseMiddleware(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (ValidationException ex)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Validation failed: {Message}", ex.Message);

        // Set error response
        var responseFeature = context.Features.Get<IResponseFeature<Response>>();
        if (responseFeature != null)
        {
            responseFeature.Response = new Response($"Validation error: {ex.Message}");
        }

        // Don't re-throw - error handled gracefully
    }
    catch (Exception ex)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception");
        throw; // Re-throw unhandled exceptions
    }
});
```

### Validation Middleware

```csharp title="Program.cs"
using AwsLambda.Host.Abstractions.Features;

lambda.UseMiddleware(async (context, next) =>
{
    var eventFeature = context.Features.Get<IEventFeature<OrderRequest>>();
    if (eventFeature != null)
    {
        var request = eventFeature.Event;

        // Validate request
        if (string.IsNullOrEmpty(request.OrderId))
        {
            throw new ValidationException("OrderId is required");
        }

        if (request.Amount <= 0)
        {
            throw new ValidationException("Amount must be positive");
        }
    }

    await next(context);
});
```

### Metrics Middleware

```csharp title="Program.cs"
using System.Diagnostics;

lambda.UseMiddleware(async (context, next) =>
{
    var metrics = context.ServiceProvider.GetRequiredService<IMetricsCollector>();
    var stopwatch = Stopwatch.StartNew();

    metrics.IncrementCounter("requests.total");

    try
    {
        await next(context);

        stopwatch.Stop();
        metrics.RecordDuration("requests.duration", stopwatch.Elapsed);
        metrics.IncrementCounter("requests.success");
    }
    catch (Exception)
    {
        stopwatch.Stop();
        metrics.IncrementCounter("requests.failed");
        throw;
    }
});
```

### Caching Middleware

```csharp title="Program.cs"
using AwsLambda.Host.Abstractions.Features;

lambda.UseMiddleware(async (context, next) =>
{
    var cache = context.ServiceProvider.GetRequiredService<ICache>();
    var eventFeature = context.Features.Get<IEventFeature<Request>>();

    if (eventFeature != null)
    {
        var request = eventFeature.Event;
        var cacheKey = $"request:{request.Id}";

        // Check cache
        if (cache.TryGet<Response>(cacheKey, out var cachedResponse))
        {
            var responseFeature = context.Features.Get<IResponseFeature<Response>>();
            if (responseFeature != null)
            {
                responseFeature.Response = cachedResponse;
            }
            return; // Short-circuit - skip handler
        }

        // Execute handler
        await next(context);

        // Cache response
        var finalResponseFeature = context.Features.Get<IResponseFeature<Response>>();
        if (finalResponseFeature?.Response != null)
        {
            cache.Set(cacheKey, finalResponseFeature.Response, TimeSpan.FromMinutes(5));
        }
    }
    else
    {
        await next(context);
    }
});
```

## Class-Based Middleware

For reusable middleware, create a class with constructor injection.

### Defining Middleware Classes

```csharp title="Middleware/LoggingMiddleware.cs"
using AwsLambda.Host.Abstractions;
using Microsoft.Extensions.Logging;

public class LoggingMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(ILambdaHostContext context, LambdaInvocationDelegate next)
    {
        _logger.LogInformation("Request starting");

        try
        {
            await next(context);
            _logger.LogInformation("Request completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request failed");
            throw;
        }
    }
}
```

### Registering Class-Based Middleware

```csharp title="Program.cs"
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;

var builder = LambdaApplication.CreateBuilder();

// Register middleware as Singleton
builder.Services.AddSingleton<LoggingMiddleware>();

var lambda = builder.Build();

// Use class-based middleware
lambda.UseMiddleware<LoggingMiddleware>();

lambda.MapHandler(([Event] Request request) => new Response("Success"));

await lambda.RunAsync();
```

### Multiple Class-Based Middleware

```csharp title="Program.cs"
builder.Services.AddSingleton<LoggingMiddleware>();
builder.Services.AddSingleton<MetricsMiddleware>();
builder.Services.AddSingleton<ValidationMiddleware>();

var lambda = builder.Build();

// Execution order: Logging → Metrics → Validation → Handler
lambda.UseMiddleware<LoggingMiddleware>();
lambda.UseMiddleware<MetricsMiddleware>();
lambda.UseMiddleware<ValidationMiddleware>();

lambda.MapHandler(([Event] Request request) => new Response("Success"));
```

## Advanced Patterns

### Conditional Middleware

Execute middleware only under certain conditions:

```csharp title="Program.cs"
lambda.UseMiddleware(async (context, next) =>
{
    var eventFeature = context.Features.Get<IEventFeature<Request>>();

    // Only log for specific request types
    if (eventFeature?.Event?.Type == "order")
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Processing order request");
    }

    await next(context);
});
```

### Short-Circuiting

Skip the handler and downstream middleware:

```csharp title="Program.cs"
lambda.UseMiddleware(async (context, next) =>
{
    var cache = context.ServiceProvider.GetRequiredService<ICache>();
    var eventFeature = context.Features.Get<IEventFeature<Request>>();

    if (eventFeature != null && cache.TryGet(eventFeature.Event.Id, out Response cached))
    {
        var responseFeature = context.Features.Get<IResponseFeature<Response>>();
        if (responseFeature != null)
        {
            responseFeature.Response = cached;
        }
        return; // SHORT-CIRCUIT: Skip handler
    }

    await next(context); // Continue pipeline
});
```

### Middleware with Configuration

```csharp title="Middleware/CachingMiddleware.cs"
using AwsLambda.Host.Abstractions;
using Microsoft.Extensions.Options;

public class CachingMiddleware
{
    private readonly ICache _cache;
    private readonly CachingOptions _options;

    public CachingMiddleware(ICache cache, IOptions<CachingOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public async Task InvokeAsync(ILambdaHostContext context, LambdaInvocationDelegate next)
    {
        if (!_options.Enabled)
        {
            await next(context);
            return;
        }

        var eventFeature = context.Features.Get<IEventFeature<Request>>();
        if (eventFeature != null)
        {
            var cacheKey = $"request:{eventFeature.Event.Id}";

            if (_cache.TryGet<Response>(cacheKey, out var cached))
            {
                var responseFeature = context.Features.Get<IResponseFeature<Response>>();
                if (responseFeature != null)
                {
                    responseFeature.Response = cached;
                }
                return;
            }

            await next(context);

            var finalResponseFeature = context.Features.Get<IResponseFeature<Response>>();
            if (finalResponseFeature?.Response != null)
            {
                _cache.Set(cacheKey, finalResponseFeature.Response, _options.CacheDuration);
            }
        }
        else
        {
            await next(context);
        }
    }
}
```

```csharp title="Program.cs"
builder.Services.Configure<CachingOptions>(
    builder.Configuration.GetSection("Caching")
);

builder.Services.AddSingleton<ICache, MemoryCache>();
builder.Services.AddSingleton<CachingMiddleware>();

var lambda = builder.Build();
lambda.UseMiddleware<CachingMiddleware>();
```

### Middleware Factory Pattern

```csharp title="Program.cs"
lambda.UseMiddleware(async (context, next) =>
{
    var factory = context.ServiceProvider.GetRequiredService<IMiddlewareFactory>();
    var customMiddleware = factory.Create("validation");

    await customMiddleware.InvokeAsync(context, next);
});
```

## Execution Order

Understanding middleware execution order is critical for building correct pipelines.

### Registration Order

Middleware executes in the order registered:

```csharp title="Program.cs"
// Execution order: 1 → 2 → 3 → Handler → 3 → 2 → 1
lambda.UseMiddleware<Middleware1>(); // 1
lambda.UseMiddleware<Middleware2>(); // 2
lambda.UseMiddleware<Middleware3>(); // 3
lambda.MapHandler(/* handler */);
```

### Typical Ordering

```csharp title="Program.cs"
// 1. Error handling (catch all exceptions)
lambda.UseMiddleware<ErrorHandlingMiddleware>();

// 2. Logging (log all requests)
lambda.UseMiddleware<LoggingMiddleware>();

// 3. Metrics (measure all requests)
lambda.UseMiddleware<MetricsMiddleware>();

// 4. Authentication (verify identity)
lambda.UseMiddleware<AuthenticationMiddleware>();

// 5. Authorization (check permissions)
lambda.UseMiddleware<AuthorizationMiddleware>();

// 6. Validation (validate input)
lambda.UseMiddleware<ValidationMiddleware>();

// 7. Caching (cache responses)
lambda.UseMiddleware<CachingMiddleware>();

// 8. Handler
lambda.MapHandler(/* handler */);
```

**Why this order?**

- **Error handling first** – Catches exceptions from all downstream middleware
- **Logging early** – Logs all requests (including failures)
- **Metrics early** – Measures all requests (including failures)
- **Auth before validation** – No point validating unauthenticated requests
- **Caching last** – Only cache authenticated, authorized, validated requests

## State Management

### Context.Items (Per-Invocation)

Store temporary data scoped to a single invocation:

```csharp title="Program.cs"
lambda.UseMiddleware(async (context, next) =>
{
    // Set invocation-scoped data
    context.Items["RequestId"] = Guid.NewGuid().ToString();
    context.Items["UserId"] = "user123";

    await next(context);
});

lambda.UseMiddleware(async (context, next) =>
{
    // Access invocation-scoped data from previous middleware
    var requestId = context.Items["RequestId"] as string;
    var userId = context.Items["UserId"] as string;

    Console.WriteLine($"Request {requestId} for user {userId}");

    await next(context);
});
```

**Cleared after each invocation.**

### Context.Properties (Cross-Invocation)

Store shared data configured during the build phase:

```csharp title="Program.cs"
var lambda = builder.Build();

// Set cross-invocation properties
lambda.Properties["Version"] = "1.0.0";
lambda.Properties["Environment"] = "Production";

lambda.UseMiddleware(async (context, next) =>
{
    // Access shared properties
    var version = context.Properties["Version"] as string;
    var env = context.Properties["Environment"] as string;

    Console.WriteLine($"App version {version} running in {env}");

    await next(context);
});
```

**Persists across invocations.**

## Best Practices

### ✅ Do: Keep Middleware Focused

```csharp
// GOOD: Single responsibility
public class LoggingMiddleware
{
    public async Task InvokeAsync(ILambdaHostContext context, LambdaInvocationDelegate next)
    {
        _logger.LogInformation("Request starting");
        await next(context);
        _logger.LogInformation("Request completed");
    }
}
```

### ❌ Don't: Mix Concerns in Middleware

```csharp
// BAD: Too many responsibilities
public class EverythingMiddleware
{
    public async Task InvokeAsync(ILambdaHostContext context, LambdaInvocationDelegate next)
    {
        // Logging
        _logger.LogInformation("Request starting");

        // Metrics
        _metrics.IncrementCounter("requests");

        // Validation
        if (string.IsNullOrEmpty(request.Id)) throw new ValidationException();

        // Caching
        if (_cache.TryGet(key, out var cached)) return cached;

        await next(context);

        // More logic...
    }
}
```

### ✅ Do: Use Class-Based Middleware for Reusability

```csharp
// GOOD: Reusable across projects
public class MetricsMiddleware
{
    private readonly IMetricsCollector _metrics;

    public MetricsMiddleware(IMetricsCollector metrics)
    {
        _metrics = metrics;
    }

    public async Task InvokeAsync(ILambdaHostContext context, LambdaInvocationDelegate next)
    {
        _metrics.IncrementCounter("requests.total");
        await next(context);
    }
}
```

### ✅ Do: Handle Exceptions Appropriately

```csharp
// GOOD: Catch, log, and re-throw
lambda.UseMiddleware(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Request failed");
        throw; // Re-throw to let Lambda handle it
    }
});
```

### ❌ Don't: Swallow Exceptions Silently

```csharp
// BAD: Silent failure
lambda.UseMiddleware(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch
    {
        // Exception swallowed - Lambda thinks it succeeded!
    }
});
```

### ✅ Do: Order Middleware Intentionally

```csharp
// GOOD: Error handling wraps everything
lambda.UseMiddleware<ErrorHandlingMiddleware>();
lambda.UseMiddleware<LoggingMiddleware>();
lambda.UseMiddleware<ValidationMiddleware>();
```

### ❌ Don't: Place Error Handling Last

```csharp
// BAD: Error handling can't catch validation errors
lambda.UseMiddleware<ValidationMiddleware>();
lambda.UseMiddleware<ErrorHandlingMiddleware>(); // Too late!
```

## Anti-Patterns to Avoid

### ❌ Blocking Async Code

```csharp
// BAD: Blocking async operations
lambda.UseMiddleware(async (context, next) =>
{
    var result = SomeAsyncMethod().Result; // DON'T!
    await next(context);
});
```

**Better approach:**

```csharp
// GOOD: Proper async/await
lambda.UseMiddleware(async (context, next) =>
{
    var result = await SomeAsyncMethod();
    await next(context);
});
```

---

### ❌ Forgetting to Call next()

```csharp
// BAD: Handler never executes
lambda.UseMiddleware(async (context, next) =>
{
    Console.WriteLine("Before handler");
    // Forgot to call next(context) - handler never runs!
});
```

**Better approach:**

```csharp
// GOOD: Always call next() unless short-circuiting intentionally
lambda.UseMiddleware(async (context, next) =>
{
    Console.WriteLine("Before handler");
    await next(context);
    Console.WriteLine("After handler");
});
```

---

### ❌ Mutating Context Incorrectly

```csharp
// BAD: Replacing context breaks downstream middleware
lambda.UseMiddleware(async (context, next) =>
{
    context = new LambdaHostContext(); // DON'T!
    await next(context);
});
```

**Better approach:**

```csharp
// GOOD: Use Items or Properties to store data
lambda.UseMiddleware(async (context, next) =>
{
    context.Items["CustomData"] = "value";
    await next(context);
});
```

## Testing Middleware

### Unit Testing Inline Middleware

```csharp title="Tests/MiddlewareTests.cs"
using Xunit;
using NSubstitute;
using AwsLambda.Host.Abstractions;

public class MiddlewareTests
{
    [Fact]
    public async Task LoggingMiddleware_LogsBeforeAndAfter()
    {
        // Arrange
        var context = Substitute.For<ILambdaHostContext>();
        var logger = Substitute.For<ILogger<Program>>();
        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(typeof(ILogger<Program>)).Returns(logger);
        context.ServiceProvider.Returns(serviceProvider);

        var nextCalled = false;
        Task Next(ILambdaHostContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await LoggingMiddleware(context, Next);

        // Assert
        Assert.True(nextCalled);
        logger.Received(1).LogInformation(Arg.Any<string>());
    }

    private async Task LoggingMiddleware(ILambdaHostContext context, LambdaInvocationDelegate next)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Request starting");
        await next(context);
    }
}
```

### Unit Testing Class-Based Middleware

```csharp title="Tests/LoggingMiddlewareTests.cs"
using Xunit;
using NSubstitute;
using AwsLambda.Host.Abstractions;

public class LoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_LogsRequestStartAndCompletion()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingMiddleware>>();
        var middleware = new LoggingMiddleware(logger);
        var context = Substitute.For<ILambdaHostContext>();

        var nextCalled = false;
        Task Next(ILambdaHostContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await middleware.InvokeAsync(context, Next);

        // Assert
        Assert.True(nextCalled);
        logger.Received(1).LogInformation("Request starting");
        logger.Received(1).LogInformation("Request completed successfully");
    }
}
```

## Key Takeaways

1. **Middleware Pipelines** – Compose cross-cutting concerns around handlers
2. **Execution Order** – Middleware executes in registration order
3. **ILambdaHostContext** – Access services, items, and features
4. **Class-Based Middleware** – Reusable with constructor injection
5. **Short-Circuiting** – Skip handler by not calling `next()`
6. **Error Handling** – Place error-handling middleware first
7. **State Management** – Use `Items` (per-invocation) or `Properties` (shared)
8. **Testing** – Unit test middleware in isolation

## Next Steps

Now that you understand middleware, explore related topics:

- **[Dependency Injection](/guides/dependency-injection.md)** – Inject services into middleware
- **[Lifecycle Management](/guides/lifecycle-management.md)** – OnInit and OnShutdown hooks
- **[Error Handling](/guides/error-handling.md)** – Build error-handling middleware
- **[Testing](/guides/testing.md)** – Test middleware components
- **[Handler Registration](/guides/handler-registration.md)** – Understand handler execution

---

Congratulations! You now understand how to build composable middleware pipelines for your Lambda functions.
