using Amazon.Lambda.Core;

namespace Lambda.Host;

public interface ILambdaCancellationTokenSourceFactory
{
    public CancellationTokenSource NewCancellationTokenSource(ILambdaContext context);
}
