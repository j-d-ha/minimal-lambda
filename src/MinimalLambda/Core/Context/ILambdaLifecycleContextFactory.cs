namespace MinimalLambda;

internal interface ILambdaLifecycleContextFactory
{
    ILambdaLifecycleContext Create(
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken);
}
