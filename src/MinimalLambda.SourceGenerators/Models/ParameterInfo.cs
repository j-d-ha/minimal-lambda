using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct ParameterInfo(
    string Name,
    TypeInfo TypeInfo,
    LocationInfo? LocationInfo,
    ParameterSource Source,
    KeyedServiceKeyInfo? KeyedServiceKey,
    bool IsNullable,
    bool IsOptional
)
{
    internal bool IsRequired => !IsOptional && !IsNullable;

    internal static ParameterInfo Create(IParameterSymbol parameter)
    {
        var typeInfo = TypeInfo.Create(parameter.Type);
        var name = parameter.Name;
        var location = Models.LocationInfo.CreateFrom(parameter);
        var (source, keyedService) = GetSourceFromAttribute(
            parameter.GetAttributes(),
            typeInfo.FullyQualifiedType
        );
        var isNullable = parameter.NullableAnnotation == NullableAnnotation.Annotated;
        var isOptional = parameter.IsOptional;

        return new ParameterInfo(
            name,
            typeInfo,
            location,
            source,
            keyedService,
            isNullable,
            isOptional
        );
    }

    internal string ToPublicString() =>
        $"{nameof(ParameterInfo)} {{ "
        + $"Type = {TypeInfo.FullyQualifiedType}, "
        + $"{nameof(Name)} = {Name}, "
        + $"{nameof(Source)} = {Source}, "
        + $"{nameof(IsNullable)} = {IsNullable}, "
        + $"{nameof(IsOptional)} = {IsOptional}"
        + $"{(KeyedServiceKey.HasValue ? ", " + KeyedServiceKey.Value.ToPublicString() + " " : "")}}}";

    private static (
        ParameterSource Source,
        KeyedServiceKeyInfo? KeyedServiceKey
    ) GetSourceFromAttribute(IEnumerable<AttributeData> attributes, string type)
    {
        // try and extract source from attributes
        foreach (var attribute in attributes)
            switch (attribute.AttributeClass?.ToString())
            {
                case AttributeConstants.EventAttribute:
                    return (ParameterSource.Event, null);

                case AttributeConstants.FromKeyedService:
                    var keyedServiceKey = KeyedServiceKeyInfo.Create(attribute);
                    return (ParameterSource.KeyedService, keyedServiceKey);
            }

        // fallback to get source from type
        return type switch
        {
            TypeConstants.CancellationToken => (ParameterSource.CancellationToken, null),
            TypeConstants.ILambdaContext => (ParameterSource.Context, null),
            TypeConstants.ILambdaHostContext => (ParameterSource.Context, null),
            _ => (ParameterSource.Service, null),
        };
    }
}
