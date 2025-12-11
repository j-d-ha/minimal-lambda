using System.Text.Json;
using AwsLambda.Host.Envelopes;
using AwsLambda.Host.Options;

namespace MinimalLambda.Envelopes.CloudWatchLogs;

/// <summary>
///     Processes CloudWatch Logs events and deserializes each log message into a strongly typed
///     object of type <typeparamref name="T" />.
/// </summary>
/// <typeparam name="T">The type to deserialize each log message into.</typeparam>
/// <remarks>
///     <para>
///         Use this envelope when log messages contain structured data (e.g., JSON) that should be
///         deserialized. Deserialization uses <see cref="System.Text.Json.JsonSerializer" /> with the
///         <see cref="EnvelopeOptions.JsonOptions" />.
///     </para>
///     <para>
///         For plain string log messages that do not need deserialization, use
///         <see cref="CloudWatchLogsEnvelope" /> instead.
///     </para>
/// </remarks>
public sealed class CloudWatchLogsEnvelope<T> : CloudWatchLogsEnvelopeBase<T>
{
    /// <inheritdoc cref="IRequestEnvelope" />
    /// <remarks>
    ///     This implementation deserializes the base64-decoded and decompressed CloudWatch Logs data
    ///     from JSON.
    /// </remarks>
    public override void ExtractPayload(EnvelopeOptions options)
    {
        base.ExtractPayload(options);

        foreach (var logEvent in AwslogsContent!.LogEvents)
            logEvent.MessageContent = JsonSerializer.Deserialize<T>(
                logEvent.Message,
                options.JsonOptions
            );
    }
}
