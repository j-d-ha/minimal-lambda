using System.Runtime.CompilerServices;

namespace AwsLambda.Host.SourceGenerators.UnitTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
