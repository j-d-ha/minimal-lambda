using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

/// <summary>A Lambda application that provides host functionality for running AWS Lambda handlers.</summary>
public sealed class LambdaApplication : IHost, ILambdaApplication, IAsyncDisposable
{
    private readonly DelegateHolder _delegateHolder;
    private readonly IHost _host;

    internal LambdaApplication(IHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        _host = host;
        _delegateHolder =
            Services.GetRequiredService<DelegateHolder>() ?? throw new InvalidOperationException();

        var settings = Services.GetRequiredService<IOptions<LambdaHostOptions>>().Value;

        if (settings.ClearLambdaOutputFormatting)
            this.OnInitClearLambdaOutputFormatting();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ((IAsyncDisposable)_host).DisposeAsync();

    /// <inheritdoc />
    public void Dispose() => _host.Dispose();

    /// <inheritdoc />
    /// <remarks>
    ///     Before starting the host, this method applies default middleware to the invocation
    ///     pipeline.
    /// </remarks>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        // add default middleware to the end of the pipeline
        AddDefaultMiddleware();

        return _host.StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken = default) =>
        _host.StopAsync(cancellationToken);

    /// <inheritdoc cref="ILambdaApplication.Services" />
    public IServiceProvider Services => _host.Services;

    /// <inheritdoc />
    public ILambdaApplication MapHandler(
        LambdaInvocationDelegate handler,
        Func<ILambdaHostContext, ILambdaSerializer, Stream, Task>? deserializer,
        Func<ILambdaHostContext, ILambdaSerializer, Task<Stream>>? serializer
    )
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (_delegateHolder.IsHandlerSet)
            throw new InvalidOperationException(
                "Lambda Handler is already set. Only one is allowed."
            );

        _delegateHolder.Handler = handler;

        _delegateHolder.Deserializer = deserializer;
        _delegateHolder.Serializer = serializer;

        return this;
    }

    /// <inheritdoc />
    public ILambdaApplication Use(
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware
    )
    {
        ArgumentNullException.ThrowIfNull(middleware);

        _delegateHolder.Middlewares.Add(middleware);

        return this;
    }

    /// <inheritdoc />
    public ILambdaApplication OnInit(LambdaInitDelegate handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _delegateHolder.InitHandlers.Add(handler);

        return this;
    }

    /// <inheritdoc />
    public ILambdaApplication OnShutdown(LambdaShutdownDelegate handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _delegateHolder.ShutdownHandlers.Add(handler);

        return this;
    }

    private void AddDefaultMiddleware() =>
        // Add Envelope middleware
        this.UseExtractAndPackEnvelope();
}
