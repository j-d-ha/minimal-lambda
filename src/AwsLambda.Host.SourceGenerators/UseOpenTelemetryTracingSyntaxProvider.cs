using System.Threading;
using AwsLambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace AwsLambda.Host.SourceGenerators;

internal static class UseOpenTelemetryTracingSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken _) =>
        node.TryGetMethodName(out var name)
        && name == GeneratorConstants.UseOpenTelemetryTracingMethodName;

    internal static UseOpenTelemetryTracingInfo? Transformer(
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
                        Name: "Host",
                        ContainingNamespace:
                        { Name: "AwsLambda", ContainingNamespace.IsGlobalNamespace: true },
                    },
                } targetOperation
            && targetOperation.TargetMethod.ContainingAssembly.Name
                == "AwsLambda.Host.OpenTelemetry"
        )
        {
            var interceptableLocation = context.SemanticModel.GetInterceptableLocation(
                (InvocationExpressionSyntax)targetOperation.Syntax,
                cancellationToken
            )!;

            return new UseOpenTelemetryTracingInfo(
                LocationInfo: LocationInfo.CreateFrom(context.Node),
                InterceptableLocationInfo: InterceptableLocationInfo.CreateFrom(
                    interceptableLocation
                )
            );
        }

        return null;
    }
}
