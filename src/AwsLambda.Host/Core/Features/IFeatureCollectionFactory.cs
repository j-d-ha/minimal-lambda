namespace AwsLambda.Host.Core;

internal interface IFeatureCollectionFactory
{
    IFeatureCollection Create();

    IFeatureCollection Create(IEnumerable<IFeatureProvider> featureProviders);
}
