using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AwsLambda.Host.SourceGenerators;

internal static class SyntaxExtensions
{
    internal static bool TryGetMethodName(
        this SyntaxNode node,
        [NotNullWhen(true)] out string? methodName
    )
    {
        methodName = null;
        if (
            node is InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: var method },
            }
        )
        {
            methodName = method;
            return true;
        }

        return false;
    }

    internal static bool IsGeneratedFile(this SyntaxNode node) =>
        node.SyntaxTree.FilePath.EndsWith(".g.cs");
}
