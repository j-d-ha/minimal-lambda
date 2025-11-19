using System.Diagnostics.CodeAnalysis;

namespace AwsLambda.Host;

/// <summary>
///     Provides extension methods for safely accessing typed event and response data from the
///     <see cref="IFeatureCollection" /> on <see cref="ILambdaHostContext" />.
/// </summary>
public static class FeatureLambdaHostContextExtensions
{
    extension(ILambdaHostContext context)
    {
        public T? GetEvent<T>()
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Features.Get<IEventFeature>()?.GetEvent(context) is T data
                ? data
                : default;
        }

        public bool TryGetEvent<T>([NotNullWhen(true)] out T? result)
        {
            ArgumentNullException.ThrowIfNull(context);

            result = context.GetEvent<T>();
            return result is not null;
        }

        public T? GetResponse<T>()
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Features.Get<IResponseFeature>()?.GetResponse() is T data
                ? data
                : default;
        }

        public bool TryGetResponse<T>([NotNullWhen(true)] out T? result)
        {
            ArgumentNullException.ThrowIfNull(context);

            result = context.GetResponse<T>();
            return result is not null;
        }
    }
}
