using System.Diagnostics.CodeAnalysis;
using AwsLambda.Host.Core;

namespace AwsLambda.Host;

/// <summary>
///     Provides extension methods for safely accessing typed event and response data from the
///     <see cref="IFeatureCollection" /> on <see cref="ILambdaHostContext" />.
/// </summary>
public static class FeatureLambdaHostContextExtensions
{
    extension(ILambdaHostContext context)
    {
        /// <summary>Gets the typed event data from the <see cref="IEventFeature" /> in the Lambda context.</summary>
        /// <typeparam name="T">The type of event data to retrieve.</typeparam>
        /// <returns>The typed event data, or null if not found or not of the specified type.</returns>
        public T? GetEvent<T>()
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Features.Get<IEventFeature>()?.GetEvent(context) is T data
                ? data
                : default;
        }

        /// <summary>
        ///     Attempts to get the typed event data from the <see cref="IEventFeature" /> in the Lambda
        ///     context.
        /// </summary>
        /// <typeparam name="T">The type of event data to retrieve.</typeparam>
        /// <param name="result">The typed event data, or null if not found or not of the specified type.</param>
        /// <returns>True if the event data was found and is of the specified type; otherwise false.</returns>
        public bool TryGetEvent<T>([NotNullWhen(true)] out T? result)
        {
            ArgumentNullException.ThrowIfNull(context);

            result = context.GetEvent<T>();
            return result is not null;
        }

        /// <summary>
        ///     Gets the typed response data from the <see cref="IResponseFeature" /> in the Lambda
        ///     context.
        /// </summary>
        /// <typeparam name="T">The type of response data to retrieve.</typeparam>
        /// <returns>The typed response data, or null if not found or not of the specified type.</returns>
        public T? GetResponse<T>()
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Features.Get<IResponseFeature>()?.GetResponse() is T data
                ? data
                : default;
        }

        /// <summary>
        ///     Attempts to get the typed response data from the <see cref="IResponseFeature" /> in the
        ///     Lambda context.
        /// </summary>
        /// <typeparam name="T">The type of response data to retrieve.</typeparam>
        /// <param name="result">The typed response data, or null if not found or not of the specified type.</param>
        /// <returns>True if the response data was found and is of the specified type; otherwise false.</returns>
        public bool TryGetResponse<T>([NotNullWhen(true)] out T? result)
        {
            ArgumentNullException.ThrowIfNull(context);

            result = context.GetResponse<T>();
            return result is not null;
        }
    }
}
