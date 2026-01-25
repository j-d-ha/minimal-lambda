using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MinimalLambda.SourceGenerators;

internal static class SyntaxNodeExtensions
{
    extension(SyntaxNode node)
    {
        internal bool TryGetMethodName([NotNullWhen(true)] out string? methodName)
        {
            methodName = null;
            if (node is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Name.Identifier.ValueText: var method,
                    },
                })
            {
                methodName = method;
                return true;
            }

            return false;
        }

        internal bool IsGeneratedFile() => node.SyntaxTree.FilePath.EndsWith(".g.cs");
    }
}
