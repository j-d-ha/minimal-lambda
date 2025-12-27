using System.Linq;
using Microsoft.CodeAnalysis;

namespace MinimalLambda.SourceGenerators.WellKnownTypes;

internal static class WellKnownTypesExtensions
{
    extension(WellKnownTypes wellKnownTypes)
    {
        internal bool IsTypeMatch(ITypeSymbol type, WellKnownTypeData.WellKnownType wellKnownType)
        {
            var foundType = wellKnownTypes.Get(wellKnownType);
            return type.Equals(foundType, SymbolEqualityComparer.Default);
        }

        internal bool IsAnyTypeMatch(
            ITypeSymbol type,
            params WellKnownTypeData.WellKnownType[] types
        ) =>
            types
                .Select(wellKnownTypes.Get)
                .Any(foundType => type.Equals(foundType, SymbolEqualityComparer.Default));
    }
}
