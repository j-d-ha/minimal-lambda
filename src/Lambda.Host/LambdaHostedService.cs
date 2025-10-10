using Microsoft.Extensions.Hosting;

namespace Lambda.Host;

public abstract class LambdaHostedService : IHostedService
{
    public virtual Task StartAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public virtual Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
