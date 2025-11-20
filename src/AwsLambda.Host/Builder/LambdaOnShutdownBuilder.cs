using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

internal class LambdaOnShutdownBuilder : ILambdaOnShutdownBuilder
{
    private readonly IList<LambdaShutdownDelegate> _handlers = [];
    private readonly LambdaHostOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public LambdaOnShutdownBuilder(
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
    public IReadOnlyList<LambdaShutdownDelegate> ShutdownHandlers => _handlers.AsReadOnly();

    public ILambdaOnShutdownBuilder OnShutdown(LambdaShutdownDelegate handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _handlers.Add(handler);
        return this;
    }

    public LambdaShutdownDelegate Build()
    {
        if (_handlers.Count == 0)
            return (_, _) => Task.CompletedTask;

        return async Task (_, cancellationToken) =>
        {
            var tasks = _handlers.Select(h => RunShutdownHandler(h, cancellationToken));

            var output = await Task.WhenAll(tasks).ConfigureAwait(false);

            var errors = output.Where(x => x is not null).Select(x => x!).ToArray();

            if (errors.Length != 0)
                throw new AggregateException(
                    "Encountered errors while running OnShutdown handlers:",
                    errors
                );
        };
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
}
