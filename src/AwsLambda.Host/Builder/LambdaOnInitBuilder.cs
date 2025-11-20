using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

internal class LambdaOnInitBuilder : ILambdaOnInitBuilder
{
    private readonly IList<LambdaInitDelegate> _handlers = [];
    private readonly LambdaHostOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public LambdaOnInitBuilder(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<LambdaHostOptions> options
    )
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(options);

        Services = serviceProvider;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    public IServiceProvider Services { get; }

    public IReadOnlyList<LambdaInitDelegate> InitHandlers => _handlers.AsReadOnly();

    public ILambdaOnInitBuilder OnInit(LambdaInitDelegate handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _handlers.Add(handler);
        return this;
    }

    public LambdaInitDelegate Build()
    {
        if (_handlers.Count == 0)
            return (_, _) => Task.FromResult(true);

        return async Task<bool> (_, stoppingToken) =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            cts.CancelAfter(_options.InitTimeout);

            var tasks = _handlers.Select(h => RunInitHandler(h, cts.Token));

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
                    "Encountered errors while running OnInit handlers:",
                    errors
                );

            return shouldContinue;
        };
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
