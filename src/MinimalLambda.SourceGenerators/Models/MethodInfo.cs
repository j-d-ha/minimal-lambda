using System.Collections.Generic;
using System.Linq;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct MethodInfo(
    int ArgumentCount,
    EquatableArray<AttributeInfo> AttributeInfos,
    EquatableArray<ParameterInfo> Parameters
);

internal static class ConstructorInfoExtensions
{
    extension(MethodInfo)
    {
        internal static MethodInfo Create(IMethodSymbol constructor)
        {
            var attributeInfos = constructor
                .GetAttributes()
                .Where(a => a.AttributeClass is not null)
                .Select(AttributeInfo.Create)
                .ToEquatableArray();

            var parameterInfos = constructor
                .Parameters.Select(ParameterInfo.Create)
                .ToEquatableArray();

            return new MethodInfo(parameterInfos.Count, attributeInfos, parameterInfos);
        }
    }
}

internal readonly record struct AttributeInfo(LocationInfo? LocationInfo, string FullName);

internal static class AttributeInfoExtensions
{
    extension(AttributeInfo)
    {
        internal static AttributeInfo Create(AttributeData attributeData)
        {
            var syntax = attributeData.ApplicationSyntaxReference?.GetSyntax();
            var location = syntax?.GetLocation();
            var locationData = location?.CreateLocationInfo();

            var name = attributeData.AttributeClass?.ToString() ?? "UNKNOWN";

            return new AttributeInfo(locationData, name);
        }
    }
}
