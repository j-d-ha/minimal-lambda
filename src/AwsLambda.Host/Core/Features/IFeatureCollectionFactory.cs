namespace AwsLambda.Host.Core;

internal interface IFeatureCollectionFactory
{
    IFeatureCollection Create(IEnumerable<IFeatureProvider> featureProviders);
}
