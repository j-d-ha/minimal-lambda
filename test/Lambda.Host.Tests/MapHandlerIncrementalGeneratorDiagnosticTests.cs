using System.Collections.Immutable;
using Amazon.Lambda.Core;
using AwesomeAssertions;
using Basic.Reference.Assemblies;
using Lambda.Host.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace Lambda.Host.Tests;

public class MapHandlerIncrementalGeneratorDiagnosticTests
{
    [Fact]
    public void Test_MultipleMapHandlersFound()
    {
        var diagnostics = GenerateDiagnostics(
            """
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(() => "hello world");

            lambda.MapHandler(() => "hello world2");

            await lambda.RunAsync();
            """
        );

        diagnostics.Length.Should().Be(1);
        diagnostics[0].Id.Should().Be("LH0001");
        diagnostics[0].Severity.Should().Be(DiagnosticSeverity.Error);
    }

    private static ImmutableArray<Diagnostic> GenerateDiagnostics(string source)
    {
        var driver = GeneratorTestHelpers.GenerateFromSource(source);

        driver.Should().NotBeNull();

        return driver.GetRunResult().Diagnostics;
    }
}
