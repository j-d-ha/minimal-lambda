using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MinimalLambda.SourceGenerators.Extensions;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct ParameterInfo2(string Assignment, string InfoComment);

internal static class ParameterInfo2Extensions
{
    private static Func<string, DiagnosticResult<ParameterInfo2>> Success(string infoComment)
    {
        var info = new ParameterInfo2 { InfoComment = infoComment };
        return assignment =>
            DiagnosticResult<ParameterInfo2>.Success(info with { Assignment = assignment });
    }

    extension(ParameterInfo2)
    {
        internal static DiagnosticResult<ParameterInfo2> CreateForInvocationHandler(
            IParameterSymbol parameter,
            GeneratorContext context
        )
        {
            var stream = context.WellKnownTypes.Get(WellKnownType.System_IO_Stream);
            var lambdaContext = context.WellKnownTypes.Get(
                WellKnownType.Amazon_Lambda_Core_ILambdaContext
            );
            var lambdaInvocationContext = context.WellKnownTypes.Get(
                WellKnownType.MinimalLambda_ILambdaInvocationContext
            );
            var cancellationToken = context.WellKnownTypes.Get(
                WellKnownType.System_Threading_CancellationToken
            );

            var paramType = parameter.Type.ToGloballyQualifiedName();

            var (isEvent, isKeyedServices) = parameter.IsFromEventOrFromKeyedService(
                context,
                out var keyResult
            );

            var success = Success("");

            // event
            if (isEvent)
            {
                // stream event
                if (SymbolEqualityComparer.Default.Equals(parameter.Type, stream))
                    return success(
                        "context.Features.GetRequired<IInvocationDataFeature>().EventStream"
                    );

                // non stream event
                return success($"context.GetRequiredEvent<{paramType}>()");
            }

            // context
            if (
                SymbolEqualityComparer.Default.Equals(parameter.Type, lambdaContext)
                || SymbolEqualityComparer.Default.Equals(parameter.Type, lambdaInvocationContext)
            )
                return success("context");

            // cancellation token
            if (SymbolEqualityComparer.Default.Equals(parameter.Type, cancellationToken))
                return success("context.CancellationToken");

            // keyed services
            if (isKeyedServices)
                return keyResult!.Bind(key =>
                    parameter.IsOptional
                        ? success($"context.ServiceProvider.GetKeyedService<{paramType}>({key})")
                        : success(
                            $"context.ServiceProvider.GetRequiredKeyedService<{paramType}>({key})"
                        )
                );

            return success(
                parameter.IsOptional
                    // default - inject from DI - optional
                    ? $"context.ServiceProvider.GetService<{paramType}>()"
                    // default - inject required from DI
                    : $"context.ServiceProvider.GetRequiredService<{paramType}>()"
            );
        }

        internal static ParameterInfo2 CreateForLifecycleHandler() => new();
    }
}

internal static class MapHandlerExtractors
{
    internal static IEnumerable<(string?, DiagnosticInfo?)> GetParameterAssignments(
        IMethodSymbol methodSymbol,
        GeneratorContext context
    )
    {
        var stream = context.WellKnownTypes.Get(WellKnownType.System_IO_Stream);
        var lambdaContext = context.WellKnownTypes.Get(
            WellKnownType.Amazon_Lambda_Core_ILambdaContext
        );
        var lambdaInvocationContext = context.WellKnownTypes.Get(
            WellKnownType.MinimalLambda_ILambdaInvocationContext
        );
        var cancellationToken = context.WellKnownTypes.Get(
            WellKnownType.System_Threading_CancellationToken
        );

        return methodSymbol.Parameters.Select(
            (string?, DiagnosticInfo?) (parameter) =>
            {
                var paramType = parameter.Type.ToGloballyQualifiedName();

                var (isEvent, isKeyedServices) = parameter.IsFromEventOrFromKeyedService(
                    context,
                    out var keyResult
                );

                // event
                if (isEvent)
                {
                    // stream event
                    if (SymbolEqualityComparer.Default.Equals(parameter.Type, stream))
                        return (
                            "context.Features.GetRequired<IInvocationDataFeature>().EventStream",
                            null
                        );

                    // non stream event
                    return ($"context.GetRequiredEvent<{paramType}>()", null);
                }

                // context
                if (
                    SymbolEqualityComparer.Default.Equals(parameter.Type, lambdaContext)
                    || SymbolEqualityComparer.Default.Equals(
                        parameter.Type,
                        lambdaInvocationContext
                    )
                )
                    return ("context", null);

                // cancellation token
                if (SymbolEqualityComparer.Default.Equals(parameter.Type, cancellationToken))
                    return ("context.CancellationToken", null);

                // keyed services
                if (isKeyedServices)
                {
                    // get key for keyed service
                    if (keyResult?.IsSuccess == false)
                        return (null, keyResult.Error!.Value);

                    // KeyedService - optional
                    if (parameter.IsOptional)
                        return (
                            $"context.ServiceProvider.GetKeyedService<{paramType}>({keyResult?.Value})",
                            null
                        );

                    // KeyedService
                    return (
                        $"context.ServiceProvider.GetRequiredKeyedService<{paramType}>({keyResult?.Value})",
                        null
                    );
                }

                // default - inject from DI - optional
                if (parameter.IsOptional)
                    return ($"context.ServiceProvider.GetService<{paramType}>()", null);

                // default - inject required from DI
                return ($"context.ServiceProvider.GetRequiredService<{paramType}>()", null);
            }
        );
    }

    extension(IParameterSymbol parameterSymbol)
    {
        internal (bool IsFromEvent, bool IsFromKeyedService) IsFromEventOrFromKeyedService(
            GeneratorContext context,
            out DiagnosticResult<string>? keyResult
        )
        {
            keyResult = null;

            var eventAttr = context.WellKnownTypes.Get(
                WellKnownType.MinimalLambda_Builder_EventAttribute
            );
            var fromEventAttr = context.WellKnownTypes.Get(
                WellKnownType.MinimalLambda_Builder_FromEventAttribute
            );
            var fromKeyedServicesAttr = context.WellKnownTypes.Get(
                WellKnownType.Microsoft_Extensions_DependencyInjection_FromKeyedServicesAttribute
            );

            foreach (var attribute in parameterSymbol.GetAttributes())
            {
                if (attribute is null)
                    continue;

                var attrClass = attribute.AttributeClass;

                // check event
                if (SymbolEqualityComparer.Default.Equals(attrClass, eventAttr))
                    return (true, false);

                // check from event
                if (SymbolEqualityComparer.Default.Equals(attrClass, fromEventAttr))
                    return (true, false);

                // check keyed service
                if (SymbolEqualityComparer.Default.Equals(attrClass, fromKeyedServicesAttr))
                {
                    keyResult = attribute.ExtractKeyedServiceKey();
                    return (false, true);
                }
            }

            return (false, false);
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
                    Diagnostics.InvalidAttributeArgument,
                    attributeData.GetAttributeArgumentLocation(0),
                    argument.Type?.ToGloballyQualifiedName()
                );

            return DiagnosticResult<string>.Success(
                argument.Kind switch
                {
                    TypedConstantKind.Primitive when value is string strValue =>
                        SymbolDisplay.FormatLiteral(strValue, true),

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
                    .CreateLocationInfo()
                : null;
    }
}
