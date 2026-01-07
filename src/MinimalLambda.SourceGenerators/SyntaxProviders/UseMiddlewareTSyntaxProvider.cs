using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using MinimalLambda.SourceGenerators.Models;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace MinimalLambda.SourceGenerators;

internal static class UseMiddlewareTSyntaxProvider
{
    private const string TargetMethodName = "UseMiddleware";

    internal static bool Predicate(SyntaxNode node, CancellationToken _) =>
        !node.IsGeneratedFile() && node.TryGetMethodName(out var name) && name == TargetMethodName;

    internal static UseMiddlewareTInfo? Transformer(
        GeneratorSyntaxContext syntaxContext,
        CancellationToken cancellationToken
    )
    {
        var context = new GeneratorContext(syntaxContext, cancellationToken);

        return !TryGetInvocationOperation(context, out var targetOperation)
            ? null
            : UseMiddlewareTInfo.Create(targetOperation, context);
    }

    private static bool TryGetInvocationOperation(
        GeneratorContext context,
        [NotNullWhen(true)] out IInvocationOperation? invocationOperation
    )
    {
        invocationOperation = null;

        var operation = context.SemanticModel.GetOperation(context.Node, context.CancellationToken);

        if (
            operation
                is IInvocationOperation
                {
                    TargetMethod:
                    {
                        IsGenericMethod: true,
                        ContainingAssembly.Name: "MinimalLambda",
                        ContainingNamespace:
                        {
                            Name: "Builder",
                            ContainingNamespace:
                            { Name: "MinimalLambda", ContainingNamespace.IsGlobalNamespace: true },
                        },
                    },
                } targetOperation
            && targetOperation.TargetMethod.ConstructedFrom.TypeParameters.FirstOrDefault()
                is { } typeParameter
            && typeParameter.ConstraintTypes.Any(c =>
                context.WellKnownTypes.IsType(c, WellKnownType.MinimalLambda_ILambdaMiddleware)
            )
        )
        {
            invocationOperation = targetOperation;
            return true;
        }

        return false;
    }
}
