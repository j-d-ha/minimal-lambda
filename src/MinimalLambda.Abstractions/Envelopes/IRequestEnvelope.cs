namespace MinimalLambda.Envelopes;

/// <summary>Defines a contract for extracting and deserializing incoming Lambda event payloads.</summary>
/// <remarks>
///     <para>
///         AWS Lambda events often contain nested payloads where the actual data is serialized and
///         escaped as a string within the outer JSON structure. A request envelope extracts the inner
///         payload from the outer Lambda event and prepares it for handler processing.
///     </para>
/// </remarks>
/// <seealso cref="IResponseEnvelope" />
public interface IRequestEnvelope
{
    /// <summary>Extracts and deserializes the inner payload from the outer Lambda event structure.</summary>
    /// <remarks>
    ///     <para>
    ///         Automatically invoked by the envelope middleware before passing the event to the handler.
    ///         Implementations should extract the payload from the outer JSON, deserialize it, and prepare
    ///         it for handler processing.
    ///     </para>
    /// </remarks>
    /// <param name="options">Configuration options for payload extraction and deserialization.</param>
    /// <seealso cref="EnvelopeOptions" />
    void ExtractPayload(EnvelopeOptions options);
}
