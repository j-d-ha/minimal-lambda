using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace AwsLambda.Host.Testing;

public class LambdaServerV2 : IAsyncDisposable
{
    private readonly LambdaClientOptions _clientOptions;
    private readonly Task<Exception?> _entryPointCompletion;
    private readonly TaskCompletionSource<InitResponse> _initCompletionTcs;
    private readonly SemaphoreSlim _invocationAddedSignal;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly Channel<string> _pendingInvocationIds;
    private readonly ConcurrentDictionary<string, PendingInvocation> _pendingInvocations;
    private readonly ILambdaRuntimeRouteManager _routeManager;
    private readonly CancellationTokenSource _shutdownCts;
    private readonly Channel<LambdaHttpTransaction> _transactionChannel;

    private IHost? _host;
    private Task? _processingTask;
    private int _requestCounter;
    private ServerState _state;

    internal LambdaServerV2(
        Task<Exception?>? entryPointCompletion,
        CancellationToken shutdownToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entryPointCompletion);

        _entryPointCompletion = entryPointCompletion;

        _transactionChannel = Channel.CreateUnbounded<LambdaHttpTransaction>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
        );
        _pendingInvocationIds = Channel.CreateUnbounded<string>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
        );
        _pendingInvocations = new ConcurrentDictionary<string, PendingInvocation>();
        _routeManager = new LambdaRuntimeRouteManager();
        _jsonSerializerOptions = new JsonSerializerOptions();
        _shutdownCts = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken);
        _initCompletionTcs = new TaskCompletionSource<InitResponse>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        _invocationAddedSignal = new SemaphoreSlim(0);
        _state = ServerState.Created;
        _clientOptions = new LambdaClientOptions();
    }

    public IServiceProvider Services => _host.Services;

    public async ValueTask DisposeAsync()
    {
        await StopAsync();

        _transactionChannel.Writer.TryComplete();

        _state = ServerState.Disposed;
    }

    internal void SetHost(IHost host)
    {
        ArgumentNullException.ThrowIfNull(host);
        _host = host;
    }

    internal HttpMessageHandler CreateHandler() =>
        new LambdaTestingHttpHandler(_transactionChannel);

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                        Public API                        │
    //      └──────────────────────────────────────────────────────────┘

    public async Task<InitResponse> StartAsync(CancellationToken cancellationToken = default)
    {
        if (_state != ServerState.Created)
            throw new InvalidOperationException("Server is already started.");

        if (_host is null)
            throw new InvalidOperationException("Host is not set.");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _state = ServerState.Starting;

        // Start the host
        await _host.StartAsync(cts.Token);

        // Start background processing
        _processingTask = Task.Run(ProcessTransactionsAsync, cts.Token);

        var exceptions = await WhenAny(
            _processingTask,
            _entryPointCompletion,
            _initCompletionTcs.Task
        );

        if (exceptions.Length > 0)
            throw exceptions.Length > 0
                ? new AggregateException(
                    "Multiple exceptions encountered while running StartAsync",
                    exceptions
                )
                : exceptions[0];

        if (_entryPointCompletion.IsCompleted)
            return new InitResponse { InitStatus = InitStatus.HostExited };

        if (_initCompletionTcs.Task.IsCompleted)
        {
            _state =
                _initCompletionTcs.Task.Result.InitStatus == InitStatus.InitCompleted
                    ? ServerState.Running
                    : ServerState.Stopped;

            return _initCompletionTcs.Task.Result;
        }

        throw new InvalidOperationException(
            "Server initialization failed with neither an error nor completion."
        );
    }

    private static async Task<Exception[]> WhenAny(params Task[] tasks)
    {
        await Task.WhenAny(tasks);
        return ExtractExceptions(tasks);
    }

    private static async Task<Exception[]> WhenAll(params Task[] tasks)
    {
        await Task.WhenAll(tasks);
        return ExtractExceptions(tasks);
    }

    private static Exception[] ExtractExceptions(Task[] tasks) =>
        tasks
            .Where(t => t is { IsFaulted: true, Exception: not null })
            .Select(e =>
                e.Exception!.InnerExceptions.Count > 1
                    ? e.Exception
                    : e.Exception.InnerExceptions[0]
            )
            .ToArray();

    public async Task<InvocationResponse<TResponse>> InvokeAsync<TResponse, TEvent>(
        TEvent invokeEvent,
        CancellationToken cancellationToken = default
    )
    {
        if (_state != ServerState.Running)
            throw new InvalidOperationException("Server is not started.");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Generate unique request ID
        var requestId = GetRequestId();

        // Create the event response with Lambda headers
        var eventResponse = CreateEventResponse(invokeEvent, requestId);
        var deadlineUtc = DateTimeOffset.UtcNow.Add(
            _clientOptions.InvocationHeaderOptions.ClientWaitTimeout
        );

        cts.CancelAfter(_clientOptions.InvocationHeaderOptions.ClientWaitTimeout);

        var pending = PendingInvocation.Create(requestId, eventResponse, deadlineUtc);

        if (!_pendingInvocations.TryAdd(requestId, pending))
            throw new InvalidOperationException($"Duplicate request ID: {requestId}");
        _pendingInvocationIds.Writer.TryWrite(requestId);

        var completion = await pending.ResponseTcs.Task.WaitAsync(cts.Token);

        var responseMessage = completion.Request;
        var wasSuccess = completion.RequestType == RequestType.PostResponse;

        var invocationResponse = new InvocationResponse<TResponse>
        {
            WasSuccess = wasSuccess,
            Response = wasSuccess
                ? await (
                    responseMessage.Content?.ReadFromJsonAsync<TResponse>(
                        _jsonSerializerOptions,
                        cts.Token
                    ) ?? Task.FromResult<TResponse?>(default)
                )
                : default,
            Error = !wasSuccess
                ? await (
                    responseMessage.Content?.ReadFromJsonAsync<ErrorResponse>(
                        _jsonSerializerOptions,
                        cts.Token
                    ) ?? Task.FromResult<ErrorResponse?>(null)
                )
                : null,
        };

        return invocationResponse;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_state != ServerState.Running)
            return;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _state = ServerState.Stopped;
    }

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                  Internal Server Logic                   │
    //      └──────────────────────────────────────────────────────────┘

    private async Task ProcessTransactionsAsync()
    {
        await foreach (
            var transaction in _transactionChannel.Reader.ReadAllAsync(_shutdownCts.Token)
        )
        {
            if (
                !_routeManager.TryMatch(
                    transaction.Request,
                    out var requestType,
                    out var routeValues
                )
            )
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
    }

    private async Task HandleGetNextInvocationAsync(LambdaHttpTransaction transaction)
    {
        if (_state == ServerState.Starting)
            _initCompletionTcs.SetResult(
                new InitResponse { InitStatus = InitStatus.InitCompleted }
            );

        if (await _pendingInvocationIds.Reader.WaitToReadAsync(_shutdownCts.Token))
        {
            var requestId = await _pendingInvocationIds.Reader.ReadAsync(_shutdownCts.Token);
            if (!_pendingInvocations.TryGetValue(requestId, out var pendingInvocation))
                throw new InvalidOperationException($"Missing pending invocation for {requestId}");
            transaction.ResponseTcs.SetResult(pendingInvocation.EventResponse);
        }
    }

    private async Task HandlePostResponseAsync(
        LambdaHttpTransaction transaction,
        RouteValueDictionary routeValues
    )
    {
        var requestId = routeValues["requestId"]?.ToString();
        if (requestId is null || !_pendingInvocations.TryGetValue(requestId, out var pending))
            throw new InvalidOperationException($"Missing pending invocation for {requestId}");

        // Acknowledge to Bootstrap
        transaction.Respond(CreateSuccessResponse());

        pending.ResponseTcs.SetResult(
            await CreateCompletionAsync(RequestType.PostResponse, transaction.Request)
        );
    }

    private async Task HandlePostErrorAsync(
        LambdaHttpTransaction transaction,
        RouteValueDictionary routeValues
    )
    {
        var requestId = routeValues["requestId"]?.ToString();
        if (requestId is null || !_pendingInvocations.TryGetValue(requestId, out var pending))
            throw new InvalidOperationException($"Missing pending invocation for {requestId}");

        // Acknowledge to Bootstrap
        transaction.Respond(CreateSuccessResponse());

        pending.ResponseTcs.SetResult(
            await CreateCompletionAsync(RequestType.PostError, transaction.Request)
        );
    }

    private async Task HandlePostInitErrorAsync(LambdaHttpTransaction transaction)
    {
        if (_state == ServerState.Starting)
            _initCompletionTcs.SetResult(
                new InitResponse
                {
                    Error = await (
                        transaction.Request.Content?.ReadFromJsonAsync<ErrorResponse>(
                            _jsonSerializerOptions
                        ) ?? Task.FromResult<ErrorResponse?>(null)
                    ),
                    InitStatus = InitStatus.InitError,
                }
            );

        throw new InvalidOperationException(
            "Server is already started and as such an initialization error cannot be reported."
        );
    }

    private HttpResponseMessage CreateEventResponse<TEvent>(TEvent invokeEvent, string requestId)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(invokeEvent, _jsonSerializerOptions),
                Encoding.UTF8,
                "application/json"
            ),
            Version = Version.Parse("1.1"),
        };

        // Add standard HTTP headers
        response.Headers.Date = new DateTimeOffset(_clientOptions.InvocationHeaderOptions.Date);
        response.Headers.TransferEncodingChunked = _clientOptions
            .InvocationHeaderOptions
            .TransferEncodingChunked;

        // Add custom Lambda runtime headers
        var deadlineMs = DateTimeOffset
            .UtcNow.Add(_clientOptions.InvocationHeaderOptions.FunctionTimeout)
            .ToUnixTimeMilliseconds();
        response.Headers.Add("Lambda-Runtime-Deadline-Ms", deadlineMs.ToString());
        response.Headers.Add("Lambda-Runtime-Aws-Request-Id", requestId);
        response.Headers.Add(
            "Lambda-Runtime-Trace-Id",
            _clientOptions.InvocationHeaderOptions.TraceId
        );
        response.Headers.Add(
            "Lambda-Runtime-Invoked-Function-Arn",
            _clientOptions.InvocationHeaderOptions.FunctionArn
        );

        // Add any additional custom headers
        foreach (var header in _clientOptions.InvocationHeaderOptions.AdditionalHeaders)
            response.Headers.Add(header.Key, header.Value);

        return response;
    }

    private string GetRequestId() =>
        Interlocked.Increment(ref _requestCounter).ToString().PadLeft(12, '0');

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

    private static HttpResponseMessage CreateSuccessResponse() =>
        new(HttpStatusCode.Accepted)
        {
            Content = new StringContent(
                """
                {"status":"success"}
                """,
                Encoding.UTF8,
                "application/json"
            ),
            Version = Version.Parse("1.1"),
        };
}

// public static class Temp
// {
//     public static async Task Run()
//     {
//         await using var server = new LambdaServerV2();
//         await server.StartAsync();
//         var result = await server.InvokeAsync<string, string>("Jonas", CancellationToken.None);
//         await server.StopAsync();
//     }
// }
