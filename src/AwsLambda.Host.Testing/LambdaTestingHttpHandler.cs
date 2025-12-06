using System.Threading.Channels;

namespace AwsLambda.Host.Testing;

/// <summary>
/// HTTP message handler that intercepts Lambda Bootstrap HTTP calls and
/// routes them through the test server via transactions.
/// </summary>
internal class LambdaTestingHttpHandler(Channel<LambdaHttpTransaction> transactionChannel)
    : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        // Buffer the content to make it re-readable for downstream consumers
        if (request.Content != null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            request.Content = new ByteArrayContent(bytes);
        }

        // Create transaction with request and completion mechanism
        var transaction = LambdaHttpTransaction.Create(request);

        // Register cancellation to cancel the transaction TCS
        using var registration = cancellationToken.Register(() => transaction.Cancel());

        // Send transaction to server
        if (!transactionChannel.Writer.TryWrite(transaction))
        {
            // Server is shutting down; propagate cancellation to caller
            var canceled = new TaskCompletionSource<HttpResponseMessage>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            canceled.TrySetCanceled();
            return await canceled.Task;
        }

        // Wait for server to complete the transaction
        var response = await transaction.ResponseTcs.Task;
        return response;
    }
}
