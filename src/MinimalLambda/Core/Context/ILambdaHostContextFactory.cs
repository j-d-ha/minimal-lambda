using Amazon.Lambda.Core;

namespace MinimalLambda;

internal interface ILambdaHostContextFactory
{
    ILambdaHostContext Create(
        ILambdaContext lambdaContext,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken
    );
}
