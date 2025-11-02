using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using AwsLambda.Host.SourceGenerators.Extensions;
using AwsLambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace AwsLambda.Host.SourceGenerators;

internal static class OnShutdownSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken cancellationToken) =>
        node.TryGetMethodName(out var name)
        && name == GeneratorConstants.OnShutdownMethodName
        && !node.IsGeneratedFile();

    internal static HigherOrderMethodInfo? Transformer(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        var operation = context.SemanticModel.GetOperation(context.Node, cancellationToken);

        if (
            operation
                is not IInvocationOperation
                {
                    TargetMethod.ContainingNamespace:
                    {
                        Name: "Host",
                        ContainingNamespace:
                        { Name: "AwsLambda", ContainingNamespace.IsGlobalNamespace: true },
                    },
                } targetOperation
            || targetOperation.TargetMethod.ContainingAssembly.Name != "AwsLambda.Host"
        )
            return null;

        if (context.Node is not InvocationExpressionSyntax invocationExpr)
            return null;

        var handler = invocationExpr.ArgumentList.Arguments.ElementAtOrDefault(0)?.Expression;

        var delegateInfo = handler?.ExtractDelegateInfo(context, cancellationToken);
        if (delegateInfo is null)
            return null;

        // filter out non-generic shutdown method calls
        if (delegateInfo.Value.IsBaseOnShutdownCall())
            return null;

        // get generic type arguments
        var typeArguments = targetOperation
            .TargetMethod.TypeArguments.Zip(
                targetOperation.TargetMethod.TypeParameters,
                (argument, parameter) => new GenericInfo(argument.GetAsGlobal(), parameter.Name)
            )
            .ToImmutableArray();

        // get interceptable location
        var interceptableLocation = context.SemanticModel.GetInterceptableLocation(
            invocationExpr,
            cancellationToken
        )!;

        return new HigherOrderMethodInfo(
            LocationInfo: LocationInfo.CreateFrom(context.Node),
            DelegateInfo: delegateInfo.Value,
            InterceptableLocationInfo: InterceptableLocationInfo.CreateFrom(interceptableLocation),
            GenericTypeArguments: typeArguments
        );
    }

    // we want to filter out the non-generic shutdown method calls that use the method signature
    // defined in ILambdaApplication. this is LambdaShutdownDelegate.
    // Func<IServiceProvider, CancellationToken, Task>
    private static bool IsBaseOnShutdownCall(this DelegateInfo delegateInfo) =>
        delegateInfo
            is {
                FullResponseType: TypeConstants.Task,
                Parameters: [
                    { Type: TypeConstants.IServiceProvider },
                    { Type: TypeConstants.CancellationToken },
                ],
            };
}
