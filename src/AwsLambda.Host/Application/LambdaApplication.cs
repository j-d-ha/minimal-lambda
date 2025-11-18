using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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

        var settings = Services.GetRequiredService<IOptions<LambdaHostOptions>>().Value;

        if (settings.ClearLambdaOutputFormatting)
            this.OnInitClearLambdaOutputFormatting();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ((IAsyncDisposable)_host).DisposeAsync();

    public IServiceProvider Services => _host.Services;

    /// <inheritdoc />
    public void Dispose() => _host.Dispose();

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return _host.StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken = default) =>
        _host.StopAsync(cancellationToken);

    public IDictionary<string, object?> Properties { get; }
    public List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares { get; }
    public LambdaInvocationDelegate? Handler { get; }

    public ILambdaInvocationBuilder Handle(LambdaInvocationDelegate handler) =>
        throw new NotImplementedException();

    public ILambdaInvocationBuilder Use(
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware
    ) => throw new NotImplementedException();

    public LambdaInvocationDelegate Build() => throw new NotImplementedException();

    public List<LambdaInitDelegate> InitHandlers { get; }
    public List<LambdaShutdownDelegate> ShutdownHandlers { get; }
}
