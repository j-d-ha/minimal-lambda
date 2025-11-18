using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AwsLambda.Host;

/// <summary>A Lambda application that provides host functionality for running AWS Lambda handlers.</summary>
public sealed class LambdaApplication
    : IHost,
        ILambdaInvocationBuilder,
        ILambdaOnInitBuilder,
        ILambdaOnShutdownBuilder,
        IAsyncDisposable
{
    private readonly IHost _host;

    internal LambdaApplication(IHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        _host = host;
    }

    public IConfiguration Configuration =>
        field ??= _host.Services.GetRequiredService<IConfiguration>();

    public IHostEnvironment Environment =>
        field ??= _host.Services.GetRequiredService<IHostEnvironment>();

    public IHostApplicationLifetime Lifetime =>
        field ??= _host.Services.GetRequiredService<IHostApplicationLifetime>();

    public ILogger Logger =>
        field ??=
            _host
                .Services.GetService<ILoggerFactory>()
                ?.CreateLogger(Environment.ApplicationName ?? nameof(LambdaApplication))
            ?? NullLogger.Instance;

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ((IAsyncDisposable)_host).DisposeAsync();

    public IServiceProvider Services => _host.Services;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken = default) =>
        _host.StartAsync(cancellationToken);

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken = default) =>
        _host.StopAsync(cancellationToken);

    /// <inheritdoc />
    public void Dispose() => _host.Dispose();

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                 ILambdaInvocationBuilder                 │
    //      └──────────────────────────────────────────────────────────┘

    public IDictionary<string, object?> Properties { get; }
    public List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares { get; }
    public LambdaInvocationDelegate? Handler { get; }

    public ILambdaInvocationBuilder Handle(LambdaInvocationDelegate handler) =>
        throw new NotImplementedException();

    public ILambdaInvocationBuilder Use(
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware
    ) => throw new NotImplementedException();

    public LambdaInvocationDelegate Build() => throw new NotImplementedException();

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                   ILambdaOnInitBuilder                   │
    //      └──────────────────────────────────────────────────────────┘

    public List<LambdaInitDelegate> InitHandlers { get; }

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                 ILambdaOnShutdownBuilder                 │
    //      └──────────────────────────────────────────────────────────┘

    public List<LambdaShutdownDelegate> ShutdownHandlers { get; }
}
