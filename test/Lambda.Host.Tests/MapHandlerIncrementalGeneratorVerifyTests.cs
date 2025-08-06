using System.Reflection;
using Amazon.Lambda.Core;
using AwesomeAssertions;
using Basic.Reference.Assemblies;
using Lambda.Host.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpSourceGeneratorTest<
    Lambda.Host.SourceGenerators.MapHandlerIncrementalGenerator,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace Lambda.Host.Tests;

public class MapHandlerIncrementalGeneratorVerifyTests
{
    [Fact]
    public async Task Test_ExpressionLambda_NoInputReturningString() =>
        await Verify(
            """
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(() => "hello world");

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_DiAndAsync() =>
        await Verify(
            """
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();

            var lambda = builder.Build();

            lambda.MapHandler(
                async ([Request] string input, IService service) => service.GetMessage().ToUpper()
            );

            await lambda.RunAsync();

            public interface IService
            {
                string GetMessage();
            }

            public class Service : IService
            {
                public string GetMessage() => "hello world";
            }
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_DiAndAsyncAndAwait() =>
        await Verify(
            """
            using System.Threading.Tasks;
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();

            var lambda = builder.Build();

            lambda.MapHandler(
                async ([Request] string input, IService service) => (await service.GetMessage()).ToUpper()
            );

            await lambda.RunAsync();

            public interface IService
            {
                Task<string> GetMessage();
            }

            public class Service : IService
            {
                public Task<string> GetMessage() => Task.FromResult("hello world");
            }
            """
        );

    [Fact]
    public async Task Test_StaticMethodHandler_BlockBodyWithKeyedServices() =>
        await Verify(
            """
            using Amazon.Lambda.Core;
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            builder.Services.AddKeyedSingleton<IService>("key", (sp, _) => sp.GetRequiredService<IService>());
            var lambda = builder.Build();

            lambda.MapHandler(HandlerFactory.Handler);

            await lambda.RunAsync();

            public interface IService
            {
                string GetMessage();
            }

            public class Service : IService
            {
                public string GetMessage() => "hello world";
            }

            public static class HandlerFactory
            {
                public static string Handler(
                    [Request] string input,
                    ILambdaContext context,
                    [FromKeyedServices("key")] IService service
                )
                {
                    return service.GetMessage();
                }
            }
            """
        );

    [Fact]
    public async Task Test_BlockLambda_ExplicitReturnType() =>
        await Verify(
            """
            using Amazon.Lambda.Core;
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(
                IService ([Request] string input, IService service) =>
                {
                    if (input == "other")
                    {
                        return new Service();
                    }

                    return new OtherService();
                }
            );

            await lambda.RunAsync();

            public interface IService
            {
                Task<string> GetMessage();
            }

            public class Service : IService
            {
                public Task<string> GetMessage() => Task.FromResult("hello world");
            }

            public class OtherService : IService
            {
                public Task<string> GetMessage() => Task.FromResult("hello world");
            }
            """
        );

    [Fact]
    public async Task Test_BlockLambda_ReturningString() =>
        await Verify(
            """
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(
                ([Request] string input) =>
                {
                    return input;
                }
            );

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_ExplicitReturnType() =>
        await Verify(
            """
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(
                IService ([Request] string input) => input == "other" ? new Service() : new OtherService()
            );

            await lambda.RunAsync();

            public interface IService
            {
                Task<string> GetMessage();
            }

            public class Service : IService
            {
                public Task<string> GetMessage() => Task.FromResult("hello world");
            }

            public class OtherService : IService
            {
                public Task<string> GetMessage() => Task.FromResult("hello world");
            }

            """
        );

    [Fact]
    public async Task Test_BlockLambda_TypeCast() =>
        await Verify(
            """
            using System;
            using System.Threading.Tasks;
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            builder.Services.AddKeyedSingleton<IService>("key", (sp, _) => sp.GetRequiredService<IService>());
            var lambda = builder.Build();

            lambda.MapHandler(
                (Func<IService, string>)(
                    ([FromKeyedServices("key")] IService service) =>
                    {
                        Console.WriteLine("hello world");
                        return "hello world";
                    }
                )
            );

            await lambda.RunAsync();

            public interface IService
            {
                Task<string> GetMessage();
            }

            public class Service : IService
            {
                public Task<string> GetMessage() => Task.FromResult("hello world");
            }
            """
        );

    [Fact]
    public async Task Test_BlockLambda_TypeCast_NoReturn() =>
        await Verify(
            """
            using System;
            using Amazon.Lambda.Core;
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(
                (Action<string, IService>)(
                    ([Request] string input, IService service) =>
                    {
                        Console.WriteLine("hello world");
                    }
                )
            );

            await lambda.RunAsync();

            public interface IService
            {
                Task<string> GetMessage();
            }

            public class Service : IService
            {
                public Task<string> GetMessage() => Task.FromResult("hello world");
            }
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_NullableInput_ImplicitNullableOutput() =>
        await Verify(
            """
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(([Request] string? input, IService service) => service.GetMessage());

            await lambda.RunAsync();

            public interface IService
            {
                string? GetMessage();
            }

            public class Service : IService
            {
                public string? GetMessage() => "hello world";
            }
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_NullableInput_ExplicitNullableOutput() =>
        await Verify(
            """
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(string? ([Request] string? input, IService service) => service.GetMessage());

            await lambda.RunAsync();

            public interface IService
            {
                string? GetMessage();
            }

            public class Service : IService
            {
                public string? GetMessage() => "hello world";
            }
            """
        );

    [Fact]
    public async Task Test_Simple_Nullable_Input_Lambda() =>
        await Verify(
            """
            using Amazon.Lambda.Core;
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(([Request] string? input) => "hello world");

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_Static_Void_Method_Handler() =>
        await Verify(
            """
            using Amazon.Lambda.Core;
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            builder.Services.AddKeyedSingleton<IService>("key", (sp, _) => sp.GetRequiredService<IService>());
            var lambda = builder.Build();

            lambda.MapHandler(Handler);

            await lambda.RunAsync();

            static async void Handler(
                [Request] string input,
                ILambdaContext context,
                [FromKeyedServices("key")] IService service
            ) { }

            public interface IService
            {
                Task<string> GetMessage();
            }

            public class Service : IService
            {
                public Task<string> GetMessage() => Task.FromResult("hello world");
            }
            """
        );

    // Additional handler type not shown in the examples - generic handlers with complex custom types
    [Fact]
    public async Task Test_Generic_Handler_Complex_Types() =>
        await Verify(
            """
            using Amazon.Lambda.Core;
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler<CustomRequest, CustomResponse>(
                async ([Request] CustomRequest request, IService service) => 
                new CustomResponse 
                { 
                    Result = await service.GetMessage(), 
                    Success = true 
                }
            );

            await lambda.RunAsync();

            public class CustomRequest
            {
                public string Name { get; set; } = string.Empty;
                public int Value { get; set; }
            }

            public class CustomResponse
            {
                public string Result { get; set; } = string.Empty;
                public bool Success { get; set; }
            }

            public interface IService
            {
                Task<string> GetMessage();
            }

            public class Service : IService
            {
                public Task<string> GetMessage() => Task.FromResult("hello world");
            }
            """
        );

    private static Task Verify(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        List<MetadataReference> references =
        [
            .. Net80.References.All,
            MetadataReference.CreateFromFile(typeof(LambdaApplication).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(FromKeyedServicesAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ILambdaContext).Assembly.Location),
        ];

        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        var compilation = CSharpCompilation.Create(
            "Tests",
            [syntaxTree],
            references,
            compilationOptions
        );

        var generator = new MapHandlerIncrementalGenerator().AsSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);
        driver.Should().NotBeNull();

        var result = driver.GetRunResult();
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Length.Should().Be(1);

        return Verifier.Verify(driver).UseDirectory("Snapshots").DisableDiff();
    }
}
