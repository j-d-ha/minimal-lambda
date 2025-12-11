using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MinimalLambda.SourceGenerators.Extensions;

internal static class TypeExtractorExtensions
{
    private static readonly SymbolDisplayFormat Format =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

    internal static string GetAsGlobal(this ITypeSymbol typeSymbol, TypeSyntax? typeSyntax = null)
    {
        var baseTypeName = typeSymbol.ToDisplayString(Format);

        return typeSyntax is NullableTypeSyntax && !baseTypeName.EndsWith("?")
            ? baseTypeName + "?"
            : baseTypeName;
    }
}
