using System.Threading;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class OnShutdownSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken cancellationToken) =>
        HandlerInfoExtractor.Predicate(node, GeneratorConstants.OnShutdownMethodName);

    internal static InvocationMethodInfo? Transformer(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    ) => HandlerInfoExtractor.Transformer(context, IsBaseOnShutdownCall, cancellationToken);

    // we want to filter out the non-generic shutdown method calls that use the method signature
    // defined in ILambdaOnShutdownBuilder. this is LambdaShutdownDelegate.
    // Func<IServiceProvider, CancellationToken, Task>
    private static bool IsBaseOnShutdownCall(this DelegateInfo delegateInfo) =>
        delegateInfo
            is {
                ReturnTypeInfo.FullyQualifiedType: TypeConstants.Task,
                Parameters: [
                    { TypeInfo.FullyQualifiedType: TypeConstants.ILambdaLifecycleContext },
                ],
            };
}
