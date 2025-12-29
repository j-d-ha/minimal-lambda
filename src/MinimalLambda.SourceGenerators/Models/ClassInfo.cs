using System.Collections.Generic;
using System.Linq;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Extensions;
using MinimalLambda.SourceGenerators.WellKnownTypes;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace MinimalLambda.SourceGenerators.Models;

internal record ClassInfo(
    string GloballyQualifiedName,
    string ShortName,
    EquatableArray<MethodInfo> ConstructorInfos,
    bool ImplementsDisposable,
    bool ImplementsAsyncDisposable
)
{
    internal string NonNullableGloballyQualifiedName
    {
        get
        {
            field ??= GloballyQualifiedName.EndsWith("?")
                ? GloballyQualifiedName[..^1]
                : GloballyQualifiedName;

            return field;
        }
    }
}

internal static class ClassInfoExtensions
{
    extension(ClassInfo classInfo)
    {
        internal bool AnyParameters => true;

        internal static (ClassInfo? classInfo, List<DiagnosticInfo> DiagnosticInfos) Create(
            INamedTypeSymbol typeSymbol,
            GeneratorContext context
        )
        {
            List<DiagnosticInfo> diagnostics = [];

            // get the globally qualified name of the class
            var globallyQualifiedName = typeSymbol.ToGloballyQualifiedName();

            // get short name, i.e., not qualified
            var shortName = typeSymbol.Name;

            // get constructor
            var (constructor, constructorDiagnostics) = GetConstructor(typeSymbol, context);
            diagnostics.AddRange(constructorDiagnostics);

            // handle each instance constructor on the type
            var constructorInfo = ((INamedTypeSymbol)typeSymbol)
                .InstanceConstructors.Select(MethodInfo.Create)
                .ToEquatableArray();

            // implements IDisposable
            var implementsIDisposable = context.WellKnownTypes.TypeImplementsInterface(
                typeSymbol,
                WellKnownType.System_IDisposable
            );

            // implements IAsyncDisposable
            var implementsIAsyncDisposable = context.WellKnownTypes.TypeImplementsInterface(
                typeSymbol,
                WellKnownType.System_IAsyncDisposable
            );

            return (
                new ClassInfo(
                    globallyQualifiedName,
                    shortName,
                    constructorInfo,
                    implementsIDisposable,
                    implementsIAsyncDisposable
                ),
                diagnostics
            );
        }
    }

    private static (IMethodSymbol? MethodSymbol, DiagnosticInfo[] DiagnosticInfos) GetConstructor(
        INamedTypeSymbol namedTypeSymbol,
        GeneratorContext context
    )
    {
        // 1. Get constructors annotated with `[MiddlewareConstructor]`
        var constructors = namedTypeSymbol
            .InstanceConstructors.Where(c =>
                c.GetAttributes()
                    .Any(a =>
                        a.AttributeClass is not null
                        && context.WellKnownTypes.IsType(
                            a.AttributeClass,
                            [WellKnownType.MinimalLambda_Builder_MiddlewareConstructorAttribute]
                        )
                    )
            )
            .ToArray();

        return constructors.Length switch
        {
            // if more than one found, we will return diagnostics
            > 1 => (
                MethodSymbol: null,
                DiagnosticInfos: constructors
                    .Skip(1)
                    .Select(c =>
                        DiagnosticInfo.Create(
                            Diagnostics.MultipleConstructorsWithAttribute,
                            c.Locations.FirstOrDefault()?.ToLocationInfo(),
                            [AttributeConstants.MiddlewareConstructor]
                        )
                    )
                    .ToArray()
            ),

            // return single constructor that has an `[MiddlewareConstructor]` attribute
            1 => (MethodSymbol: constructors.FirstOrDefault(), DiagnosticInfos: []),

            // 2. default to constructor with most parameters
            _ => (
                MethodSymbol: constructors.OrderByDescending(c => c.Parameters.Length).First(),
                DiagnosticInfos: []
            ),
        };
    }
}
