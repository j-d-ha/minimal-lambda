using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace AwsLambda.Host.Testing;

public class LambdaTestServer : IServer
{
    public void Dispose() => throw new NotImplementedException();

    public Task StartAsync<TContext>(
        IHttpApplication<TContext> application,
        CancellationToken cancellationToken
    ) => throw new NotImplementedException();

    public Task StopAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public IFeatureCollection Features { get; }
}
