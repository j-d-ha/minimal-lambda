using Amazon.Lambda.Core;

namespace AwsLambda.Host.Core;

internal class ResponseFeatureProviderFactory(ILambdaSerializer lambdaSerializer)
    : IResponseFeatureProviderFactory
{
    public IFeatureProvider Create<T>() => new DefaultResponseFeatureProvider<T>(lambdaSerializer);
}
