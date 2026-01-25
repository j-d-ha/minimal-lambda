using Amazon.Lambda.Core;

namespace MinimalLambda;

internal interface ILambdaInvocationContextFactory
{
    ILambdaInvocationContext Create(
        ILambdaContext lambdaContext,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken);
}
