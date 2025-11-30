# Lifecycle Management

AWS Lambda functions have three distinct execution phases: **Init**, **Invocation**, and **Shutdown**. Understanding and leveraging these phases is key to building high-performance, resource-efficient Lambda functions.

## Introduction

Lambda execution follows this lifecycle:

```
Cold Start → OnInit → Invocation 1 → Invocation 2 → ... → OnShutdown → Termination
```

- **OnInit**: Runs once during cold start (function initialization)
- **Invocation**: Runs for each Lambda event
- **OnShutdown**: Runs once before Lambda container terminates

aws-lambda-host provides explicit control over each phase through lifecycle handlers.

## Lambda Lifecycle Phases

### Phase 1: OnInit (Cold Start)

The OnInit phase executes once when Lambda initializes a new execution environment. Use this phase for:

- Warming up caches
- Establishing database connections
- Preloading configuration
- Initializing HTTP clients
- Loading machine learning models

**Characteristics:**
- Runs **once per execution environment**
- Executes **before the first invocation**
- Shares execution time with the first invocation
- Multiple handlers execute **concurrently**

### Phase 2: Invocation

The invocation phase processes each incoming Lambda event. This is where your handler logic executes.

**Characteristics:**
- Runs **for each event**
- Isolated scope per invocation
- Scoped services created fresh for each invocation

### Phase 3: OnShutdown

The OnShutdown phase executes once before Lambda terminates the execution environment. Use this phase for:

- Flushing logs and metrics
- Closing database connections
- Cleaning up temporary resources
- Graceful shutdown of background tasks

**Characteristics:**
- Runs **once before termination**
- Triggered by SIGTERM signal
- Limited time to complete (configurable)
- Multiple handlers execute **sequentially**

## OnInit Handlers

### Basic OnInit

```csharp title="Program.cs"
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

lambda.OnInit(async (IServiceProvider services, CancellationToken ct) =>
{
    Console.WriteLine("Initializing Lambda function...");

    // Perform one-time setup
    var cache = services.GetRequiredService<ICache>();
    await cache.WarmUpAsync(ct);

    return true; // true = continue, false = abort initialization
});

lambda.MapHandler(([Event] Request request) => new Response("Success"));

await lambda.RunAsync();
```

**Return Value:**
- `true` – Continue initialization
- `false` – Abort initialization (Lambda reports failure)

### Multiple OnInit Handlers

Register multiple initialization handlers—they execute **concurrently**:

```csharp title="Program.cs"
lambda.OnInit(async (IServiceProvider services, CancellationToken ct) =>
{
    Console.WriteLine("Warming up cache");
    var cache = services.GetRequiredService<ICache>();
    await cache.WarmUpAsync(ct);
    return true;
});

lambda.OnInit(async (IServiceProvider services, CancellationToken ct) =>
{
    Console.WriteLine("Initializing database connection pool");
    var db = services.GetRequiredService<IDatabase>();
    await db.InitializeAsync(ct);
    return true;
});

lambda.OnInit(async (IServiceProvider services, CancellationToken ct) =>
{
    Console.WriteLine("Preloading configuration");
    var config = services.GetRequiredService<IConfigService>();
    await config.LoadAsync(ct);
    return true;
});
```

**Execution**: All three handlers run **concurrently** for faster cold starts.

### OnInit with Dependency Injection

OnInit handlers can inject any registered service:

```csharp title="Program.cs"
lambda.OnInit(async (
    ICache cache,
    IDatabase database,
    ILogger<Program> logger,
    CancellationToken ct
) =>
{
    logger.LogInformation("Starting initialization");

    await Task.WhenAll(
        cache.WarmUpAsync(ct),
        database.InitializeAsync(ct)
    );

    logger.LogInformation("Initialization complete");
    return true;
});
```

### Handling OnInit Failures

If any handler returns `false`, initialization aborts:

```csharp title="Program.cs"
lambda.OnInit(async (IServiceProvider services, CancellationToken ct) =>
{
    var config = services.GetRequiredService<IConfigService>();

    try
    {
        await config.LoadAsync(ct);
        return true; // Success
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Failed to load configuration: {ex.Message}");
        return false; // Abort initialization
    }
});
```

**Result**: Lambda reports initialization failure, preventing invocations.

### OnInit Timeout

Configure OnInit timeout using `LambdaHostOptions`:

```csharp title="Program.cs"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.InitTimeout = TimeSpan.FromSeconds(10); // Default: 5 seconds
});

var lambda = builder.Build();

lambda.OnInit(async (IServiceProvider services, CancellationToken ct) =>
{
    // This cancellation token will fire after 10 seconds
    try
    {
        await LongRunningInitializationAsync(ct);
        return true;
    }
    catch (OperationCanceledException)
    {
        Console.Error.WriteLine("Initialization timed out");
        return false;
    }
});
```

## OnShutdown Handlers

### Basic OnShutdown

```csharp title="Program.cs"
lambda.OnShutdown(async (IServiceProvider services, CancellationToken ct) =>
{
    Console.WriteLine("Shutting down Lambda function...");

    var cache = services.GetRequiredService<ICache>();
    await cache.FlushAsync(ct);
});

lambda.MapHandler(([Event] Request request) => new Response("Success"));

await lambda.RunAsync();
```

**No return value required** – handlers execute and complete.

### Multiple OnShutdown Handlers

Register multiple shutdown handlers—they execute **sequentially**:

```csharp title="Program.cs"
lambda.OnShutdown(async (IServiceProvider services, CancellationToken ct) =>
{
    Console.WriteLine("1. Flushing metrics");
    var metrics = services.GetRequiredService<IMetrics>();
    await metrics.FlushAsync(ct);
});

lambda.OnShutdown(async (IServiceProvider services, CancellationToken ct) =>
{
    Console.WriteLine("2. Closing database connections");
    var db = services.GetRequiredService<IDatabase>();
    await db.CloseAsync(ct);
});

lambda.OnShutdown(async (IServiceProvider services, CancellationToken ct) =>
{
    Console.WriteLine("3. Cleanup complete");
});
```

**Execution**: Handlers run **sequentially** in registration order.

### OnShutdown with Dependency Injection

OnShutdown handlers support dependency injection:

```csharp title="Program.cs"
lambda.OnShutdown(async (
    IMetrics metrics,
    IDatabase database,
    ILogger<Program> logger,
    CancellationToken ct
) =>
{
    logger.LogInformation("Starting shutdown");

    // Sequential cleanup
    await metrics.FlushAsync(ct);
    await database.CloseAsync(ct);

    logger.LogInformation("Shutdown complete");
});
```

### Handling OnShutdown Errors

OnShutdown errors are logged but don't prevent shutdown:

```csharp title="Program.cs"
lambda.OnShutdown(async (IServiceProvider services, CancellationToken ct) =>
{
    try
    {
        var cache = services.GetRequiredService<ICache>();
        await cache.FlushAsync(ct);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Cache flush failed: {ex.Message}");
        // Shutdown continues despite error
    }
});
```

### OnShutdown Timeout

Configure shutdown timeout using `LambdaHostOptions`:

```csharp title="Program.cs"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    // Time between SIGTERM and SIGKILL
    options.ShutdownDuration = TimeSpan.FromMilliseconds(500); // Default: 500ms

    // Buffer to ensure completion before SIGKILL
    options.ShutdownDurationBuffer = TimeSpan.FromMilliseconds(50); // Default: 50ms
});

var lambda = builder.Build();

lambda.OnShutdown(async (IServiceProvider services, CancellationToken ct) =>
{
    // Cancellation token fires after (500ms - 50ms = 450ms)
    try
    {
        await GracefulShutdownAsync(ct);
    }
    catch (OperationCanceledException)
    {
        Console.Error.WriteLine("Shutdown timed out");
    }
});
```

**Shutdown Duration Options:**

```csharp
// No extension time (0ms)
options.ShutdownDuration = ShutdownDuration.NoExtensions;

// Internal extensions only (300ms)
options.ShutdownDuration = ShutdownDuration.InternalExtensions;

// External extensions (500ms) - DEFAULT
options.ShutdownDuration = ShutdownDuration.ExternalExtensions;

// Custom duration
options.ShutdownDuration = TimeSpan.FromSeconds(2);
```

## Common Patterns

### Warming Up Caches

```csharp title="Program.cs"
builder.Services.AddSingleton<ICache, MemoryCache>();

var lambda = builder.Build();

lambda.OnInit(async (ICache cache, CancellationToken ct) =>
{
    Console.WriteLine("Warming up cache...");

    // Preload frequently accessed data
    await cache.SetAsync("config", await LoadConfigAsync(ct), ct);
    await cache.SetAsync("lookup", await LoadLookupDataAsync(ct), ct);

    Console.WriteLine("Cache warmed");
    return true;
});
```

### Database Connection Pooling

```csharp title="Program.cs"
builder.Services.AddSingleton<IDatabase, Database>();

var lambda = builder.Build();

lambda.OnInit(async (IDatabase db, ILogger<Program> logger, CancellationToken ct) =>
{
    logger.LogInformation("Initializing database connection pool");
    await db.InitializePoolAsync(ct);
    return true;
});

lambda.OnShutdown(async (IDatabase db, ILogger<Program> logger, CancellationToken ct) =>
{
    logger.LogInformation("Closing database connections");
    await db.ClosePoolAsync(ct);
});
```

### Telemetry Flushing

```csharp title="Program.cs"
builder.Services.AddSingleton<ITelemetry, TelemetryService>();

var lambda = builder.Build();

lambda.OnShutdown(async (ITelemetry telemetry, ILogger<Program> logger, CancellationToken ct) =>
{
    logger.LogInformation("Flushing telemetry");
    await telemetry.FlushAsync(ct);
});
```

### Loading ML Models

```csharp title="Program.cs"
builder.Services.AddSingleton<IModelService, ModelService>();

var lambda = builder.Build();

lambda.OnInit(async (IModelService models, ILogger<Program> logger, CancellationToken ct) =>
{
    logger.LogInformation("Loading ML model");
    await models.LoadAsync("model-v1.0", ct);
    logger.LogInformation("Model loaded");
    return true;
});
```

### Configuration Preloading

```csharp title="Program.cs"
builder.Services.AddSingleton<IConfigService, ConfigService>();

var lambda = builder.Build();

lambda.OnInit(async (IConfigService config, ILogger<Program> logger, CancellationToken ct) =>
{
    logger.LogInformation("Preloading configuration from SSM");

    try
    {
        await config.LoadFromParameterStoreAsync(ct);
        logger.LogInformation("Configuration loaded successfully");
        return true;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to load configuration");
        return false; // Abort initialization
    }
});
```

## Lifecycle Configuration

### LambdaHostOptions Reference

```csharp title="Program.cs"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    // OnInit timeout (default: 5 seconds)
    options.InitTimeout = TimeSpan.FromSeconds(10);

    // Invocation cancellation buffer (default: 3 seconds)
    // Buffer before Lambda timeout to allow graceful cancellation
    options.InvocationCancellationBuffer = TimeSpan.FromSeconds(5);

    // Shutdown duration (default: 500ms)
    options.ShutdownDuration = ShutdownDuration.ExternalExtensions;

    // Shutdown buffer (default: 50ms)
    options.ShutdownDurationBuffer = TimeSpan.FromMilliseconds(100);

    // Clear Lambda output formatting (default: false)
    options.ClearLambdaOutputFormatting = true;
});
```

### InitTimeout

Controls how long OnInit handlers can run before cancellation:

```csharp
builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.InitTimeout = TimeSpan.FromSeconds(10);
});

lambda.OnInit(async (ICache cache, CancellationToken ct) =>
{
    // 'ct' fires after 10 seconds
    await cache.WarmUpAsync(ct);
    return true;
});
```

### InvocationCancellationBuffer

Controls when the invocation cancellation token fires relative to Lambda timeout:

```csharp
builder.Services.ConfigureLambdaHostOptions(options =>
{
    // Fire cancellation 5 seconds before Lambda times out
    options.InvocationCancellationBuffer = TimeSpan.FromSeconds(5);
});

lambda.MapHandler(async ([Event] Request request, CancellationToken ct) =>
{
    // If Lambda timeout is 30s, 'ct' fires after 25s
    await LongRunningOperationAsync(ct);
    return new Response("Success");
});
```

### ShutdownDuration and ShutdownDurationBuffer

Control shutdown timing:

```csharp
builder.Services.ConfigureLambdaHostOptions(options =>
{
    // Time between SIGTERM and SIGKILL
    options.ShutdownDuration = TimeSpan.FromSeconds(1);

    // Buffer to ensure completion
    options.ShutdownDurationBuffer = TimeSpan.FromMilliseconds(100);

    // Actual shutdown timeout: 1000ms - 100ms = 900ms
});

lambda.OnShutdown(async (IMetrics metrics, CancellationToken ct) =>
{
    // 'ct' fires after 900ms
    await metrics.FlushAsync(ct);
});
```

## Best Practices

### ✅ Do: Keep OnInit Fast

```csharp
// GOOD: Concurrent initialization
lambda.OnInit(async (ICache cache, CancellationToken ct) =>
{
    await cache.WarmUpAsync(ct);
    return true;
});

lambda.OnInit(async (IDatabase db, CancellationToken ct) =>
{
    await db.InitializeAsync(ct);
    return true;
});

// Both run concurrently - faster cold start
```

### ❌ Don't: Perform Slow Sequential Operations

```csharp
// BAD: Sequential initialization
lambda.OnInit(async (IServiceProvider services, CancellationToken ct) =>
{
    var cache = services.GetRequiredService<ICache>();
    await cache.WarmUpAsync(ct); // Wait...

    var db = services.GetRequiredService<IDatabase>();
    await db.InitializeAsync(ct); // Then wait again...

    return true;
});
```

### ✅ Do: Use CancellationToken in OnInit

```csharp
// GOOD: Respects timeout
lambda.OnInit(async (IConfigService config, CancellationToken ct) =>
{
    try
    {
        await config.LoadAsync(ct);
        return true;
    }
    catch (OperationCanceledException)
    {
        Console.Error.WriteLine("Initialization timed out");
        return false;
    }
});
```

### ✅ Do: Flush Telemetry in OnShutdown

```csharp
// GOOD: Ensure metrics are sent
lambda.OnShutdown(async (ITelemetry telemetry, CancellationToken ct) =>
{
    await telemetry.FlushAsync(ct);
});
```

### ❌ Don't: Ignore OnShutdown Errors

```csharp
// BAD: Silent failure
lambda.OnShutdown(async (IMetrics metrics, CancellationToken ct) =>
{
    await metrics.FlushAsync(ct); // What if this fails?
});
```

**Better:**

```csharp
// GOOD: Log errors
lambda.OnShutdown(async (IMetrics metrics, ILogger<Program> logger, CancellationToken ct) =>
{
    try
    {
        await metrics.FlushAsync(ct);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to flush metrics");
    }
});
```

### ✅ Do: Return false on Critical OnInit Failures

```csharp
// GOOD: Abort initialization on critical failure
lambda.OnInit(async (IDatabase db, ILogger<Program> logger, CancellationToken ct) =>
{
    try
    {
        await db.ConnectAsync(ct);
        return true;
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Failed to connect to database");
        return false; // Prevent invocations
    }
});
```

### ✅ Do: Use Singleton Services for OnInit/OnShutdown

```csharp
// GOOD: Singleton persists across invocations
builder.Services.AddSingleton<ICache, MemoryCache>();

lambda.OnInit(async (ICache cache, CancellationToken ct) =>
{
    await cache.WarmUpAsync(ct);
    return true;
});

lambda.OnShutdown(async (ICache cache, CancellationToken ct) =>
{
    await cache.FlushAsync(ct);
});

// Same instance used in OnInit, invocations, and OnShutdown
```

## Anti-Patterns to Avoid

### ❌ Blocking Async Code

```csharp
// BAD: Blocking async operations
lambda.OnInit((ICache cache, CancellationToken ct) =>
{
    cache.WarmUpAsync(ct).Wait(); // DON'T!
    return Task.FromResult(true);
});
```

**Better:**

```csharp
// GOOD: Proper async/await
lambda.OnInit(async (ICache cache, CancellationToken ct) =>
{
    await cache.WarmUpAsync(ct);
    return true;
});
```

---

### ❌ Ignoring CancellationToken

```csharp
// BAD: Ignoring timeout signal
lambda.OnInit(async (IConfigService config, CancellationToken ct) =>
{
    await Task.Delay(TimeSpan.FromMinutes(5)); // Ignores 'ct'!
    return true;
});
```

**Better:**

```csharp
// GOOD: Respect cancellation
lambda.OnInit(async (IConfigService config, CancellationToken ct) =>
{
    await Task.Delay(TimeSpan.FromSeconds(3), ct);
    return true;
});
```

---

### ❌ Heavy Work in OnShutdown

```csharp
// BAD: Too much work during shutdown
lambda.OnShutdown(async (IServiceProvider services, CancellationToken ct) =>
{
    var processor = services.GetRequiredService<IProcessor>();

    // Processing 10,000 records during shutdown? No!
    await processor.ProcessAllPendingRecordsAsync(ct);
});
```

**Better:**

```csharp
// GOOD: Minimal cleanup only
lambda.OnShutdown(async (IMetrics metrics, CancellationToken ct) =>
{
    // Just flush metrics
    await metrics.FlushAsync(ct);
});
```

---

### ❌ Returning false on Non-Critical Failures

```csharp
// BAD: Aborting initialization for minor issues
lambda.OnInit(async (ICache cache, CancellationToken ct) =>
{
    try
    {
        await cache.WarmUpAsync(ct);
        return true;
    }
    catch
    {
        // Cache warming failed, but Lambda can still work!
        return false; // Don't abort for this
    }
});
```

**Better:**

```csharp
// GOOD: Continue even if cache warming fails
lambda.OnInit(async (ICache cache, ILogger<Program> logger, CancellationToken ct) =>
{
    try
    {
        await cache.WarmUpAsync(ct);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Cache warm-up failed, continuing anyway");
    }

    return true; // Continue initialization
});
```

## Testing Lifecycle Handlers

### Testing OnInit

```csharp title="Tests/OnInitTests.cs"
using Xunit;
using NSubstitute;
using Microsoft.Extensions.DependencyInjection;

public class OnInitTests
{
    [Fact]
    public async Task OnInit_WarmsUpCache_ReturnsTrue()
    {
        // Arrange
        var cache = Substitute.For<ICache>();
        var services = new ServiceCollection()
            .AddSingleton(cache)
            .BuildServiceProvider();

        var cts = new CancellationTokenSource();

        // Act
        var result = await OnInitHandler(services, cts.Token);

        // Assert
        Assert.True(result);
        await cache.Received(1).WarmUpAsync(cts.Token);
    }

    private async Task<bool> OnInitHandler(IServiceProvider services, CancellationToken ct)
    {
        var cache = services.GetRequiredService<ICache>();
        await cache.WarmUpAsync(ct);
        return true;
    }
}
```

### Testing OnShutdown

```csharp title="Tests/OnShutdownTests.cs"
using Xunit;
using NSubstitute;
using Microsoft.Extensions.DependencyInjection;

public class OnShutdownTests
{
    [Fact]
    public async Task OnShutdown_FlushesMetrics()
    {
        // Arrange
        var metrics = Substitute.For<IMetrics>();
        var services = new ServiceCollection()
            .AddSingleton(metrics)
            .BuildServiceProvider();

        var cts = new CancellationTokenSource();

        // Act
        await OnShutdownHandler(services, cts.Token);

        // Assert
        await metrics.Received(1).FlushAsync(cts.Token);
    }

    private async Task OnShutdownHandler(IServiceProvider services, CancellationToken ct)
    {
        var metrics = services.GetRequiredService<IMetrics>();
        await metrics.FlushAsync(ct);
    }
}
```

## Key Takeaways

1. **OnInit** – Runs once on cold start for resource initialization
2. **OnShutdown** – Runs once before termination for cleanup
3. **Multiple Handlers** – OnInit runs concurrently, OnShutdown runs sequentially
4. **Return Values** – OnInit returns `true`/`false`; OnShutdown has no return value
5. **CancellationToken** – Always respect timeout signals
6. **Configuration** – Use `LambdaHostOptions` to configure timeouts
7. **DI Support** – Both phases support dependency injection
8. **Keep It Fast** – Minimize cold start time by keeping OnInit lean

## Next Steps

Now that you understand lifecycle management, explore related topics:

- **[Dependency Injection](/guides/dependency-injection.md)** – Inject services into lifecycle handlers
- **[Configuration](/guides/configuration.md)** – Configure lifecycle timeouts
- **[Error Handling](/guides/error-handling.md)** – Handle errors in lifecycle handlers
- **[Testing](/guides/testing.md)** – Test lifecycle handlers in isolation
- **[Handler Registration](/guides/handler-registration.md)** – Understand the invocation phase

---

Congratulations! You now understand how to control Lambda lifecycle phases for optimal performance and resource management.
