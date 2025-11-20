using Amazon.Lambda.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

/// <summary>
///     Orchestrates the Lambda hosting environment lifecycle. Delegates specific concerns to
///     specialized components.
/// </summary>
internal sealed class LambdaHostedService : IHostedService, IDisposable
{
    private readonly ILambdaBootstrapOrchestrator _bootstrap;
    private readonly ILambdaHandlerFactory _handlerFactory;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IOnInitBuilderFactory _onInitBuilderFactory;
    private readonly IOnShutdownBuilderFactory _onShutdownBuilderFactory;
    private readonly LambdaHostedServiceOptions _options;
    private bool _disposed;

    private Task? _executeTask;
    private Func<CancellationToken, Task>? _shutdownHandler;
    private CancellationTokenSource? _stoppingCts;

    public LambdaHostedService(
        ILambdaBootstrapOrchestrator bootstrap,
        ILambdaHandlerFactory handlerFactory,
        IHostApplicationLifetime lifetime,
        IOnInitBuilderFactory onInitBuilderFactory,
        IOptions<LambdaHostedServiceOptions> lambdaHostOptions,
        IOnShutdownBuilderFactory onShutdownBuilderFactory
    )
    {
        ArgumentNullException.ThrowIfNull(bootstrap);
        ArgumentNullException.ThrowIfNull(handlerFactory);
        ArgumentNullException.ThrowIfNull(lifetime);
        ArgumentNullException.ThrowIfNull(onInitBuilderFactory);
        ArgumentNullException.ThrowIfNull(lambdaHostOptions);
        ArgumentNullException.ThrowIfNull(onShutdownBuilderFactory);

        _bootstrap = bootstrap;
        _handlerFactory = handlerFactory;
        _lifetime = lifetime;
        _onInitBuilderFactory = onInitBuilderFactory;
        _options = lambdaHostOptions.Value;
        _onShutdownBuilderFactory = onShutdownBuilderFactory;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _stoppingCts?.Cancel();

        if (_disposed)
            return;

        if (_executeTask?.IsCompleted == true)
            _executeTask.Dispose();

        _stoppingCts?.Dispose();

        _disposed = true;
    }

    /// <summary>Triggered when the application host is ready to start the service.</summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous Start operation.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Create a linked token to allow cancelling the executing task from the provided token
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Create a fully composed handler with middleware and request processing.
        var requestHandler = _handlerFactory.CreateHandler(_stoppingCts.Token);

        // Create an optional initialization handler.
        var onInitBuilder = _onInitBuilderFactory.CreateBuilder();
        _options.ConfigureOnInitBuilder?.Invoke(onInitBuilder);
        var onInitHandler = onInitBuilder.Build();

        // Create the optional shutdown handler.
        var onShutdownBuilder = _onShutdownBuilderFactory.CreateBuilder();
        _options.ConfigureOnShutdownBuilder?.Invoke(onShutdownBuilder);
        _shutdownHandler = onShutdownBuilder.Build();

        // Store the task we're executing
        _executeTask = ExecuteAsync(requestHandler, onInitHandler, _stoppingCts.Token);

        // If the task is completed, then return it, this will bubble cancellation and failure to
        // the caller
        return _executeTask.IsCompleted ? _executeTask : Task.CompletedTask;
    }

    /// <summary>Triggered when the application host is performing a graceful shutdown.</summary>
    /// <param name="cancellationToken">
    ///     A timeout token that limits how long the method will wait for the
    ///     bootstrap operation to complete.
    /// </param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous Stop operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Stop called without start
        if (_executeTask == null)
            return;

        // Signal cancellation to the executing method. If disposed or called, this might throw.
        await (_stoppingCts?.CancelAsync() ?? Task.CompletedTask).ConfigureAwait(
            ConfigureAwaitOptions.SuppressThrowing
        );

        List<Exception> exceptions = [];

        // Wait until the lambda task completes or the stop token triggers
        try
        {
            await _executeTask.WaitAsync(cancellationToken);
        }
        catch (TaskCanceledException)
        {
            // if the task completes due to the cancellation token triggering, then we need to tell
            // the user that shutdown failed
            exceptions.Add(
                new OperationCanceledException(
                    "Graceful shutdown of the Lambda function failed: the bootstrap operation did "
                        + "not complete within the allocated timeout period."
                )
            );
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        try
        {
            // Handle shutdown tasks and add any exceptions to the list of exceptions
            var shutdownTask = _shutdownHandler?.Invoke(cancellationToken) ?? Task.CompletedTask;
            await shutdownTask.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        if (exceptions.Count > 0)
            throw new AggregateException(
                $"{nameof(LambdaHostedService)} encountered errors while running",
                exceptions
            );
    }

    /// <summary>Executes the Lambda hosting environment startup sequence.</summary>
    private async Task ExecuteAsync(
        Func<Stream, ILambdaContext, Task<Stream>> handler,
        Func<CancellationToken, Task<bool>> initializer,
        CancellationToken stoppingToken
    )
    {
        try
        {
            // Handle the bootstrap with the processed handler. Once the task completes, we will
            // manually trigger the stop of the application.
            await _bootstrap.RunAsync(handler, initializer, stoppingToken);
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }
}
