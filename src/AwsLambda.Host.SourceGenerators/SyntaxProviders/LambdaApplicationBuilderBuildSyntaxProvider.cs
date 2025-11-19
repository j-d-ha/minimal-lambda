using System.Threading;
using AwsLambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace AwsLambda.Host.SourceGenerators;

internal static class LambdaApplicationBuilderBuildSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken _) =>
        node.TryGetMethodName(out var name) && name == "Build";

    internal static SimpleMethodInfo? Transformer(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        var operation = context.SemanticModel.GetOperation(context.Node, cancellationToken);

        if (
            operation
                is IInvocationOperation
                {
                    TargetMethod:
                    {
                        ContainingType.Name: "LambdaApplicationBuilder",
                        ContainingNamespace:
                        {
                            Name: "Host",
                            ContainingNamespace:
                            { Name: "AwsLambda", ContainingNamespace.IsGlobalNamespace: true },
                        }
                    },
                } targetOperation
            && targetOperation.TargetMethod.ContainingAssembly.Name == "AwsLambda.Host"
        )
        {
            var interceptableLocation = context.SemanticModel.GetInterceptableLocation(
                (InvocationExpressionSyntax)targetOperation.Syntax,
                cancellationToken
            )!;

            return new SimpleMethodInfo(
                "Build",
                LocationInfo.CreateFrom(context.Node),
                InterceptableLocationInfo.CreateFrom(interceptableLocation)
            );
        }

        return null;
    }
}
