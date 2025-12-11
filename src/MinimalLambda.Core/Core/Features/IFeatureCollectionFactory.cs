namespace MinimalLambda.Core;

internal interface IFeatureCollectionFactory
{
    IFeatureCollection Create(IEnumerable<IFeatureProvider> featureProviders);
}
