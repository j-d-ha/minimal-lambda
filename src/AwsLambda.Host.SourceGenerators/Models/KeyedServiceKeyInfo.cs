using AwsLambda.Host.SourceGenerators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct KeyedServiceKeyInfo(
    string? DisplayValue,
    string? Type,
    string? BaseType,
    LocationInfo? LocationInfo
)
{
    internal static KeyedServiceKeyInfo Create(AttributeData attribute)
    {
        var (key, keyType, keyBaseType) = ExtractKeyedServiceKey(attribute.ConstructorArguments[0]);

        var keyedServiceKeyInfo = new KeyedServiceKeyInfo(key, keyType, keyBaseType, null);

        // conditionally get location info only if a key is null as a diagnostic needs to be
        // provided in that case.
        if (
            key is null
            && attribute.ApplicationSyntaxReference?.GetSyntax()
                is AttributeSyntax { ArgumentList: { Arguments.Count: > 0 } argumentList }
        )
        {
            var argument = argumentList.Arguments[0];
            var location = argument.Expression.GetLocation();
            var locationInfo = Models.LocationInfo.CreateFrom(location);
            return keyedServiceKeyInfo with { LocationInfo = locationInfo };
        }

        return keyedServiceKeyInfo;
    }

    internal string ToPublicString() =>
        $"{nameof(KeyedServiceKeyInfo)} {{ "
        + $"{nameof(DisplayValue)} = {DisplayValue}, "
        + $"{nameof(Type)} = {Type}, "
        + $"{nameof(BaseType)} = {BaseType} }}";

    private static (string? Key, string? KeyType, string? KeyBaseType) ExtractKeyedServiceKey(
        TypedConstant argument
    )
    {
        var keyBaseType = argument.Type?.BaseType?.GetAsGlobal();
        var keyType = argument.Type?.GetAsGlobal();

        if (argument.IsNull)
            return ("null", keyType, keyBaseType);

        object? value;
        try
        {
            value = argument.Value;
        }
        catch
        {
            return (null, keyType, keyBaseType);
        }

        if (value is null)
            return (null, keyType, keyBaseType);

        // Generate the literal C# code to recreate this value
        var keyLiteral = argument.Kind switch
        {
            TypedConstantKind.Primitive when value is string strValue =>
                SymbolDisplay.FormatLiteral(strValue, true),

            TypedConstantKind.Primitive when value is char charValue => $"'{charValue}'",

            TypedConstantKind.Primitive when value is bool boolValue => boolValue
                ? "true"
                : "false",

            TypedConstantKind.Primitive or TypedConstantKind.Enum =>
                $"({argument.Type?.GetAsGlobal()}){value}",

            TypedConstantKind.Type when value is ITypeSymbol typeValue =>
                $"typeof({typeValue.GetAsGlobal()})",

            _ => value.ToString(),
        };

        return (keyLiteral, keyType, keyBaseType);
    }
}
