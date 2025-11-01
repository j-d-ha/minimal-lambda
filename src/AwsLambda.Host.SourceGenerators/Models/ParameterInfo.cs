using System.Collections.Generic;
using System.Linq;
using AwsLambda.Host.SourceGenerators.Extensions;
using Microsoft.CodeAnalysis;

namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct ParameterInfo(
    string Type,
    string Name,
    LocationInfo? LocationInfo,
    ParameterSource Source,
    string? KeyedServiceKey,
    bool IsNullable = false,
    bool IsOptional = false
)
{
    internal bool IsRequired => !IsOptional && !IsNullable;

    internal static ParameterInfo Create(IParameterSymbol parameter)
    {
        var type = parameter.Type.GetAsGlobal();
        var name = parameter.Name;
        var location = Models.LocationInfo.CreateFrom(parameter);
        var (source, key) = GetSourceFromAttribute(parameter.GetAttributes(), type);
        var isNullable = parameter.NullableAnnotation == NullableAnnotation.Annotated;
        var isOptional = parameter.IsOptional;

        return new ParameterInfo(type, name, location, source, key, isNullable, isOptional);
    }

    internal string ToPublicString() =>
        $"{nameof(ParameterInfo)} {{ "
        + $"{nameof(Type)} = {Type}, "
        + $"{nameof(Name)} = {Name}, "
        + $"{nameof(Source)} = {Source}, "
        + $"{nameof(KeyedServiceKey)} = {KeyedServiceKey}, "
        + $"{nameof(IsNullable)} = {IsNullable}, "
        + $"{nameof(IsOptional)} = {IsOptional} }}";

    private static (ParameterSource Source, string? KeyedServiceKey) GetSourceFromAttribute(
        IEnumerable<AttributeData> attributes,
        string type
    )
    {
        // try and extract source from attributes
        foreach (var attribute in attributes)
            switch (attribute.AttributeClass?.ToString())
            {
                case AttributeConstants.EventAttribute:
                    return (ParameterSource.Event, null);

                case AttributeConstants.FromKeyedService:
                    var key = attribute
                        .ConstructorArguments.Where(a => a.Value is not null)
                        .Select(a => a.Value!.ToString())
                        .SingleOrDefault();
                    return (ParameterSource.KeyedService, key);
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
};
