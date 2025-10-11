using System.Linq;
using System.Threading;
using Lambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambda.Host.SourceGenerators;

internal static class LambdaHostSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        // Quick check: must be a class declaration
        if (node is not ClassDeclarationSyntax classDeclaration)
            return false;

        // Check if it has the partial modifier (required for source generators)
        if (!classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            return false;

        // Check if it's public or internal
        var hasPublicOrInternal = classDeclaration.Modifiers.Any(m =>
            m.IsKind(SyntaxKind.PublicKeyword)
        );

        if (!hasPublicOrInternal)
            return false;

        return true;
    }

    internal static StartupClassInfo? Transformer(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        // The class declaration that has our attribute
        var classDeclaration = (ClassDeclarationSyntax)context.TargetNode;

        // Get the semantic model symbol for the class
        var classSymbol = context.TargetSymbol as INamedTypeSymbol;
        if (classSymbol == null)
            return null;

        // Verify it's partial
        if (!classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            return null;

        // Check accessibility (public)
        if (classSymbol.DeclaredAccessibility != Accessibility.Public)
            return null;

        // Check if it inherits from the specified abstract base class (direct inheritance only)
        var directBase = classSymbol.BaseType;

        if (directBase == null)
            return null;

        var fullName = directBase.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        if (fullName != TypeConstants.LambdaHostedService)
            return null;

        // Extract relevant information
        return new StartupClassInfo
        {
            Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
            ClassName = classSymbol.Name,
            LocationInfo = LocationInfo.CreateFrom(classDeclaration),
        };
    }
}
