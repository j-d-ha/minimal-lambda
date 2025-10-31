using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AwsLambda.Host.SourceGenerators.UnitTests;

public class VerifyTests
{
    [Fact]
    public async Task Test_ExpressionLambda_NoInputNoOutput() =>
        await Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(() => { });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_NoInputReturningString() =>
        await Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(() => "hello world");

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_NoInputReturningNullablePrimitive() =>
        await Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(int? () => 1);

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_NoInputReturningGenericObject() =>
        await Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(Response<Data> () => new Response<Data>(new Data("hello world")));

            await lambda.RunAsync();

            record Response<T>(T Data);

            record Data(string Message);
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_DiAndAsync() =>
        await Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();

            var lambda = builder.Build();

            lambda.MapHandler(
                async ([Event] string input, IService service) => service.GetMessage().ToUpper()
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
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();

            var lambda = builder.Build();

            lambda.MapHandler(
                async ([Event] string input, IService service) => (await service.GetMessage()).ToUpper()
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
    public async Task Test_ExpressionLambda_DiAndAsyncAndAwait_CustomEventAndResponseTypes() =>
        await Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;
            using MyNamespace;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();

            var lambda = builder.Build();

            lambda.MapHandler(
                async ([Event] Event input, IService service) =>
                    new Response((await service.GetMessage()).ToUpper())
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

            namespace MyNamespace
            {
                public record Event(string Input);

                public record Response(string Message);
            }
            """
        );

    [Fact]
    public async Task Test_StaticMethodHandler_BlockBodyWithKeyedServices() =>
        await Verify(
            """
            using Amazon.Lambda.Core;
            using AwsLambda.Host;
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
                    [Event] string input,
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
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(
                IService ([Event] string input, IService service) =>
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
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(
                ([Event] string input) =>
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
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(
                IService ([Event] string input) => input == "other" ? new Service() : new OtherService()
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
            using AwsLambda.Host;
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
    public async Task Test_BlockLambda_TypeCast_ReturnsTask() =>
        await Verify(
            """
            using System;
            using System.Threading.Tasks;
            using AwsLambda.Host;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(
                (Func<Task>)(
                    () =>
                    {
                        return Task.CompletedTask;
                    }
                )
            );
            """
        );

    [Fact]
    public async Task Test_BlockLambda_TypeCast_AsyncReturnsTaskWithString() =>
        await Verify(
            """
            using System;
            using System.Threading.Tasks;
            using AwsLambda.Host;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler((Func<Task<string>>)(async () => "hello world"));
            """
        );

    [Fact]
    public async Task Test_BlockLambda_SimpleLambdaTypeCast() =>
        await Verify(
            """
            using System;
            using AwsLambda.Host;
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
            using AwsLambda.Host;
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
            using AwsLambda.Host;
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
            using AwsLambda.Host;
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
            using AwsLambda.Host;
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
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(
                (Action<string, IService>)(
                    ([Event] string input, IService service) =>
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
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(([Event] string? input, IService service) => service.GetMessage());

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
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(string? ([Event] int? input, IService service) => service.GetMessage());

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
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(
                ([Event] string input, IService service) =>
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

    [Fact]
    public async Task Test_StaticMethodHandler_NoInput_NoOutput() =>
        await Verify(
            """
            using System;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(Handler);

            await lambda.RunAsync();

            return;

            static void Handler()
            {
                Console.WriteLine("Hello World!");
            }
            """
        );

    [Fact]
    public async Task Test_StaticMethodHandler_ReturnTask() =>
        await Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(Handler);

            await lambda.RunAsync();

            return;

            static Task Handler()
            {
                return Task.CompletedTask;
            }
            """
        );

    [Fact]
    public async Task Test_StaticMethodHandler_ReturnTaskString() =>
        await Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(Handler);

            await lambda.RunAsync();

            return;

            static Task<string> Handler()
            {
                return Task.FromResult("Hello World!");
            }
            """
        );

    [Fact]
    public async Task Test_StaticMethodHandler_ReturnAsyncTaskString() =>
        await Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(Handler);

            await lambda.RunAsync();

            return;

            static async Task<string> Handler()
            {
                return "Hello World!";
            }
            """
        );

    [Fact]
    public async Task Test_StaticMethodHandler_AsyncVoid() =>
        await Verify(
            """
            using System;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(Handler);

            await lambda.RunAsync();

            return;

            static async void Handler()
            {
                Console.WriteLine("Hello World!");
            }
            """
        );

    // Additional handler type not shown in the examples - generic handlers with complex custom types
    [Fact]
    public async Task Test_ExpressionLambda_ComplexInput_ComplexOutput() =>
        await Verify(
            """
            using System.Threading.Tasks;
            using Amazon.Lambda.Core;
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();
            var lambda = builder.Build();

            lambda.MapHandler(
                async ([Event] CustomRequest request, IService service, ILambdaContext context) =>
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
            using AwsLambda.Host;
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
            using AwsLambda.Host;
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
            using AwsLambda.Host;
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
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler((CancellationToken ct, ILambdaContext ctx) => "hello world");

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_AsksForCancellationTokenAndLambdaHostContext() =>
        await Verify(
            """
            using System.Threading;
            using Amazon.Lambda.Core;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler((CancellationToken ct, ILambdaHostContext ctx) => "hello world");

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_StreamRequest() =>
        await Verify(
            """
            using System.IO;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(([Event] Stream input) => { });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_BlockLambda_StreamResponse() =>
        await Verify(
            """
            using System.IO;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(Stream () => new FileStream("hello.txt", FileMode.Open));

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OtelEnabled_EventAndResponse() =>
        await Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.UseOpenTelemetryTracing();

            lambda.MapHandler(([Event] Request request) => new Response($"Hello {request.Name}!"));

            await lambda.RunAsync();

            record Request(string Name);

            record Response(string Message);
            """
        );

    [Fact]
    public async Task Test_OtelEnabled_OnlyResponse() =>
        await Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.UseOpenTelemetryTracing();

            lambda.MapHandler(() => new Response("Hello world!"));

            await lambda.RunAsync();

            record Response(string Message);
            """
        );

    [Fact]
    public async Task Test_OtelEnabled_OnlyEvent() =>
        await Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.UseOpenTelemetryTracing();

            lambda.MapHandler(([Event] Request request) => { });

            await lambda.RunAsync();

            record Request(string Name);
            """
        );

    [Fact]
    public async Task Test_OtelEnabled_NoEventNoResponse() =>
        await Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.UseOpenTelemetryTracing();

            lambda.MapHandler(() => { });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OtelEnabled_ExpressionLambda_DiAndAsyncAndAwait() =>
        await Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddSingleton<IService, Service>();

            var lambda = builder.Build();

            lambda.UseOpenTelemetryTracing();

            lambda.MapHandler(
                async ([Event] string input, IService service) => (await service.GetMessage()).ToUpper()
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

    private static Task Verify(string source)
    {
        var (driver, originalCompilation) = GeneratorTestHelpers.GenerateFromSource(source);

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

        result.GeneratedTrees.Length.Should().Be(1);

        return Verifier.Verify(driver).UseDirectory("Snapshots").DisableDiff();
    }
}
