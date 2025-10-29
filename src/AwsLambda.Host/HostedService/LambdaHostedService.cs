using Microsoft.Extensions.Hosting;

namespace AwsLambda.Host;

/// <summary>
///     Orchestrates the Lambda hosting environment lifecycle.
///     Delegates specific concerns to specialized components.
/// </summary>
internal sealed class LambdaHostedService : IHostedService, IDisposable
{
    private readonly ILambdaBootstrapOrchestrator _bootstrap;
    private readonly List<Exception> _exceptions = [];
    private readonly ILambdaHandlerFactory _handlerFactory;
    private readonly IHostApplicationLifetime _lifetime;

    private Task? _executeTask;
    private CancellationTokenSource? _stoppingCts;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LambdaHostedService" /> class.
    /// </summary>
    /// <param name="bootstrap">The orchestrator responsible for managing the AWS Lambda bootstrap loop.</param>
    /// <param name="handlerFactory">The factory responsible for creating and composing the Lambda request
    ///     handler.</param>
    /// <param name="lifetime">The application lifetime.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bootstrap" /> or
    ///     <paramref name="handlerFactory" /> is null.</exception>
    public LambdaHostedService(
        ILambdaBootstrapOrchestrator bootstrap,
        ILambdaHandlerFactory handlerFactory,
        IHostApplicationLifetime lifetime
    )
    {
        ArgumentNullException.ThrowIfNull(bootstrap);
        ArgumentNullException.ThrowIfNull(handlerFactory);
        ArgumentNullException.ThrowIfNull(lifetime);

        _bootstrap = bootstrap;
        _handlerFactory = handlerFactory;
        _lifetime = lifetime;
    }

    /// <inheritdoc />
    public void Dispose() => _stoppingCts?.Cancel();

    /// <summary>
    ///     Triggered when the application host is ready to start the service.
    /// </summary>
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

    /// <summary>
    ///     Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">A timeout token that limits how long the method will wait for the
    ///     bootstrap operation to complete.</param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous Stop operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Stop called without start
        if (_executeTask == null)
            return;

        // Signal cancellation to the executing method. If disposed or called, this might throw.
        try
        {
            // ReSharper disable once MethodHasAsyncOverload
            _stoppingCts?.Cancel();
        }
        catch
        {
            // ignored
        }

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

        if (_exceptions.Count > 0)
            throw new AggregateException(
                $"{nameof(LambdaHostedService)} encountered errors while running",
                _exceptions
            );
    }

    /// <summary>
    ///     Executes the Lambda hosting environment startup sequence.
    /// </summary>
    /// <remarks>
    ///     This method orchestrates the startup of the Lambda service by:
    ///     1. Creating a fully composed handler with middleware pipeline and request processing
    ///     2. Running the AWS Lambda bootstrap loop with the composed handler
    ///     The bootstrap loop continues until the service is stopped or an exception occurs.
    /// </remarks>
    /// <param name="stoppingToken">The cancellation token triggered when the service is shutting down.</param>
    /// <returns>A task representing the asynchronous bootstrap operation.</returns>
    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create a fully composed handler with middleware and request processing.
        // throw new Exception("ExecuteAsync error");
        var requestHandler = _handlerFactory.CreateHandler(stoppingToken);

        // Run the bootstrap with the processed handler. Once the task completes, we will manually
        // trigger the stop of the application.
        try
        {
            await _bootstrap.RunAsync(requestHandler, stoppingToken);
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }
}
