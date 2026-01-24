using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators.Emitters;

internal static class LifecycleHandlerEmitter
{
    private const string GenericHandlerTemplateFile = "Templates/GenericHandler.scriban";

    internal static void Emit(
        SourceProductionContext context,
        ImmutableArray<LifecycleMethodInfo> infos)
    {
        if (infos.Length == 0)
            return;

        var name = infos.First().MethodType;

        var code = TemplateHelper.Render(
            GenericHandlerTemplateFile,
            new { TemplateHelper.GeneratedCodeAttribute, Name = name, Calls = infos });

        context.AddSource($"MinimalLambda.{name}Handlers.g.cs", code);
    }
}
