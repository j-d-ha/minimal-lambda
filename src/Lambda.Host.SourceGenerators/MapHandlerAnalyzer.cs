using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Lambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Lambda.Host.SourceGenerators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MapHandlerAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [Diagnostics.MultipleMethodCalls, Diagnostics.MultipleParametersOfSameType];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(
            syntaxContext =>
            {
                var node = syntaxContext.Node;

                // Use the shared predicate to filter candidates
                if (
                    !MapHandlerSyntaxHelper.IsMapHandlerInvocation(
                        node,
                        syntaxContext.CancellationToken
                    )
                )
                    return;

                // Extract invocation info using the shared helper
                if (
                    !MapHandlerSyntaxHelper.TryGetMapHandlerInfo(
                        (InvocationExpressionSyntax)node,
                        syntaxContext.SemanticModel,
                        syntaxContext.CancellationToken,
                        out var info
                    ) || info is null
                )
                    return;

                // Report duplicate parameter types immediately (LH0002)
                CheckForDuplicateTypeParameters(
                    syntaxContext,
                    info.DelegateInfo.Parameters,
                    TypeConstants.CancellationToken,
                    (InvocationExpressionSyntax)node
                );

                CheckForDuplicateTypeParameters(
                    syntaxContext,
                    info.DelegateInfo.Parameters,
                    TypeConstants.ILambdaContext,
                    (InvocationExpressionSyntax)node
                );
            },
            SyntaxKind.InvocationExpression
        );

        // For LH0001 (multiple MapHandler calls), we still need compilation-level analysis
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var invocations = new ConcurrentBag<MapHandlerInvocationInfo>();

            compilationContext.RegisterSyntaxNodeAction(
                syntaxContext =>
                {
                    var node = syntaxContext.Node;

                    if (
                        !MapHandlerSyntaxHelper.IsMapHandlerInvocation(
                            node,
                            syntaxContext.CancellationToken
                        )
                    )
                        return;

                    if (
                        MapHandlerSyntaxHelper.TryGetMapHandlerInfo(
                            (InvocationExpressionSyntax)node,
                            syntaxContext.SemanticModel,
                            syntaxContext.CancellationToken,
                            out var info
                        ) && info is not null
                    )
                        invocations.Add(info);
                },
                SyntaxKind.InvocationExpression
            );

            compilationContext.RegisterCompilationEndAction(endContext =>
            {
                var invocationList = invocations.ToList();

                // Report multiple MapHandler calls (LH0001)
                foreach (var invocation in invocationList.Skip(1))
                    endContext.ReportDiagnostic(
                        Diagnostic.Create(
                            Diagnostics.MultipleMethodCalls,
                            invocation.LocationInfo?.ToLocation(),
                            "LambdaApplication.MapHandler(Delegate)"
                        )
                    );
            });
        });
    }

    private static void CheckForDuplicateTypeParameters(
        SyntaxNodeAnalysisContext context,
        IEnumerable<ParameterInfo> parameterInfos,
        string type,
        InvocationExpressionSyntax invocationNode
    )
    {
        var duplicates = parameterInfos.Where(p => p.Type == type).Skip(1);

        foreach (var parameter in duplicates)
        {
            var location = parameter.LocationInfo?.ToLocation() ?? invocationNode.GetLocation();

            context.ReportDiagnostic(
                Diagnostic.Create(Diagnostics.MultipleParametersOfSameType, location, type)
            );

            context.ReportDiagnostic(
                Diagnostic.Create(Diagnostics.MultipleParametersOfSameType, location, type)
            );
        }
    }
}
