using System;
using System.Collections.Generic;
using System.Linq;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace MinimalLambda.SourceGenerators.Models;

internal record UseMiddlewareTInfo(
    string? InterceptableLocationAttribute,
    ClassInfo? ClassInfo,
    EquatableArray<DiagnosticInfo> DiagnosticInfos
);

internal static class UseMiddlewareTInfoExtensions
{
    extension(UseMiddlewareTInfo)
    {
        internal static UseMiddlewareTInfo Create(
            IInvocationOperation invocationOperation,
            GeneratorContext context
        )
        {
            if (
                invocationOperation.Syntax
                is not InvocationExpressionSyntax invocationExpressionSyntax
            )
                throw new InvalidOperationException("Syntax is not InvocationExpressionSyntax");

            List<DiagnosticInfo> diagnosticInfos = [];

            TryGetLocationInfo(invocationExpressionSyntax, out var locationInfo);

            var interceptableLocation = (
                context.SemanticModel.GetInterceptableLocation(
                    invocationExpressionSyntax,
                    context.CancellationToken
                ) ?? throw new InvalidOperationException("Interceptable location is null")
            )
                .ToInterceptableLocationInfo()
                .ToInterceptsLocationAttribute();

            var middlewareClassType = invocationOperation
                .TargetMethod.TypeArguments.FirstOrDefault()
                .Map(typeSymbol =>
                    typeSymbol as INamedTypeSymbol
                    ?? throw new InvalidOperationException(
                        "Middleware class type is not INamedTypeSymbol"
                    )
                );

            var (classInfo, diagnostics) = ClassInfo.Create(middlewareClassType, context);
            diagnosticInfos.AddRange(diagnostics);

            return new UseMiddlewareTInfo(
                interceptableLocation,
                classInfo,
                diagnosticInfos.ToEquatableArray()
            );
        }
    }

    private static bool TryGetLocationInfo(
        InvocationExpressionSyntax invocationExpressionSyntax,
        out LocationInfo? locationInfo
    )
    {
        locationInfo = null;
        if (
            invocationExpressionSyntax is
            { Expression: MemberAccessExpressionSyntax { Name: GenericNameSyntax genericName } }
        )
        {
            var typeArgument = genericName.TypeArgumentList.Arguments[0];
            locationInfo = typeArgument.GetLocation().ToLocationInfo();
            return true;
        }

        return false;
    }
}
