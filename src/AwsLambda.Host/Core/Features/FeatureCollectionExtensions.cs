namespace AwsLambda.Host.Core.Features;

public static class FeatureCollectionExtensions
{
    extension(IFeatureCollection featureCollection)
    {
        public void Set<T>(T instance)
        {
            ArgumentNullException.ThrowIfNull(featureCollection);
            ArgumentNullException.ThrowIfNull(instance);

            featureCollection.Set(typeof(T), instance);
        }
    }
}
