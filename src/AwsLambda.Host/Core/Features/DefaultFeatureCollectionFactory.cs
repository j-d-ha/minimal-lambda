namespace AwsLambda.Host.Core;

internal class DefaultFeatureCollectionFactory(IEnumerable<IFeatureProvider> featureProviders)
    : IFeatureCollectionFactory
{
    public IFeatureCollection Create() => new DefaultFeatureCollection(featureProviders);
}
