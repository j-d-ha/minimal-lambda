using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace AwsLambda.Host.Testing;

/// <summary>
/// Test server that manages the Lambda host lifecycle and invocation processing.
/// Provides explicit StartAsync/StopAsync control and direct invocation capabilities.
/// </summary>
public class LambdaTestServer : IAsyncDisposable
{
    private readonly IHost _host;
    private readonly InvocationProcessor _processor;
    private readonly LambdaClientOptions _defaultClientOptions;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly Task<Exception?> _entryPointCompletion;
    private int _requestCounter;
    private ServerState _state;

    internal LambdaTestServer(
        IHost host,
        InvocationProcessor processor,
        LambdaClientOptions? defaultClientOptions = null,
        JsonSerializerOptions? jsonSerializerOptions = null,
        Task<Exception?>? entryPointCompletion = null
    )
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _defaultClientOptions = defaultClientOptions ?? new LambdaClientOptions();
        _jsonSerializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions();
        _entryPointCompletion = entryPointCompletion ?? Task.FromResult<Exception?>(null);
        _state = ServerState.Created;
    }

    /// <summary>
    /// Gets the current lifecycle state of the server.
    /// </summary>
    public ServerState State => _state;

    /// <summary>
    /// Gets the service provider from the host.
    /// Only available after the server has been started.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if server is not in Running state.</exception>
    public IServiceProvider Services => _host.Services;

    /// <summary>
    /// Starts the Lambda host and waits for initialization to complete.
    /// Returns initialization result indicating success or failure.
    /// If initialization fails, the server is automatically stopped.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>InitResponse indicating whether initialization succeeded or failed.</returns>
    /// <exception cref="InvalidOperationException">Thrown if server is not in Created state.</exception>
    public async Task<InitResponse> StartAsync(CancellationToken cancellationToken = default)
    {
        if (_state != ServerState.Created)
            throw new InvalidOperationException(
                $"Server can only be started from Created state. Current state: {_state}"
            );

        try
        {
            _state = ServerState.Starting;

            // Start the host
            await _host.StartAsync(cancellationToken);

            // Start background processing
            _processor.StartProcessing();

            _state = ServerState.Running;

            // Race initialization completion against entry point failure
            var initTask = _processor.GetInitCompletionAsync();
            var completed = await Task.WhenAny(initTask, _entryPointCompletion)
                .WaitAsync(cancellationToken);

            // Check if entry point failed before init completed
            if (completed == _entryPointCompletion)
            {
                var exception = await _entryPointCompletion;
                if (exception != null)
                    throw new InvalidOperationException(
                        "Entry point failed during initialization",
                        exception
                    );
            }

            // Wait for init to complete
            var initResponse = await initTask;

            // If init failed, auto-stop the server
            if (!initResponse.InitSuccess)
                await StopAsync(CancellationToken.None);

            return initResponse;
        }
        catch
        {
            // Cleanup on failure
            await DisposeAsync();
            throw;
        }
    }

    /// <summary>
    /// Stops the Lambda host.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        // Idempotent - allow multiple calls
        if (_state == ServerState.Stopped || _state == ServerState.Disposed)
            return;

        if (_state != ServerState.Running)
            throw new InvalidOperationException(
                $"Server can only be stopped from Running state. Current state: {_state}"
            );

        try
        {
            _state = ServerState.Stopping;
            await _host.StopAsync(cancellationToken);
            _state = ServerState.Stopped;
        }
        catch (OperationCanceledException)
        {
            // Still mark as stopped even if shutdown was canceled
            _state = ServerState.Stopped;
            throw;
        }
    }

    /// <summary>
    /// Invokes the Lambda function with the given event and waits for the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="invokeEvent">The event to invoke with.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invocation response.</returns>
    /// <exception cref="InvalidOperationException">Thrown if server is not in Running state.</exception>
    public async Task<InvocationResponse<TResponse>> InvokeAsync<TResponse, TEvent>(
        TEvent invokeEvent,
        CancellationToken cancellationToken = default
    ) => await InvokeAsync<TResponse, TEvent>(invokeEvent, null, cancellationToken);

    /// <summary>
    /// Invokes the Lambda function with the given event and waits for the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="invokeEvent">The event to invoke with.</param>
    /// <param name="options">Client options for this invocation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invocation response.</returns>
    /// <exception cref="InvalidOperationException">Thrown if server is not in Running state.</exception>
    public async Task<InvocationResponse<TResponse>> InvokeAsync<TResponse, TEvent>(
        TEvent invokeEvent,
        LambdaClientOptions? options,
        CancellationToken cancellationToken = default
    )
    {
        if (_state != ServerState.Running)
            throw new InvalidOperationException(
                _state == ServerState.Created
                    ? "Server must be started before invoking. Call StartAsync() first."
                    : $"Server is {_state}. Only Running servers can process invocations."
            );

        // Use provided options or fall back to defaults
        var clientOptions = options ?? _defaultClientOptions;

        // Generate unique request ID
        var requestId = GetRequestId();

        // Create the event response with Lambda headers
        var eventResponse = CreateEventResponse(invokeEvent, requestId, clientOptions);
        var deadlineUtc = DateTimeOffset.UtcNow.Add(
            clientOptions.InvocationHeaderOptions.ClientWaitTimeout
        );

        // Queue invocation and wait for Bootstrap to process it
        var waitTimeout = clientOptions.InvocationHeaderOptions.ClientWaitTimeout;

        using var timeoutCts = new CancellationTokenSource(waitTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCts.Token
        );

        var completion = await _processor.QueueInvocationAsync(
            requestId,
            eventResponse,
            deadlineUtc,
            linkedCts.Token
        );

        var responseMessage = completion.Request;
        var wasSuccess = completion.RequestType == RequestType.PostResponse;

        var invocationResponse = new InvocationResponse<TResponse>
        {
            WasSuccess = wasSuccess,
            Response = wasSuccess
                ? await (
                    responseMessage.Content?.ReadFromJsonAsync<TResponse>(
                        _jsonSerializerOptions,
                        cancellationToken
                    ) ?? Task.FromResult<TResponse?>(default)
                )
                : default,
            Error = !wasSuccess
                ? await (
                    responseMessage.Content?.ReadFromJsonAsync<ErrorResponse>(
                        _jsonSerializerOptions,
                        cancellationToken
                    ) ?? Task.FromResult<ErrorResponse?>(null)
                )
                : null,
        };

        return invocationResponse;
    }

    /// <summary>
    /// Disposes the server, stopping the host if needed.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_state == ServerState.Disposed)
            return;

        try
        {
            // Stop if running
            if (_state == ServerState.Running)
                try
                {
                    await StopAsync();
                }
                catch
                {
                    // Best effort - continue with disposal
                }

            // Dispose processor
            await _processor.DisposeAsync();

            // Dispose host
            if (_host is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                _host.Dispose();
        }
        finally
        {
            _state = ServerState.Disposed;
        }
    }

    private HttpResponseMessage CreateEventResponse<TEvent>(
        TEvent invokeEvent,
        string requestId,
        LambdaClientOptions clientOptions
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
        response.Headers.Date = new DateTimeOffset(clientOptions.InvocationHeaderOptions.Date);
        response.Headers.TransferEncodingChunked = clientOptions
            .InvocationHeaderOptions
            .TransferEncodingChunked;

        // Add custom Lambda runtime headers
        var deadlineMs = DateTimeOffset
            .UtcNow.Add(clientOptions.InvocationHeaderOptions.FunctionTimeout)
            .ToUnixTimeMilliseconds();
        response.Headers.Add("Lambda-Runtime-Deadline-Ms", deadlineMs.ToString());
        response.Headers.Add("Lambda-Runtime-Aws-Request-Id", requestId);
        response.Headers.Add(
            "Lambda-Runtime-Trace-Id",
            clientOptions.InvocationHeaderOptions.TraceId
        );
        response.Headers.Add(
            "Lambda-Runtime-Invoked-Function-Arn",
            clientOptions.InvocationHeaderOptions.FunctionArn
        );

        // Add any additional custom headers
        foreach (var header in clientOptions.InvocationHeaderOptions.AdditionalHeaders)
            response.Headers.Add(header.Key, header.Value);

        return response;
    }

    private string GetRequestId() =>
        Interlocked.Increment(ref _requestCounter).ToString().PadLeft(12, '0');
}
