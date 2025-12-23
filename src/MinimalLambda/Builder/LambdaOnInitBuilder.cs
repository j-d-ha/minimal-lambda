using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MinimalLambda.Builder;

internal class LambdaOnInitBuilder : ILambdaOnInitBuilder
{
    private readonly IList<LambdaInitDelegate> _handlers = [];
    private readonly LambdaHostOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILambdaLifecycleContextFactory _contextFactory;

    public LambdaOnInitBuilder(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<LambdaHostOptions> options,
        ILambdaLifecycleContextFactory contextFactory
    )
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(contextFactory);

        Services = serviceProvider;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _contextFactory = contextFactory;
    }

    public IServiceProvider Services { get; }

    public ConcurrentDictionary<string, object?> Properties { get; } = new();

    public IReadOnlyList<LambdaInitDelegate> InitHandlers => _handlers.AsReadOnly();

    public ILambdaOnInitBuilder OnInit(LambdaInitDelegate handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _handlers.Add(handler);
        return this;
    }

    public Func<CancellationToken, Task<bool>>? Build() =>
        _handlers.Count == 0
            ? null
            : async Task<bool> (stoppingToken) =>
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                cts.CancelAfter(_options.InitTimeout);

                // ReSharper disable once AccessToDisposedClosure
                var tasks = _handlers.Select(h => RunInitHandler(h, cts.Token));

                (Exception? Error, bool ShouldContinue)[] results;
                try
                {
                    results = await Task.WhenAll(tasks).WaitAsync(cts.Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException) when (cts.Token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(
                        "Running OnInit handlers did not complete within the allocated timeout period."
                    );
                }

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

    private async Task<(Exception? Error, bool ShouldContinue)> RunInitHandler(
        LambdaInitDelegate handler,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var context = _contextFactory.Create(Properties, cancellationToken);

            await using (context as IAsyncDisposable)
            {
                using var scope = _scopeFactory.CreateScope();
                var result = await handler(context).ConfigureAwait(false);
                return (null, result);
            }
        }
        catch (Exception ex)
        {
            return (ex, false);
        }
    }
}
