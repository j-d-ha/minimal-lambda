using Amazon.Lambda.Core;

namespace AwsLambda.Host.Core;

internal class EventFeatureProviderFactory(ILambdaSerializer lambdaSerializer)
    : IEventFeatureProviderFactory
{
    public IFeatureProvider Create<T>() => new DefaultEventFeatureProvider<T>(lambdaSerializer);
}
