using Amazon.Lambda.Core;

namespace MinimalLambda.Core;

internal class ResponseFeatureProviderFactory(ILambdaSerializer lambdaSerializer)
    : IResponseFeatureProviderFactory
{
    public IFeatureProvider Create<T>() => new DefaultResponseFeatureProvider<T>(lambdaSerializer);
}
