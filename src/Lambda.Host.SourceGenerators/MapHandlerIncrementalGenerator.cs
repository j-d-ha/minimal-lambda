using System.Linq;
using System.Threading;
using Lambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambda.Host.SourceGenerators;

[Generator]
public class MapHandlerIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var startupClassInfo = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "Lambda.Host.StartupHostAttribute",
                IsCandidateClass,
                GetClassToGenerate
            )
            .Where(x => x is not null)
            .Select((x, _) => x!.Value);

        // Get the compilation and check for errors
        var compilationHasErrors = context.CompilationProvider.Select(
            (compilation, cancellationToken) =>
                compilation
                    .GetDiagnostics(cancellationToken)
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Any(d =>
                        d.Location.SourceTree != null
                        && !d.Location.SourceTree.FilePath.Contains(".g.cs")
                    )
        );

        // Find all MapHandler method calls with lambda analysis
        var mapHandlerCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                MapHandlerSyntaxProvider.Predicate,
                MapHandlerSyntaxProvider.Transformer
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // combine the compilation and map handler calls
        var combined = startupClassInfo
            .Combine(compilationHasErrors)
            .Combine(mapHandlerCalls.Collect())
            .Select(
                (t, _) =>
                    new CompilationInfo
                    {
                        MapHandlerInvocationInfos = t.Right,
                        StartupClassInfo = t.Left.Item1,
                        CompilationHasErrors = t.Left.Item2,
                    }
            );

        // Generate source when calls are found
        context.RegisterSourceOutput(combined, MapHandlerSourceOutput.Generate);
    }

    /// <summary>
    /// Fast syntax-based check to filter potential candidates.
    /// This runs before semantic analysis, so it should be lightweight.
    /// </summary>
    private static bool IsCandidateClass(SyntaxNode node, CancellationToken cancellationToken)
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

    /// <summary>
    ///     Performs semantic analysis to validate and extract class information.
    /// </summary>
    private static StartupClassInfo? GetClassToGenerate(
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

        // Check accessibility (public or internal)
        if (
            classSymbol.DeclaredAccessibility != Accessibility.Public
            && classSymbol.DeclaredAccessibility != Accessibility.Internal
        )
            return null;

        // Check if it inherits from the specified abstract base class (direct inheritance only)
        var directBase = classSymbol.BaseType;

        if (directBase == null)
            return null;

        var fullName = directBase.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // return fullName == fullTypeName;
        // bool inheritsFromAbstractBase = InheritsFromType(classSymbol, AbstractBaseClassFullName);

        if (fullName != "global::Lambda.Host.LambdaHost")
            return null;

        // Extract relevant information
        // return new ClassInfo(
        //     Namespace: classSymbol.ContainingNamespace.ToDisplayString(),
        //     ClassName: classSymbol.Name,
        //     IsPublic: classSymbol.DeclaredAccessibility == Accessibility.Public,
        // IsInternal: classSymbol.DeclaredAccessibility == Accessibility.Internal,
        //     IsPartial: true,
        //     BaseClassName: GetDirectBaseClassName(classSymbol)
        // );

        return new StartupClassInfo
        {
            Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
            ClassName = classSymbol.Name,
        };
    }
}
