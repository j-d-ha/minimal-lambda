using Amazon.Lambda.CloudWatchLogsEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.CloudWatchLogs;

/// <inheritdoc cref="CloudWatchLogsEvent" />
/// <remarks>
///     This abstract class extends <see cref="CloudWatchLogsEvent" /> and provides a foundation
///     for strongly typed CloudWatch Logs handling. Derived classes implement
///     <see cref="ExtractPayload" /> to deserialize the CloudWatch Logs data into strongly typed
///     <see cref="LogEnvelope" /> records using their chosen deserialization strategy.
/// </remarks>
public abstract class CloudWatchLogsEnvelopeBase<T> : CloudWatchLogsEvent, IRequestEnvelope
{
    /// <inheritdoc cref="CloudWatchLogsEvent.Awslogs" />
    public new required LogEnvelope Awslogs { get; set; }

    /// <inheritdoc />
    public abstract void ExtractPayload(EnvelopeOptions options);

    /// <inheritdoc cref="CloudWatchLogsEvent.Log" />
    public class LogEnvelope : Log
    {
        /// <summary>
        ///     Gets and sets the deserialized
        ///     <see cref="Amazon.Lambda.CloudWatchLogsEvents.CloudWatchLogsEvent.LogData.Data" /> content
        ///     after base64 decoding and decompression
        /// </summary>
        public T? DataContent { get; set; }
    }
}
