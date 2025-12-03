namespace AwsLambda.Host.Builder;

internal class LambdaInvocationBuilder : ILambdaInvocationBuilder
{
    internal const string EventFeatureProviderKey = "__EventFeatureProvider";
    internal const string ResponseFeatureProviderKey = "__ResponseFeatureProvider";

    private readonly List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> _middleware =
    [];

    public LambdaInvocationBuilder(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        Services = services;
    }

    public IServiceProvider Services { get; }

    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

    public IReadOnlyList<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares =>
        _middleware;

    public LambdaInvocationDelegate? Handler { get; private set; }

    public ILambdaInvocationBuilder Handle(LambdaInvocationDelegate handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (Handler is not null)
            throw new InvalidOperationException("Lambda Handler has already been set.");

        Handler = handler;

        return this;
    }

    public ILambdaInvocationBuilder Use(
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware
    )
    {
        ArgumentNullException.ThrowIfNull(middleware);

        _middleware.Add(middleware);

        return this;
    }

    public LambdaInvocationDelegate Build()
    {
        if (Handler is null)
            throw new InvalidOperationException("Lambda Handler has not been set.");

        var handler = Handler;

        for (var i = Middlewares.Count - 1; i >= 0; i--)
            handler = Middlewares[i](handler);

        return handler;
    }
}
