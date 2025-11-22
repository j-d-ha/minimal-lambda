namespace AwsLambda.Host;

/// <summary>Provides type-safe access to the Lambda invocation response of a specific type.</summary>
/// <typeparam name="T">The type of the response object.</typeparam>
/// <remarks>
///     <para>
///         <see cref="IResponseFeature{T}" /> extends <see cref="IResponseFeature" /> to provide
///         generic, type-safe methods for setting and retrieving responses. Use this interface when
///         you need to work with strongly-typed responses.
///     </para>
/// </remarks>
public interface IResponseFeature<T> : IResponseFeature
{
    /// <summary>Sets the response object for the current Lambda invocation.</summary>
    /// <param name="response">The response object to set.</param>
    void SetResponse(T response);

    /// <summary>Gets the current response object of type <typeparamref name="T" />, if any.</summary>
    /// <returns>
    ///     The response object of type <typeparamref name="T" />, or <c>null</c> if no response has
    ///     been set.
    /// </returns>
    new T? GetResponse();
}
