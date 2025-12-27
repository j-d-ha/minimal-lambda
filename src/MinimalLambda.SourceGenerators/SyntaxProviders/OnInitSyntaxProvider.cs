using System.Threading;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class OnInitSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken cancellationToken) =>
        HandlerInfoExtractor.Predicate(node, GeneratorConstants.OnInitMethodName);

    internal static InvocationMethodInfo? Transformer(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    ) => HandlerInfoExtractor.Transformer(context, IsBaseOnShutdownCall, cancellationToken);

    // we want to filter out the non-generic init method calls that use the method signature
    // defined in ILambdaOnInitBuilder. this is LambdaInitDelegate.
    // Func<IServiceProvider, CancellationToken, Task<bool>>
    private static bool IsBaseOnShutdownCall(this DelegateInfo delegateInfo) =>
        delegateInfo
            is {
                ReturnTypeInfo.FullyQualifiedType: TypeConstants.TaskBool,
                Parameters: [
                    { TypeInfo.FullyQualifiedType: TypeConstants.ILambdaLifecycleContext },
                ],
            };
}
