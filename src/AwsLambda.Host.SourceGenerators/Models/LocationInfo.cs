using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct LocationInfo(
    string FilePath,
    TextSpan TextSpan,
    LinePositionSpan LineSpan
)
{
    internal Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);

    internal static LocationInfo? CreateFrom(SyntaxNode node) => CreateFrom(node.GetLocation());

    internal static LocationInfo? CreateFrom(ISymbol symbol) =>
        symbol.Locations.FirstOrDefault() is not null and var location
            ? CreateFrom(location)
            : null;

    internal static LocationInfo? CreateFrom(Location location) =>
        location.SourceTree is null
            ? null
            : new LocationInfo(
                location.SourceTree.FilePath,
                location.SourceSpan,
                location.GetLineSpan().Span
            );
}
