using System.Runtime.CompilerServices;

namespace Lambda.Host.SourceGenerators.UnitTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
