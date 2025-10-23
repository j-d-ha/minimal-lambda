using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators.Extensions;

internal static class TypeExtractorExtensions
{
    internal static string GetAsGlobal(this ITypeSymbol typeSymbol) =>
        typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
}
