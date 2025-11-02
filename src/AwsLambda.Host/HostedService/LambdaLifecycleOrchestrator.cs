using System.Diagnostics;
using Amazon.Lambda.RuntimeSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

/// <summary>
///     Implements the Lambda lifecycle orchestrator, responsible for coordinating the execution
///     of shutdown handlers when the Lambda runtime initiates shutdown.
/// </summary>
internal class LambdaLifecycleOrchestrator : ILambdaLifecycleOrchestrator
{
    private readonly DelegateHolder _delegateHolder;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LambdaHostOptions _settings;

    /// <summary>Initializes a new instance of the <see cref="LambdaLifecycleOrchestrator" /> class.</summary>
    /// <param name="scopeFactory">
    ///     The service scope factory used to create service scopes for shutdown
    ///     handlers.
    /// </param>
    /// <param name="delegateHolder">The delegate holder containing the registered shutdown handlers.</param>
    /// <param name="lambdaHostSettings">The Lambda host options used to configure lifecycle behavior.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="scopeFactory" />,
    ///     <paramref name="delegateHolder" />, or <paramref name="lambdaHostSettings" /> is null.
    /// </exception>
    public LambdaLifecycleOrchestrator(
        IServiceScopeFactory scopeFactory,
        DelegateHolder delegateHolder,
        IOptions<LambdaHostOptions> lambdaHostSettings
    )
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(delegateHolder);
        ArgumentNullException.ThrowIfNull(lambdaHostSettings);

        _scopeFactory = scopeFactory;
        _delegateHolder = delegateHolder;
        _settings = lambdaHostSettings.Value;
    }

    /// <inheritdoc />
    public LambdaBootstrapInitializer OnInit(CancellationToken stoppingToken)
    {
        return Initializer;

        async Task<bool> Initializer()
        {
            if (_delegateHolder.InitHandlers.Count == 0)
                return true;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            cts.CancelAfter(_settings.InitTimeout);

            var tasks = _delegateHolder.InitHandlers.Select(h =>
            {
                // ReSharper disable once AccessToDisposedClosure
                Debug.Assert(cts != null, nameof(cts) + " != null");
                // ReSharper disable once AccessToDisposedClosure
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
    }

    /// <inheritdoc />
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
