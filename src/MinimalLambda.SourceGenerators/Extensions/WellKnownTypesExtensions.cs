using System.Linq;
using Microsoft.CodeAnalysis;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace MinimalLambda.SourceGenerators.WellKnownTypes;

internal static class WellKnownTypesExtensions
{
    // Open issue w/ extension blocks: https://github.com/dotnet/roslyn/issues/80024
    // ReSharper disable once MoveToExtensionBlock
    internal static bool IsAnyTypeMatch(
        this WellKnownTypes wellKnownTypes,
        ITypeSymbol type,
        params WellKnownType[] types
    ) =>
        types
            .Select(wellKnownTypes.Get)
            .Any(foundType => type.Equals(foundType, SymbolEqualityComparer.Default));

    extension(WellKnownTypes wellKnownTypes)
    {
        internal bool IsTypeMatch(ITypeSymbol type, WellKnownType wellKnownType)
        {
            var foundType = wellKnownTypes.Get(wellKnownType);
            return type.Equals(foundType, SymbolEqualityComparer.Default);
        }

        internal bool TypeImplementsInterface(
            INamedTypeSymbol namedTypeSymbol,
            WellKnownType interfaceType
        ) => namedTypeSymbol.AllInterfaces.Any(i => wellKnownTypes.IsTypeMatch(i, interfaceType));
    }
}
