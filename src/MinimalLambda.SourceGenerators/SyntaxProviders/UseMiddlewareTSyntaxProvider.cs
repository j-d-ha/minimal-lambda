using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using MinimalLambda.SourceGenerators.Models;
using MinimalLambda.SourceGenerators.WellKnownTypes;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace MinimalLambda.SourceGenerators;

internal static class UseMiddlewareTSyntaxProvider
{
    private static readonly string TargetMethodName = "UseMiddleware";

    internal static bool Predicate(SyntaxNode node, CancellationToken _) =>
        !node.IsGeneratedFile() && node.TryGetMethodName(out var name) && name == TargetMethodName;

    internal static UseMiddlewareTInfo? Transformer(
        GeneratorSyntaxContext syntaxContext,
        CancellationToken cancellationToken
    )
    {
        var context = new GeneratorContext(syntaxContext, cancellationToken);

        if (!TryGetInvocationOperation(context, out var targetOperation))
            return null;

        return UseMiddlewareTInfo.Create(targetOperation, context);

        // // get class TypeInfo
        // var middlewareClassType = targetOperation.TargetMethod.TypeArguments[0];
        //
        // // Get location of the generic argument
        // Location? genericArgumentLocation = null;
        // if (
        //     targetOperation.Syntax is InvocationExpressionSyntax
        //     {
        //         Expression: MemberAccessExpressionSyntax { Name: GenericNameSyntax genericName },
        //     }
        // )
        // {
        //     // Get the first type argument's location
        //     var typeArgument = genericName.TypeArgumentList.Arguments[0];
        //     genericArgumentLocation = typeArgument.GetLocation();
        // }
        //
        // var classInfo = ClassInfo.Create(middlewareClassType);
        //
        // var interceptableLocation = context.SemanticModel.GetInterceptableLocation(
        //     (InvocationExpressionSyntax)targetOperation.Syntax,
        //     cancellationToken
        // )!;
        //
        // var useMiddlewareTInfo = new UseMiddlewareTInfo(
        //     InterceptableLocationInfo.CreateFrom(interceptableLocation),
        //     classInfo,
        //     genericArgumentLocation?.ToLocationInfo()
        // );
        //
        // return useMiddlewareTInfo;
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
                context.WellKnownTypes.IsTypeMatch(c, WellKnownType.MinimalLambda_ILambdaMiddleware)
            )
        )
        {
            invocationOperation = targetOperation;
            return true;
        }

        return false;
    }
}
