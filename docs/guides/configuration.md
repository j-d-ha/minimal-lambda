# Configuration

Proper configuration is essential for building robust Lambda functions. aws-lambda-host provides
flexible configuration through `LambdaHostOptions` for framework behavior, plus standard ASP.NET
Core configuration patterns for application settings.

## Introduction

Configuration in aws-lambda-host falls into two categories:

1. **Framework Configuration** – `LambdaHostOptions` controls Lambda lifecycle, timeouts, and
   serialization
2. **Application Configuration** – `appsettings.json`, environment variables, and the options
   pattern for your business logic

Both integrate seamlessly with `Microsoft.Extensions.Configuration` and dependency injection.

## LambdaHostOptions

`LambdaHostOptions` controls core framework behavior.

### Configuration Method

```csharp title="Program.cs"
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;

var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.InitTimeout = TimeSpan.FromSeconds(10);
    options.InvocationCancellationBuffer = TimeSpan.FromSeconds(5);
    options.ShutdownDuration = ShutdownDuration.ExternalExtensions;
    options.ShutdownDurationBuffer = TimeSpan.FromMilliseconds(100);
    options.ClearLambdaOutputFormatting = true;
});

var lambda = builder.Build();
```

### Configuration Options Reference

| Option                         | Type                     | Default   | Description                                                |
|--------------------------------|--------------------------|-----------|------------------------------------------------------------|
| `InitTimeout`                  | `TimeSpan`               | 5 seconds | Maximum time for OnInit phase before cancellation          |
| `InvocationCancellationBuffer` | `TimeSpan`               | 3 seconds | Buffer before Lambda timeout to trigger cancellation token |
| `ShutdownDuration`             | `TimeSpan`               | 500ms     | Time between SIGTERM and SIGKILL signals                   |
| `ShutdownDurationBuffer`       | `TimeSpan`               | 50ms      | Safety buffer subtracted from ShutdownDuration             |
| `ClearLambdaOutputFormatting`  | `bool`                   | `false`   | Remove Lambda runtime log formatting                       |
| `BootstrapHttpClient`          | `HttpClient?`            | `null`    | Custom HTTP client for Lambda bootstrap                    |
| `BootstrapOptions`             | `LambdaBootstrapOptions` | new()     | Lambda runtime bootstrap configuration                     |

### InitTimeout

Controls how long `OnInit` handlers can run before cancellation.

```csharp title="Program.cs"
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

**When to adjust:**

- **Increase** if OnInit performs expensive operations (loading ML models, warming large caches)
- **Decrease** for faster feedback during initialization failures

**Default:** 5 seconds

### InvocationCancellationBuffer

Controls when the invocation `CancellationToken` fires relative to Lambda's hard timeout.

```csharp title="Program.cs"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    // Fire cancellation 5 seconds before Lambda times out
    options.InvocationCancellationBuffer = TimeSpan.FromSeconds(5);
});

lambda.MapHandler(async ([Event] Order order, IOrderService service, CancellationToken ct) =>
{
    // If Lambda timeout is 30s, 'ct' fires after 25s
    return await service.ProcessAsync(order, ct);
});
```

**Why it's needed:**

- Allows graceful cancellation before Lambda's hard timeout
- Gives time for cleanup and error responses
- Prevents incomplete transactions

**When to adjust:**

- **Increase** for operations requiring significant cleanup time
- **Decrease** to maximize processing time per invocation

**Default:** 3 seconds

### ShutdownDuration

Time between SIGTERM (shutdown signal) and SIGKILL (forced termination).

```csharp title="Program.cs"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    // Use predefined constant for external extensions
    options.ShutdownDuration = ShutdownDuration.ExternalExtensions; // 500ms
});
```

**Available Constants:**

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

**When to adjust:**

- **Use NoExtensions** (0ms) if no Lambda extensions installed
- **Use InternalExtensions** (300ms) for internal-only extensions
- **Use ExternalExtensions** (500ms) if using external extensions (default)
- **Increase** if shutdown tasks require more time

**Default:** `ShutdownDuration.ExternalExtensions` (500ms)

### ShutdownDurationBuffer

Safety buffer subtracted from `ShutdownDuration` to ensure cleanup completes before SIGKILL.

```csharp title="Program.cs"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.ShutdownDuration = TimeSpan.FromSeconds(1);
    options.ShutdownDurationBuffer = TimeSpan.FromMilliseconds(100);

    // Actual shutdown timeout: 1000ms - 100ms = 900ms
});

lambda.OnShutdown(async (IMetrics metrics, CancellationToken ct) =>
{
    // 'ct' fires after 900ms
    await metrics.FlushAsync(ct);
});
```

**Default:** 50ms

### ClearLambdaOutputFormatting

Removes Lambda runtime's custom log formatting, useful for structured logging frameworks.

```csharp title="Program.cs"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.ClearLambdaOutputFormatting = true;
});
```

**Why use it:**

- Lambda runtime adds formatting that interferes with JSON logging
- Structured logging (Serilog, NLog) produces malformed JSON without this
- CloudWatch Logs Insights parses logs correctly

**Default:** `false`

## Application Configuration

Use standard ASP.NET Core configuration for application settings.

### appsettings.json

Create `appsettings.json` in your project root:

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
  },
  "ExternalApi": {
    "BaseUrl": "https://api.example.com",
    "ApiKey": ""
    // Set via environment variable
  }
}
```

**Important:** Include in `.csproj`:

```xml

<ItemGroup>
  <Content Include="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

### Options Pattern

Define strongly-typed options classes:

```csharp title="Configuration/OrderProcessingOptions.cs"
namespace MyLambda.Configuration;

public class OrderProcessingOptions
{
    public int MaxRetries { get; init; }
    public int TimeoutSeconds { get; init; }
    public bool EnableCaching { get; init; }
}
```

Bind configuration sections to options:

```csharp title="Program.cs"
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLambda.Configuration;

var builder = LambdaApplication.CreateBuilder();

// Bind configuration section to options class
builder.Services.Configure<OrderProcessingOptions>(
    builder.Configuration.GetSection("OrderProcessing")
);

builder.Services.AddScoped<IOrderService, OrderService>();

var lambda = builder.Build();
```

Inject options into services:

```csharp title="Services/OrderService.cs"
using Microsoft.Extensions.Options;
using MyLambda.Configuration;

public class OrderService : IOrderService
{
    private readonly OrderProcessingOptions _options;
    private readonly IOrderRepository _repository;

    public OrderService(
        IOptions<OrderProcessingOptions> options,
        IOrderRepository repository)
    {
        _options = options.Value;
        _repository = repository;
    }

    public async Task<OrderResponse> ProcessAsync(Order order)
    {
        if (_options.EnableCaching)
        {
            // Check cache
        }

        for (int retry = 0; retry < _options.MaxRetries; retry++)
        {
            try
            {
                return await _repository.SaveAsync(order);
            }
            catch (Exception) when (retry < _options.MaxRetries - 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        throw new Exception("Max retries exceeded");
    }
}
```

### IOptions<T> vs IOptionsSnapshot<T> vs IOptionsMonitor<T>

| Interface             | Lifetime  | Reloads        | Use Case                                                            |
|-----------------------|-----------|----------------|---------------------------------------------------------------------|
| `IOptions<T>`         | Singleton | Never          | **Recommended for Lambda** – Config doesn't change during execution |
| `IOptionsSnapshot<T>` | Scoped    | Per invocation | Use if config can change between invocations                        |
| `IOptionsMonitor<T>`  | Singleton | On change      | Rarely needed in Lambda                                             |

**Recommendation:** Use `IOptions<T>` for Lambda functions.

```csharp
// GOOD: IOptions<T> for Lambda
public OrderService(IOptions<OrderProcessingOptions> options)
{
    _options = options.Value;
}
```

## Environment-Specific Configuration

Use multiple configuration files for different environments.

### File Structure

```
MyLambda/
├── appsettings.json              # Base configuration
├── appsettings.Development.json  # Development overrides
└── appsettings.Production.json   # Production overrides
```

### Configuration Loading

```csharp title="Program.cs"
var builder = LambdaApplication.CreateBuilder();

// Configuration loaded automatically in this order:
// 1. appsettings.json (base)
// 2. appsettings.{Environment}.json (environment-specific)
// 3. Environment variables (highest priority)

// Add additional configuration sources if needed
builder.Configuration.AddEnvironmentVariables();
```

### Environment-Specific Settings

```json title="appsettings.Development.json"
{
  "OrderProcessing": {
    "EnableCaching": false
  },
  "Database": {
    "ConnectionString": "Server=localhost;Database=orders_dev"
  }
}
```

```json title="appsettings.Production.json"
{
  "OrderProcessing": {
    "EnableCaching": true
  },
  "Database": {
    "ConnectionString": ""
    // Set via environment variable
  }
}
```

## Environment Variables

Environment variables override configuration files.

### Setting Environment Variables

In Lambda, set environment variables through:

- AWS Console
- AWS SAM template
- AWS CDK
- Terraform

```yaml title="template.yaml" linenums="1"
Resources:
  MyFunction:
    Type: AWS::Serverless::Function
    Properties:
      Environment:
        Variables:
          DATABASE_CONNECTION_STRING: !Ref DatabaseConnectionString
          API_KEY: !Ref ApiKey
          ASPNETCORE_ENVIRONMENT: Production
```

### Accessing Environment Variables

#### Direct Access

```csharp title="Program.cs"
var apiKey = Environment.GetEnvironmentVariable("API_KEY");
```

#### Through Configuration

```csharp title="Program.cs"
var apiKey = builder.Configuration["API_KEY"];
```

#### Binding to Options

```csharp title="Program.cs"
public class ExternalApiOptions
{
    public string BaseUrl { get; init; } = "";
    public string ApiKey { get; init; } = "";
}

builder.Services.Configure<ExternalApiOptions>(options =>
{
    options.BaseUrl = builder.Configuration["ExternalApi:BaseUrl"] ?? "";
    options.ApiKey = builder.Configuration["API_KEY"] ?? "";
});
```

### Environment Variable Naming

Configuration binding supports hierarchical keys with `:` or `__`:

```bash
# Both work
ExternalApi:BaseUrl=https://api.example.com
ExternalApi__BaseUrl=https://api.example.com
```

**Prefer `__`** for environment variables (`:` not supported in all shells).

## JSON Serialization Configuration

Configure JSON serialization for Lambda events and responses.

### Default Serialization

The framework uses `System.Text.Json` with sensible defaults:

```csharp
// Default configuration (no action needed)
// - Camel case property names
// - Case-insensitive deserialization
// - Ignores null values
```

### Custom JsonSerializerContext (AOT)

For Native AOT compilation, define a JSON serializer context:

```csharp title="Program.cs"
using System.Text.Json.Serialization;

[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(Response))]
[JsonSerializable(typeof(Order))]
public partial class MySerializerContext : JsonSerializerContext;

var builder = LambdaApplication.CreateBuilder();

// Register custom serializer context
builder.Services.AddLambdaSerializerWithContext<MySerializerContext>();

var lambda = builder.Build();
```

**Benefits:**

- Required for Native AOT
- Compile-time serialization metadata
- Better trimming
- Faster performance

### Custom JsonSerializerOptions

Customize JSON serialization options:

```csharp title="Program.cs"
using System.Text.Json;

builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.WriteIndented = false;
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
```

## Secrets Management

Never hardcode secrets in configuration files.

### AWS Secrets Manager

```csharp title="Program.cs"
using Amazon;
using Amazon.Extensions.Configuration.SystemsManager;

var builder = LambdaApplication.CreateBuilder();

// Add AWS Secrets Manager as configuration source
builder.Configuration.AddSecretsManager(
    region: RegionEndpoint.USEast1,
    configurator: options =>
    {
        options.SecretFilter = entry => entry.Name.StartsWith("MyLambda/");
        options.PollingInterval = TimeSpan.FromMinutes(5);
    }
);
```

**Install NuGet package:**

```bash
dotnet add package Amazon.Extensions.Configuration.SystemsManager
```

### Environment Variables for Secrets

```csharp title="Program.cs"
public class DatabaseOptions
{
    public string ConnectionString { get; init; } = "";
}

builder.Services.Configure<DatabaseOptions>(options =>
{
    // Set via Lambda environment variable
    options.ConnectionString = builder.Configuration["DATABASE_CONNECTION_STRING"] ?? "";
});
```

**In template.yaml:**

```yaml
Environment:
  Variables:
    DATABASE_CONNECTION_STRING: !Sub '{{resolve:secretsmanager:${DatabaseSecret}:SecretString:connectionString}}'
```

## Configuration Validation

Validate configuration at startup to catch errors early.

### Data Annotations

```csharp title="Configuration/OrderProcessingOptions.cs"
using System.ComponentModel.DataAnnotations;

public class OrderProcessingOptions
{
    [Range(1, 10)]
    public int MaxRetries { get; init; }

    [Range(1, 300)]
    public int TimeoutSeconds { get; init; }

    public bool EnableCaching { get; init; }
}
```

### Validation on Startup

```csharp title="Program.cs"
builder.Services.AddOptions<OrderProcessingOptions>()
    .Bind(builder.Configuration.GetSection("OrderProcessing"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

**Validation fails on startup** if configuration is invalid.

### Custom Validation

```csharp title="Configuration/OrderProcessingOptions.cs"
public class OrderProcessingOptions : IValidatableObject
{
    public int MaxRetries { get; init; }
    public int TimeoutSeconds { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MaxRetries < 1)
        {
            yield return new ValidationResult(
                "MaxRetries must be at least 1",
                new[] { nameof(MaxRetries) }
            );
        }

        if (TimeoutSeconds > 300)
        {
            yield return new ValidationResult(
                "TimeoutSeconds cannot exceed 300",
                new[] { nameof(TimeoutSeconds) }
            );
        }
    }
}
```

## Best Practices

### ✅ Do: Use Strongly-Typed Options

```csharp
// GOOD: Strongly-typed options
builder.Services.Configure<OrderProcessingOptions>(
    builder.Configuration.GetSection("OrderProcessing")
);
```

### ❌ Don't: Use Magic Strings

```csharp
// BAD: Magic strings everywhere
var maxRetries = int.Parse(builder.Configuration["OrderProcessing:MaxRetries"]);
var timeout = int.Parse(builder.Configuration["OrderProcessing:TimeoutSeconds"]);
```

### ✅ Do: Validate Configuration at Startup

```csharp
// GOOD: Validate on startup
builder.Services.AddOptions<OrderProcessingOptions>()
    .Bind(builder.Configuration.GetSection("OrderProcessing"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### ✅ Do: Use Environment-Specific Files

```csharp
// GOOD: Environment-specific configuration
// appsettings.json
// appsettings.Development.json
// appsettings.Production.json
```

### ❌ Don't: Hardcode Configuration Values

```csharp
// BAD: Hardcoded values
public class OrderService
{
    private const int MaxRetries = 3;
    private const string ApiUrl = "https://api.example.com";
}
```

### ✅ Do: Use Environment Variables for Secrets

```csharp
// GOOD: Secrets via environment variables
var apiKey = builder.Configuration["API_KEY"];
```

### ❌ Don't: Commit Secrets to Source Control

```json
// BAD: API key in appsettings.json
{
  "ExternalApi": {
    "ApiKey": "sk_live_51234567890abcdef"
    // DON'T!
  }
}
```

### ✅ Do: Include appsettings.json in Deployment

```xml
<!-- GOOD: Copy appsettings.json to output -->
<ItemGroup>
  <Content Include="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

### ✅ Do: Use IOptions<T> for Lambda

```csharp
// GOOD: IOptions<T> for Lambda (config doesn't reload)
public OrderService(IOptions<OrderProcessingOptions> options)
{
    _options = options.Value;
}
```

### ❌ Don't: Use IOptionsMonitor<T> in Lambda

```csharp
// BAD: IOptionsMonitor<T> rarely needed in Lambda
public OrderService(IOptionsMonitor<OrderProcessingOptions> options)
{
    _options = options.CurrentValue; // Unnecessary overhead
}
```

## Troubleshooting

### Configuration Section Not Found

**Problem:** `options.Value` is null or has default values.

**Solution:** Verify configuration section exists and binding is correct:

```csharp
// Check configuration exists
var section = builder.Configuration.GetSection("OrderProcessing");
if (!section.Exists())
{
    throw new InvalidOperationException("OrderProcessing configuration section not found");
}

// Bind with validation
builder.Services.AddOptions<OrderProcessingOptions>()
    .Bind(section)
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### appsettings.json Not Found

**Problem:** Configuration file not deployed with Lambda.

**Solution:** Ensure `appsettings.json` is copied to output:

```xml

<ItemGroup>
  <Content Include="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

### Environment Variable Not Loaded

**Problem:** Environment variable set but not accessible.

**Solution:** Ensure environment variables are loaded:

```csharp
builder.Configuration.AddEnvironmentVariables();
```

**Or:** Check Lambda environment variable configuration in AWS Console/template.

## Key Takeaways

1. **LambdaHostOptions** – Configure framework behavior (timeouts, shutdown, serialization)
2. **appsettings.json** – Store application configuration
3. **Options Pattern** – Use `IOptions<T>` for strongly-typed configuration
4. **Environment Variables** – Override configuration, store secrets
5. **Validation** – Validate configuration at startup
6. **JSON Serialization** – Customize with `JsonSerializerOptions` or `JsonSerializerContext`
7. **Secrets** – Use environment variables or AWS Secrets Manager, never commit secrets
8. **IOptions<T>** – Prefer over `IOptionsSnapshot<T>` or `IOptionsMonitor<T>` for Lambda

## Next Steps

Now that you understand configuration, explore related topics:

- **[Lifecycle Management](/guides/lifecycle-management.md)** – Use configuration in OnInit and
  OnShutdown
- **[Dependency Injection](/guides/dependency-injection.md)** – Inject configured options into
  services
- **[Error Handling](/guides/error-handling.md)** – Configure timeout buffers
- **[Deployment](/guides/deployment.md)** – Set environment variables in deployment templates
- **[Testing](/guides/testing.md)** – Test services with different configurations

---

Congratulations! You now understand how to configure aws-lambda-host and your Lambda functions.
