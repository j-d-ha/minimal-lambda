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
    private readonly ILambdaInvocationBuilder _invocationBuilder;
    private readonly ILambdaOnInitBuilder _onInitBuilder;
    private readonly ILambdaOnShutdownBuilder _onShutdownBuilder;

    internal LambdaApplication(IHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        _host = host;

        _invocationBuilder = Services
            .GetRequiredService<IInvocationBuilderFactory>()
            .CreateBuilder();

        _onInitBuilder = Services.GetRequiredService<IOnInitBuilderFactory>().CreateBuilder();

        _onShutdownBuilder = Services
            .GetRequiredService<IOnShutdownBuilderFactory>()
            .CreateBuilder();
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

    public IDictionary<string, object?> Properties => _invocationBuilder.Properties;

    public List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares =>
        _invocationBuilder.Middlewares;

    public LambdaInvocationDelegate? Handler => _invocationBuilder.Handler;

    public ILambdaInvocationBuilder Handle(LambdaInvocationDelegate handler)
    {
        _invocationBuilder.Handle(handler);
        return this;
    }

    public ILambdaInvocationBuilder Use(
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware
    )
    {
        _invocationBuilder.Use(middleware);
        return this;
    }

    public LambdaInvocationDelegate Build() => _invocationBuilder.Build();

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                   ILambdaOnInitBuilder                   │
    //      └──────────────────────────────────────────────────────────┘

    public IList<LambdaInitDelegate> InitHandlers => _onInitBuilder.InitHandlers;

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                 ILambdaOnShutdownBuilder                 │
    //      └──────────────────────────────────────────────────────────┘

    public IList<LambdaShutdownDelegate> ShutdownHandlers => _onShutdownBuilder.ShutdownHandlers;
}
