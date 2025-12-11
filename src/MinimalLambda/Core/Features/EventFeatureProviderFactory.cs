using Amazon.Lambda.Core;

namespace MinimalLambda;

internal class EventFeatureProviderFactory(ILambdaSerializer lambdaSerializer)
    : IEventFeatureProviderFactory
{
    public IFeatureProvider Create<T>() => new DefaultEventFeatureProvider<T>(lambdaSerializer);
}
