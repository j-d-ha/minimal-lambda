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

        internal string GetTypeKind()
        {
            // Check if it's an interface
            if (typeSymbol.TypeKind == TypeKind.Interface)
                return "interface";

            // Check if it's a class
            if (typeSymbol.TypeKind == TypeKind.Class)
            {
                // Check if it's abstract
                if (typeSymbol.IsAbstract && typeSymbol.IsSealed)
                    return "static class";

                if (typeSymbol.IsAbstract)
                    return "abstract class";

                if (typeSymbol.IsSealed)
                    return "sealed class";

                if (typeSymbol.IsRecord)
                    return "record class";

                return "class";
            }

            // Check if it's a struct
            if (typeSymbol.TypeKind == TypeKind.Struct)
            {
                if (typeSymbol.IsRecord)
                    return "record struct";

                if (typeSymbol.IsReadOnly)
                    return "readonly struct";

                if (typeSymbol.IsRefLikeType)
                    return "ref struct";

                return "struct";
            }

            // Check if it's an enum
            if (typeSymbol.TypeKind == TypeKind.Enum)
                return "enum";

            // Check if it's a delegate
            if (typeSymbol.TypeKind == TypeKind.Delegate)
                return "delegate";

            // Other types
            return typeSymbol.TypeKind.ToString().ToLower();
        }
    }
}
