namespace AwsLambda.Host.Core;

internal class DefaultFeatureCollectionFactory : IFeatureCollectionFactory
{
    private readonly IEnumerable<IFeatureProvider> _featureProviders;

    public DefaultFeatureCollectionFactory(IEnumerable<IFeatureProvider?> featureProviders) =>
        _featureProviders = featureProviders.Where(x => x is not null).Select(x => x!).ToArray();

    public IFeatureCollection Create() => new DefaultFeatureCollection(_featureProviders);
}
