# Dependency Injection

Dependency injection (DI) is central to building maintainable Lambda functions with aws-lambda-host. The framework integrates Microsoft.Extensions.DependencyInjection, providing the same DI patterns you use in ASP.NET Core—but optimized for Lambda's unique execution model.

## Introduction

In Lambda functions, understanding service lifetimes is critical:

- **Singleton services** persist across invocations (cold start to shutdown)
- **Scoped services** are created fresh for each invocation
- **Transient services** are created each time they're requested

This guide teaches you how to leverage DI effectively in Lambda functions.

## Service Lifetimes

### Singleton Services

Singleton services are created once during the first invocation and reused for the lifetime of the Lambda execution environment.

**When to use:**
- Stateless services
- HTTP clients (with connection pooling)
- Cache instances
- Configuration objects
- Database connection pools

```csharp title="Program.cs"
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;

var builder = LambdaApplication.CreateBuilder();

// Singleton: Created once, reused across all invocations
builder.Services.AddSingleton<ICache, MemoryCache>();
builder.Services.AddSingleton<IHttpClientFactory, HttpClientFactory>();
builder.Services.AddSingleton<IMetricsCollector, MetricsCollector>();

var lambda = builder.Build();
```

**Benefits:**
- ✅ Optimal performance (no repeated initialization)
- ✅ Shared state across invocations (caching, connection pools)
- ✅ Lower memory usage

**⚠️ Warning:** Singleton services must be thread-safe since Lambda can process concurrent invocations in the same execution environment.

### Scoped Services

Scoped services are created once per Lambda invocation and disposed when the invocation completes.

**When to use:**
- Services with per-request state
- Database contexts (Entity Framework)
- Unit of Work patterns
- Request-specific logging contexts
- Services that shouldn't be shared between requests

```csharp title="Program.cs"
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;

var builder = LambdaApplication.CreateBuilder();

// Scoped: New instance per invocation
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IRequestContext, RequestContext>();

var lambda = builder.Build();
```

**Benefits:**
- ✅ Isolated per invocation (no cross-request contamination)
- ✅ Automatic disposal after each request
- ✅ Safe for services with mutable state

### Transient Services

Transient services are created each time they're requested from the service provider.

**When to use:**
- Lightweight, stateless services
- Services that need fresh instances each time
- Rarely needed in Lambda (prefer Scoped for most use cases)

```csharp title="Program.cs"
builder.Services.AddTransient<IValidator, OrderValidator>();
```

**⚠️ Note:** Transient services are rarely necessary in Lambda. Prefer Scoped for per-invocation services and Singleton for shared services.

## Service Registration Patterns

### Basic Registration

Register interface/implementation pairs:

```csharp title="Program.cs"
builder.Services.AddSingleton<ICache, MemoryCache>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddTransient<IValidator, OrderValidator>();
```

### Concrete Types

Register concrete types without interfaces:

```csharp title="Program.cs"
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddScoped<OrderProcessor>();
```

### Factory Registration

Use factories for complex initialization:

```csharp title="Program.cs"
builder.Services.AddSingleton<ICache>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<MemoryCache>>();
    return new MemoryCache(
        new MemoryCacheOptions
        {
            SizeLimit = 1024,
            CompactionPercentage = 0.25
        },
        logger
    );
});
```

### Multiple Implementations

Register multiple implementations of the same interface:

```csharp title="Program.cs"
builder.Services.AddSingleton<INotificationService, EmailNotificationService>();
builder.Services.AddSingleton<INotificationService, SmsNotificationService>();
builder.Services.AddSingleton<INotificationService, PushNotificationService>();

// Inject all implementations
lambda.MapHandler(([Event] Order order, IEnumerable<INotificationService> notifiers) =>
{
    foreach (var notifier in notifiers)
    {
        notifier.Notify(order);
    }
});
```

### Try Add

Register services only if not already registered:

```csharp title="Program.cs"
builder.Services.TryAddSingleton<ICache, MemoryCache>();
// If ICache is already registered, this does nothing
```

### Replace Services

Replace existing service registrations:

```csharp title="Program.cs"
builder.Services.Replace(
    ServiceDescriptor.Singleton<ICache, RedisCache>()
);
```

## Dependency Injection in Handlers

Handlers can inject any registered service, plus framework-provided types.

### Injectable Types

The framework automatically injects these types into handlers:

| Type | Description | Lifetime |
|------|-------------|----------|
| `[Event] T` | The Lambda event payload (deserialized) | Per invocation |
| `IServiceType` | Any registered service | As registered |
| `ILambdaHostContext` | Lambda invocation context | Per invocation |
| `CancellationToken` | Cancellation signal (timeout tracking) | Per invocation |

### Basic Handler Injection

```csharp title="Program.cs"
lambda.MapHandler(([Event] OrderRequest request, IOrderService service) =>
    service.ProcessAsync(request)
);
```

### Multiple Dependencies

Inject multiple services into handlers:

```csharp title="Program.cs"
lambda.MapHandler(async (
    [Event] OrderRequest request,
    IOrderService orderService,
    IInventoryService inventoryService,
    IPaymentService paymentService,
    ILogger<OrderHandler> logger
) =>
{
    logger.LogInformation("Processing order {OrderId}", request.OrderId);

    var inventoryAvailable = await inventoryService.CheckAsync(request.Items);
    if (!inventoryAvailable)
    {
        return new OrderResponse { Success = false, Reason = "Inventory unavailable" };
    }

    var paymentResult = await paymentService.ChargeAsync(request.Payment);
    if (!paymentResult.Success)
    {
        return new OrderResponse { Success = false, Reason = "Payment failed" };
    }

    var orderResult = await orderService.CreateAsync(request);
    return new OrderResponse { Success = true, OrderId = orderResult.OrderId };
});
```

### Injecting Context

Use `ILambdaHostContext` to access invocation metadata:

```csharp title="Program.cs"
lambda.MapHandler(async (
    [Event] OrderRequest request,
    IOrderService service,
    ILambdaHostContext context
) =>
{
    // Access scoped service provider
    var cache = context.ServiceProvider.GetRequiredService<ICache>();

    // Access cancellation token
    if (context.CancellationToken.IsCancellationRequested)
    {
        return new OrderResponse { Success = false, Reason = "Timeout" };
    }

    // Store invocation-scoped data
    context.Items["StartTime"] = DateTimeOffset.UtcNow;

    return await service.ProcessAsync(request, context.CancellationToken);
});
```

### Injecting CancellationToken

Use `CancellationToken` to handle Lambda timeouts gracefully:

```csharp title="Program.cs"
lambda.MapHandler(async (
    [Event] OrderRequest request,
    IOrderService service,
    CancellationToken cancellationToken
) =>
{
    try
    {
        return await service.ProcessAsync(request, cancellationToken);
    }
    catch (OperationCanceledException)
    {
        // Lambda is approaching timeout
        return new OrderResponse { Success = false, Reason = "Timeout" };
    }
});
```

## Dependency Injection in Middleware

Middleware can resolve services from the DI container using the `ILambdaHostContext`.

### Service Resolution in Middleware

```csharp title="Program.cs"
lambda.UseMiddleware(async (context, next) =>
{
    var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var stopwatch = Stopwatch.StartNew();

    logger.LogInformation("Request starting");

    await next(context);

    stopwatch.Stop();
    logger.LogInformation("Request completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
});
```

### Middleware with Constructor Injection

Create reusable middleware classes with constructor injection:

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
        _logger.LogInformation("Starting invocation");
        await next(context);
        _logger.LogInformation("Invocation completed");
    }
}
```

```csharp title="Program.cs"
builder.Services.AddSingleton<LoggingMiddleware>();

var lambda = builder.Build();
lambda.UseMiddleware<LoggingMiddleware>();
```

## Dependency Injection in Lifecycle Hooks

Both `OnInit` and `OnShutdown` handlers support dependency injection.

### OnInit with DI

```csharp title="Program.cs"
lambda.OnInit(async (IServiceProvider services, CancellationToken ct) =>
{
    var cache = services.GetRequiredService<ICache>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Warming up cache");
    await cache.WarmUpAsync(ct);

    return true; // Continue initialization
});
```

### OnShutdown with DI

```csharp title="Program.cs"
lambda.OnShutdown(async (IServiceProvider services, CancellationToken ct) =>
{
    var cache = services.GetRequiredService<ICache>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Flushing cache");
    await cache.FlushAsync(ct);
});
```

## Configuration with Options Pattern

Use the options pattern to inject strongly-typed configuration.

### Defining Options Classes

```csharp title="Configuration/OrderProcessingOptions.cs"
namespace MyLambda.Configuration;

public class OrderProcessingOptions
{
    public int MaxRetries { get; init; }
    public int TimeoutSeconds { get; init; }
    public bool EnableCaching { get; init; }
}
```

### Binding Configuration

```csharp title="Program.cs"
using Microsoft.Extensions.Options;

var builder = LambdaApplication.CreateBuilder();

// Bind configuration section to options class
builder.Services.Configure<OrderProcessingOptions>(
    builder.Configuration.GetSection("OrderProcessing")
);

builder.Services.AddScoped<IOrderService, OrderService>();

var lambda = builder.Build();
```

### appsettings.json

```json title="appsettings.json"
{
  "OrderProcessing": {
    "MaxRetries": 3,
    "TimeoutSeconds": 30,
    "EnableCaching": true
  }
}
```

### Injecting Options

```csharp title="Services/OrderService.cs"
using Microsoft.Extensions.Options;

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

    public async Task<OrderResult> ProcessAsync(Order order)
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

### Options Variants

```csharp title="Program.cs"
// IOptions<T> - Singleton, value never reloads
builder.Services.AddScoped<ServiceWithOptions>();

// IOptionsSnapshot<T> - Scoped, reloads per invocation
builder.Services.AddScoped<ServiceWithOptionsSnapshot>();

// IOptionsMonitor<T> - Singleton, reloads when config changes
builder.Services.AddSingleton<ServiceWithOptionsMonitor>();
```

**For Lambda, prefer `IOptions<T>`** since configuration typically doesn't change during function execution.

## Best Practices

### ✅ Do: Use Scoped for Per-Invocation State

```csharp
// GOOD: Repository with per-invocation state
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
```

### ❌ Don't: Use Singleton for Stateful Services

```csharp
// BAD: Singleton with mutable state causes cross-invocation contamination
builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
```

### ✅ Do: Use Singleton for Shared Resources

```csharp
// GOOD: HTTP client pool is thread-safe and benefits from reuse
builder.Services.AddSingleton<IHttpClientFactory, HttpClientFactory>();
builder.Services.AddSingleton<ICache, MemoryCache>();
```

### ✅ Do: Register Interfaces, Not Implementations

```csharp
// GOOD: Testable and flexible
builder.Services.AddScoped<IOrderService, OrderService>();

lambda.MapHandler(([Event] Order order, IOrderService service) =>
    service.ProcessAsync(order)
);
```

### ❌ Don't: Use Concrete Types Everywhere

```csharp
// BAD: Hard to test and tightly coupled
builder.Services.AddScoped<OrderService>();

lambda.MapHandler(([Event] Order order, OrderService service) =>
    service.ProcessAsync(order)
);
```

### ✅ Do: Use Options Pattern for Configuration

```csharp
// GOOD: Strongly-typed, testable configuration
builder.Services.Configure<OrderOptions>(
    builder.Configuration.GetSection("OrderProcessing")
);
```

### ❌ Don't: Hardcode Configuration Values

```csharp
// BAD: Hardcoded, difficult to test and change
public class OrderService
{
    private const int MaxRetries = 3;
    private const string ApiUrl = "https://api.example.com";
}
```

### ✅ Do: Dispose Resources Properly

```csharp
// GOOD: Scoped services are automatically disposed after each invocation
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

public class OrderRepository : IOrderRepository, IDisposable
{
    private readonly HttpClient _httpClient;

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
```

### ✅ Do: Inject CancellationToken for Timeout Handling

```csharp
// GOOD: Gracefully handle Lambda timeouts
lambda.MapHandler(async (
    [Event] Order order,
    IOrderService service,
    CancellationToken cancellationToken
) =>
{
    return await service.ProcessAsync(order, cancellationToken);
});
```

## Anti-Patterns to Avoid

### ❌ Service Locator Pattern

```csharp
// BAD: Service locator anti-pattern
lambda.MapHandler(([Event] Order order, IServiceProvider services) =>
{
    var service = services.GetRequiredService<IOrderService>();
    return service.ProcessAsync(order);
});
```

**Why it's bad:**
- Hides dependencies (hard to test)
- Runtime errors instead of compile-time errors
- Violates dependency inversion principle

**Better approach:**

```csharp
// GOOD: Explicit dependency injection
lambda.MapHandler(([Event] Order order, IOrderService service) =>
    service.ProcessAsync(order)
);
```

---

### ❌ Registering Everything as Singleton

```csharp
// BAD: All services are Singleton
builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.Services.AddSingleton<IRequestContext, RequestContext>();
```

**Why it's bad:**
- Cross-invocation state contamination
- Memory leaks (services never disposed)
- Thread-safety issues

**Better approach:**

```csharp
// GOOD: Use appropriate lifetimes
builder.Services.AddSingleton<ICache, MemoryCache>();           // Stateless, shared
builder.Services.AddScoped<IOrderRepository, OrderRepository>(); // Per-invocation
builder.Services.AddScoped<IRequestContext, RequestContext>();   // Per-invocation
```

---

### ❌ Constructor Over-Injection

```csharp
// BAD: Too many dependencies
public class OrderService
{
    public OrderService(
        IOrderRepository orderRepo,
        IInventoryService inventoryService,
        IPaymentService paymentService,
        INotificationService notificationService,
        ILogger<OrderService> logger,
        IMetricsCollector metrics,
        ICache cache,
        IValidator validator,
        IMapper mapper
    )
    {
        // ...
    }
}
```

**Why it's bad:**
- Service Responsibility Principle violation
- Hard to test and maintain
- Usually indicates poor design

**Better approach:**

```csharp
// GOOD: Split into smaller services
public class OrderService
{
    public OrderService(
        IOrderRepository repository,
        IOrderProcessor processor,
        ILogger<OrderService> logger
    )
    {
        // ...
    }
}

public class OrderProcessor
{
    public OrderProcessor(
        IInventoryService inventory,
        IPaymentService payment,
        INotificationService notification
    )
    {
        // ...
    }
}
```

---

### ❌ Mixing Lifetimes Incorrectly

```csharp
// BAD: Singleton depends on Scoped service
builder.Services.AddSingleton<IOrderCache, OrderCache>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

public class OrderCache : IOrderCache
{
    public OrderCache(IOrderRepository repository) // PROBLEM!
    {
        // Singleton capturing Scoped dependency
    }
}
```

**Why it's bad:**
- Scoped service is captured by Singleton
- First invocation's scoped instance is reused forever
- Cross-invocation state contamination

**Better approach:**

```csharp
// GOOD: Inject IServiceProvider and resolve per-invocation
public class OrderCache : IOrderCache
{
    private readonly IServiceProvider _serviceProvider;

    public OrderCache(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Order> GetAsync(string id)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        return await repository.GetAsync(id);
    }
}
```

## Common Patterns

### Repository Pattern

```csharp title="Program.cs"
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

var lambda = builder.Build();

lambda.MapHandler(([Event] OrderRequest request, IOrderService service) =>
    service.ProcessAsync(request)
);
```

```csharp title="Services/OrderService.cs"
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderResponse> ProcessAsync(OrderRequest request)
    {
        var order = new Order(request.Id, request.Amount);
        await _repository.SaveAsync(order);
        return new OrderResponse(order.Id, true);
    }
}
```

### Unit of Work Pattern

```csharp title="Program.cs"
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository>(sp =>
    sp.GetRequiredService<IUnitOfWork>().Orders
);

var lambda = builder.Build();
```

```csharp title="Data/UnitOfWork.cs"
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly DbContext _context;

    public UnitOfWork(DbContext context)
    {
        _context = context;
        Orders = new OrderRepository(context);
    }

    public IOrderRepository Orders { get; }

    public async Task<int> CommitAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context?.Dispose();
}
```

### Decorator Pattern

```csharp title="Program.cs"
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.Decorate<IOrderService, CachedOrderService>();

var lambda = builder.Build();
```

```csharp title="Services/CachedOrderService.cs"
public class CachedOrderService : IOrderService
{
    private readonly IOrderService _inner;
    private readonly ICache _cache;

    public CachedOrderService(IOrderService inner, ICache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<OrderResponse> ProcessAsync(OrderRequest request)
    {
        var cacheKey = $"order:{request.Id}";
        if (_cache.TryGet(cacheKey, out OrderResponse cached))
        {
            return cached;
        }

        var result = await _inner.ProcessAsync(request);
        _cache.Set(cacheKey, result);
        return result;
    }
}
```

## Troubleshooting

### Service Not Found

**Error:**

```
InvalidOperationException: No service for type 'IOrderService' has been registered.
```

**Solution:**

Ensure the service is registered before building:

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();
var lambda = builder.Build();
```

### Singleton Capturing Scoped Dependency

**Problem:** Singleton service depends on Scoped service.

**Solution:** Use `IServiceProvider` to resolve Scoped services dynamically:

```csharp
public class MySingletonService
{
    private readonly IServiceProvider _serviceProvider;

    public MySingletonService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DoWorkAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedService = scope.ServiceProvider.GetRequiredService<IMyScopedService>();
        await scopedService.DoWorkAsync();
    }
}
```

### Disposed Service Access

**Error:**

```
ObjectDisposedException: Cannot access a disposed object.
```

**Problem:** Attempting to use a Scoped service after the invocation completes.

**Solution:** Ensure services are only used within the invocation scope. Don't store Scoped services in Singleton fields.

## Key Takeaways

1. **Understand Lifetimes**: Use Singleton for stateless shared resources, Scoped for per-invocation services
2. **Inject Dependencies**: Use constructor injection, not service locator pattern
3. **Options Pattern**: Use `IOptions<T>` for strongly-typed configuration
4. **CancellationToken**: Always inject `CancellationToken` for timeout handling
5. **Avoid Anti-Patterns**: Don't mix lifetimes incorrectly or over-inject dependencies
6. **Reusable Middleware**: Create middleware classes with constructor injection
7. **Lifecycle DI**: Use `IServiceProvider` in `OnInit` and `OnShutdown` handlers

## Next Steps

Now that you understand dependency injection, explore related topics:

- **[Middleware](/guides/middleware.md)** – Build middleware pipelines with DI
- **[Lifecycle Management](/guides/lifecycle-management.md)** – Use DI in OnInit and OnShutdown
- **[Configuration](/guides/configuration.md)** – Advanced configuration patterns
- **[Testing](/guides/testing.md)** – Test services with dependency injection
- **[Handler Registration](/guides/handler-registration.md)** – Advanced handler patterns with DI

---

Congratulations! You now understand how to leverage dependency injection to build maintainable, testable Lambda functions.
