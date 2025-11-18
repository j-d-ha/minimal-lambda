namespace AwsLambda.Host;

internal class LambdaInvocationBuilder : ILambdaInvocationBuilder
{
    public LambdaInvocationBuilder(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        Services = services;
    }

    public IServiceProvider Services { get; }

    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

    public List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares { get; } = [];

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

        Middlewares.Add(middleware);

        return this;
    }

    public LambdaInvocationDelegate Build()
    {
        if (Handler is null)
            throw new InvalidOperationException("Lambda Handler has not been set.");

        LambdaInvocationDelegate handler = Handler;

        for (var i = Middlewares.Count - 1; i >= 0; i--)
        {
            handler = Middlewares[i](handler);
        }

        return handler;
    }
}
