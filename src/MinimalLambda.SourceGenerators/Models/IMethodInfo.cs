using LayeredCraft.SourceGeneratorTools.Types;

namespace MinimalLambda.SourceGenerators.Models;

internal interface IMethodInfo
{
    EquatableArray<DiagnosticInfo> DiagnosticInfos { get; }
    MethodType MethodType { get; }
}
