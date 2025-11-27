using System.Text.Json;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.Sns;

/// <inheritdoc cref="SnsEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for deserializing SNS message payloads using
///     <see cref="System.Text.Json.JsonSerializer" /> with the configured
///     <see cref="EnvelopeOptions.JsonOptions" />.
/// </remarks>
public sealed class SnsEnvelope<T> : SnsEnvelopeBase<T>
{
    /// <inheritdoc cref="IRequestEnvelope" />
    /// <remarks>This implementation deserializes each message body from JSON.</remarks>
    public override void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
            record.Sns.MessageContent = JsonSerializer.Deserialize<T>(
                record.Sns.Message,
                options.JsonOptions
            );
    }
}
