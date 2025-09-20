using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambda.Host.SourceGenerators;

internal static class TypeExtractorExtensions
{
    private const string Global = "global::";

    internal static string? GetAsGlobal(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is IErrorTypeSymbol)
            return null;

        return typeSymbol.ShouldOmitGlobalPrefix() ? typeSymbol.ToString() : Global + typeSymbol;
    }

    internal static string GetAsGlobal(this ITypeSymbol typeSymbol, TypeSyntax typeSyntax)
    {
        var globalType = typeSymbol.GetAsGlobal();

        return typeSyntax is NullableTypeSyntax && !globalType.EndsWith("?")
            ? globalType + "?"
            : globalType;
    }

    private static bool ShouldOmitGlobalPrefix(this ITypeSymbol typeSymbol) =>
        typeSymbol.SpecialType switch
        {
            SpecialType.System_Boolean => true,
            SpecialType.System_Byte => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            SpecialType.System_Decimal => true,
            SpecialType.System_Char => true,
            SpecialType.System_String => true,
            SpecialType.System_Object => true,
            SpecialType.System_Void => true,
            _ => false,
        };
}
