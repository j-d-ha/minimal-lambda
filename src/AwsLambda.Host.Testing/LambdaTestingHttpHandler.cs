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
        // Create transaction with request and completion mechanism
        var transaction = LambdaHttpTransaction.Create(request);

        // Register cancellation to cancel the transaction TCS
        using var registration = cancellationToken.Register(() => transaction.Cancel());

        // Send transaction to server
        await transactionChannel.Writer.WriteAsync(transaction, cancellationToken);

        // Wait for server to complete the transaction
        return await transaction.ResponseTcs.Task;
    }
}
