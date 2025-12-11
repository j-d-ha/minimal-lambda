using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class UseOpenTelemetryTracingSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken _) =>
        node.TryGetMethodName(out var name)
        && name == GeneratorConstants.UseOpenTelemetryTracingMethodName;

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
                    TargetMethod.ContainingNamespace:
                    {
                        Name: "Builder",
                        ContainingNamespace:
                        {
                            Name: "Host",
                            ContainingNamespace:
                            { Name: "AwsLambda", ContainingNamespace.IsGlobalNamespace: true },
                        },
                    },
                } targetOperation
            && targetOperation.TargetMethod.ContainingAssembly.Name == "MinimalLambda.OpenTelemetry"
        )
        {
            var interceptableLocation = context.SemanticModel.GetInterceptableLocation(
                (InvocationExpressionSyntax)targetOperation.Syntax,
                cancellationToken
            )!;

            return new SimpleMethodInfo(
                targetOperation.TargetMethod.Name,
                LocationInfo.CreateFrom(context.Node),
                InterceptableLocationInfo.CreateFrom(interceptableLocation)
            );
        }

        return null;
    }
}
