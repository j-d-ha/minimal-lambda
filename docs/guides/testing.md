# Testing

MinimalLambda.Testing works like ASP.NET Core’s `WebApplicationFactory`: it boots your real Lambda
entry point in memory, speaking the same Runtime API contract that AWS uses. It is the
end-to-end/integration layer above pure unit tests: you run the real pipeline (handlers, middleware,
lifecycle hooks, DI) without deploying or opening ports.

## When to Use

- **End-to-end pipeline coverage** – Exercise source-generated handlers, middleware, envelopes, and
  lifecycle hooks with real DI and serialization.
- **Regression nets** – Verify bootstrapping, cold-start logic, and error payloads stay stable.
- **Host customization** – Override configuration/services per test via `WithHostBuilder`.
- Prefer plain unit tests for isolated logic; reach for MinimalLambda.Testing when you need
  confidence in the Lambda runtime behavior.

## Quick Start

Install both packages:

```bash
dotnet add package MinimalLambda
dotnet add package MinimalLambda.Testing
```

Write an end-to-end test with xUnit v3:

```csharp title="HelloWorldTests.cs" linenums="1"
using MinimalLambda.Testing;
using Xunit;

public class HelloWorldTests
{
    [Fact]
    public async Task HelloWorld_ReturnsGreeting()
    {
        await using var factory = new LambdaApplicationFactory<Program>()
            .WithCancellationToken(TestContext.Current.CancellationToken);

        // Optional: StartAsync mirrors Lambda init; InvokeAsync will start on demand if you skip this.
        await factory.TestServer.StartAsync(TestContext.Current.CancellationToken);

        var response = await factory.TestServer.InvokeAsync<string, string>(
            "World",
            TestContext.Current.CancellationToken
        );

        Assert.True(response.WasSuccess);
        Assert.Equal("Hello World!", response.Response);
    }
}
```

## Invocation APIs

- `InvokeAsync<TEvent, TResponse>(event, token)` – Send a strongly typed event, expect a typed
  response; fails with an `InvocationResponse<TResponse>` containing error details on handler
  exceptions.
- `InvokeNoEventAsync<TResponse>(token)` – Invoke a handler that does not take an event payload.
- `InvokeNoResponseAsync<TEvent>(event, token)` – Fire-and-forget style; skips response
  deserialization for handlers that return `void`/`Task` or write directly to streams.
- **Trace IDs** – Pass `traceId` into `InvokeAsync` to control the `Lambda-Runtime-Trace-Id` header
  (defaults to a new GUID).

`InvocationResponse`/`InvocationResponse<T>` include `WasSuccess`, `Response`, and structured
`Error` information that mirrors Lambda runtime error payloads—assert on these to verify failures.

## Working with Cancellation

- **Propagate test cancellation** – Call `WithCancellationToken(...)` on the factory to flow your
  test framework’s token into the in-memory runtime. All server operations observe it.
- **Per-call tokens** – Pass tokens to `StartAsync` and `Invoke*` to bound individual operations.
- **Pre-canceled tokens** – A pre-canceled token will fail the invocation immediately (see
  `SimpleLambda_WithPreCanceledToken_CancelsInvocation` in the test suite).
- **Timeouts** – Combine short tokens with `LambdaServerOptions.FunctionTimeout` to mirror Lambda’s
  deadline behavior and catch slow handlers.

## Host Customization and Fixtures

### Override Host Configuration

Use `WithHostBuilder` to tweak services and configuration for a specific test run:

```csharp title="DI override" linenums="1"
await using var factory = new LambdaApplicationFactory<Program>()
    .WithHostBuilder(builder =>
    {
        builder.ConfigureServices((_, services) =>
        {
            // Swap implementations or inject test doubles
            services.Configure<LambdaHostOptions>(options =>
            {
                options.BootstrapOptions.RuntimeApiEndpoint = "http://localhost:9001";
            });
        });
    });
```

You can also override app configuration (`ConfigureAppConfiguration`) or swap the DI container using
`UseServiceProviderFactory` / `ConfigureContainer` (Autofac, etc.)—the factory will replay those
changes before the Lambda host boots.

### Content Roots for File Fixtures

Add `LambdaApplicationFactoryContentRootAttribute` to your test assembly when you need a predictable
content root (e.g., static files, JSON fixtures). The factory will pick it up and set the content
root before booting the host.

### Tuning the Runtime Shim

`LambdaServerOptions` controls the simulated runtime headers and timing (ARN, deadline/timeout,
extra headers). Access it via `factory.ServerOptions` before starting the server if you need
test-specific values.

## Initialization and Shutdown Behavior

- `StartAsync` returns `InitResponse` with `InitStatus` values:
  - `InitCompleted` / `InitAlreadyCompleted` – Ready to invoke.
  - `InitError` – An `ErrorResponse` from OnInit failures; server stops itself.
  - `HostExited` – Entry point exited early (e.g., OnInit signaled stop).
- `Invoke*` will start the server on-demand; if init fails it throws with the reported status.
- `StopAsync` triggers OnShutdown and aggregates any exceptions (surfaced as `AggregateException`).
- `DisposeAsync` is idempotent; safe to call multiple times.

## Patterns and Troubleshooting

- **Reuse factories per class** – Creating a new factory per test is fine; reuse within a class to
  speed up suites that share the same host configuration.
- **Parallel invocations** – The test server is concurrency-safe; the concurrent invocation test in
  the suite shows FIFO ordering.
- **Assert via DI** – Resolve services from `factory.TestServer.Services` to inspect metrics,
  in-memory stores, or other state after invocations.
- **Error assertions** – Check `InvocationResponse.Error` for message and stack trace data; it
  mirrors what the Lambda Runtime API returns.
- **Runtime headers** – Responses include the same headers Lambda sends (`Lambda-Runtime-*` plus any
  `AdditionalHeaders` you set); assert on them if you need to prove deadline/ARN behavior.

!!! warning "Fixture reuse pitfalls"
    - Using `IClassFixture`/`ICollectionFixture` with a single `LambdaApplicationFactory` means one
      host instance is shared across all tests in that scope. Avoid this pattern if you need to test
      startup/shutdown logic—use a fresh factory per test so OnInit/OnShutdown run predictably.
    - Do not mix a fixture-based factory with new factories created inside individual tests; they can
      overlap and run simultaneously, leading to multiple hosts executing in parallel and surprising
      side effects. Choose one approach (per-test or shared fixture) for a given test class/collection
      and clean up via `DisposeAsync`/`StopAsync` when done.

Ready to go deeper? The MinimalLambda.Testing source (`src/MinimalLambda.Testing/`) and its unit
tests (`tests/MinimalLambda.Testing.UnitTests/`) contain more examples of host overrides,
cancellation, and error handling patterns.
