using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MinimalLambda.SourceGenerators.Extensions;

namespace MinimalLambda.SourceGenerators.Models;

/// <summary>Represents the information associated with a named type in C# source code.</summary>
internal readonly record struct TypeInfo(
    string FullyQualifiedType,
    string? UnwrappedFullyQualifiedType,
    bool IsGeneric,
    ImmutableArray<string> ImplementedInterfaces
);

internal static class TypeInfoExtensions
{
    extension(TypeInfo typeInfo)
    {
        internal static TypeInfo Create(ITypeSymbol typeSymbol, TypeSyntax? syntax = null)
        {
            var fullyQualifiedType = typeSymbol.ToGloballyQualifiedName(syntax);
            var unwrappedFullyQualifiedType = typeSymbol.GetUnwrappedFullyQualifiedType(syntax);
            var isGeneric = typeSymbol is INamedTypeSymbol { IsGenericType: true };
            var implementedInterfaces = typeSymbol
                .AllInterfaces.Select(i => i.ToGloballyQualifiedName())
                .ToImmutableArray();

            return new TypeInfo(
                fullyQualifiedType,
                unwrappedFullyQualifiedType,
                isGeneric,
                implementedInterfaces
            );
        }

        internal static TypeInfo CreateFullyQualifiedType(string fullyQualifiedType) =>
            new(fullyQualifiedType, null, false, ImmutableArray<string>.Empty);
    }

    extension(ITypeSymbol typeSymbol)
    {
        /// <summary>Gets a fully qualified type name without it being wrapped in Task or ValueTask</summary>
        private string? GetUnwrappedFullyQualifiedType(TypeSyntax? syntax = null)
        {
            if (
                typeSymbol is not INamedTypeSymbol namedTypeSymbol
                || (!namedTypeSymbol.IsTask() && !namedTypeSymbol.IsValueTask())
            )
                return typeSymbol.ToGloballyQualifiedName(syntax);

            // if not generic Task or ValueTask, return null as no wrapped return value
            if (!namedTypeSymbol.IsGenericType || namedTypeSymbol.TypeArguments.Length == 0)
                return null;

            return namedTypeSymbol.TypeArguments.First().ToGloballyQualifiedName(syntax);
        }
    }
}
