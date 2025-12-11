namespace AwsLambda.Host.Envelopes;

/// <summary>
///     Defines a contract for serializing and packing handler results into Lambda response
///     structures.
/// </summary>
/// <remarks>
///     <para>
///         After a Lambda handler completes, its result must be packaged back into the outer
///         response structure expected by the Lambda runtime. A response envelope handles
///         serialization and placement of the handler result in the appropriate location within the
///         outer response.
///     </para>
/// </remarks>
/// <seealso cref="IRequestEnvelope" />
public interface IResponseEnvelope
{
    /// <summary>Serializes and packs the handler result into the outer Lambda response structure.</summary>
    /// <remarks>
    ///     <para>
    ///         Automatically invoked by the envelope middleware after the handler completes and before
    ///         returning the response to the Lambda runtime. Implementations should serialize the handler
    ///         result and place it in the appropriate location within the outer response structure.
    ///     </para>
    /// </remarks>
    /// <param name="options">Configuration options for payload serialization and packing.</param>
    /// <seealso cref="EnvelopeOptions" />
    void PackPayload(EnvelopeOptions options);
}
