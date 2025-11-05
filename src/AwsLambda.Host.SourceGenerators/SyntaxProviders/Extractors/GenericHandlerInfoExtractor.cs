using System;
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

internal static class GenericHandlerInfoExtractor
{
    internal static bool Predicate(SyntaxNode node, string methodName) =>
        node.TryGetMethodName(out var name) && name == methodName && !node.IsGeneratedFile();

    internal static HigherOrderMethodInfo? Transformer(
        GeneratorSyntaxContext context,
        string methodName,
        Func<DelegateInfo, bool> delegateFilter,
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
        if (delegateFilter(delegateInfo.Value))
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
            Name: methodName,
            LocationInfo: LocationInfo.CreateFrom(context.Node),
            DelegateInfo: delegateInfo.Value,
            InterceptableLocationInfo: InterceptableLocationInfo.CreateFrom(interceptableLocation),
            GenericTypeArguments: typeArguments
        );
    }
}
