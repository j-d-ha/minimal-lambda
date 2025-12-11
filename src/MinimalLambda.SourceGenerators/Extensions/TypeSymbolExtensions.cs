using Microsoft.CodeAnalysis;

namespace MinimalLambda.SourceGenerators;

internal static class TypeSymbolExtensions
{
    extension(ITypeSymbol typeSymbol)
    {
        internal bool IsTypeAwaitable() => typeSymbol.IsTask() || typeSymbol.IsValueTask();

        internal bool IsTask() =>
            typeSymbol.Name == "Task"
            && typeSymbol.ContainingNamespace?.ToDisplayString() == "System.Threading.Tasks";

        internal bool IsValueTask() =>
            typeSymbol.Name == "ValueTask"
            && typeSymbol.ContainingNamespace?.ToDisplayString() == "System.Threading.Tasks";
    }
}
