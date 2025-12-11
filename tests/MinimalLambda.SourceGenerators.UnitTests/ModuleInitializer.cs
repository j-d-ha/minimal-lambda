using System.Runtime.CompilerServices;

namespace MinimalLambda.SourceGenerators.UnitTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
