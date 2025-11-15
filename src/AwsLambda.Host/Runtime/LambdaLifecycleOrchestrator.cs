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
    /// <remarks>
    ///     All initialization handlers are executed concurrently with a timeout specified by
    ///     <see cref="LambdaHostOptions.InitTimeout" />. If any handler throws an exception, execution is
    ///     prevented from continuing. Each handler is provided with its own service scope, which is
    ///     disposed after the handler completes. The boolean values returned by each handler are
    ///     aggregated, and any false value will result in false being returned to the Lambda runtime. If
    ///     no initialization handlers are registered, the initializer returns true immediately.
    /// </remarks>
    public LambdaBootstrapInitializer OnInit(CancellationToken cancellationToken)
    {
        return Initializer;

        async Task<bool> Initializer()
        {
            if (_delegateHolder.InitHandlers.Count == 0)
                return true;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_settings.InitTimeout);

            var tasks = _delegateHolder.InitHandlers.Select(h => RunInitHandler(h, cts.Token));

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            var (errors, shouldContinue) = results.Aggregate(
                (errors: new List<Exception>(), shouldContinue: true),
                (acc, result) =>
                {
                    if (result.Error is not null)
                        acc.errors.Add(result.Error);
                    else if (!result.ShouldContinue)
                        acc.shouldContinue = false;

                    return acc;
                }
            );

            if (errors.Count > 0)
                throw new AggregateException(
                    $"{nameof(LambdaLifecycleOrchestrator)} encountered errors while running OnInit handlers:",
                    errors
                );

            return shouldContinue;
        }
    }

    /// <inheritdoc />
    /// <remarks>
    ///     All shutdown handlers are executed concurrently. Each handler is provided with its own
    ///     service scope, which is disposed after the handler completes. Exceptions thrown by handlers are
    ///     caught and collected rather than propagated, allowing all handlers to run to completion even if
    ///     some fail. If no shutdown handlers are registered, an empty collection is returned immediately.
    /// </remarks>
    public async Task<IEnumerable<Exception>> OnShutdown(CancellationToken cancellationToken)
    {
        if (_delegateHolder.ShutdownHandlers.Count == 0)
            return [];

        var tasks = _delegateHolder.ShutdownHandlers.Select(h =>
            RunShutdownHandler(h, cancellationToken)
        );

        var output = await Task.WhenAll(tasks).ConfigureAwait(false);

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
            await handler(scope.ServiceProvider, cancellationToken).ConfigureAwait(false);
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
            var result = await handler(scope.ServiceProvider, cancellationToken)
                .ConfigureAwait(false);
            return (null, result);
        }
        catch (Exception ex)
        {
            return (ex, false);
        }
    }
}
