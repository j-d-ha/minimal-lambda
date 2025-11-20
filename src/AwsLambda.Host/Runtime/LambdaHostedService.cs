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
    private readonly List<Exception> _exceptions = [];
    private readonly ILambdaHandlerFactory _handlerFactory;
    private readonly ILambdaLifecycleOrchestrator _lifecycle;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IOnInitBuilderFactory _onInitBuilderFactory;
    private readonly IOnShutdownBuilderFactory _onShutdownBuilderFactory;
    private readonly LambdaHostedServiceOptions _options;
    private bool _disposed;

    private Task? _executeTask;
    private Func<CancellationToken, Task> _shutdownHandler;
    private CancellationTokenSource? _stoppingCts;

    /// <summary>Initializes a new instance of the <see cref="LambdaHostedService" /> class.</summary>
    /// <param name="bootstrap">The orchestrator responsible for managing the AWS Lambda bootstrap loop.</param>
    /// <param name="handlerFactory">
    ///     The factory responsible for creating and composing the Lambda request
    ///     handler.
    /// </param>
    /// <param name="lifetime">The application lifetime.</param>
    /// <param name="lifecycle">The orchestrator responsible for handling startup and shutdown callbacks.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="bootstrap" /> or
    ///     <paramref name="handlerFactory" /> is null.
    /// </exception>
    public LambdaHostedService(
        ILambdaBootstrapOrchestrator bootstrap,
        ILambdaHandlerFactory handlerFactory,
        IHostApplicationLifetime lifetime,
        ILambdaLifecycleOrchestrator lifecycle,
        IOnInitBuilderFactory onInitBuilderFactory,
        IOptions<LambdaHostedServiceOptions> lambdaHostOptions,
        IOnShutdownBuilderFactory onShutdownBuilderFactory
    )
    {
        ArgumentNullException.ThrowIfNull(bootstrap);
        ArgumentNullException.ThrowIfNull(handlerFactory);
        ArgumentNullException.ThrowIfNull(lifetime);
        ArgumentNullException.ThrowIfNull(lifecycle);
        ArgumentNullException.ThrowIfNull(onInitBuilderFactory);
        ArgumentNullException.ThrowIfNull(lambdaHostOptions);
        ArgumentNullException.ThrowIfNull(onShutdownBuilderFactory);

        _bootstrap = bootstrap;
        _handlerFactory = handlerFactory;
        _lifetime = lifetime;
        _lifecycle = lifecycle;
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

        // Store the task we're executing
        _executeTask = ExecuteAsync(_stoppingCts.Token);

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

        // Wait until the lambda task completes or the stop token triggers
        try
        {
            await _executeTask.WaitAsync(cancellationToken);
        }
        catch (TaskCanceledException)
        {
            // if the task completes due to the cancellation token triggering, then we need to tell
            // the user that shutdown failed
            _exceptions.Add(
                new OperationCanceledException(
                    "Graceful shutdown of the Lambda function failed: the bootstrap operation did "
                        + "not complete within the allocated timeout period."
                )
            );
        }
        catch (Exception ex)
        {
            _exceptions.Add(ex);
        }

        try
        {
            // Handle shutdown tasks and add any exceptions to the list of exceptions
            await _shutdownHandler.Invoke(cancellationToken);
        }
        catch (Exception ex)
        {
            _exceptions.Add(ex);
        }

        if (_exceptions.Count > 0)
            throw new AggregateException(
                $"{nameof(LambdaHostedService)} encountered errors while running",
                _exceptions
            );
    }

    /// <summary>Executes the Lambda hosting environment startup sequence.</summary>
    /// <remarks>
    ///     This method orchestrates the startup of the Lambda service by: 1. Creating a fully
    ///     composed handler with middleware pipeline and request processing 2. Running the AWS Lambda
    ///     bootstrap loop with the composed handler The bootstrap loop continues until the service is
    ///     stopped or an exception occurs.
    /// </remarks>
    /// <param name="stoppingToken">The cancellation token triggered when the service is shutting down.</param>
    /// <returns>A task representing the asynchronous bootstrap operation.</returns>
    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Create a fully composed handler with middleware and request processing.
            var requestHandler = _handlerFactory.CreateHandler(stoppingToken);

            // Create an optional initialization handler.
            var onInitBuilder = _onInitBuilderFactory.CreateBuilder();
            _options.ConfigureOnInitBuilder?.Invoke(onInitBuilder);
            var onInitHandler = onInitBuilder.Build();

            // Create the optional shutdown handler.
            var onShutdownBuilder = _onShutdownBuilderFactory.CreateBuilder();
            _options.ConfigureOnShutdownBuilder?.Invoke(onShutdownBuilder);
            _shutdownHandler = onShutdownBuilder.Build();

            // Handle the bootstrap with the processed handler. Once the task completes, we will
            // manually
            // trigger the stop of the application.
            await _bootstrap.RunAsync(requestHandler, onInitHandler, stoppingToken);
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }
}
