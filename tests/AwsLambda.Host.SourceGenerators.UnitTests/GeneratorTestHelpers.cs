using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using AwesomeAssertions;
using AwsLambda.Host.Builder;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host.SourceGenerators.UnitTests;

internal static class GeneratorTestHelpers
{
    internal static Task Verify(string source, int expectedTrees = 1)
    {
        var (driver, originalCompilation) = GenerateFromSource(source);

        driver.Should().NotBeNull();

        var result = driver.GetRunResult();

        // result.Diagnostics.Length.Should().Be(0);s

        // Reparse generated trees with the same parse options as the original compilation
        // to ensure consistent syntax tree features (e.g., InterceptorsNamespaces)
        var parseOptions = originalCompilation.SyntaxTrees.First().Options;
        var reparsedTrees = result
            .GeneratedTrees.Select(tree =>
                CSharpSyntaxTree.ParseText(tree.GetText(), (CSharpParseOptions)parseOptions)
            )
            .ToArray();

        // Add generated trees to original compilation
        var outputCompilation = originalCompilation.AddSyntaxTrees(reparsedTrees);

        var errors = outputCompilation
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        errors
            .Should()
            .BeEmpty(
                "generated code should compile without errors, but found:\n"
                    + string.Join(
                        "\n",
                        errors.Select(e => $"  - {e.Id}: {e.GetMessage()} at {e.Location}")
                    )
            );

        result.GeneratedTrees.Length.Should().Be(expectedTrees);

        return Verifier.Verify(driver).UseDirectory("Snapshots").DisableDiff();
    }

    internal static (GeneratorDriver driver, Compilation compilation) GenerateFromSource(
        string source,
        Dictionary<string, ReportDiagnostic>? diagnosticsToSuppress = null,
        LanguageVersion languageVersion = LanguageVersion.CSharp14
    )
    {
        IEnumerable<KeyValuePair<string, string>> features =
        [
            new("InterceptorsNamespaces", "AwsLambda.Host"),
        ];

        var parseOptions = CSharpParseOptions
            .Default.WithLanguageVersion(languageVersion)
            .WithFeatures(features);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions, "InputFile.cs");

        List<MetadataReference> references =
        [
            .. Net90.References.All.ToList(),
            MetadataReference.CreateFromFile(typeof(LambdaApplication).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(FromKeyedServicesAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ILambdaContext).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IHost).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(HostBuilder).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DefaultLambdaJsonSerializer).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(LambdaBootstrapBuilder).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IOptions<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ILambdaHostContext).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(APIGatewayProxyResponse).Assembly.Location),
            MetadataReference.CreateFromFile(
                typeof(LambdaOpenTelemetryServiceProviderExtensions).Assembly.Location
            ),
        ];

        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.ConsoleApplication,
            nullableContextOptions: NullableContextOptions.Enable
        );

        if (diagnosticsToSuppress is not null)
            compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                diagnosticsToSuppress
            );

        var compilation = CSharpCompilation.Create(
            "Tests",
            [syntaxTree],
            references,
            compilationOptions
        );

        var generator = new MapHandlerIncrementalGenerator().AsSourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        var updatedDriver = driver.RunGenerators(compilation, CancellationToken.None);

        return (updatedDriver, compilation);
    }
}
