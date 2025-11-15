using System.Diagnostics.CodeAnalysis;

namespace AwsLambda.Host;

/// <summary>
///     Provides extension methods for safely accessing typed event and response data from an
///     <see cref="ILambdaHostContext" />.
/// </summary>
public static class LambdaHostContextExtensions
{
    extension(ILambdaHostContext context)
    {
        /// <summary>Gets the Lambda event as a strongly-typed object.</summary>
        /// <remarks>
        ///     <para>
        ///         This method attempts to retrieve the deserialized event from
        ///         <see cref="ILambdaHostContext.Event" /> and cast it to the requested type
        ///         <typeparamref name="T" />. If the event is not of the expected type, <c>null</c> is
        ///         returned.
        ///     </para>
        /// </remarks>
        /// <typeparam name="T">The expected type of the event object.</typeparam>
        /// <returns>
        ///     The deserialized event cast to type <typeparamref name="T" />, or <c>null</c> if the event
        ///     is not of the expected type or is not set.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <see cref="ILambdaHostContext" /> is
        ///     <c>null</c>.
        /// </exception>
        /// <seealso cref="TryGetEvent{T}(ILambdaHostContext, out T)" />
        public T? GetEvent<T>()
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.Event is T eventT)
                return eventT;

            return default;
        }

        /// <summary>Attempts to get the Lambda event as a strongly-typed object using the TryGet pattern.</summary>
        /// <remarks>
        ///     <para>
        ///         This method attempts to retrieve and cast the event from
        ///         <see cref="ILambdaHostContext.Event" /> to the requested type <typeparamref name="T" />.
        ///         The <paramref name="result" /> parameter will contain the typed event if successful, or the
        ///         default value of <typeparamref name="T" /> if not.
        ///     </para>
        ///     <para>
        ///         This method is useful in scenarios where you want to safely retrieve the event with a
        ///         single call and check the success status with the return value.
        ///     </para>
        /// </remarks>
        /// <typeparam name="T">The expected type of the event object.</typeparam>
        /// <param name="result">
        ///     When this method returns, contains the deserialized event cast to type
        ///     <typeparamref name="T" />, or the default value of <typeparamref name="T" /> if the event is
        ///     not of the expected type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the event was successfully retrieved and cast to type
        ///     <typeparamref name="T" />; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <see cref="ILambdaHostContext" /> is
        ///     <c>null</c>.
        /// </exception>
        /// <seealso cref="GetEvent{T}(ILambdaHostContext)" />
        public bool TryGetEvent<T>([NotNullWhen(true)] out T? result)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.Event is T eventT)
            {
                result = eventT;
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>Gets the Lambda response as a strongly-typed object.</summary>
        /// <remarks>
        ///     <para>
        ///         This method attempts to retrieve the response from
        ///         <see cref="ILambdaHostContext.Response" /> and cast it to the requested type
        ///         <typeparamref name="T" />. If the response is not of the expected type, <c>null</c> is
        ///         returned.
        ///     </para>
        ///     <para>The response is typically set by handler functions during invocation processing.</para>
        /// </remarks>
        /// <typeparam name="T">The expected type of the response object.</typeparam>
        /// <returns>
        ///     The response cast to type <typeparamref name="T" />, or <c>null</c> if the response is not
        ///     of the expected type or is not set.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <see cref="ILambdaHostContext" /> is
        ///     <c>null</c>.
        /// </exception>
        /// <seealso cref="TryGetResponse{T}(ILambdaHostContext, out T)" />
        public T? GetResponse<T>()
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.Response is T responseT)
                return responseT;

            var x = default(T?);

            return x;
        }

        /// <summary>Attempts to get the Lambda response as a strongly-typed object using the TryGet pattern.</summary>
        /// <remarks>
        ///     <para>
        ///         This method attempts to retrieve and cast the response from
        ///         <see cref="ILambdaHostContext.Response" /> to the requested type <typeparamref name="T" />.
        ///         The <paramref name="result" /> parameter will contain the typed response if successful, or
        ///         the default value of <typeparamref name="T" /> if not.
        ///     </para>
        ///     <para>
        ///         This method is useful in middleware or handlers where you need to safely access the
        ///         response with a single call and check the success status with the return value.
        ///     </para>
        /// </remarks>
        /// <typeparam name="T">The expected type of the response object.</typeparam>
        /// <param name="result">
        ///     When this method returns, contains the response cast to type
        ///     <typeparamref name="T" />, or the default value of <typeparamref name="T" /> if the response is
        ///     not of the expected type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the response was successfully retrieved and cast to type
        ///     <typeparamref name="T" />; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <see cref="ILambdaHostContext" /> is
        ///     <c>null</c>.
        /// </exception>
        /// <seealso cref="GetResponse{T}(ILambdaHostContext)" />
        public bool TryGetResponse<T>([NotNullWhen(true)] out T? result)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.Response is T responseT)
            {
                result = responseT;
                return true;
            }

            result = default;
            return false;
        }
    }
}
