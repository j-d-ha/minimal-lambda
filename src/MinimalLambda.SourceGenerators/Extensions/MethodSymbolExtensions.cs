using System.Linq;
using MinimalLambda.SourceGenerators;
using MinimalLambda.SourceGenerators.Extensions;
using MinimalLambda.SourceGenerators.WellKnownTypes;

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
            var task = context.WellKnownTypes.Get(
                WellKnownTypeData.WellKnownType.System_Threading_Tasks_Task
            );
            if (returnType.Equals(task, SymbolEqualityComparer.Default))
                return true;

            var taskOfT = context.WellKnownTypes.Get(
                WellKnownTypeData.WellKnownType.System_Threading_Tasks_Task_T
            );
            if (returnType.Equals(taskOfT, SymbolEqualityComparer.Default))
                return true;

            // Check for ValueTask and ValueTask<T>
            var valueTask = context.WellKnownTypes.Get(
                WellKnownTypeData.WellKnownType.System_Threading_Tasks_ValueTask
            );
            if (returnType.Equals(valueTask, SymbolEqualityComparer.Default))
                return true;

            var valueTaskOfT = context.WellKnownTypes.Get(
                WellKnownTypeData.WellKnownType.System_Threading_Tasks_ValueTask_T
            );
            if (returnType.OriginalDefinition.Equals(valueTaskOfT, SymbolEqualityComparer.Default))
                return true;

            // Check for custom awaitable pattern (has GetAwaiter method)
            return returnType
                .GetMembers("GetAwaiter")
                .OfType<IMethodSymbol>()
                .Any(m => m.Parameters.Length == 0 && !m.IsStatic);
        }

        internal bool HasMeaningfulReturnType(GeneratorContext context)
        {
            var returnType = methodSymbol.ReturnType;

            var voidType = context.WellKnownTypes.Get(WellKnownTypeData.WellKnownType.System_Void);
            if (returnType.Equals(voidType, SymbolEqualityComparer.Default))
                return false;

            var task = context.WellKnownTypes.Get(
                WellKnownTypeData.WellKnownType.System_Threading_Tasks_Task
            );
            if (returnType.Equals(task, SymbolEqualityComparer.Default))
                return false;

            var valueTask = context.WellKnownTypes.Get(
                WellKnownTypeData.WellKnownType.System_Threading_Tasks_ValueTask
            );
            if (returnType.Equals(valueTask, SymbolEqualityComparer.Default))
                return false;

            return true;
        }
    }
}
