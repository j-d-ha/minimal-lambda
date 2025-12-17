using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using MinimalLambda.SourceGenerators.Extensions;
using MinimalLambda.SourceGenerators.Models;
using MinimalLambda.SourceGenerators.Types;

namespace MinimalLambda.SourceGenerators;

internal static class UseMiddlewareTSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken _) =>
        !node.IsGeneratedFile() && node.TryGetMethodName(out var name) && name == "UseMiddleware";

    internal static UseMiddlewareTInfo? Transformer(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        var operation = context.SemanticModel.GetOperation(context.Node, cancellationToken);

        if (
            operation
                is not IInvocationOperation
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
            || !targetOperation
                .TargetMethod.ConstructedFrom.TypeParameters[0]
                .ConstraintTypes.Any(c =>
                    c.Name == "ILambdaMiddleware"
                    && c.ContainingNamespace
                        is { Name: "MinimalLambda", ContainingNamespace.IsGlobalNamespace: true }
                )
        )
            return null;

        // get class TypeInfo
        var middlewareClassType = targetOperation.TargetMethod.TypeArguments[0];

        // get globally qualified name of the class
        var globallyQualifiedName = middlewareClassType.GetAsGlobal();

        // handle each instance constructor on the type
        var constructorInfo = ((INamedTypeSymbol)middlewareClassType)
            .InstanceConstructors.Select(constructor =>
            {
                return new ConstructorInfo(0);
            })
            .ToEquatableArray();

        var classInfo = new ClassInfo { GloballyQualifiedName = globallyQualifiedName };

        var interceptableLocation = context.SemanticModel.GetInterceptableLocation(
            (InvocationExpressionSyntax)targetOperation.Syntax,
            cancellationToken
        )!;

        var useMiddlewareTInfo = new UseMiddlewareTInfo(
            InterceptableLocationInfo.CreateFrom(interceptableLocation),
            classInfo,
            constructorInfo
        );

        return useMiddlewareTInfo;
    }
}
