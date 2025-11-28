using System.Text.Json;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.CloudWatchLogs;

/// <inheritdoc cref="CloudWatchLogsEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for deserializing CloudWatch Logs data payloads using
///     <see cref="System.Text.Json.JsonSerializer" /> with the configured
///     <see cref="EnvelopeOptions.JsonOptions" />.
/// </remarks>
public sealed class CloudWatchLogsEnvelope<T> : CloudWatchLogsEnvelopeBase<T>
{
    /// <inheritdoc cref="IRequestEnvelope" />
    /// <remarks>
    ///     This implementation deserializes the base64-decoded and decompressed CloudWatch Logs data
    ///     from JSON.
    /// </remarks>
    public override void ExtractPayload(EnvelopeOptions options) =>
        Awslogs.DataContent = JsonSerializer.Deserialize<T>(
            Awslogs.DecodeData(),
            options.JsonOptions
        );
}
