using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MinimalLambda.SourceGenerators.Extensions;

internal static class TypeExtractorExtensions
{
    private static readonly SymbolDisplayFormat NullableFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

    private static readonly SymbolDisplayFormat NotNullableFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.ExpandNullable
        );

    extension(ITypeSymbol typeSymbol)
    {
        internal string ToGloballyQualifiedName(TypeSyntax? typeSyntax = null)
        {
            var baseTypeName = typeSymbol.ToDisplayString(NullableFormat);

            return typeSyntax is NullableTypeSyntax && !baseTypeName.EndsWith("?")
                ? baseTypeName + "?"
                : baseTypeName;
        }

        internal string ToNotNullableGloballyQualifiedName() =>
            typeSymbol.ToDisplayString(NotNullableFormat);
    }
}
