using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.CloudWatchLogs;

/// <summary>
///     Processes CloudWatch Logs events where log messages are plain strings that do not require
///     deserialization.
/// </summary>
/// <remarks>
///     Use this envelope when working with string-based log messages. For structured log data
///     that needs to be deserialized into typed objects, use <see cref="CloudWatchLogsEnvelope{T}" />
///     instead.
/// </remarks>
public sealed class CloudWatchLogsEnvelope : CloudWatchLogsEnvelopeBase<string>
{
    /// <inheritdoc />
    /// <remarks>
    ///     Sets each log event's
    ///     <see cref="CloudWatchLogsEnvelopeBase{T}.LogEventEnvelope.MessageContent" /> to the raw string
    ///     <see cref="CloudWatchLogsEnvelopeBase{T}.LogEventEnvelope.Message" /> without performing any
    ///     deserialization.
    /// </remarks>
    public override void ExtractPayload(EnvelopeOptions options)
    {
        base.ExtractPayload(options);

        foreach (var logEvent in AwslogsContent!.LogEvents)
            logEvent.MessageContent = logEvent.Message;
    }
}
