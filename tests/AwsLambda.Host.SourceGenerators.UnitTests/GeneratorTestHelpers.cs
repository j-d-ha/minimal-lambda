using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host.SourceGenerators.UnitTests;

internal static class GeneratorTestHelpers
{
    internal static (GeneratorDriver driver, Compilation compilation) GenerateFromSource(
        string source,
        Dictionary<string, ReportDiagnostic>? diagnosticsToSuppress = null
    )
    {
        var parseOptions = CSharpParseOptions
            .Default.WithLanguageVersion(LanguageVersion.CSharp11)
            .WithFeatures(
                [new KeyValuePair<string, string>("InterceptorsNamespaces", "AwsLambda.Host")]
            );

        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions, path: "InputFile.cs");

        List<MetadataReference> references =
        [
            .. Net80.References.All.ToList(),
            MetadataReference.CreateFromFile(typeof(LambdaApplication).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(FromKeyedServicesAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ILambdaContext).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IHost).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(HostBuilder).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DefaultLambdaJsonSerializer).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(LambdaBootstrapBuilder).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IOptions<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(LambdaOpenTelemetry).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ILambdaApplication).Assembly.Location),
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
        var updatedDriver = driver.RunGenerators(compilation);

        return (updatedDriver, compilation);
    }
}
