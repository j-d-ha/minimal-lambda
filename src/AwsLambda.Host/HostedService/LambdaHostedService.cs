using Microsoft.Extensions.Hosting;

namespace AwsLambda.Host;

/// <summary>
/// Orchestrates the Lambda hosting environment lifecycle.
/// Delegates specific concerns to specialized components.
/// </summary>
internal sealed class LambdaHostedService : BackgroundService
{
    private readonly ILambdaBootstrapOrchestrator _bootstrap;

    private readonly List<Exception> _exceptions = [];
    private readonly ILambdaHandlerFactory _handlerFactory;

    public LambdaHostedService(
        ILambdaHandlerFactory handlerFactory,
        ILambdaBootstrapOrchestrator bootstrap
    )
    {
        ArgumentNullException.ThrowIfNull(handlerFactory);
        ArgumentNullException.ThrowIfNull(bootstrap);

        _handlerFactory = handlerFactory;
        _bootstrap = bootstrap;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create a fully composed handler with middleware and request processing.
        var requestHandler = _handlerFactory.CreateHandler(stoppingToken);

        // Run the bootstrap with the processed handler.
        return _bootstrap.RunAsync(requestHandler, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // await the background service stop and capture any exceptions that occur.
        try
        {
            await base.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _exceptions.Add(ex);
        }

        // if any exceptions were captured, rethrow them.
        if (_exceptions.Count > 0)
            throw new AggregateException(_exceptions);
    }
}
