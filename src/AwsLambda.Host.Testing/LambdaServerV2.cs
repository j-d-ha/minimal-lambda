using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace AwsLambda.Host.Testing;

public class LambdaServerV2 : IAsyncDisposable
{
    private readonly TaskCompletionSource<InitResponse> _initCompletionTcs;
    private readonly SemaphoreSlim _invocationAddedSignal;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ConcurrentQueue<string> _pendingInvocationIds;
    private readonly ConcurrentDictionary<string, PendingInvocation> _pendingInvocations;
    private readonly ILambdaRuntimeRouteManager _routeManager;
    private readonly CancellationTokenSource _shutdownCts;
    private readonly Channel<LambdaHttpTransaction> _transactionChannel;
    private readonly IHost _host;

    private Task? _processingTask;
    private ServerState _state;

    internal LambdaServerV2(IHost host, CancellationToken shutdownToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);

        _host = host;

        _transactionChannel = Channel.CreateUnbounded<LambdaHttpTransaction>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
        );
        _pendingInvocationIds = new ConcurrentQueue<string>();
        _pendingInvocations = new ConcurrentDictionary<string, PendingInvocation>();
        _routeManager = new LambdaRuntimeRouteManager();
        _jsonSerializerOptions = new JsonSerializerOptions();
        _shutdownCts = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken);
        _initCompletionTcs = new TaskCompletionSource<InitResponse>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        _invocationAddedSignal = new SemaphoreSlim(0);
        _state = ServerState.Created;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();

        _transactionChannel.Writer.TryComplete();

        _state = ServerState.Disposed;
    }

    public IServiceProvider Services => _host.Services;

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                        Public API                        │
    //      └──────────────────────────────────────────────────────────┘

    public async Task<InitResponse> StartAsync(CancellationToken cancellationToken = default)
    {
        if (_state != ServerState.Created)
            throw new InvalidOperationException("Server is already started.");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _state = ServerState.Starting;

        // Start the host
        await _host.StartAsync(cts.Token);

        // Start background processing
        _processingTask = Task.Run(ProcessTransactionsAsync, cts.Token);

        _state = ServerState.Running;

        return default;
    }

    public async Task<InvocationResponse<TResponse>> InvokeAsync<TResponse, TEvent>(
        TEvent invokeEvent,
        CancellationToken cancellationToken = default
    )
    {
        if (_state != ServerState.Running)
            throw new InvalidOperationException("Server is not started.");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        return default;
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
            try
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
            catch (Exception ex)
            {
                // Fail the transaction and continue processing
                transaction.Fail(ex);
            }
    }

    private async Task HandleGetNextInvocationAsync(LambdaHttpTransaction transaction) { }

    private async Task HandlePostResponseAsync(
        LambdaHttpTransaction transaction,
        RouteValueDictionary routeValues
    ) { }

    private async Task HandlePostErrorAsync(
        LambdaHttpTransaction transaction,
        RouteValueDictionary routeValues
    ) { }

    private async Task HandlePostInitErrorAsync(LambdaHttpTransaction transaction) { }
}

public static class Temp
{
    public static async Task Run()
    {
        await using var server = new LambdaServerV2(null!);
        await server.StartAsync();
        var result = await server.InvokeAsync<string, string>("Jonas", CancellationToken.None);
        await server.StopAsync();
    }
}
