using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinimalLambda.Options;

namespace MinimalLambda.Testing;

public class LambdaTestServer : IAsyncDisposable
{
    /// <summary>
    /// Options used to configure how the server interacts with the Lambda.
    /// </summary>
    private readonly LambdaServerOptions _serverOptions;

    /// <summary>
    /// Task that represents the running Host application that has been captured.
    /// </summary>
    private readonly Task<Exception?> _entryPointCompletion;

    /// <summary>
    /// TCS used to signal the startup has completed
    /// </summary>
    private readonly TaskCompletionSource<InitResponse> _initCompletionTcs = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );

    /// <summary>
    /// JSON serializer options used to serialize/deserilize Lambda events and responses.
    /// </summary>
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Channel used to queue pending invocations in a FIFO manner.
    /// </summary>
    private readonly Channel<string> _pendingInvocationIds = Channel.CreateUnbounded<string>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
    );

    /// <summary>
    /// Dictionary to track all invocations that have been sent to Lambda.
    /// </summary>
    private readonly ConcurrentDictionary<string, PendingInvocation> _pendingInvocations = new();

    /// <summary>
    /// Route manager to determine the route of the incoming request from the Lambda.
    /// </summary>
    private readonly LambdaRuntimeRouteManager _routeManager = new();

    /// <summary>
    /// CTS used to signal shutdown of the server and cancellation of pending tasks.
    /// </summary>
    private readonly CancellationTokenSource _shutdownCts;

    /// <summary>
    /// Channel used to by the Lambda to send events to the server.
    /// </summary>
    private readonly Channel<LambdaHttpTransaction> _transactionChannel =
        Channel.CreateUnbounded<LambdaHttpTransaction>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
        );

    /// <summary>
    /// Host application lifetime used to signal shutdown to the captioned Host.
    /// </summary>
    private IHostApplicationLifetime _applicationLifetime;

    /// <summary>
    /// The captured Host instance.
    /// </summary>
    private IHost? _host;

    /// <summary>
    /// Task that is running the background processing loop to handle incoming requests from Lambda.
    /// </summary>
    private Task? _processingTask;

    /// <summary>
    /// Counter used to generate unique request IDs.
    /// </summary>
    private int _requestCounter;

    /// <summary>
    /// Current state of the server used to enforce lifecycle rules.
    /// </summary>
    private ServerState _state;

    internal LambdaTestServer(
        Task<Exception?>? entryPointCompletion,
        LambdaServerOptions serverOptions,
        CancellationToken shutdownToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entryPointCompletion);

        _entryPointCompletion = entryPointCompletion;
        _shutdownCts = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken);
        _state = ServerState.Created;

        _jsonSerializerOptions = DefaultLambdaJsonSerializerOptions.Create();
        _serverOptions = serverOptions;
    }

    public IServiceProvider Services => _host!.Services;

    public async ValueTask DisposeAsync()
    {
        if (_state == ServerState.Running)
            await StopAsync();

        _transactionChannel.Writer.TryComplete();

        await _shutdownCts.CancelAsync();

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
            throw new InvalidOperationException(
                "TestServer has already been started and cannot be restarted."
            );

        if (_host is null)
            throw new InvalidOperationException("Host is not set.");

        using var cts = LinkedCts(cancellationToken);

        _state = ServerState.Starting;

        _applicationLifetime = _host.Services.GetRequiredService<IHostApplicationLifetime>();

        // Start the host
        await _host.StartAsync(cts.Token);

        // Start background processing
        _processingTask = Task.Run(ProcessTransactionsAsync, cts.Token);

        await TaskHelpers
            .WhenAny(_processingTask, _entryPointCompletion, _initCompletionTcs.Task)
            .UnwrapAndThrow("Exception(s) encountered while running StartAsync");

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
            "TestServer initialization failed with neither an error nor completion."
        );
    }

    public async Task<InvocationResponse<TResponse>> InvokeAsync<TResponse, TEvent>(
        TEvent invokeEvent,
        string? traceId = null,
        CancellationToken cancellationToken = default
    )
    {
        if (_state != ServerState.Running)
            throw new InvalidOperationException(
                "TestServer is not Running and as such an event cannot be invoked."
            );

        using var cts = LinkedCtsWithInvocationDeadline(cancellationToken);

        traceId ??= Guid.NewGuid().ToString();

        // Generate unique request ID
        var requestId = GetRequestId();

        // Create the event response with Lambda headers
        var eventResponse = CreateEventResponse(invokeEvent, requestId, traceId);
        var deadlineUtc = DateTimeOffset.UtcNow.Add(_serverOptions.FunctionTimeout);

        var pending = PendingInvocation.Create(requestId, eventResponse, deadlineUtc);

        _pendingInvocations.AddRequired(requestId, pending);

        if (!_pendingInvocationIds.Writer.TryWrite(requestId))
            throw new InvalidOperationException("Failed to enqueue pending invocation");

        var completion = await pending.ResponseTcs.Task.WaitAsync(cts.Token);

        var responseMessage = completion.Request;
        var wasSuccess = completion.RequestType == RequestType.PostResponse;

        var response = wasSuccess
            ? await (
                responseMessage.Content?.ReadFromJsonAsync<TResponse>(
                    _jsonSerializerOptions,
                    cts.Token
                ) ?? Task.FromResult<TResponse?>(default)
            )
            : default;

        var error = !wasSuccess
            ? await (
                responseMessage.Content?.ReadFromJsonAsync<ErrorResponse>(
                    _jsonSerializerOptions,
                    cts.Token
                ) ?? Task.FromResult<ErrorResponse?>(null)
            )
            : null;

        return new InvocationResponse<TResponse>
        {
            WasSuccess = wasSuccess,
            Response = response,
            Error = error,
        };
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_state != ServerState.Running)
            throw new InvalidOperationException(
                "TestServer is not running and as such cannot be stopped."
            );

        _state = ServerState.Stopping;

        await _shutdownCts.CancelAsync();

        _applicationLifetime.StopApplication();

        await TaskHelpers
            .WhenAll(_entryPointCompletion, _processingTask!)
            .UnwrapAndThrow("Exception(s) encountered while running StopAsync")
            .WaitAsync(cancellationToken);

        _state = ServerState.Stopped;
    }

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                Internal TestServer Logic                 │
    //      └──────────────────────────────────────────────────────────┘

    private async Task ProcessTransactionsAsync()
    {
        try
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
                        $"Unexpected request received from the Lambda HTTP handler: {transaction.Request.Method} {transaction.Request.RequestUri}"
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
        catch (OperationCanceledException) when (_shutdownCts.IsCancellationRequested)
        {
            // Expected when task is canceled
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
            _pendingInvocations.GetRequired(requestId, out var pendingInvocation);
            transaction.ResponseTcs.SetResult(pendingInvocation.EventResponse);
        }
    }

    private async Task HandlePostResponseAsync(
        LambdaHttpTransaction transaction,
        RouteValueDictionary routeValues
    )
    {
        _pendingInvocations.GetRequired(routeValues["requestId"]?.ToString(), out var pending);

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
        _pendingInvocations.GetRequired(routeValues["requestId"]?.ToString(), out var pending);

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
            "TestServer is already started and as such an initialization error cannot be reported."
        );
    }

    private HttpResponseMessage CreateEventResponse<TEvent>(
        TEvent invokeEvent,
        string requestId,
        string traceId
    )
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
        response.Headers.Date = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero);
        response.Headers.TransferEncodingChunked = true;

        // Add custom Lambda runtime headers
        var deadlineMs = DateTimeOffset
            .UtcNow.Add(_serverOptions.FunctionTimeout)
            .ToUnixTimeMilliseconds();
        response.Headers.Add("Lambda-Runtime-Deadline-Ms", deadlineMs.ToString());
        response.Headers.Add("Lambda-Runtime-Aws-Request-Id", requestId);
        response.Headers.Add("Lambda-Runtime-Trace-Id", traceId);
        response.Headers.Add("Lambda-Runtime-Invoked-Function-Arn", _serverOptions.FunctionArn);

        // Add any additional custom headers
        foreach (var header in _serverOptions.AdditionalHeaders)
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

    private CancellationTokenSource LinkedCtsWithInvocationDeadline(
        CancellationToken cancellationTokens
    )
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationTokens,
            _shutdownCts.Token
        );
        cts.CancelAfter(_serverOptions.FunctionTimeout);

        return cts;
    }

    private CancellationTokenSource LinkedCts(CancellationToken cancellationTokens) =>
        CancellationTokenSource.CreateLinkedTokenSource(cancellationTokens, _shutdownCts.Token);
}
