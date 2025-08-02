using AwesomeAssertions;
using Lambda.Host.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Hosting;

namespace Lambda.Host.Tests;

public class MapHandlerIncrementalGeneratorVerifyTests
{
    [Fact]
    public async Task Test_Expression_Lambda_No_Input_No_Output()
    {
        const string source = """
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(() => "hello world"));

            await lambda.RunAsync();
            """;

        await Verify(source);
    }

    private static Task Verify(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Create references for assemblies we require
        // We could add multiple references if required
        IEnumerable<PortableExecutableReference> references =
        [
            // MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            // Add your Lambda.Host assembly
            MetadataReference.CreateFromFile(typeof(LambdaApplication).Assembly.Location),
            // Add Microsoft.Extensions.Hosting
            MetadataReference.CreateFromFile(typeof(IHost).Assembly.Location),
            // Add any other assemblies your source generator needs to resolve
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location), // System.Threading.Tasks
            // Add more as needed based on what your generator analyzes
        ];

        var compilation = CSharpCompilation.Create("Tests", [syntaxTree], references); // 👈 pass the references to the compilation

        var generator = new MapHandlerIncrementalGenerator().AsSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);
        driver.Should().NotBeNull();

        var result = driver.GetRunResult();
        result.Diagnostics.Should().BeEmpty();
        // result.GeneratedTrees.Length.Should().Be(4);

        return Verifier.Verify(driver).UseDirectory("Snapshots");
    }
}
