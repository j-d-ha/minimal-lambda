namespace MinimalLambda;

internal interface IFeatureCollectionFactory
{
    IFeatureCollection Create(IEnumerable<IFeatureProvider> featureProviders);
}
