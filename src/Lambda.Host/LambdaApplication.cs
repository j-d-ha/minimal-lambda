using Microsoft.Extensions.Hosting;

namespace Lambda.Host;

public sealed class LambdaApplication : IHost, IAsyncDisposable
{
    private readonly IHost _host;
    private Delegate _handler;

    public LambdaApplication(IHost host) =>
        _host = host ?? throw new ArgumentNullException(nameof(host));

    public ValueTask DisposeAsync() => ((IAsyncDisposable)_host).DisposeAsync();

    public void Dispose() => _host.Dispose();

    public Task StartAsync(CancellationToken cancellationToken = default) =>
        _host.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken = default) =>
        _host.StopAsync(cancellationToken);

    public IServiceProvider Services => _host.Services;

    public void MapHandler(Delegate handler) =>
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
}
