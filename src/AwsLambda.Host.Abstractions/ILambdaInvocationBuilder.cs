namespace AwsLambda.Host;

public interface ILambdaInvocationBuilder
{
    IServiceProvider Services { get; }

    IDictionary<string, object?> Properties { get; }

    List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares { get; }

    LambdaInvocationDelegate? Handler { get; }

    ILambdaInvocationBuilder Handle(LambdaInvocationDelegate handler);

    ILambdaInvocationBuilder Use(
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware
    );

    LambdaInvocationDelegate Build();
}
