namespace AwsLambda.Host.Core;

public interface IResponseFeatureProviderFactory
{
    IFeatureProvider Create<T>();
}
