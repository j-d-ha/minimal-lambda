namespace AwsLambda.Host.Core.Features;

internal class FeatureCollectionFactory : IFeatureCollectionFactory
{
    public IFeatureCollection Create() => new FeatureCollection();
}
