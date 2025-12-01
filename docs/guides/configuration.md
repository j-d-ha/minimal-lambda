# Configuration

`AwsLambda.Host` embraces the same configuration primitives as the rest of .NET: `IConfiguration` aggregates
settings from files and environment variables, and options bind those settings into strongly typed
objects. On top of that, `LambdaHostOptions` control the Lambda-specific runtime behavior (timeouts,
shutdown windows, serializer choices, etc.). This guide covers both layers.

## How Configuration Is Built

`LambdaApplication.CreateBuilder()` wires up the standard .NET defaults:

1. `appsettings.json` (optional) in the application root.
2. `appsettings.{Environment}.json` (optional) based on `DOTNET_ENVIRONMENT`/`ASPNETCORE_ENVIRONMENT`.
3. User secrets (Development only).
4. Environment variables (`AWS_`, `DOTNET_`, and general variables).

The builder also binds the `AwsLambdaHost` section (from JSON or environment variables) into
`LambdaHostOptions` so framework settings can live next to your app configuration.

```json title="appsettings.json"
{
  "AwsLambdaHost": {
    "InitTimeout": "00:00:10",
    "InvocationCancellationBuffer": "00:00:05",
    "ShutdownDuration": "00:00:00.5000000",
    "ShutdownDurationBuffer": "00:00:00.0500000",
    "ClearLambdaOutputFormatting": true
  }
}
```

Alternatively, set environment variables using the `AwsLambdaHost__{Option}` naming convention:

```bash
AwsLambdaHost__InvocationCancellationBuffer=00:00:05
AwsLambdaHost__ClearLambdaOutputFormatting=true
```

## LambdaHostOptions Reference

Use `builder.Services.ConfigureLambdaHostOptions` to override framework behavior at runtime or during tests.
Configuration settings and imperative code are additive—the explicit delegate runs after configuration binding.

```csharp title="Program.cs" linenums="1"
var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.InitTimeout = TimeSpan.FromSeconds(10);
    options.InvocationCancellationBuffer = TimeSpan.FromSeconds(5);
    options.ShutdownDuration = ShutdownDuration.ExternalExtensions;
    options.ShutdownDurationBuffer = TimeSpan.FromMilliseconds(100);
    options.ClearLambdaOutputFormatting = true;
});
```

| Option                         | Type                     | Default                            | Description                                                                 |
|--------------------------------|--------------------------|------------------------------------|-----------------------------------------------------------------------------|
| `InitTimeout`                  | `TimeSpan`               | 5 seconds                          | Maximum time all `OnInit` handlers collectively have before cancellation.   |
| `InvocationCancellationBuffer` | `TimeSpan`               | 3 seconds                          | Buffer subtracted from remaining time before the invocation token fires.   |
| `ShutdownDuration`             | `TimeSpan`               | `ShutdownDuration.ExternalExtensions` (500 ms) | Expected window between SIGTERM and SIGKILL.                                 |
| `ShutdownDurationBuffer`       | `TimeSpan`               | 50 ms                              | Safety margin deducted from `ShutdownDuration` to guarantee cleanup.       |
| `ClearLambdaOutputFormatting`  | `bool`                   | `false`                            | Automatically register the built-in OnInit handler that clears console formatting. |
| `BootstrapHttpClient`          | `HttpClient?`            | `null`                             | Custom client for the Lambda bootstrap runtime API.                        |
| `BootstrapOptions`             | `LambdaBootstrapOptions` | `new()`                            | Low-level runtime bootstrap configuration (timeouts, heartbeats, etc.).    |

### `InitTimeout`

Controls how long `OnInit` handlers can run before the host cancels them. Keep cold starts fast by
splitting heavy work into multiple handlers—they execute in parallel but still share this budget.

```csharp title="Program.cs" linenums="1"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.InitTimeout = TimeSpan.FromSeconds(10);
});

lambda.OnInit(async (ICache cache, CancellationToken ct) =>
{
    await cache.WarmUpAsync(ct); // ct fires after 10 seconds
    return true;
});
```

### `InvocationCancellationBuffer`

Determines when the invocation-scoped `CancellationToken` fires relative to the Lambda timeout. A
buffer gives you time to short-circuit work and flush telemetry before AWS abruptly stops the process.

```csharp title="Program.cs" linenums="1"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    // Fire cancellation 5 seconds before the Lambda timeout expires
    options.InvocationCancellationBuffer = TimeSpan.FromSeconds(5);
});

lambda.MapHandler(async ([Event] Order order, IOrderService service, CancellationToken ct) =>
    await service.ProcessAsync(order, ct));
```

### `ShutdownDuration` and `ShutdownDurationBuffer`

Lambda signals shutdown with SIGTERM and later SIGKILL. Configure how long you expect to have between
those signals and how much buffer `AwsLambda.Host` should reserve before cancelling `OnShutdown`
handlers.

```csharp title="Program.cs" linenums="1"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.ShutdownDuration = ShutdownDuration.ExternalExtensions; // 500 ms
    options.ShutdownDurationBuffer = TimeSpan.FromMilliseconds(75);  // cancel after 425 ms
});

lambda.OnShutdown(async (ITelemetry telemetry, CancellationToken ct) =>
    await telemetry.FlushAsync(ct));
```

Choose from the provided constants when possible:

- `ShutdownDuration.NoExtensions` – No extensions installed (0 ms window).
- `ShutdownDuration.InternalExtensions` – Only internal extensions (300 ms).
- `ShutdownDuration.ExternalExtensions` – External extension installed (500 ms, default).
- Any custom `TimeSpan` when your environment requires more/less time.

### `ClearLambdaOutputFormatting`

The .NET Lambda runtime captures stdout/stderr and wraps each line with its own structured record.
When you rely on structured loggers (Serilog, MEL JSON) or run locally, disable that extra formatting.
`AwsLambda.Host` registers the built-in OnInit handler automatically when this option is `true`.

```csharp title="Program.cs" linenums="1"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.ClearLambdaOutputFormatting = true;
});
```

Prefer configuration files? Set `"AwsLambdaHost": { "ClearLambdaOutputFormatting": true }` or the
`AwsLambdaHost__ClearLambdaOutputFormatting` environment variable.

### `BootstrapHttpClient`

Override the HTTP client the Lambda bootstrap uses to poll the runtime API. Most projects never touch
this, but it is handy when you need custom TLS settings or want to share a pre-configured handler for
proxy scenarios.

```csharp title="Program.cs" linenums="1"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.BootstrapHttpClient = new HttpClient(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        Proxy = WebRequest.DefaultWebProxy,
    });
});
```

### `BootstrapOptions`

`LambdaBootstrapOptions` mirror the parameters in `Amazon.Lambda.RuntimeSupport.Bootstrap`. Expose
these only when you need to tune lower-level runtime behavior such as the number of inflight
invocations or how long the bootstrap waits when polling.

```csharp title="Program.cs" linenums="1"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.BootstrapOptions = new LambdaBootstrapOptions
    {
        Client = LambdaBootstrapOptions.Client.HttpClient,
        TelemetryFlushLatency = TimeSpan.FromMilliseconds(25)
    };
});
```

## Application Configuration

Everything outside `LambdaHostOptions` works the same way it does in .NET. Use
`appsettings.json`, bind to options classes, and inject `IOptions<T>` into services.

```json title="appsettings.json"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "OrderProcessing": {
    "MaxRetries": 3,
    "TimeoutSeconds": 30,
    "EnableCaching": true
  },
  "Database": {
    "ConnectionString": "Server=localhost;Database=orders",
    "CommandTimeout": 30
  }
}
```

Include configuration files in your deployment output so Lambda can read them at runtime:

```xml title="MyLambda.csproj"
<ItemGroup>
  <Content Include="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Include="appsettings.Production.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

### Options Pattern

Create strongly typed classes and bind them inside `Program.cs`:

```csharp title="Program.cs" linenums="1"
builder.Services.Configure<OrderProcessingOptions>(
    builder.Configuration.GetSection("OrderProcessing"));

builder.Services.AddScoped<IOrderService, OrderService>();
```

Inside services, inject `IOptions<T>` (or `IOptionsSnapshot<T>`/`IOptionsMonitor<T>` when you truly
need scoped/snapshot behavior). Lambda deployments rarely change configuration at runtime, so
`IOptions<T>`—captured once during cold start—is the most predictable choice.

```csharp title="OrderService.cs" linenums="1"
public class OrderService : IOrderService
{
    private readonly OrderProcessingOptions _options;

    public OrderService(IOptions<OrderProcessingOptions> options) =>
        _options = options.Value;

    public Task<OrderResponse> ProcessAsync(Order order, CancellationToken ct) =>
        _options.EnableCaching
            ? ProcessWithCacheAsync(order, ct)
            : ProcessWithoutCacheAsync(order, ct);
}
```

| Interface             | Lifetime  | Reloads        | Lambda Guidance                                                    |
|-----------------------|-----------|----------------|--------------------------------------------------------------------|
| `IOptions<T>`         | Singleton | Never          | ✅ Recommended—configuration ships with the deployment package.     |
| `IOptionsSnapshot<T>` | Scoped    | Per invocation | Use only if you vary config between invocations (rare).            |
| `IOptionsMonitor<T>`  | Singleton | On change      | Rarely useful unless you reload from remote providers at runtime.  |

### Environment-Specific Files

Ship overrides per environment by naming convention:

```
MyLambda/
├── appsettings.json
├── appsettings.Development.json
└── appsettings.Production.json
```

The builder automatically loads `appsettings.{Environment}.json` when `DOTNET_ENVIRONMENT` is set.
You can add more sources manually via `builder.Configuration.AddJsonFile(...)` or
`builder.Configuration.AddEnvironmentVariables()`.

### Environment Variables

Environment variables override configuration files. Configure them via the AWS Console, SAM/CDK
templates, Terraform, or GitHub Actions.

```yaml title="template.yaml" linenums="1"
Resources:
  MyFunction:
    Type: AWS::Serverless::Function
    Properties:
      Environment:
        Variables:
          DATABASE__CONNECTIONSTRING: !Ref DatabaseConnectionString
          API_KEY: !Ref ApiKey
          AwsLambdaHost__ClearLambdaOutputFormatting: true
```

Prefer the double underscore `__` separator when targeting hierarchical keys.

Access the values directly or through bound options:

```csharp title="Program.cs" linenums="1"
var apiKey = builder.Configuration["API_KEY"];

builder.Services.Configure<ExternalApiOptions>(options =>
{
    options.BaseUrl = builder.Configuration["ExternalApi:BaseUrl"] ?? string.Empty;
    options.ApiKey = builder.Configuration["API_KEY"] ?? string.Empty;
});
```

## JSON Serialization Configuration

`AwsLambda.Host` defaults to `System.Text.Json` with camelCase naming, case-insensitive reads, and
null-value ignoring. Because the Lambda serializer is an `ILambdaSerializer` singleton, you configure
it by registering the serializer instance you want (either the built-in
`DefaultLambdaJsonSerializer` with custom `JsonSerializerOptions` or your own implementation).

### Native AOT / Source-Generated Serialization

Native AOT deployments require a `JsonSerializerContext` that describes every type the handler emits
or consumes.

```csharp title="MySerializerContext.cs" linenums="1"
[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(Response))]
public partial class MySerializerContext : JsonSerializerContext;

builder.Services.AddLambdaSerializerWithContext<MySerializerContext>();
```

### Custom Serializer Options

```csharp title="Program.cs" linenums="1"
builder.Services.AddSingleton<ILambdaSerializer>(_ =>
    new DefaultLambdaJsonSerializer(options =>
    {
        options.PropertyNamingPolicy = null; // keep property names as-is
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    })
);
```

Registering the serializer replaces the default `ILambdaSerializer`. Use this pattern when you need to
tweak naming policies, converters, or reference handling without adopting a source-generated context.

## Configuration Validation

Validate bound options at startup so bad configuration never reaches production.

```csharp title="Program.cs" linenums="1"
builder.Services.AddOptions<OrderProcessingOptions>()
    .Bind(builder.Configuration.GetSection("OrderProcessing"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

For more complex rules implement `IValidatableObject` or add a custom validator.

## Best Practices

- **Prefer `IOptions<T>`** – Configuration rarely changes per invocation; capture it during cold start.
- **Validate on startup** – `ValidateOnStart()` catches missing sections before Lambda accepts traffic.
- **Copy `appsettings.*` to the output** – Without it, Lambda cannot load the files.
- **Use environment variables for secrets** – Combine SAM/CDK parameters with Secrets Manager references.
- **Stick to `AwsLambdaHost` section for framework knobs** – Keeps host settings discoverable and
  separate from business configuration.
- **Clear Lambda output formatting when you own logging** – Avoid double-wrapping JSON payloads.

## Troubleshooting

**Configuration section not found** – Call `Exists()` on the section and ensure the file is copied to
`bin/Release/net8.0/publish`.

**`appsettings.json` missing in production** – Confirm `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>` is set for each config file.

**Environment variable not loaded** – Remember to add `builder.Configuration.AddEnvironmentVariables()` if you call `CreateBuilder(new LambdaApplicationOptions { DisableDefaults = true })`.

**LambdaHostOptions ignored** – Verify the JSON is under `"AwsLambdaHost"` or that the environment
variable uses `AwsLambdaHost__OptionName`.

## Next Steps

- **[Lifecycle Management](lifecycle-management.md)** – Use `LambdaHostOptions` to fine-tune OnInit/OnShutdown.
- **[Dependency Injection](dependency-injection.md)** – Inject configured options and keyed services safely.
- **[Error Handling](error-handling.md)** – Tie timeout settings to retry logic.
- **[Testing](testing.md)** – Override configuration in unit tests with in-memory providers.
