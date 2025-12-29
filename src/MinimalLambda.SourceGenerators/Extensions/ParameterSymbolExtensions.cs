using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MinimalLambda.SourceGenerators;
using MinimalLambda.SourceGenerators.Extensions;
using MinimalLambda.SourceGenerators.Models;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace Microsoft.CodeAnalysis;

internal static class ParameterSymbolExtensions
{
    extension(IParameterSymbol parameterSymbol)
    {
        internal bool IsFromEvent(GeneratorContext context)
        {
            var eventAttr = context.WellKnownTypes.Get(
                WellKnownType.MinimalLambda_Builder_EventAttribute
            );
            var fromEventAttr = context.WellKnownTypes.Get(
                WellKnownType.MinimalLambda_Builder_FromEventAttribute
            );

            return parameterSymbol
                .GetAttributes()
                .Any(attribute =>
                {
                    // check event
                    if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, eventAttr))
                        return true;

                    // check from event
                    return SymbolEqualityComparer.Default.Equals(
                        attribute.AttributeClass,
                        fromEventAttr
                    );
                });
        }

        internal bool IsFromKeyedService(
            GeneratorContext context,
            out DiagnosticResult<string>? keyResult
        )
        {
            keyResult = null;

            var fromKeyedServicesAttr = context.WellKnownTypes.Get(
                WellKnownType.Microsoft_Extensions_DependencyInjection_FromKeyedServicesAttribute
            );

            foreach (var attribute in parameterSymbol.GetAttributes())
            {
                if (attribute is null)
                    continue;

                var attrClass = attribute.AttributeClass;

                // check keyed service
                if (!SymbolEqualityComparer.Default.Equals(attrClass, fromKeyedServicesAttr))
                    continue;

                keyResult = attribute.ExtractKeyedServiceKey();
                return true;
            }

            return false;
        }

        internal DiagnosticResult<(string Assignment, string? Key)> GetDiParameterAssignment(
            GeneratorContext context
        )
        {
            var paramType = parameterSymbol.Type.ToGloballyQualifiedName();

            var isKeyedServices = parameterSymbol.IsFromKeyedService(context, out var keyResult);

            var isRequired =
                parameterSymbol.IsOptional
                || parameterSymbol.NullableAnnotation == NullableAnnotation.Annotated;

            // keyed services
            if (isKeyedServices)
                return keyResult!.Bind(key =>
                    DiagnosticResult<(string, string?)>.Success(
                        (
                            isRequired
                                ? $"context.ServiceProvider.GetKeyedService<{paramType}>({key})"
                                : $"context.ServiceProvider.GetRequiredKeyedService<{paramType}>({key})",
                            key
                        )
                    )
                );

            return DiagnosticResult<(string, string?)>.Success(
                (
                    isRequired
                        // default - inject from DI - optional
                        ? $"context.ServiceProvider.GetService<{paramType}>()"
                        // default - inject required from DI
                        : $"context.ServiceProvider.GetRequiredService<{paramType}>()",
                    null
                )
            );
        }
    }

    extension(AttributeData attributeData)
    {
        private DiagnosticResult<string> ExtractKeyedServiceKey()
        {
            var argument = attributeData.ConstructorArguments[0];

            if (argument.IsNull)
                return DiagnosticResult<string>.Success("null");

            object? value = null;
            try
            {
                value = argument.Value;
            }
            catch
            {
                // ignore
            }

            if (value is null)
                return DiagnosticResult<string>.Failure(
                    MinimalLambda.SourceGenerators.Diagnostics.InvalidAttributeArgument,
                    attributeData.GetAttributeArgumentLocation(0),
                    argument.Type?.ToGloballyQualifiedName()
                );

            return DiagnosticResult<string>.Success(
                argument.Kind switch
                {
                    TypedConstantKind.Primitive when value is string strValue =>
                        CSharp.SymbolDisplay.FormatLiteral(strValue, true),

                    TypedConstantKind.Primitive when value is char charValue => $"'{charValue}'",

                    TypedConstantKind.Primitive when value is bool boolValue => boolValue
                        ? "true"
                        : "false",

                    TypedConstantKind.Primitive or TypedConstantKind.Enum =>
                        $"({argument.Type?.ToGloballyQualifiedName()}){value}",

                    TypedConstantKind.Type when value is ITypeSymbol typeValue =>
                        $"typeof({typeValue.ToGloballyQualifiedName()})",

                    _ => value.ToString(),
                }
            );
        }

        private LocationInfo? GetAttributeArgumentLocation(int index) =>
            attributeData.ApplicationSyntaxReference?.GetSyntax()
                is AttributeSyntax { ArgumentList: { } argumentList }
                ? argumentList
                    .Arguments.ElementAtOrDefault(index)
                    ?.Expression.GetLocation()
                    .ToLocationInfo()
                : null;
    }
}
