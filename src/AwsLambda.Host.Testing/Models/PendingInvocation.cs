namespace AwsLambda.Host.Testing;

/// <summary>
/// Represents a pending Lambda invocation waiting for Bootstrap to process and respond.
/// </summary>
internal class PendingInvocation
{
    /// <summary>
    /// Timestamp when this invocation was created (for timeout tracking).
    /// </summary>
    internal DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The HTTP response containing the serialized event payload and Lambda headers
    /// to send to Bootstrap when it polls for the next invocation.
    /// </summary>
    internal required HttpResponseMessage EventResponse { get; init; }

    /// <summary>
    /// Unique request ID for this invocation (will be in Lambda-Runtime-Aws-Request-Id header).
    /// </summary>
    internal required string RequestId { get; init; }

    /// <summary>
    /// Task completion source for the invocation result.
    /// Completed when Bootstrap posts response or error with the HTTP request containing the result payload.
    /// </summary>
    internal required TaskCompletionSource<HttpRequestMessage> ResponseTcs { get; init; }

    /// <summary>
    /// Creates a pending invocation with proper TCS configuration.
    /// </summary>
    internal static PendingInvocation Create(string requestId, HttpResponseMessage eventResponse) =>
        new()
        {
            RequestId = requestId,
            EventResponse = eventResponse,
            ResponseTcs = new TaskCompletionSource<HttpRequestMessage>(
                TaskCreationOptions.RunContinuationsAsynchronously
            ),
        };
}
