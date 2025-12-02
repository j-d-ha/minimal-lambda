namespace AwsLambda.Host.Core;

public interface IEventFeatureProviderFactory
{
    IFeatureProvider Create<T>();
}
