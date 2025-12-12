using System.Threading.Channels;

namespace MinimalLambda.Testing;

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
            var originalContent = request.Content;
            var bytes = await originalContent.ReadAsByteArrayAsync(cancellationToken);
            var bufferedContent = new ByteArrayContent(bytes);

            foreach (var header in originalContent.Headers)
                bufferedContent.Headers.TryAddWithoutValidation(header.Key, header.Value);

            request.Content = bufferedContent;
            originalContent.Dispose();
        }

        // Create transaction with request and completion mechanism
        var transaction = LambdaHttpTransaction.Create(request);

        // Register cancellation to cancel the transaction TCS
        using var registration = cancellationToken.Register(() => transaction.Cancel());

        // Send transaction to server
        try
        {
            await transactionChannel.Writer.WriteAsync(transaction, cancellationToken);
        }
        catch (ChannelClosedException)
        {
            // TestServer is shutting down; propagate cancellation to caller
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
