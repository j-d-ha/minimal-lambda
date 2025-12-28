using System.Linq;
using LayeredCraft.SourceGeneratorTools.Types;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class GenericHandlerSources
{
    /// <summary>
    ///     Generates C# code for a generic handler. The handler is a wrapper around the actual
    ///     handler. The return type of the wrapper is a <c>Task</c> or <c>Task&lt;T&gt;</c> depending on
    ///     the return type of the actual handler.
    /// </summary>
    internal static string Generate(EquatableArray<LifecycleMethodInfo> lifecycleMethodInfos) =>
        TemplateHelper.Render(
            GeneratorConstants.GenericHandlerTemplateFile,
            new
            {
                Name = lifecycleMethodInfos.First().MethodType,
                Calls = lifecycleMethodInfos,
                MinimalLambdaEmitter.GeneratedCodeAttribute,
            }
        );
}
