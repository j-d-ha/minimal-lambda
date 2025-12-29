using System.Collections.Generic;
using System.Linq;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Extensions;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct ClassInfo(
    string GloballyQualifiedName,
    string ShortName,
    EquatableArray<MethodInfo> ConstructorInfos,
    EquatableArray<string> ImplementedInterfaces,
    string TypeKind,
    bool ImplementsIDisposable = false,
    bool ImplementsIAsyncDisposable = false
);

internal static class ClassInfoExtensions
{
    extension(ClassInfo classInfo)
    {
        internal static DiagnosticResult<ClassInfo> Create(
            INamedTypeSymbol typeSymbol,
            GeneratorContext context
        )
        {
            var typeKind = typeSymbol.GetTypeKind();

            // get the globally qualified name of the class
            var globallyQualifiedName = typeSymbol.ToGloballyQualifiedName();

            // get short name
            var shortName = typeSymbol.Name;

            // handle each instance constructor on the type
            var constructorInfo = ((INamedTypeSymbol)typeSymbol)
                .InstanceConstructors.Select(MethodInfo.Create)
                .ToEquatableArray();

            // get all interfaces
            var interfaceNames = typeSymbol
                .AllInterfaces.Select(i => i.ToGloballyQualifiedName())
                .ToEquatableArray();

            return DiagnosticResult<ClassInfo>.Success(
                new ClassInfo(
                    globallyQualifiedName,
                    shortName,
                    constructorInfo,
                    interfaceNames,
                    typeKind
                )
            );
        }

        internal bool IsInterfaceImplemented(string interfaceName) =>
            classInfo.ImplementedInterfaces.Any(i => i == interfaceName);
    }
}
