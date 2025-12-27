using System.Linq;
using MinimalLambda.SourceGenerators;
using MinimalLambda.SourceGenerators.Extensions;
using MinimalLambda.SourceGenerators.WellKnownTypes;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace Microsoft.CodeAnalysis;

internal static class MethodSymbolExtensions
{
    extension(IMethodSymbol methodSymbol)
    {
        internal string GetCastableSignature()
        {
            var returnType = methodSymbol.ReturnType.ToGloballyQualifiedName();
            var parameters = methodSymbol
                .Parameters.Select(
                    (p, i) =>
                    {
                        var type = p.Type.ToGloballyQualifiedName();
                        var defaultValue = p.IsOptional ? " = default" : "";
                        return $"{type} arg{i}{defaultValue}";
                    }
                )
                .ToArray();
            var parameterList = string.Join(", ", parameters);

            return $"{returnType} ({parameterList}) => throw null!";
        }

        internal bool IsAwaitable(GeneratorContext context)
        {
            var returnType = methodSymbol.ReturnType;

            // Check for Task and Task<T>
            var task = context.WellKnownTypes.Get(WellKnownType.System_Threading_Tasks_Task);
            if (returnType.Equals(task, SymbolEqualityComparer.Default))
                return true;

            var taskOfT = context.WellKnownTypes.Get(WellKnownType.System_Threading_Tasks_Task_T);
            if (returnType.Equals(taskOfT, SymbolEqualityComparer.Default))
                return true;

            // Check for ValueTask and ValueTask<T>
            var valueTask = context.WellKnownTypes.Get(
                WellKnownType.System_Threading_Tasks_ValueTask
            );
            if (returnType.Equals(valueTask, SymbolEqualityComparer.Default))
                return true;

            var valueTaskOfT = context.WellKnownTypes.Get(
                WellKnownType.System_Threading_Tasks_ValueTask_T
            );
            if (returnType.OriginalDefinition.Equals(valueTaskOfT, SymbolEqualityComparer.Default))
                return true;

            // Check for custom awaitable pattern (has GetAwaiter method)
            return returnType
                .GetMembers("GetAwaiter")
                .OfType<IMethodSymbol>()
                .Any(m => m.Parameters.Length == 0 && !m.IsStatic);
        }

        internal bool HasMeaningfulReturnType(
            GeneratorContext context,
            out ITypeSymbol unwrappedReturnType
        )
        {
            var returnType = methodSymbol.ReturnType;

            if (IsVoidLike(returnType))
            {
                unwrappedReturnType = returnType;
                return false;
            }

            unwrappedReturnType = methodSymbol.UnwrapReturnType(context);
            return true;

            bool IsVoidLike(ITypeSymbol type) =>
                context.WellKnownTypes.IsAnyTypeMatch(
                    type,
                    WellKnownType.System_Void,
                    WellKnownType.System_Threading_Tasks_Task,
                    WellKnownType.System_Threading_Tasks_ValueTask
                );
        }

        private ITypeSymbol UnwrapReturnType(GeneratorContext context)
        {
            if (methodSymbol.ReturnType is not INamedTypeSymbol namedReturnType)
                return (INamedTypeSymbol)methodSymbol.ReturnType;

            var taskOfT = context.WellKnownTypes.Get(WellKnownType.System_Threading_Tasks_Task_T);
            var valueTaskOfT = context.WellKnownTypes.Get(
                WellKnownType.System_Threading_Tasks_ValueTask_T
            );

            var originalDef = namedReturnType.OriginalDefinition;

            if (
                (
                    originalDef.Equals(taskOfT, SymbolEqualityComparer.Default)
                    || originalDef.Equals(valueTaskOfT, SymbolEqualityComparer.Default)
                )
                && namedReturnType.TypeArguments.Length > 0
            )
                return namedReturnType.TypeArguments[0];

            return namedReturnType;
        }
    }
}
