using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;

namespace Lambda.Host.Tests.SourceGenerators;

public class MapHandlerIncrementalGeneratorDiagnosticTests
{
    [Fact]
    public void Test_MultipleMapHandlersNotFound()
    {
        var diagnostics = GenerateDiagnostics(
            """
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(() => "hello world");

            await lambda.RunAsync();
            """
        );

        diagnostics.Length.Should().Be(0);
    }

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
        var (driver, _) = GeneratorTestHelpers.GenerateFromSource(source);

        driver.Should().NotBeNull();

        return driver.GetRunResult().Diagnostics;
    }
}
