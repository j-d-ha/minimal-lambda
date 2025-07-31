using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lambda.Host;

public sealed class LambdaApplication : IHost, IAsyncDisposable
{
    private readonly IHost _host;

    public LambdaApplication(IHost host) =>
        _host = host ?? throw new ArgumentNullException(nameof(host));

    public ValueTask DisposeAsync() => ((IAsyncDisposable)_host).DisposeAsync();

    public void Dispose() => _host.Dispose();

    public Task StartAsync(CancellationToken cancellationToken = default) =>
        _host.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken = default) =>
        _host.StopAsync(cancellationToken);

    public IServiceProvider Services => _host.Services;

    public LambdaApplication MapHandler(Delegate handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var delegateHolder = Services.GetRequiredService<DelegateHolder>();

        if (delegateHolder.IsHandlerSet)
            throw new InvalidOperationException("Handler is already set");

        delegateHolder.Handler = handler;

        return this;
    }

    public static LambdaApplicationBuilder CreateBuilder() => new();
}
