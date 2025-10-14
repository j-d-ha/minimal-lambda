using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lambda.Host;

public sealed class LambdaApplication : IHost, IAsyncDisposable
{
    private readonly IHost _host;

    internal LambdaApplication(IHost host) =>
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
        var delegateHolder = Services.GetRequiredService<DelegateHolder>();

        if (delegateHolder.IsHandlerSet)
            throw new InvalidOperationException("Handler is already set");

        delegateHolder.Handler = handler ?? throw new ArgumentNullException(nameof(handler));

        return this;
    }

    //  ┌──────────────────────────────────────────────────────────┐
    //  │                 Builder Factory Methods                  │
    //  └──────────────────────────────────────────────────────────┘

    public static LambdaApplicationBuilder CreateBuilder() => new();

    public static LambdaApplicationBuilder CreateBuilder<T>()
        where T : LambdaHostedService
    {
        var builder = new LambdaApplicationBuilder();

        builder.Services.AddSingleton<LambdaHostedService, T>();

        return builder;
    }
}
