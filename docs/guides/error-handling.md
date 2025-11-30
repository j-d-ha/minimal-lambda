# Error Handling

Comprehensive error handling is essential for building resilient AWS Lambda functions. This guide
covers exception handling patterns, cancellation token usage, lifecycle error handling, retry
strategies, and AWS Lambda integration patterns specific to the aws-lambda-host framework.

---

## Introduction

AWS Lambda functions face unique error handling challenges:

- **Timeout constraints**: Lambda functions must complete within a configured timeout
- **Cold starts**: Initialization failures prevent the function from starting
- **Graceful shutdown**: Limited time window for cleanup before container termination
- **Retry behavior**: Event source determines retry semantics

The aws-lambda-host framework provides built-in error handling mechanisms while allowing you to
implement custom error handling strategies.

---

## Handler Exception Handling

### Exception Bubbling

Exceptions thrown in Lambda handlers naturally bubble up through the middleware pipeline. This
allows middleware to intercept, log, or transform exceptions before they reach the Lambda runtime.

```csharp title="Program.cs"
lambda.MapHandler(async ([Event] OrderRequest request, IOrderService service) =>
{
    // Unhandled exceptions bubble through middleware to Lambda runtime
    return await service.ProcessAsync(request);
});
```

**How it works**:

1. Handler throws exception
2. Exception propagates through middleware pipeline (innermost to outermost)
3. Each middleware can catch, log, or re-throw
4. If unhandled, Lambda runtime receives the exception

### Try-Catch in Handlers

For fine-grained error handling, use try-catch blocks to handle specific exception types and return
error responses.

```csharp title="Program.cs" linenums="1"
lambda.MapHandler(async ([Event] OrderRequest request, IOrderService service) =>
{
    try
    {
        var result = await service.ProcessAsync(request);
        return new OrderResponse
        {
            OrderId = result.Id,
            Status = "Success"
        };
    }
    catch (ValidationException ex)
    {
        // Return structured error response for validation failures
        return new OrderResponse
        {
            Status = "ValidationError",
            Error = ex.Message
        };
    }
    catch (InvalidOperationException ex)
    {
        // Log and return business logic errors
        Console.WriteLine($"Business logic error: {ex.Message}");
        return new OrderResponse
        {
            Status = "BusinessError",
            Error = "Unable to process order"
        };
    }
    // Let other exceptions bubble to middleware/runtime
});
```

!!! tip "When to catch exceptions in handlers"

- Validation errors that should return specific HTTP status codes or error structures
- Expected business logic exceptions (insufficient inventory, duplicate orders, etc.)
- Errors requiring custom response formatting

---

## Middleware Error Handling

Middleware is ideal for global error handling policies that apply to all invocations.

### Global Error Handler Pattern

```csharp title="Program.cs" linenums="1"
lambda.UseMiddleware(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (ValidationException ex)
    {
        // Log validation errors with context
        Console.WriteLine($"[Validation Error] RequestId: {context.LambdaContext.AwsRequestId}, Error: {ex.Message}");

        // Set error response (if using API Gateway integration)
        // context.Items["StatusCode"] = 400;
        // context.Items["ErrorMessage"] = ex.Message;

        // Re-throw to Lambda runtime (which may have retry logic)
        throw;
    }
    catch (OperationCanceledException)
    {
        // Log timeout/cancellation
        Console.WriteLine($"[Timeout] RequestId: {context.LambdaContext.AwsRequestId} cancelled");
        throw;
    }
    catch (Exception ex)
    {
        // Log unexpected errors with full stack trace
        Console.WriteLine($"[Unexpected Error] RequestId: {context.LambdaContext.AwsRequestId}");
        Console.WriteLine($"Exception: {ex}");

        // Optional: Send to error tracking service (Sentry, Rollbar, etc.)
        // await errorTracker.CaptureAsync(ex);

        throw;
    }
});
```

### Error Transformation Middleware

Transform exceptions into structured error responses before they reach the Lambda runtime.

```csharp title="Program.cs" linenums="1"
lambda.UseMiddleware(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (ValidationException ex)
    {
        // Transform validation exception into error response
        var errorResponse = new ErrorResponse
        {
            Type = "ValidationError",
            Message = ex.Message,
            RequestId = context.LambdaContext.AwsRequestId,
            Timestamp = DateTime.UtcNow
        };

        // Serialize error response to context
        var responseFeature = context.Features.Get<IResponseFeature>();
        if (responseFeature != null)
        {
            // Set error response (implementation depends on response handling)
            context.Items["ErrorResponse"] = errorResponse;
        }

        // Don't re-throw - error has been handled
    }
    catch (Exception ex)
    {
        // Log and re-throw unhandled errors
        Console.WriteLine($"Unhandled error: {ex}");
        throw;
    }
});
```

!!! warning "Middleware execution order matters"
Error handling middleware should typically be registered first (outer layer) so it can catch
exceptions from all subsequent middleware and handlers.

---

## Cancellation Token Handling

AWS Lambda enforces strict timeout limits. The framework provides automatic cancellation token
management to ensure your code can gracefully handle timeouts.

### InvocationCancellationBuffer

The `InvocationCancellationBuffer` configuration prevents hard Lambda timeouts by firing the
cancellation token before Lambda terminates your function.

**How it works**:

```
Lambda Timeout = 30 seconds
InvocationCancellationBuffer = 3 seconds (default)
------------------------
CancellationToken fires at: 27 seconds
Lambda hard timeout at: 30 seconds
```

This gives your code 3 seconds to complete current operations, flush metrics, and gracefully exit
before Lambda forcefully terminates the container.

```csharp title="Program.cs" linenums="1"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    // Default is 3 seconds
    options.InvocationCancellationBuffer = TimeSpan.FromSeconds(5);
});
```

### Timeout Calculation

From `DefaultLambdaCancellationFactory`:

```csharp
var maxAllowedDuration = context.RemainingTime - _bufferDuration;

if (maxAllowedDuration <= TimeSpan.Zero)
{
    throw new InvalidOperationException(
        "CancellationTokenSource provided with insufficient time. "
            + $"Lambda Remaining Time = {context.RemainingTime:c}, "
            + $"Cancellation Token Buffer = {_bufferDuration:c}, "
            + $"Candidate Token Duration = {maxAllowedDuration:c}"
    );
}
```

!!! danger "Insufficient time error"
If `InvocationCancellationBuffer` exceeds the Lambda's remaining time, the framework throws
`InvalidOperationException` with diagnostic information. This typically occurs when:

    - Lambda timeout is very short (< 3 seconds)
    - Buffer is configured larger than Lambda timeout
    - Previous operations consumed too much time

### Respecting Cancellation Tokens

Always pass and respect cancellation tokens in async operations:

```csharp title="Program.cs" linenums="1"
lambda.MapHandler(async (
    [Event] OrderRequest request,
    IOrderService orderService,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
{
    // Pass cancellation token to all async operations
    var order = await orderService.CreateOrderAsync(request, cancellationToken);

    // HTTP calls should respect cancellation
    var httpClient = httpClientFactory.CreateClient();
    var response = await httpClient.PostAsJsonAsync(
        "https://api.example.com/notify",
        order,
        cancellationToken
    );

    return new OrderResponse { OrderId = order.Id };
});
```

### Handling OperationCanceledException

When a cancellation token fires, async operations throw `OperationCanceledException`:

```csharp title="Program.cs" linenums="1"
lambda.MapHandler(async (
    [Event] ProcessingRequest request,
    IDataService dataService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var data = await dataService.ProcessLargeDatasetAsync(request.DatasetId, cancellationToken);
        return new ProcessingResponse { Status = "Completed", RecordsProcessed = data.Count };
    }
    catch (OperationCanceledException)
    {
        // Timeout occurred - return partial results or failure status
        Console.WriteLine($"Processing timeout for dataset {request.DatasetId}");
        return new ProcessingResponse
        {
            Status = "Timeout",
            RecordsProcessed = dataService.GetProcessedCount()
        };
    }
});
```

!!! tip "Best practice for long-running operations"

- Check `cancellationToken.IsCancellationRequested` periodically in loops
- Pass cancellation tokens to all I/O operations (HTTP, database, file operations)
- Catch `OperationCanceledException` to handle graceful degradation
- Flush any buffered data before returning on cancellation

---

## Lifecycle Error Handling

The OnInit and OnShutdown lifecycle phases have specialized error handling behavior.

### OnInit Error Handling

**Behavior**:

- All OnInit handlers execute **in parallel**
- Handlers have an `InitTimeout` (default 5 seconds)
- Each handler gets its own **service scope**
- Exceptions are caught and **aggregated**
- Handlers can return `false` to **abort startup without throwing**
- All handlers complete, then errors are bundled into `AggregateException`

```csharp title="Program.cs" linenums="1"
lambda.OnInit(async (ICache cache, CancellationToken cancellationToken) =>
{
    try
    {
        // Warm up cache during cold start
        await cache.WarmUpAsync(cancellationToken);
        return true;  // Startup succeeds
    }
    catch (OperationCanceledException)
    {
        // InitTimeout fired (default 5 seconds)
        Console.WriteLine("Cache warmup timeout");
        return false;  // Abort startup - prevents Lambda from starting
    }
    catch (Exception ex)
    {
        // Log initialization error
        Console.WriteLine($"Cache warmup failed: {ex.Message}");

        // Option 1: Return false to abort startup gracefully
        return false;

        // Option 2: Throw to include in AggregateException
        // throw;
    }
});

lambda.OnInit(async (IDatabaseMigrator migrator, CancellationToken cancellationToken) =>
{
    // Second handler - runs in parallel with first
    await migrator.EnsureMigratedAsync(cancellationToken);
    return true;
});
```

**Configuration**:

```csharp title="Program.cs"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    // Maximum time for all OnInit handlers (default 5 seconds)
    options.InitTimeout = TimeSpan.FromSeconds(10);
});
```

**Error scenarios**:

| Scenario          | Handler 1          | Handler 2          | Result                                    |
|-------------------|--------------------|--------------------|-------------------------------------------|
| Both succeed      | `return true`      | `return true`      | Lambda starts                             |
| One returns false | `return false`     | `return true`      | Lambda aborts (no exception)              |
| One throws        | throws `Exception` | `return true`      | `AggregateException` thrown               |
| Both throw        | throws `Exception` | throws `Exception` | `AggregateException` with both exceptions |

!!! note "Parallel execution"
From `LambdaOnInitBuilder.cs`:

```csharp
var tasks = _handlers.Select(h => RunInitHandler(h, cts.Token));
var results = await Task.WhenAll(tasks).ConfigureAwait(false);
```

All init handlers run concurrently, not sequentially.

### OnShutdown Error Handling

**Behavior**:

- All OnShutdown handlers execute **in parallel**
- Each handler gets its own **service scope**
- Individual exceptions are caught and collected
- All handlers complete before throwing `AggregateException`
- **Does not interrupt execution of other handlers**

```csharp title="Program.cs" linenums="1"
lambda.OnShutdown(async (IMetricsService metrics, CancellationToken cancellationToken) =>
{
    try
    {
        // Flush metrics before shutdown
        await metrics.FlushAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        // Log shutdown error - won't prevent other handlers from running
        Console.WriteLine($"Metrics flush error: {ex.Message}");

        // Error is aggregated but other shutdown handlers continue
        throw;
    }
});

lambda.OnShutdown(async (IConnectionPool connectionPool, CancellationToken cancellationToken) =>
{
    // Runs in parallel with metrics flush
    await connectionPool.DrainAsync(cancellationToken);
});
```

**Shutdown timing configuration**:

```csharp title="Program.cs" linenums="1"
builder.Services.ConfigureLambdaHostOptions(options =>
{
    // Time between SIGTERM and SIGKILL (default 500ms for external extensions)
    options.ShutdownDuration = ShutdownDuration.ExternalExtensions;  // 500ms

    // options.ShutdownDuration = ShutdownDuration.InternalExtensions;  // 300ms
    // options.ShutdownDuration = ShutdownDuration.NoExtensions;        // 0ms

    // Buffer to ensure cleanup completes (default 50ms)
    options.ShutdownDurationBuffer = TimeSpan.FromMilliseconds(100);
});
```

**Shutdown timeline**:

```
1. AWS sends SIGTERM
2. ShutdownDuration starts (500ms for ExternalExtensions)
3. CancellationToken fires at (ShutdownDuration - ShutdownDurationBuffer) = 450ms
4. OnShutdown handlers execute with 450ms to complete
5. AWS sends SIGKILL at 500ms (forceful termination)
```

!!! warning "Limited shutdown time"
Shutdown handlers must complete quickly (typically < 500ms). Long-running cleanup operations may be
terminated by SIGKILL. Use cancellation tokens to detect imminent shutdown.

---

## Application-Level Retry Patterns

The aws-lambda-host framework **does not provide built-in retry mechanisms**. You implement retry
logic in your application code or use libraries like Polly.

### Simple Exponential Backoff

```csharp title="RetryHelper.cs" linenums="1"
public static class RetryHelper
{
    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (i < maxRetries - 1 && IsTransientError(ex))
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, i));  // 1s, 2s, 4s
                Console.WriteLine($"Retry {i + 1}/{maxRetries} after {delay.TotalSeconds}s due to: {ex.Message}");
                await Task.Delay(delay, cancellationToken);
            }
        }

        throw new Exception($"Operation failed after {maxRetries} retries");
    }

    private static bool IsTransientError(Exception ex)
    {
        return ex is HttpRequestException or TimeoutException or IOException;
    }
}
```

**Usage**:

```csharp title="Program.cs"
lambda.MapHandler(async ([Event] Request request, IHttpClientFactory httpClientFactory, CancellationToken ct) =>
{
    var httpClient = httpClientFactory.CreateClient();

    var response = await RetryHelper.RetryAsync(async () =>
        await httpClient.GetAsync($"https://api.example.com/data/{request.Id}", ct),
        maxRetries: 3,
        cancellationToken: ct
    );

    return await response.Content.ReadAsStringAsync(ct);
});
```

### Using Polly

[Polly](https://github.com/App-vNext/Polly) is the recommended library for resilience and transient
fault handling.

**Installation**:

```bash
dotnet add package Polly
```

**Retry policy**:

```csharp title="Program.cs" linenums="1"
using Polly;
using Polly.Retry;

// Configure resilience in service registration
builder.Services.AddSingleton<AsyncRetryPolicy>(sp =>
{
    return Policy
        .Handle<HttpRequestException>()
        .Or<TimeoutException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (exception, timeSpan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to {exception.Message}");
            }
        );
});

// Use in handler
lambda.MapHandler(async (
    [Event] Request request,
    IHttpClientFactory httpClientFactory,
    AsyncRetryPolicy retryPolicy,
    CancellationToken ct) =>
{
    var httpClient = httpClientFactory.CreateClient();

    var response = await retryPolicy.ExecuteAsync(async () =>
        await httpClient.GetAsync($"https://api.example.com/data/{request.Id}", ct)
    );

    return await response.Content.ReadAsStringAsync(ct);
});
```

**Advanced Polly policies**:

```csharp title="Program.cs" linenums="1"
using Polly;
using Polly.CircuitBreaker;

// Circuit breaker policy
var circuitBreakerPolicy = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(30)
    );

// Timeout policy
var timeoutPolicy = Policy
    .TimeoutAsync(TimeSpan.FromSeconds(10));

// Combine policies (timeout → retry → circuit breaker)
var combinedPolicy = Policy.WrapAsync(circuitBreakerPolicy, retryPolicy, timeoutPolicy);

await combinedPolicy.ExecuteAsync(async () =>
    await httpClient.GetAsync(url, cancellationToken)
);
```

!!! tip "Retry best practices"

- Only retry **transient errors** (network failures, timeouts, 5xx HTTP responses)
- **Don't retry** validation errors, authentication failures, or 4xx responses
- Use **exponential backoff** to avoid overwhelming downstream services
- Add **jitter** to prevent thundering herd problem
- Respect **Lambda timeout** - ensure retries complete before cancellation

---

## AWS Lambda Integration

### Event Source Retries

AWS Lambda provides built-in retry mechanisms for asynchronous event sources:

| Event Source                   | Retry Behavior                                                                                                    |
|--------------------------------|-------------------------------------------------------------------------------------------------------------------|
| **SQS**                        | Configurable `maxReceiveCount` on queue. After retries exhausted, message moves to DLQ (if configured).           |
| **EventBridge**                | Automatic retries for up to 24 hours with exponential backoff.                                                    |
| **Kinesis / DynamoDB Streams** | Retries until record expires or processed successfully. Use `BisectBatchOnFunctionError` to isolate poison pills. |
| **SNS**                        | Retries twice with exponential backoff. After failure, message discarded (configure DLQ on SNS topic).            |
| **S3**                         | No automatic retries. Use S3 Event Notifications with SQS for retry capability.                                   |

**Synchronous invocations** (API Gateway, ALB, Lambda Function URLs):

- **No automatic retries** - client must retry
- Errors return immediately to caller
- Use application-level retries (Polly) or API Gateway retry configuration

### Dead Letter Queues (DLQ)

DLQs are configured **at the AWS Lambda level**, not in the application code.

**Asynchronous invocations**:

```yaml title="template.yaml (SAM)" linenums="1"
Resources:
  MyFunction:
    Type: AWS::Serverless::Function
    Properties:
      Handler: bootstrap
      Runtime: provided.al2023
      DeadLetterQueue:
        Type: SQS
        TargetArn: !GetAtt MyFunctionDLQ.Arn

  MyFunctionDLQ:
    Type: AWS::SQS::Queue
    Properties:
      QueueName: MyFunction-DLQ
      MessageRetentionPeriod: 1209600  # 14 days
```

**SQS event source**:

```yaml title="template.yaml (SAM)" linenums="1"
Resources:
  MyQueue:
    Type: AWS::SQS::Queue
    Properties:
      QueueName: MyQueue
      RedrivePolicy:
        deadLetterTargetArn: !GetAtt MyQueueDLQ.Arn
        maxReceiveCount: 3  # Retry 3 times before DLQ

  MyQueueDLQ:
    Type: AWS::SQS::Queue
    Properties:
      QueueName: MyQueue-DLQ
```

**Monitoring DLQs**:

```yaml title="template.yaml (SAM)" linenums="1"
Resources:
  DLQAlarm:
    Type: AWS::CloudWatch::Alarm
    Properties:
      AlarmName: MyFunction-DLQ-Alarm
      MetricName: ApproximateNumberOfMessagesVisible
      Namespace: AWS/SQS
      Statistic: Sum
      Period: 300
      EvaluationPeriods: 1
      Threshold: 1
      ComparisonOperator: GreaterThanOrEqualToThreshold
      Dimensions:
        - Name: QueueName
          Value: !GetAtt MyFunctionDLQ.QueueName
      AlarmActions:
        - !Ref AlertTopic
```

---

## Best Practices

### ✅ Do

- **Use specific exception types** to distinguish error categories (validation, business logic,
  infrastructure)
- **Log errors with context**: Request IDs, correlation IDs, input parameters (sanitized)
- **Respect cancellation tokens**: Pass to all async operations, check `IsCancellationRequested` in
  loops
- **Implement retries for transient failures**: Network errors, timeouts, 5xx HTTP responses
- **Use middleware for global error handling**: Logging, metrics, error tracking service integration
- **Return structured error responses**: Consistent error format for API clients
- **Configure appropriate `InvocationCancellationBuffer`**: Balance between graceful handling and
  Lambda timeout
- **Handle `OperationCanceledException` gracefully**: Return partial results or meaningful error
  response
- **Monitor DLQs with CloudWatch alarms**: Get notified when messages accumulate
- **Use OnInit return value for graceful abort**: Return `false` instead of throwing when startup
  should abort

### ❌ Don't

- **Don't swallow exceptions without logging**: Silent failures make debugging impossible
- **Don't ignore cancellation tokens**: Leads to hard Lambda timeouts and incomplete work
- **Don't retry non-transient errors**: Validation errors, authentication failures, 4xx responses
- **Don't let OnInit timeout**: Keep initialization fast (< 5 seconds by default)
- **Don't assume OnShutdown always completes**: AWS sends SIGKILL after ShutdownDuration
- **Don't use long shutdown operations**: Limited time window (typically < 500ms)
- **Don't log sensitive data**: Sanitize PII, credentials, and secrets before logging
- **Don't retry indefinitely**: Respect Lambda timeout, use exponential backoff with max retries

---

## Anti-Patterns to Avoid

### ❌ Ignoring Cancellation Tokens

```csharp title="❌ Bad - No cancellation token"
lambda.MapHandler(async ([Event] Request request, IDataService dataService) =>
{
    // This operation can't be cancelled - will continue until Lambda timeout
    var data = await dataService.ProcessLargeDatasetAsync(request.DatasetId);
    return new Response { Data = data };
});
```

```csharp title="✅ Good - Respects cancellation"
lambda.MapHandler(async (
    [Event] Request request,
    IDataService dataService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var data = await dataService.ProcessLargeDatasetAsync(request.DatasetId, cancellationToken);
        return new Response { Data = data };
    }
    catch (OperationCanceledException)
    {
        // Handle timeout gracefully
        return new Response { Status = "Timeout", PartialData = dataService.GetPartialResults() };
    }
});
```

### ❌ OnInit Timeout Loops

```csharp title="❌ Bad - Infinite loop ignores timeout"
lambda.OnInit(async (CancellationToken cancellationToken) =>
{
    while (true)  // Will timeout after InitTimeout (default 5s)
    {
        await Task.Delay(100);
    }
    return true;
});
```

```csharp title="✅ Good - Respects cancellation token"
lambda.OnInit(async (ICache cache, CancellationToken cancellationToken) =>
{
    try
    {
        await cache.WarmUpAsync(cancellationToken);
        return true;
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Init timeout - cache warmup incomplete");
        return false;  // Abort startup gracefully
    }
});
```

### ❌ Retrying Non-Transient Errors

```csharp title="❌ Bad - Retries validation errors"
for (int i = 0; i < 3; i++)
{
    try
    {
        return await orderService.CreateOrderAsync(order);
    }
    catch (Exception)  // Catches ALL exceptions including validation errors
    {
        await Task.Delay(1000);  // Wastes time retrying invalid input
    }
}
```

```csharp title="✅ Good - Only retries transient errors"
for (int i = 0; i < 3; i++)
{
    try
    {
        return await orderService.CreateOrderAsync(order);
    }
    catch (ValidationException)
    {
        throw;  // Don't retry validation errors
    }
    catch (HttpRequestException) when (i < 2)
    {
        await Task.Delay(1000);  // Retry transient network errors
    }
}
```

---

## Troubleshooting

### "CancellationTokenSource provided with insufficient time"

**Error message**:

```
InvalidOperationException: CancellationTokenSource provided with insufficient time.
Lambda Remaining Time = 00:00:02, Cancellation Token Buffer = 00:00:03, Candidate Token Duration = -00:00:01
```

**Cause**: `InvocationCancellationBuffer` exceeds Lambda's remaining time.

**Solutions**:

1. **Reduce the buffer**:
   ```csharp
   builder.Services.ConfigureLambdaHostOptions(options =>
   {
       options.InvocationCancellationBuffer = TimeSpan.FromSeconds(1);
   });
   ```

2. **Increase Lambda timeout** (in SAM/CDK/Terraform template):
   ```yaml
   MyFunction:
     Type: AWS::Serverless::Function
     Properties:
       Timeout: 30  # Increase from current value
   ```

3. **Optimize previous operations** to leave more remaining time.

### "Encountered errors while running OnInit handlers"

**Error message**:

```
AggregateException: Encountered errors while running OnInit handlers:
  ---> InvalidOperationException: Database connection failed
  ---> TimeoutException: Cache warmup timed out
```

**Cause**: One or more OnInit handlers threw exceptions.

**Solutions**:

1. **Check inner exceptions**:
   ```csharp
   try
   {
       await lambdaHost.RunAsync();
   }
   catch (AggregateException aggEx)
   {
       foreach (var innerEx in aggEx.InnerExceptions)
       {
           Console.WriteLine($"Init error: {innerEx.Message}");
       }
   }
   ```

2. **Add error handling to OnInit handlers**:
   ```csharp
   lambda.OnInit(async (ICache cache, CancellationToken ct) =>
   {
       try
       {
           await cache.WarmUpAsync(ct);
           return true;
       }
       catch (Exception ex)
       {
           Console.WriteLine($"Cache warmup failed: {ex.Message}");
           return false;  // Abort gracefully instead of throwing
       }
   });
   ```

3. **Increase InitTimeout**:
   ```csharp
   builder.Services.ConfigureLambdaHostOptions(options =>
   {
       options.InitTimeout = TimeSpan.FromSeconds(10);
   });
   ```

### "Graceful shutdown of the Lambda function failed"

**Error message**:

```
OperationCanceledException: Graceful shutdown of the Lambda function failed: the bootstrap operation did not complete within the allocated timeout period.
```

**Cause**: Bootstrap process didn't complete within shutdown timeout.

**Solutions**:

1. **Ensure OnShutdown handlers respect cancellation token**:
   ```csharp
   lambda.OnShutdown(async (IMetrics metrics, CancellationToken ct) =>
   {
       try
       {
           await metrics.FlushAsync(ct);  // Must support cancellation
       }
       catch (OperationCanceledException)
       {
           Console.WriteLine("Metrics flush cancelled due to shutdown timeout");
       }
   });
   ```

2. **Reduce OnShutdown work**:
   ```csharp
   lambda.OnShutdown(async (IMetrics metrics) =>
   {
       // Quick flush - don't wait for acknowledgment
       metrics.FlushAndForget();
       await Task.CompletedTask;
   });
   ```

3. **Increase shutdown duration** (if using external extensions):
   ```csharp
   builder.Services.ConfigureLambdaHostOptions(options =>
   {
       options.ShutdownDuration = ShutdownDuration.ExternalExtensions;  // 500ms
       options.ShutdownDurationBuffer = TimeSpan.FromMilliseconds(50);
   });
   ```

---

## Key Takeaways

1. **Exception handling**: Use middleware for global error policies, handlers for specific error
   responses
2. **Cancellation tokens**: Critical for respecting Lambda timeouts and graceful shutdown
3. **InvocationCancellationBuffer**: Prevents hard timeouts by firing cancellation before Lambda
   terminates
4. **OnInit errors**: Handlers run in parallel, errors aggregated, can return `false` to abort
5. **OnShutdown errors**: All handlers execute despite individual failures, errors aggregated
6. **Retries**: Framework provides no built-in retry - use Polly or custom logic
7. **DLQs**: Configure at AWS Lambda level for asynchronous event sources
8. **Error logging**: Always include request context (request ID, correlation ID)

---

## Next Steps

- **[Testing](testing.md)** - Learn how to test error handling logic
- **[Lifecycle Management](lifecycle-management.md)** - Deep dive into OnInit and OnShutdown
- **[Configuration](configuration.md)** - Configure timeout and cancellation settings
- **[Deployment](deployment.md)** - Configure DLQs and CloudWatch alarms in infrastructure
