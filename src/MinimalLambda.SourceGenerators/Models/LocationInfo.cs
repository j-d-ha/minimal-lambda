using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct LocationInfo(
    string FilePath,
    TextSpan TextSpan,
    LinePositionSpan LineSpan
);

internal static class LocationInfoExtensions
{
    extension(LocationInfo locationInfo)
    {
        internal Location ToLocation() =>
            Location.Create(locationInfo.FilePath, locationInfo.TextSpan, locationInfo.LineSpan);

        internal static LocationInfo? Create(Location? location) =>
            location?.SourceTree is null
                ? null
                : new LocationInfo(
                    location.SourceTree.FilePath,
                    location.SourceSpan,
                    location.GetLineSpan().Span
                );

        internal static LocationInfo? Create(ISymbol symbol) =>
            LocationInfo.Create(symbol.Locations.FirstOrDefault());

        internal static LocationInfo? Create(SyntaxNode syntaxNode) =>
            LocationInfo.Create(syntaxNode.GetLocation());
    }

    extension(Location location)
    {
        internal LocationInfo? CreateLocationInfo() =>
            location.SourceTree is null
                ? null
                : new LocationInfo(
                    location.SourceTree.FilePath,
                    location.SourceSpan,
                    location.GetLineSpan().Span
                );
    }

    extension(ISymbol symbol)
    {
        internal LocationInfo? CreateLocationInfo() =>
            symbol.Locations.FirstOrDefault()?.CreateLocationInfo();
    }

    extension(SyntaxNode syntaxNode)
    {
        internal LocationInfo? CreateLocationInfo() =>
            syntaxNode.GetLocation().CreateLocationInfo();
    }
}
