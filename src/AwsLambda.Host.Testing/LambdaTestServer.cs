using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Routing;

namespace AwsLambda.Host.Testing;

/// <summary>
/// Test server that processes HTTP transactions from Lambda Bootstrap.
/// Routes requests, queues invocations, and manages request-response correlation.
/// </summary>
internal class LambdaTestServer : IAsyncDisposable
{
    private const int TransactionChannelCapacity = 1024;
    private const int NextRequestChannelCapacity = 1024;
    private static readonly OperationCanceledException DisposedException = new(
        "LambdaTestServer disposed"
    );

    private readonly LambdaClient _client;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ConcurrentQueue<string> _pendingInvocationIds;
    private readonly ConcurrentDictionary<string, PendingInvocation> _pendingInvocations;
    private readonly Channel<LambdaHttpTransaction> _queuedNextRequests;
    private readonly ILambdaRuntimeRouteManager _routeManager;
    private readonly CancellationTokenSource _shutdownCts;
    private readonly Channel<LambdaHttpTransaction> _transactionChannel;
    private Task? _processingTask;

    internal LambdaTestServer(
        JsonSerializerOptions? jsonSerializerOptions = null,
        ILambdaRuntimeRouteManager? routeManager = null
    )
    {
        _transactionChannel = Channel.CreateBounded<LambdaHttpTransaction>(
            new BoundedChannelOptions(TransactionChannelCapacity)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait,
            }
        );
        _pendingInvocationIds = new ConcurrentQueue<string>();
        _pendingInvocations = new ConcurrentDictionary<string, PendingInvocation>();
        _queuedNextRequests = Channel.CreateBounded<LambdaHttpTransaction>(
            new BoundedChannelOptions(NextRequestChannelCapacity)
            {
                SingleReader = false,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait,
            }
        );
        _routeManager = routeManager ?? new LambdaRuntimeRouteManager();
        _jsonSerializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions();
        _shutdownCts = new CancellationTokenSource();

        // Create client that communicates with this server
        _client = new LambdaClient(this, _jsonSerializerOptions);
    }

    public async ValueTask DisposeAsync()
    {
        await _shutdownCts.CancelAsync();

        _transactionChannel.Writer.TryComplete();
        _queuedNextRequests.Writer.TryComplete();

        // Cancel any transactions waiting for work
        while (_queuedNextRequests.Reader.TryRead(out var queuedTransaction))
            queuedTransaction.Fail(DisposedException);

        // Fail any in-flight transactions that haven't been processed yet
        while (_transactionChannel.Reader.TryRead(out var transaction))
            transaction.Fail(DisposedException);

        // Cancel any pending invocations waiting for bootstrap responses
        foreach (var pendingInvocation in _pendingInvocations.Values)
            pendingInvocation.ResponseTcs.TrySetCanceled(_shutdownCts.Token);

        if (_processingTask != null)
        {
            try
            {
                await _processingTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected when task is canceled
            }
        }

        _shutdownCts.Dispose();
    }

    /// <summary>
    /// Creates the HTTP handler for Lambda Bootstrap to use.
    /// </summary>
    internal HttpMessageHandler CreateTestingHandler() =>
        new LambdaTestingHttpHandler(_transactionChannel);

    /// <summary>
    /// Gets the client for test code to invoke Lambda functions.
    /// </summary>
    internal LambdaClient CreateLambdaClient() => _client;

    /// <summary>
    /// Starts the background processing loop.
    /// Called automatically by LambdaApplicationFactory after host starts.
    /// </summary>
    internal void Start()
    {
        if (_processingTask != null)
            throw new InvalidOperationException("Server already started");

        _processingTask = Task.Run(ProcessTransactionsAsync);
    }

    /// <summary>
    /// Queues a new invocation to be processed by Lambda Bootstrap.
    /// Called by LambdaClient.InvokeAsync().
    /// </summary>
    internal async Task<InvocationCompletion> QueueInvocationAsync(
        string requestId,
        HttpResponseMessage eventResponse,
        DateTimeOffset deadlineUtc,
        CancellationToken cancellationToken
    )
    {
        var pending = PendingInvocation.Create(requestId, eventResponse, deadlineUtc);

        if (!_pendingInvocations.TryAdd(requestId, pending))
            throw new InvalidOperationException($"Duplicate request ID: {requestId}");

        _pendingInvocationIds.Enqueue(requestId);

        // If there's a queued /next request, serve it immediately
        if (_queuedNextRequests.Reader.TryRead(out var nextTransaction))
            RespondToNextRequest(nextTransaction);

        using var cancellationRegistration = cancellationToken.Register(() =>
            CancelPendingInvocation(requestId, cancellationToken)
        );

        // Wait for Bootstrap to process and respond
        return await pending.ResponseTcs.Task.WaitAsync(cancellationToken);
    }

    /// <summary>
    /// Background loop that processes transactions from the handler.
    /// </summary>
    private async Task ProcessTransactionsAsync()
    {
        await foreach (
            var transaction in _transactionChannel.Reader.ReadAllAsync(_shutdownCts.Token)
        )
            try
            {
                await ProcessTransactionAsync(transaction);
            }
            catch (Exception ex)
            {
                // Fail the transaction and continue processing
                transaction.Fail(ex);
            }
    }

    /// <summary>
    /// Routes a single transaction based on request type.
    /// </summary>
    private async Task ProcessTransactionAsync(LambdaHttpTransaction transaction)
    {
        if (!_routeManager.TryMatch(transaction.Request, out var requestType, out var routeValues))
        {
            transaction.Respond(
                new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("Route not found"),
                }
            );
            return;
        }

        switch (requestType!.Value)
        {
            case RequestType.GetNextInvocation:
                await HandleGetNextInvocationAsync(transaction);
                break;

            case RequestType.PostResponse:
                await HandlePostResponseAsync(transaction, routeValues!);
                break;

            case RequestType.PostError:
                await HandlePostErrorAsync(transaction, routeValues!);
                break;

            default:
                transaction.Respond(
                    new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent($"Unknown request type: {requestType}"),
                    }
                );
                break;
        }
    }

    /// <summary>
    /// Handles GET /invocation/next - Bootstrap polling for work.
    /// </summary>
    private async Task HandleGetNextInvocationAsync(LambdaHttpTransaction transaction)
    {
        // Try to dequeue next pending invocation (FIFO)
        if (!TryDequeuePendingInvocation(out var pending))
        {
            // No work available - queue this /next request
            try
            {
                await _queuedNextRequests.Writer.WriteAsync(transaction, _shutdownCts.Token);
            }
            catch (OperationCanceledException)
            {
                transaction.Fail(DisposedException);
            }

            return;
        }

        RespondToNextRequest(transaction, pending);
    }

    /// <summary>
    /// Responds to a /next request with a pending invocation.
    /// </summary>
    private void RespondToNextRequest(LambdaHttpTransaction transaction, PendingInvocation pending)
    {
        // Respond with the event payload and Lambda headers
        if (transaction.Respond(pending.EventResponse))
            return;

        // Request was already canceled; re-enqueue invocation to avoid dropping it
        _pendingInvocationIds.Enqueue(pending.RequestId);
    }

    /// <summary>
    /// Responds to a /next request with a pending invocation by looking up the next one.
    /// </summary>
    private void RespondToNextRequest(LambdaHttpTransaction transaction)
    {
        // Try to dequeue next pending invocation (FIFO)
        if (!TryDequeuePendingInvocation(out var pending))
        {
            // This shouldn't happen, but if it does, respond with error
            transaction.Respond(
                new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("No pending invocations available"),
                }
            );
            return;
        }

        RespondToNextRequest(transaction, pending);
    }

    /// <summary>
    /// Handles POST /invocation/{requestId}/response - successful function execution.
    /// </summary>
    private Task HandlePostResponseAsync(
        LambdaHttpTransaction transaction,
        RouteValueDictionary routeValues
    )
    {
        var requestId = routeValues["requestId"]?.ToString();

        if (
            string.IsNullOrEmpty(requestId)
            || !_pendingInvocations.TryRemove(requestId, out var pending)
        )
        {
            transaction.Respond(
                new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(
                        string.IsNullOrEmpty(requestId)
                            ? "Missing requestId"
                            : $"No pending invocation for request ID: {requestId}"
                    ),
                }
            );
            return Task.CompletedTask;
        }

        // Complete the invocation with the response from Bootstrap
        pending.ResponseTcs.SetResult(
            new InvocationCompletion
            {
                Request = transaction.Request,
                RequestType = RequestType.PostResponse,
            }
        );

        // Acknowledge to Bootstrap
        transaction.Respond(
            new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(
                        new Dictionary<string, string> { ["status"] = "success" },
                        _jsonSerializerOptions
                    ),
                    Encoding.UTF8,
                    "application/json"
                ),
                Version = Version.Parse("1.1"),
            }
        );

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles POST /invocation/{requestId}/error - function execution failed.
    /// </summary>
    private Task HandlePostErrorAsync(
        LambdaHttpTransaction transaction,
        RouteValueDictionary routeValues
    )
    {
        var requestId = routeValues["requestId"]?.ToString();

        if (
            string.IsNullOrEmpty(requestId)
            || !_pendingInvocations.TryRemove(requestId, out var pending)
        )
        {
            transaction.Respond(
                new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(
                        string.IsNullOrEmpty(requestId)
                            ? "Missing requestId"
                            : $"No pending invocation for request ID: {requestId}"
                    ),
                }
            );
            return Task.CompletedTask;
        }

        // Complete the invocation with the error response from Bootstrap
        pending.ResponseTcs.SetResult(
            new InvocationCompletion
            {
                Request = transaction.Request,
                RequestType = RequestType.PostError,
            }
        );

        // Acknowledge to Bootstrap
        transaction.Respond(
            new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(
                    "{\"status\":\"success\"}",
                    Encoding.UTF8,
                    "application/json"
                ),
                Version = Version.Parse("1.1"),
            }
        );

        return Task.CompletedTask;
    }

    private bool TryDequeuePendingInvocation(out PendingInvocation? pendingInvocation)
    {
        var now = DateTimeOffset.UtcNow;

        while (_pendingInvocationIds.TryDequeue(out var requestId))
        {
            if (_pendingInvocations.TryGetValue(requestId, out pendingInvocation))
            {
                if (pendingInvocation.DeadlineUtc <= now)
                {
                    if (_pendingInvocations.TryRemove(requestId, out var expiredInvocation))
                        expiredInvocation.ResponseTcs.TrySetCanceled();

                    continue;
                }

                return true;
            }
        }

        pendingInvocation = null;
        return false;
    }

    private void CancelPendingInvocation(string requestId, CancellationToken cancellationToken)
    {
        if (_pendingInvocations.TryRemove(requestId, out var pendingInvocation))
            pendingInvocation.ResponseTcs.TrySetCanceled(cancellationToken);
    }
}
