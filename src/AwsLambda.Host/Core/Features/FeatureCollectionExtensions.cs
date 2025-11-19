using System.Diagnostics.CodeAnalysis;

namespace AwsLambda.Host;

public static class FeatureCollectionExtensions
{
    extension(IFeatureCollection featureCollection)
    {
        public bool TryGet<T>([NotNullWhen(true)] out T? result)
        {
            result = featureCollection.Get<T>();
            return result is not null;
        }
    }
}
