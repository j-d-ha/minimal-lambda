using AwesomeAssertions;
using Microsoft.CodeAnalysis;

namespace Lambda.Host.Tests.SourceGenerators;

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
            using System.Threading.Tasks;
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
            using System.Threading.Tasks;
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
    public async Task Test_BlockLambda_SimpleLambdaTypeCast() =>
        await Verify(
            """
            using System;
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(
                (Func<IService, string>)(
                    service =>
                    {
                        Console.WriteLine("hello world");
                        return service.GetMessage();
                    }
                )
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
    public async Task Test_BlockLambda_LambdaWithNoTypeInfoTypeCast() =>
        await Verify(
            """
            using System;
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(
                (Func<IService, string, string>)(
                    (service, input) =>
                    {
                        Console.WriteLine("hello world");
                        return service.GetMessage();
                    }
                )
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
    public async Task Test_StaticMethodHandler_IdentifierNameTypeCast() =>
        await Verify(
            """
            using System;
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler((Func<Int32>)Handler);

            await lambda.RunAsync();

            static int Handler()
            {
                return 0;
            }
            """
        );

    [Fact]
    public async Task Test_StaticMethodHandler_MemberAccessExpressionTypeCast() =>
        await Verify(
            """
            using System;
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler((Func<Int32>)Handler.Function);

            await lambda.RunAsync();

            public static class Handler
            {
                public static int Function()
                {
                    return 0;
                }
            }
            """
        );

    [Fact]
    public async Task Test_StaticMethodHandler_TypeCast_ExtraParentheses() =>
        await Verify(
            """
            using System;
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler((Func<Int32>)(Handler));

            await lambda.RunAsync();

            static int Handler()
            {
                return 0;
            }
            """
        );

    [Fact]
    public async Task Test_BlockLambda_TypeCast_NoReturn() =>
        await Verify(
            """
            using System;
            using System.Threading.Tasks;
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
    public async Task Test_BlockLambda_ImplicitNullableOutput() =>
        await Verify(
            """
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(
                ([Request] string input, IService service) =>
                {
                    return service.GetMessage();
                }
            );

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

    // // THIS TEST REQUIRES THAT LAMBDA SERIALIZER IS NOT PASSED TO LambdaBootstrapBuilder.Create
    //     [Fact]
    //     public async Task Test_StaticMethodHandler_NoInput_NoOutput() =>
    //         await Verify(
    //             """
    //             using System;
    //             using Lambda.Host;
    //             using Microsoft.Extensions.Hosting;
    //
    //             var builder = LambdaApplication.CreateBuilder();
    //             var lambda = builder.Build();
    //
    //             lambda.MapHandler(Handler);
    //
    //             await lambda.RunAsync();
    //
    //             return;
    //
    //             static async void Handler()
    //             {
    //                 Console.WriteLine("Hello World!");
    //             }
    //             """
    //         );

    // Additional handler type not shown in the examples - generic handlers with complex custom types
    [Fact]
    public async Task Test_ExpressionLambda_ComplexInput_ComplexOutput() =>
        await Verify(
            """
            using System.Threading.Tasks;
            using Amazon.Lambda.Core;
            using Lambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(
                async ([Request] CustomRequest request, IService service, ILambdaContext context) =>
                    new CustomResponse { Result = await service.GetMessage(), Success = true }
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

    [Fact]
    public async Task Test_ExpressionLambda_ReturnAsyncTask() =>
        await Verify(
            """
            using System.Threading.Tasks;
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(async Task () => { });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_ReturnTask() =>
        await Verify(
            """
            using System.Threading.Tasks;
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(Task () =>
            {
                return Task.CompletedTask;
            });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_AsksForCancellationToken() =>
        await Verify(
            """
            using System.Threading;
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler((CancellationToken cancellationToken) => "hello world");

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_AsksForCancellationTokenAndLambdaContext() =>
        await Verify(
            """
            using System.Threading;
            using Amazon.Lambda.Core;
            using Lambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler((CancellationToken ct, ILambdaContext ctx) => "hello world");

            await lambda.RunAsync();
            """
        );

    private static Task Verify(string source)
    {
        var (driver, originalCompilation) = GeneratorTestHelpers.GenerateFromSource(source);

        driver.Should().NotBeNull();

        var result = driver.GetRunResult();

        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Length.Should().Be(1);

        // Add generated trees to original compilation
        var outputCompilation = originalCompilation.AddSyntaxTrees(result.GeneratedTrees);

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

        return Verifier.Verify(driver).UseDirectory("Snapshots").DisableDiff();
    }
}
