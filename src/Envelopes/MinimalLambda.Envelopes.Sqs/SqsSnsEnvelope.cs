using System.Text.Json;
using AwsLambda.Host.Envelopes;
using AwsLambda.Host.Options;
using MinimalLambda.Envelopes.Sns;

namespace MinimalLambda.Envelopes.Sqs;

/// <inheritdoc cref="SqsEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for deserializing SQS messages containing SNS message
///     payloads. This envelope handles the SNS-to-SQS subscription pattern where SNS topics deliver
///     messages to SQS queues.
///     <para>
///         Unlike simpler envelopes that perform single-stage deserialization, this envelope
///         performs a two-stage deserialization process using
///         <see cref="System.Text.Json.JsonSerializer" />:
///     </para>
///     <para>
///         1. The SQS message body is deserialized into an SNS message envelope using
///         <see cref="EnvelopeOptions.LambdaDefaultJsonOptions" />.
///     </para>
///     <para>
///         2. The SNS message content is then deserialized into the final payload type using
///         <see cref="EnvelopeOptions.JsonOptions" />.
///     </para>
/// </remarks>
public sealed class SqsSnsEnvelope<T> : SqsEnvelopeBase<SnsEnvelopeBase<T>.SnsMessageEnvelope>
{
    /// <inheritdoc cref="IRequestEnvelope" />
    /// <remarks>
    ///     This implementation performs two-stage deserialization for each SQS record: the message
    ///     body is first deserialized into an SNS envelope using
    ///     <see cref="EnvelopeOptions.LambdaDefaultJsonOptions" />, then the SNS message content is
    ///     deserialized into type <typeparamref name="T" /> using
    ///     <see cref="EnvelopeOptions.JsonOptions" />.
    /// </remarks>
    public override void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
        {
            record.BodyContent = JsonSerializer.Deserialize<SnsEnvelopeBase<T>.SnsMessageEnvelope>(
                record.Body,
                options.LambdaDefaultJsonOptions
            );

            record.BodyContent!.MessageContent = JsonSerializer.Deserialize<T>(
                record.BodyContent.Message,
                options.JsonOptions
            );
        }
    }
}
