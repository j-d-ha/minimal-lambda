using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AwsLambda.Host;

internal class LambdaHostContext : ILambdaHostContext, IAsyncDisposable
{
    private readonly ILambdaContext _lambdaContext;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private IServiceProvider? _instanceServiceProvider;
    private IServiceScope? _instanceServicesScope;

    public LambdaHostContext(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        CancellationToken cancellationToken
    )
    {
        _lambdaContext = lambdaContext ?? throw new ArgumentNullException(nameof(lambdaContext));
        _serviceScopeFactory =
            serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        CancellationToken = cancellationToken;
    }

    public async ValueTask DisposeAsync()
    {
        if (_instanceServicesScope is IAsyncDisposable instanceServicesScopeAsyncDisposable)
            await instanceServicesScopeAsyncDisposable.DisposeAsync();

        _instanceServicesScope?.Dispose();

        _instanceServicesScope = null;
        _instanceServiceProvider = null!;
        Items.Clear();
    }

    public string AwsRequestId => _lambdaContext.AwsRequestId;
    public IClientContext ClientContext => _lambdaContext.ClientContext;
    public string FunctionName => _lambdaContext.FunctionName;
    public string FunctionVersion => _lambdaContext.FunctionVersion;
    public ICognitoIdentity Identity => _lambdaContext.Identity;
    public string InvokedFunctionArn => _lambdaContext.InvokedFunctionArn;
    public ILambdaLogger Logger => _lambdaContext.Logger;
    public string LogGroupName => _lambdaContext.LogGroupName;
    public string LogStreamName => _lambdaContext.LogStreamName;
    public int MemoryLimitInMB => _lambdaContext.MemoryLimitInMB;
    public TimeSpan RemainingTime => _lambdaContext.RemainingTime;

    public object? Event { get; set; }

    public object? Response { get; set; }

    public IServiceProvider ServiceProvider
    {
        get
        {
            if (_instanceServicesScope == null)
            {
                _instanceServicesScope = _serviceScopeFactory.CreateScope();
                _instanceServiceProvider = _instanceServicesScope.ServiceProvider;
            }

            return _instanceServiceProvider!;
        }
        set => _instanceServiceProvider = value;
    }

    public IDictionary<object, object?> Items { get; set; } = new Dictionary<object, object?>();

    public CancellationToken CancellationToken { get; }
}
