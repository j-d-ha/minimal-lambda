using Microsoft.Extensions.Hosting;

namespace Lambda.Host;

public sealed class LambdaApplication : IHost, IAsyncDisposable
{
    public void Dispose() => throw new NotImplementedException();

    public Task StartAsync(CancellationToken cancellationToken = new()) =>
        throw new NotImplementedException();

    public Task StopAsync(CancellationToken cancellationToken = new()) =>
        throw new NotImplementedException();

    public IServiceProvider Services { get; }

    public ValueTask DisposeAsync() => throw new NotImplementedException();
}
