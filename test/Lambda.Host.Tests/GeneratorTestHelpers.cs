using Amazon.Lambda.Core;
using AwesomeAssertions;
using Basic.Reference.Assemblies;
using Lambda.Host.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace Lambda.Host.Tests;

internal static class GeneratorTestHelpers
{
    internal static GeneratorDriver GenerateFromSource(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        List<MetadataReference> references =
        [
            .. Net80.References.All,
            MetadataReference.CreateFromFile(typeof(LambdaApplication).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(FromKeyedServicesAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ILambdaContext).Assembly.Location),
        ];

        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: NullableContextOptions.Enable
        );

        var compilation = CSharpCompilation.Create(
            "Tests",
            [syntaxTree],
            references,
            compilationOptions
        );

        var generator = new MapHandlerIncrementalGenerator().AsSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        return driver.RunGenerators(compilation);
    }
}
