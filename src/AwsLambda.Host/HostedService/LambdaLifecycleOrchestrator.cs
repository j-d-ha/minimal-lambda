using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace AwsLambda.Host;

/// <summary>
/// Implements the Lambda lifecycle orchestrator, responsible for coordinating the execution of shutdown handlers
/// when the Lambda runtime initiates shutdown.
/// </summary>
internal class LambdaLifecycleOrchestrator : ILambdaLifecycleOrchestrator
{
    private readonly DelegateHolder _delegateHolder;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaLifecycleOrchestrator"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory used to create service scopes for shutdown handlers.</param>
    /// <param name="delegateHolder">The delegate holder containing the registered shutdown handlers.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="scopeFactory"/> or <paramref name="delegateHolder"/> is null.</exception>
    public LambdaLifecycleOrchestrator(
        IServiceScopeFactory scopeFactory,
        DelegateHolder delegateHolder
    )
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(delegateHolder);

        _scopeFactory = scopeFactory;
        _delegateHolder = delegateHolder;
    }

    public async Task<bool> OnInit()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(20));

        var tasks = _delegateHolder.InitHandlers.Select(h =>
        {
            Debug.Assert(cts != null, nameof(cts) + " != null");
            return RunInitHandler(h, cts.Token);
        });

        var results = await Task.WhenAll(tasks);

        var (errors, shouldContinue) = results.Aggregate(
            (errors: new List<Exception>(), shouldContinue: true),
            (acc, result) =>
            {
                if (result.Error is not null)
                {
                    acc.errors.Add(result.Error);
                    acc.shouldContinue = false;
                }

                return acc;
            }
        );

        if (errors.Count > 0)
            throw new AggregateException(
                $"{nameof(LambdaHostedService)} encountered errors while running OnInit handlers:",
                errors
            );

        return shouldContinue;
    }

    /// <summary>
    /// Executes the shutdown lifecycle, running all registered shutdown handlers concurrently
    /// and collecting any exceptions that occur.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to signal shutdown handlers to stop processing.</param>
    /// <returns>A task representing the asynchronous shutdown operation that returns any exceptions thrown by shutdown handlers.</returns>
    public async Task<IEnumerable<Exception>> OnShutdown(CancellationToken cancellationToken)
    {
        var tasks = _delegateHolder.ShutdownHandlers.Select(h =>
            RunShutdownHandler(h, cancellationToken)
        );

        var output = await Task.WhenAll(tasks);

        return output.Where(x => x is not null).Select(x => x!);
    }

    private async Task<Exception?> RunShutdownHandler(
        LambdaShutdownDelegate handler,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            await handler(scope.ServiceProvider, cancellationToken);
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    private async Task<(Exception? Error, bool ShouldContinue)> RunInitHandler(
        LambdaInitDelegate handler,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var result = await handler(scope.ServiceProvider, cancellationToken);
            return (null, result);
        }
        catch (Exception ex)
        {
            return (ex, false);
        }
    }
}
