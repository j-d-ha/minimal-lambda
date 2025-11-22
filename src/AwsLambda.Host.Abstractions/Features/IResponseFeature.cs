namespace AwsLambda.Host;

/// <summary>Provides access to the Lambda invocation response.</summary>
/// <remarks>
///     <para>
///         <see cref="IResponseFeature" /> manages the response object for the current Lambda
///         invocation, allowing it to be set, retrieved, and serialized to the response stream. This
///         feature is typically registered as a feature provider in <see cref="IFeatureCollection" />.
///     </para>
/// </remarks>
public interface IResponseFeature
{
    /// <summary>Gets the current response object, if any.</summary>
    /// <returns>The response object, or <c>null</c> if no response has been set.</returns>
    object? GetResponse();

    /// <summary>Serializes the response object to the Lambda response stream.</summary>
    /// <param name="context">The <see cref="ILambdaHostContext" /> for the current invocation.</param>
    void SerializeToStream(ILambdaHostContext context);
}
