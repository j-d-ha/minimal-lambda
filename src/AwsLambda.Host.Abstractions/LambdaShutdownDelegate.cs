namespace AwsLambda.Host;

public delegate Task LambdaShutdownDelegate(
    IServiceProvider services,
    CancellationToken cancellationToken
);
