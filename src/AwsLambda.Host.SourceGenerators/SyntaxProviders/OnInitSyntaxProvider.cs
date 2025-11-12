using System.Threading;
using AwsLambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace AwsLambda.Host.SourceGenerators;

internal static class OnInitSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken cancellationToken) =>
        GenericHandlerInfoExtractor.Predicate(node, GeneratorConstants.OnInitMethodName);

    internal static HigherOrderMethodInfo? Transformer(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    ) =>
        GenericHandlerInfoExtractor.Transformer(
            context,
            GeneratorConstants.OnInitMethodName,
            IsBaseOnShutdownCall,
            cancellationToken
        );

    // we want to filter out the non-generic shutdown method calls that use the method signature
    // defined in ILambdaApplication. this is LambdaOnInitDelegate.
    // Func<IServiceProvider, CancellationToken, Task<bool>>
    private static bool IsBaseOnShutdownCall(this DelegateInfo delegateInfo) =>
        delegateInfo
            is {
                ReturnTypeInfo.FullyQualifiedType: TypeConstants.TaskBool,
                Parameters: [
                    { TypeInfo.FullyQualifiedType: TypeConstants.IServiceProvider },
                    { TypeInfo.FullyQualifiedType: TypeConstants.CancellationToken },
                ],
            };
}
