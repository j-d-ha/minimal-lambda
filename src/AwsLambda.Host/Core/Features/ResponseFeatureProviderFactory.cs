#region

using Amazon.Lambda.Core;

#endregion

namespace AwsLambda.Host.Core;

internal class ResponseFeatureProviderFactory(ILambdaSerializer lambdaSerializer)
    : IResponseFeatureProviderFactory
{
    public IFeatureProvider Create<T>() => new DefaultResponseFeatureProvider<T>(lambdaSerializer);
}
