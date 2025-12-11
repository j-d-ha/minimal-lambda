namespace MinimalLambda.Host.Core;

internal interface IFeatureCollectionFactory
{
    IFeatureCollection Create(IEnumerable<IFeatureProvider> featureProviders);
}
