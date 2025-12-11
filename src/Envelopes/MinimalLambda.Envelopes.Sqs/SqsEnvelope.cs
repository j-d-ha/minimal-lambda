using System.Text.Json;
using AwsLambda.Host.Envelopes;
using AwsLambda.Host.Options;

namespace MinimalLambda.Envelopes.Sqs;

/// <inheritdoc cref="SqsEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for deserializing SQS message payloads using
///     <see cref="System.Text.Json.JsonSerializer" /> with the configured
///     <see cref="EnvelopeOptions.JsonOptions" />.
/// </remarks>
public sealed class SqsEnvelope<T> : SqsEnvelopeBase<T>
{
    /// <inheritdoc cref="IRequestEnvelope" />
    /// <remarks>This implementation deserializes each message body from JSON.</remarks>
    public override void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
            record.BodyContent = JsonSerializer.Deserialize<T>(record.Body, options.JsonOptions);
    }
}
