using Amazon.Lambda.Core;

namespace Lambda.Host;

internal class LambdaHostContext : ILambdaHostContext
{
    private readonly ILambdaContext _lambdaContext;

    public LambdaHostContext(ILambdaContext lambdaContext, IServiceProvider serviceProvider)
    {
        _lambdaContext = lambdaContext;
        ServiceProvider = serviceProvider;
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

    public object? Request { get; set; }
    public object? Response { get; set; }

    public IServiceProvider ServiceProvider { get; }

    public CancellationToken CancellationToken { get; }
}
