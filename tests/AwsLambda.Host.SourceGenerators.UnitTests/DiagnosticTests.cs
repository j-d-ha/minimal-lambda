using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;

namespace AwsLambda.Host.SourceGenerators.UnitTests;

public class MapHandlerIncrementalGeneratorDiagnosticTests
{
    [Fact]
    public void Test_MultipleMapHandlersNotFound()
    {
        var diagnostics = GenerateDiagnostics(
            """
            using AwsLambda.Host;
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
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(() => "hello world");

            lambda.MapHandler(() => "hello world2");

            await lambda.RunAsync();
            """
        );

        diagnostics.Length.Should().Be(2);

        foreach (var diagnostic in diagnostics)
        {
            diagnostic.Id.Should().Be("LH0001");
            diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        }
    }

    [Fact]
    public void Test_MultipleParametersWithRequestAttribute()
    {
        var diagnostics = GenerateDiagnostics(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(([Event] string input1, [Event] string input2) => "hello world");

            await lambda.RunAsync();
            """
        );

        diagnostics.Length.Should().Be(2);

        foreach (var diagnostic in diagnostics)
        {
            diagnostic.Id.Should().Be("LH0002");
            diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        }
    }

    private static ImmutableArray<Diagnostic> GenerateDiagnostics(string source)
    {
        var (driver, _) = GeneratorTestHelpers.GenerateFromSource(source);

        driver.Should().NotBeNull();

        return driver.GetRunResult().Diagnostics;
    }
}
