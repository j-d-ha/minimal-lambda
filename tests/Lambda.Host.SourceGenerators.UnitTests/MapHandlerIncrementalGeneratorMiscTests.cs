using AwesomeAssertions;

namespace Lambda.Host.SourceGenerators.UnitTests;

public class MapHandlerIncrementalGeneratorMiscTests
{
    [Fact]
    public void Test_CompilationHasErrors_ExpectNoOutput()
    {
        var code = """
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(([Request] s input) => "hello world");

            await lambda.RunAsync();
            """;

        var (driver, _) = GeneratorTestHelpers.GenerateFromSource(code);

        driver.Should().NotBeNull();

        var result = driver.GetRunResult();

        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Length.Should().Be(0);
    }
}
