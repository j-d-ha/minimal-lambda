using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Routing;

namespace AwsLambda.Host.Testing;

/// <summary>
/// Internal processor that handles HTTP transactions from Lambda Bootstrap.
/// Routes requests, queues invocations, and manages request-response correlation.
/// </summary>
internal class InvocationProcessor : IAsyncDisposable
{
    private const int TransactionChannelCapacity = 1024;

    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ConcurrentQueue<string> _pendingInvocationIds;
    private readonly ConcurrentDictionary<string, PendingInvocation> _pendingInvocations;
    private readonly ILambdaRuntimeRouteManager _routeManager;
    private readonly CancellationTokenSource _shutdownCts;
    private readonly Channel<LambdaHttpTransaction> _transactionChannel;
    private readonly TaskCompletionSource<InitResponse> _initCompletionTcs;
    private readonly SemaphoreSlim _invocationAddedSignal;
    private ProcessorState _state;
    private Task? _processingTask;

    internal InvocationProcessor(
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
        _routeManager = routeManager ?? new LambdaRuntimeRouteManager();
        _jsonSerializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions();
        _shutdownCts = new CancellationTokenSource();
        _initCompletionTcs = new TaskCompletionSource<InitResponse>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        _invocationAddedSignal = new SemaphoreSlim(0);
        _state = ProcessorState.Created;
    }

    public async ValueTask DisposeAsync()
    {
        // Transition to stopping state
        if (_state != ProcessorState.Stopped)
            _state = ProcessorState.Stopping;

        await _shutdownCts.CancelAsync();

        // Complete init if still pending (server shutting down before init)
        if (_state == ProcessorState.Initializing || _state == ProcessorState.Stopping)
            _initCompletionTcs.TrySetCanceled(_shutdownCts.Token);

        _transactionChannel.Writer.TryComplete();

        // Fail any in-flight transactions that haven't been processed yet
        var disposedException = new OperationCanceledException("InvocationProcessor disposed");
        while (_transactionChannel.Reader.TryRead(out var transaction))
            transaction.Fail(disposedException);

        // Cancel any pending invocations waiting for bootstrap responses
        foreach (var pendingInvocation in _pendingInvocations.Values)
            pendingInvocation.ResponseTcs.TrySetCanceled(_shutdownCts.Token);

        if (_processingTask != null)
            try
            {
                await _processingTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected when task is canceled
            }

        _state = ProcessorState.Stopped;
        _shutdownCts.Dispose();
        _invocationAddedSignal.Dispose();
    }

    /// <summary>
    /// Gets a task that completes when initialization finishes (either success or failure).
    /// Used by LambdaTestServer.StartAsync to wait for initialization to complete.
    /// </summary>
    /// <returns>Task that completes with InitResponse indicating success or failure.</returns>
    internal Task<InitResponse> GetInitCompletionAsync() => _initCompletionTcs.Task;

    /// <summary>
    /// Creates the HTTP handler for Lambda Bootstrap to use.
    /// </summary>
    internal HttpMessageHandler CreateTestingHandler() =>
        new LambdaTestingHttpHandler(_transactionChannel);

    /// <summary>
    /// Starts the background processing loop.
    /// Called by LambdaTestServer after host starts.
    /// </summary>
    internal void StartProcessing()
    {
        if (_processingTask != null)
            throw new InvalidOperationException("Processor already started");

        _state = ProcessorState.Initializing;
        _processingTask = Task.Run(ProcessTransactionsAsync);
    }

    /// <summary>
    /// Queues a new invocation to be processed by Lambda Bootstrap.
    /// Called by LambdaTestServer.InvokeAsync() or LambdaClient.InvokeAsync().
    /// </summary>
    internal async Task<InvocationCompletion> QueueInvocationAsync(
        string requestId,
        HttpResponseMessage eventResponse,
        DateTimeOffset deadlineUtc,
        CancellationToken cancellationToken
    )
    {
        if (_state != ProcessorState.Running)
            throw new InvalidOperationException(
                $"Cannot queue invocation when processor is in {_state} state. Processor must be in Running state."
            );

        var pending = PendingInvocation.Create(requestId, eventResponse, deadlineUtc);

        if (!_pendingInvocations.TryAdd(requestId, pending))
            throw new InvalidOperationException($"Duplicate request ID: {requestId}");

        _pendingInvocationIds.Enqueue(requestId);

        // Signal any waiting /next request that work is available
        _invocationAddedSignal.Release();

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
            throw new InvalidOperationException(
                $"Unexpected request: {transaction.Request.Method} {transaction.Request.RequestUri}"
            );

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

            case RequestType.PostInitError:
                await HandlePostInitErrorAsync(transaction);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unexpected request type {requestType} for {transaction.Request.RequestUri}"
                );
        }
    }

    /// <summary>
    /// Handles GET /invocation/next - Bootstrap polling for work.
    /// </summary>
    private async Task HandleGetNextInvocationAsync(LambdaHttpTransaction transaction)
    {
        // First successful /next call indicates initialization succeeded
        if (_state == ProcessorState.Initializing)
        {
            _state = ProcessorState.Running;
            _initCompletionTcs.TrySetResult(new InitResponse { InitSuccess = true });
        }

        // Loop until we find work or shutdown
        while (true)
        {
            // Check if processor is still in a valid state to serve /next requests
            if (_state != ProcessorState.Running && _state != ProcessorState.Initializing)
            {
                transaction.Fail(
                    new InvalidOperationException(
                        $"Processor is in {_state} state, cannot serve /next request"
                    )
                );
                return;
            }

            // Try to dequeue next pending invocation (FIFO)
            if (TryDequeuePendingInvocation(out var pending))
            {
                RespondToNextRequest(transaction, pending);
                return;
            }

            // No work available - wait for new invocation or shutdown
            try
            {
                await _invocationAddedSignal.WaitAsync(_shutdownCts.Token);
                // Loop back to check state and try dequeue again
            }
            catch (OperationCanceledException)
            {
                // Server shutting down
                transaction.Fail(
                    new OperationCanceledException("Server stopped while waiting for work")
                );
                return;
            }
        }
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
    /// Handles POST /invocation/{requestId}/response - successful function execution.
    /// </summary>
    private async Task HandlePostResponseAsync(
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
            return;
        }

        // Complete the invocation with the response from Bootstrap
        pending.ResponseTcs.SetResult(
            await CreateCompletionAsync(RequestType.PostResponse, transaction.Request)
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
    }

    /// <summary>
    /// Handles POST /invocation/{requestId}/error - function execution failed.
    /// </summary>
    private async Task HandlePostErrorAsync(
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
            return;
        }

        // Complete the invocation with the error response from Bootstrap
        pending.ResponseTcs.SetResult(
            await CreateCompletionAsync(RequestType.PostError, transaction.Request)
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
    }

    /// <summary>
    /// Handles POST /runtime/init/error - Lambda initialization failed.
    /// Captures the error details and marks initialization as failed.
    /// The server will be automatically stopped after this is reported.
    /// </summary>
    private async Task HandlePostInitErrorAsync(LambdaHttpTransaction transaction)
    {
        // Parse error details from request body
        ErrorResponse? errorResponse = null;
        if (transaction.Request.Content != null)
            try
            {
                errorResponse = await transaction.Request.Content.ReadFromJsonAsync<ErrorResponse>(
                    _jsonSerializerOptions
                );
            }
            catch
            {
                // Fallback: create basic error from body text
                var body = await transaction.Request.Content.ReadAsStringAsync();
                errorResponse = new ErrorResponse
                {
                    ErrorType = transaction.Request.Headers.TryGetValues(
                        "Lambda-Runtime-Function-Error-Type",
                        out var values
                    )
                        ? values.FirstOrDefault() ?? "Unknown"
                        : "Unknown",
                    ErrorMessage = body,
                };
            }

        // Mark initialization as failed (only once)
        if (_state == ProcessorState.Initializing)
        {
            _state = ProcessorState.Stopped;
            _initCompletionTcs.TrySetResult(
                new InitResponse { InitSuccess = false, Error = errorResponse }
            );
            // Wake up any waiting /next requests by releasing the semaphore
            // They will check state and fail appropriately
            _invocationAddedSignal.Release();
        }

        // Acknowledge to Bootstrap with 202 Accepted
        transaction.Respond(
            new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(
                        new Dictionary<string, string> { ["status"] = "OK" },
                        _jsonSerializerOptions
                    ),
                    Encoding.UTF8,
                    "application/json"
                ),
                Version = Version.Parse("1.1"),
            }
        );
    }

    private bool TryDequeuePendingInvocation(out PendingInvocation pendingInvocation)
    {
        var now = DateTimeOffset.UtcNow;

        while (_pendingInvocationIds.TryDequeue(out var requestId))
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

        pendingInvocation = null!;
        return false;
    }

    private void CancelPendingInvocation(string requestId, CancellationToken cancellationToken)
    {
        if (_pendingInvocations.TryRemove(requestId, out var pendingInvocation))
            pendingInvocation.ResponseTcs.TrySetCanceled(cancellationToken);
    }

    private static async Task<InvocationCompletion> CreateCompletionAsync(
        RequestType requestType,
        HttpRequestMessage sourceRequest
    )
    {
        var clonedRequest = new HttpRequestMessage(sourceRequest.Method, sourceRequest.RequestUri)
        {
            Version = sourceRequest.Version,
            VersionPolicy = sourceRequest.VersionPolicy,
        };

        foreach (var header in sourceRequest.Headers)
            clonedRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);

        foreach (var option in sourceRequest.Options)
            clonedRequest.Options.TryAdd(option.Key, option.Value);

        if (sourceRequest.Content != null)
        {
            var contentBytes = await sourceRequest.Content.ReadAsByteArrayAsync();
            var clonedContent = new ByteArrayContent(contentBytes);

            foreach (var header in sourceRequest.Content.Headers)
                clonedContent.Headers.TryAddWithoutValidation(header.Key, header.Value);

            clonedRequest.Content = clonedContent;
        }

        return new InvocationCompletion { Request = clonedRequest, RequestType = requestType };
    }

    internal void FailPendingInvocations(Exception exception)
    {
        while (_transactionChannel.Reader.TryRead(out var transaction))
            transaction.Fail(exception);

        foreach (var pendingInvocation in _pendingInvocations.Values)
            pendingInvocation.ResponseTcs.TrySetException(exception);
    }
}
