namespace AwsLambda.Host;

public interface ILambdaHandlerBuilder
{
    IServiceProvider Services { get; }

    IDictionary<string, object?> Properties { get; }

    List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares { get; }

    LambdaInvocationDelegate? Handler { get; }

    ILambdaHandlerBuilder Handle(LambdaInvocationDelegate handler);

    ILambdaHandlerBuilder Use(Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware);

    LambdaInvocationDelegate Build();
}
