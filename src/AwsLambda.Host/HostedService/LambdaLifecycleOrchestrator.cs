using LanguageExt;
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

        return output.Somes();
    }

    private async Task<Option<Exception>> RunShutdownHandler(
        LambdaShutdownDelegate handler,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            await handler(scope.ServiceProvider, cancellationToken);
            return Option<Exception>.None;
        }
        catch (Exception ex)
        {
            return Option<Exception>.Some(ex);
        }
    }
}
