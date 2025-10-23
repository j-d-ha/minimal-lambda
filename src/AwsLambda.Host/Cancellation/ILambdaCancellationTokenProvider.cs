using Amazon.Lambda.Core;

namespace AwsLambda.Host;

public interface ILambdaCancellationTokenSourceFactory
{
    public CancellationTokenSource NewCancellationTokenSource(ILambdaContext context);
}
