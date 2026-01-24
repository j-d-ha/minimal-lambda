using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalLambda.Builder;

internal class LambdaOnShutdownBuilder : ILambdaOnShutdownBuilder
{
    private readonly IList<LambdaShutdownDelegate> _handlers = [];
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILambdaLifecycleContextFactory _contextFactory;

    public LambdaOnShutdownBuilder(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        ILambdaLifecycleContextFactory contextFactory)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(contextFactory);

        Services = serviceProvider;
        _scopeFactory = scopeFactory;
        _contextFactory = contextFactory;
    }

    public IServiceProvider Services { get; }

    public ConcurrentDictionary<string, object?> Properties { get; } = new();

    public IReadOnlyList<LambdaShutdownDelegate> ShutdownHandlers => _handlers.AsReadOnly();

    public ILambdaOnShutdownBuilder OnShutdown(LambdaShutdownDelegate handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _handlers.Add(handler);
        return this;
    }

    public Func<CancellationToken, Task> Build() =>
        _handlers.Count == 0
            ? _ => Task.CompletedTask
            : async Task (cancellationToken) =>
            {
                var tasks = _handlers.Select(h => RunShutdownHandler(h, cancellationToken));

                var output = await Task.WhenAll(tasks).ConfigureAwait(false);

                var errors = output.Where(x => x is not null).Select(x => x!).ToArray();

                if (errors.Length != 0)
                    throw new AggregateException(
                        "Encountered errors while running OnShutdown handlers:",
                        errors);
            };

    private async Task<Exception?> RunShutdownHandler(
        LambdaShutdownDelegate handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var context = _contextFactory.Create(Properties, cancellationToken);

            await using (context as IAsyncDisposable)
            {
                using var scope = _scopeFactory.CreateScope();
                await handler(context).ConfigureAwait(false);
                return null;
            }
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
