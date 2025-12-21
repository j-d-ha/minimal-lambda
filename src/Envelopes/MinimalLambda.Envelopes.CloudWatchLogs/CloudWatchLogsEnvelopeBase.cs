using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.CloudWatchLogsEvents;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.CloudWatchLogs;

/// <inheritdoc cref="CloudWatchLogsEvent" />
/// <remarks>
///     This abstract class extends <see cref="CloudWatchLogsEvent" /> and provides a foundation
///     for strongly typed CloudWatch Logs handling. Derived classes implement
///     <see cref="ExtractPayload" /> to deserialize the CloudWatch Logs data into strongly typed
///     <see cref="AwsLogsEnvelope" /> records using their chosen deserialization strategy.
/// </remarks>
public abstract class CloudWatchLogsEnvelopeBase<T> : CloudWatchLogsEvent, IRequestEnvelope
{
    /// <inheritdoc cref="CloudWatchLogsEvent.Awslogs" />
    [JsonIgnore]
    public AwsLogsEnvelope? AwslogsContent { get; set; }

    /// <inheritdoc />
    [UnconditionalSuppressMessage(
        "Aot",
        "IL3050:RequiresDynamicCode",
        Justification = "Safe when EnvelopeOptions.JsonOptions includes source-generated context for T"
    )]
    [UnconditionalSuppressMessage(
        "Aot",
        "IL2026:RequiresUnreferencedCode",
        Justification = "Safe when EnvelopeOptions.JsonOptions includes source-generated context for T"
    )]
    public virtual void ExtractPayload(EnvelopeOptions options)
    {
        var decodedData = Awslogs.DecodeData();
        AwslogsContent =
            JsonSerializer.Deserialize<AwsLogsEnvelope>(
                decodedData,
                options.LambdaDefaultJsonOptions
            ) ?? throw new InvalidOperationException("Invalid CloudWatch Logs data.");
    }

    /// <summary>
    ///     Represents the decoded CloudWatch Logs data structure after base64 decoding and
    ///     decompression.
    /// </summary>
    public class AwsLogsEnvelope
    {
        /// <summary>Gets or sets the array of log events included in the message.</summary>
        [JsonPropertyName("logEvents")]
        public required LogEventEnvelope[] LogEvents { get; set; }

        /// <summary>Gets or sets the name of the log group that contains the log stream.</summary>
        [JsonPropertyName("logGroup")]
        public required string LogGroup { get; set; }

        /// <summary>Gets or sets the name of the log stream that stores the log events.</summary>
        [JsonPropertyName("logStream")]
        public required string LogStream { get; set; }

        /// <summary>Gets or sets the type of CloudWatch Logs message.</summary>
        [JsonPropertyName("messageType")]
        public required string MessageType { get; set; }

        /// <summary>Gets or sets the AWS account ID of the originating log data.</summary>
        [JsonPropertyName("owner")]
        public required string Owner { get; set; }

        /// <summary>
        ///     Gets or sets the optional field specifying the policy level applied to the subscription
        ///     filter, if present.
        /// </summary>
        [JsonPropertyName("policyLevel")]
        public string? PolicyLevel { get; set; }

        /// <summary>Gets or sets the list of subscription filter names associated with the log group.</summary>
        [JsonPropertyName("subscriptionFilters")]
        public required string[] SubscriptionFilters { get; set; }
    }

    /// <summary>Represents a single log event within a CloudWatch Logs message.</summary>
    public class LogEventEnvelope
    {
        /// <summary>Gets or sets the unique identifier for the log event within the batch.</summary>
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>Gets or sets the actual log message string emitted by the service or application.</summary>
        [JsonPropertyName("message")]
        public required string Message { get; set; }

        /// <summary>Gets and sets the deserialized <see cref="Message" /> content</summary>
        [JsonIgnore]
        public T? MessageContent { get; set; }

        /// <summary>
        ///     Gets or sets the time when the event occurred in milliseconds since Jan 1, 1970 00:00:00
        ///     UTC.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public required DateTime Timestamp { get; set; }
    }
}
