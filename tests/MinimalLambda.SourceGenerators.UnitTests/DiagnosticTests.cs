using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MinimalLambda.SourceGenerators.UnitTests;

public class MapHandlerIncrementalGeneratorDiagnosticTests
{
    [Fact]
    public void Test_MultipleMapHandlersNotFound()
    {
        var diagnostics = GenerateDiagnostics(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(() => "hello world");

            await lambda.RunAsync();
            """);

        diagnostics.Length.Should().Be(0);
    }

    [Fact]
    public void Test_MultipleMapHandlersFound()
    {
        var diagnostics = GenerateDiagnostics(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(() => "hello world");

            lambda.MapHandler(() => "hello world2");

            await lambda.RunAsync();
            """);

        diagnostics.Length.Should().Be(0);
    }

    [Fact]
    public void Test_MultipleParametersWithRequestAttribute()
    {
        var diagnostics = GenerateDiagnostics(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(([FromEvent] string input1, [FromEvent] string input2) => "hello world");

            await lambda.RunAsync();
            """);

        diagnostics.Length.Should().Be(1);

        foreach (var diagnostic in diagnostics)
        {
            diagnostic.Id.Should().Be("LH0002");
            diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        }
    }

    [Fact]
    public void Test_KeyedService_InvalidKey_Array()
    {
        var diagnostics = GenerateDiagnostics(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var key = new[] { "a", "b" };
            builder.Services.AddKeyedSingleton<IService, Service>(key);

            var lambda = builder.Build();

            lambda.MapHandler(([FromKeyedServices(new[] { "a", "b" })] IService service) => { });

            await lambda.RunAsync();

            public interface IService
            {
                string GetMessage();
            }

            public class Service : IService
            {
                public string GetMessage() => "Hello";
            }
            """);

        diagnostics.Length.Should().Be(1);

        foreach (var diagnostic in diagnostics)
        {
            diagnostic.Id.Should().Be("LH0003");
            diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        }
    }

    [Fact]
    public void Test_CSharpVersionTooLow()
    {
        var diagnostics = GenerateDiagnostics(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(() => "hello world");

            await lambda.RunAsync();
            """,
            LanguageVersion.CSharp10);

        diagnostics.Length.Should().Be(1);
        foreach (var diagnostic in diagnostics)
        {
            diagnostic.Id.Should().Be("LH0004");
            diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        }
    }

    [Fact]
    public void Test_MiddlewareClassHasMoreThanOneMiddlewareConstructorAttribute()
    {
        var diagnostics = GenerateDiagnostics(
            """
            using System.Threading.Tasks;
            using Microsoft.Extensions.Hosting;
            using MinimalLambda;
            using MinimalLambda.Builder;

            var builder = LambdaApplication.CreateBuilder();

            await using var lambda = builder.Build();

            lambda.UseMiddleware<MyLambdaMiddleware>();

            lambda.MapHandler(() => { });

            await lambda.RunAsync();

            internal class MyLambdaMiddleware : ILambdaMiddleware
            {
                [MiddlewareConstructor]
                public MyLambdaMiddleware() { }

                [MiddlewareConstructor]
                public MyLambdaMiddleware(string input) { }

                public async Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
                {
                    await next(context);
                }
            }
            """);

        diagnostics.Length.Should().Be(1);
        foreach (var diagnostic in diagnostics)
        {
            diagnostic.Id.Should().Be("LH0005");
            diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        }
    }

    [Fact]
    public void Test_MiddlewareClassMustNotBeAbstractOrAnInterface()
    {
        var diagnostics = GenerateDiagnostics(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;

            var builder = LambdaApplication.CreateBuilder();

            await using var lambda = builder.Build();

            lambda.UseMiddleware<IMiddleware>();
            lambda.UseMiddleware<Middleware2>();

            interface IMiddleware : ILambdaMiddleware { }

            abstract class Middleware2 : ILambdaMiddleware
            {
                public Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next) =>
                    next(context);
            }
            """);

        diagnostics.Length.Should().Be(2);
        foreach (var diagnostic in diagnostics)
        {
            diagnostic.Id.Should().Be("LH0006");
            diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        }
    }

    private static ImmutableArray<Diagnostic> GenerateDiagnostics(
        string source,
        LanguageVersion languageVersion = LanguageVersion.CSharp11)
    {
        var (driver, _) = GeneratorTestHelpers.GenerateFromSource(
            source,
            languageVersion: languageVersion);

        driver.Should().NotBeNull();

        return driver.GetRunResult().Diagnostics;
    }
}
