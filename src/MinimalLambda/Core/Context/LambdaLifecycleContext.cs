using Microsoft.Extensions.DependencyInjection;

namespace MinimalLambda;

internal class LambdaLifecycleContext(
    LambdaLifecycleContext.Core contextCore,
    IServiceScopeFactory scopeFactory,
    IDictionary<string, object?> properties,
    CancellationToken cancellationToken) : ILambdaLifecycleContext, IAsyncDisposable
{
    private IServiceScope? _instanceServicesScope;

    public CancellationToken CancellationToken { get; } = cancellationToken;

    public IServiceProvider ServiceProvider
    {
        get
        {
            if (field is null)
            {
                _instanceServicesScope = scopeFactory.CreateScope();
                field = _instanceServicesScope.ServiceProvider;
            }

            return field;
        }
        private set;
    }

    public IDictionary<string, object?> Properties { get; } = properties;

    public TimeSpan ElapsedTime => contextCore.Stopwatch.Elapsed;
    public string? Region => contextCore.Region;
    public string? ExecutionEnvironment => contextCore.ExecutionEnvironment;
    public string? FunctionName => contextCore.FunctionName;
    public int? FunctionMemorySize => contextCore.FunctionMemorySize;
    public string? FunctionVersion => contextCore.FunctionVersion;
    public string? InitializationType => contextCore.InitializationType;
    public string? LogGroupName => contextCore.LogGroupName;
    public string? LogStreamName => contextCore.LogStreamName;
    public string? TaskRoot => contextCore.TaskRoot;

    public async ValueTask DisposeAsync()
    {
        if (_instanceServicesScope is IAsyncDisposable instanceServicesScopeAsyncDisposable)
            await instanceServicesScopeAsyncDisposable.DisposeAsync();

        _instanceServicesScope?.Dispose();
        ServiceProvider = null!;
        _instanceServicesScope = null;
    }

    internal class Core
    {
        internal required ILifetimeStopwatch Stopwatch { get; init; }
        internal string? Region { get; init; }
        internal string? ExecutionEnvironment { get; init; }
        internal string? FunctionName { get; init; }
        internal int? FunctionMemorySize { get; init; }
        internal string? FunctionVersion { get; init; }
        internal string? InitializationType { get; init; }
        internal string? LogGroupName { get; init; }
        internal string? LogStreamName { get; init; }
        internal string? TaskRoot { get; init; }
    }
}
