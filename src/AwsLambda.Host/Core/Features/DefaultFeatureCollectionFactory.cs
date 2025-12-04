namespace AwsLambda.Host.Core;

internal class DefaultFeatureCollectionFactory(IEnumerable<IFeatureProvider> providers)
    : IFeatureCollectionFactory
{
    public IFeatureCollection Create(IEnumerable<IFeatureProvider> featureProviders) =>
        new DefaultFeatureCollection(providers.Concat(featureProviders).ToArray());
}
