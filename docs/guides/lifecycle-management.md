# Lifecycle Management

`MinimalLambda` exposes the entire Lambda container lifecycle so you can prepare resources during cold start, react to each invocation, and cleanly shut down when AWS reclaims the execution environment.

## Execution Flow at a Glance

```
Cold Start → OnInit → Invocation 1..N → OnShutdown → Termination
```

- **OnInit** – Runs once per execution environment before the first invocation. Used to warm caches, hydrate clients, or clear Lambda-specific defaults.
- **Invocation** – Runs for every event in the normal middleware/handler pipeline. Cancellation tokens respect `InvocationCancellationBuffer`.
- **OnShutdown** – Runs once when the runtime receives SIGTERM or when the host stops. Used to flush telemetry or close connections.

Both OnInit and OnShutdown execute outside the invocation pipeline, but `MinimalLambda` creates a brand-new `IServiceScope` for every handler so you can resolve scoped services safely.

## OnInit: Cold Start Hooks

```csharp title="Program.cs"
using MinimalLambda;
using Microsoft.Extensions.DependencyInjection;

var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.ClearLambdaOutputFormatting = true; // Stop Lambda from wrapping console output
    options.InitTimeout = TimeSpan.FromSeconds(10); // Optional override
});

var lambda = builder.Build();

lambda.OnInit(async (IDistributedCache cache, ILogger<Program> logger, CancellationToken ct) =>
{
    logger.LogInformation("Priming cache...");
    await cache.GetStringAsync("preload-key", ct);
    return true; // Keep hosting if every handler returns true
});

lambda.MapHandler(([Event] Request request) => new Response("OK"));

await lambda.RunAsync();
```

**OnInit characteristics**

- Runs once per execution environment and shares time with your first invocation.
- Each handler receives a linked `CancellationToken` that cancels when `InitTimeout` elapses or the host stops (default 5 seconds).
- You may register multiple handlers; `MinimalLambda` runs them **concurrently** via `Task.WhenAll`.
- Returning a `bool`/`Task<bool>` is optional. If you return a value it controls whether the cold start continues (`true`) or aborts (`false`); if you return `void`/`Task`, `MinimalLambda` assumes success.
- Exceptions are aggregated. If any handler throws, the framework logs all failures and aborts initialization.

### Handling Failure and Timeouts

```csharp title="Program.cs"
lambda.OnInit(async (IConfigLoader config, ILogger<Program> logger, CancellationToken ct) =>
{
    try
    {
        await config.LoadAsync(ct);
        return true;
    }
    catch (OperationCanceledException)
    {
        logger.LogError("Config warmup timed out before InitTimeout.");
        return false;
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Failed to prepare configuration.");
        return false;
    }
});
```

If any handler returns `false`, AWS receives an initialization failure and never routes traffic to that execution environment.

### Clearing Lambda Output Formatting

The .NET Lambda runtime captures console output and re-hydrates it into Lambda platform logs. When you prefer to emit your own structured logs (Serilog, MEL JSON, etc.), enable `ClearLambdaOutputFormatting` so an OnInit handler executes before the first invocation:

```csharp title="Program.cs"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.ClearLambdaOutputFormatting = true;
});
```

You can toggle the same setting from configuration (e.g., `appsettings.json` or environment variables) via the `LambdaHostOptions` binding.

## OnShutdown: Container Teardown Hooks

```csharp title="Program.cs"
lambda.OnShutdown(async (ITelemetrySink telemetry, ILogger<Program> logger, CancellationToken ct) =>
{
    try
    {
        logger.LogInformation("Flushing telemetry before shutdown.");
        await telemetry.FlushAsync(ct);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Telemetry flush failed; continuing shutdown.");
    }
});
```

**OnShutdown characteristics**

- Runs once per execution environment when AWS sends SIGTERM or the application host stops.
- `HostOptionsPostConfiguration` sets `HostOptions.ShutdownTimeout` to `ShutdownDuration - ShutdownDurationBuffer`, so the `CancellationToken` you receive will fire after that window (500ms - 50ms by default).
- Multiple handlers execute **concurrently**. Each runs inside its own scope so scoped dependencies remain valid even though the invocation pipeline is idle.
- Exceptions are aggregated and rethrown from `StopAsync`. Use structured logging inside handlers to capture root causes before the host tears down.

### Multiple Handlers

```csharp title="Program.cs"
lambda.OnShutdown(async (IMetrics metrics, CancellationToken ct) =>
{
    await metrics.FlushAsync(ct);
});

lambda.OnShutdown(async (IDbConnectionPool pool, CancellationToken ct) =>
{
    await pool.DisposeAsync(ct);
});
```

Both handlers are awaited simultaneously. Keep shutdown work small—only the remaining `ShutdownDuration - ShutdownDurationBuffer` window is available.

## Dependency Scopes and Injection

OnInit and OnShutdown handlers support the same source-generated dependency injection experience as middleware and handlers:

- Request only what you need. The generated delegate resolves typed parameters (services, keyed services, `ILambdaHostContext`, `CancellationToken`, etc.).
- `MinimalLambda` creates a new `IServiceScope` for every handler invocation, ensuring scoped services (database units of work, caches) are isolated even though you are outside the invocation pipeline.
- The `IServiceProvider` parameter gives you direct access to the scope for manual resolution when needed.

```csharp title="Program.cs"
lambda.OnInit(async (
    IServiceProvider scope,
    KeyedService<MyClient>("primary"),
    CancellationToken ct
) =>
{
    await MyWarmupAsync(scope, MyClient, ct);
    return true;
});
```

## Configuring Lifecycle Behavior

Use `ConfigureLambdaHostOptions` to shape lifecycle behavior centrally or bind the same settings from configuration:

```csharp title="Program.cs"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.InitTimeout = TimeSpan.FromSeconds(10);                // Cold-start budget
    options.InvocationCancellationBuffer = TimeSpan.FromSeconds(5); // Pre-timeout buffer per invocation
    options.ShutdownDuration = ShutdownDuration.ExternalExtensions; // SIGTERM→SIGKILL window
    options.ShutdownDurationBuffer = TimeSpan.FromMilliseconds(100); // Safety margin before SIGKILL
    options.ClearLambdaOutputFormatting = true;                     // Run OnInitClearLambdaOutputFormatting
});
```

**Options summary**

- `InitTimeout` – Maximum time all OnInit handlers collectively have before cancellation.
- `InvocationCancellationBuffer` – Buffer subtracted from each invocation's remaining execution time; used by `ILambdaCancellationFactory`.
- `ShutdownDuration` – Expected gap between SIGTERM and SIGKILL (0 ms, 300 ms, 500 ms, or a custom `TimeSpan`).
- `ShutdownDurationBuffer` – Amount deducted from `ShutdownDuration` to guarantee shutdown completes before SIGKILL.
- `ClearLambdaOutputFormatting` – When `true`, automatically registers the built-in OnInit handler that clears Lambda’s console formatting.

## Common Patterns

### Warm Databases and Caches

```csharp title="Program.cs"
builder.Services.AddSingleton<IDatabaseSessionFactory, AuroraFactory>();

lambda.OnInit(async (IDatabaseSessionFactory factory, CancellationToken ct) =>
{
    await factory.CreateWarmSessionAsync(ct);
    return true;
});
```

### Preload Configuration or Secrets

```csharp title="Program.cs"
lambda.OnInit(async (ISecretProvider secrets, ILogger<Program> logger, CancellationToken ct) =>
{
    await secrets.PreloadAsync(ct);
    logger.LogInformation("Secrets cached for fast access.");
    return true;
});
```

### Flush Telemetry on Shutdown

```csharp title="Program.cs"
builder.Services.AddSingleton<ITelemetry, TelemetryService>();

lambda.OnShutdown(async (ITelemetry telemetry, CancellationToken ct) =>
{
    await telemetry.FlushAsync(ct);
});
```

## Best Practices

- Keep OnInit lean and parallel. Split heavy work into separate handlers so they can run concurrently.
- Always observe the provided `CancellationToken`. Respecting cancellation is the difference between graceful shutdown and a forced SIGKILL.
- Only return `false` from OnInit for truly fatal issues (missing configuration, corrupt state). Log-and-continue for non-critical failures.
- Use `ClearLambdaOutputFormatting` when you emit structured logs and want complete control over console output.
- Run diagnostics or telemetry flushes in OnShutdown, but avoid work that exceeds the remaining window.
- Keep supporting types (record definitions, helper classes) at the bottom of `Program.cs` to make the lifecycle wiring easy to read.

## Next Steps

- [Dependency Injection](dependency-injection.md) – Understand scoped lifetimes, keyed services, and context access from lifecycle handlers.
- [Middleware](middleware.md) – Build pipelines that operate during the invocation phase using the same DI primitives.
- [Configuration](../getting-started/core-concepts.md) – Review how lifecycle settings integrate with envelopes, handlers, and host options.
