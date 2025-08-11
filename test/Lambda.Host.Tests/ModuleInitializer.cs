using System.Runtime.CompilerServices;

namespace Lambda.Host.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
