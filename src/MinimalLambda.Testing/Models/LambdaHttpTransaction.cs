namespace MinimalLambda.Testing;

/// <summary>
/// Represents a single HTTP transaction from the Lambda Bootstrap.
/// Bundles the request with its response completion mechanism for automatic correlation.
/// </summary>
internal class LambdaHttpTransaction
{
    /// <summary>
    /// The HTTP request from Lambda Bootstrap.
    /// </summary>
    internal required HttpRequestMessage Request { get; init; }

    /// <summary>
    /// Task completion source for the HTTP response.
    /// Completing this sends the response back to Bootstrap.
    /// </summary>
    internal required TaskCompletionSource<HttpResponseMessage> ResponseTcs { get; init; }

    /// <summary>
    /// Creates a new transaction with RunContinuationsAsynchronously to prevent deadlocks.
    /// </summary>
    internal static LambdaHttpTransaction Create(HttpRequestMessage request) =>
        new()
        {
            Request = request,
            ResponseTcs = new TaskCompletionSource<HttpResponseMessage>(
                TaskCreationOptions.RunContinuationsAsynchronously
            ),
        };

    /// <summary>
    /// Completes the transaction with a successful HTTP response.
    /// </summary>
    internal bool Respond(HttpResponseMessage response) => ResponseTcs.TrySetResult(response);

    /// <summary>
    /// Completes the transaction with cancellation.
    /// </summary>
    internal bool Cancel() => ResponseTcs.TrySetCanceled();
}
