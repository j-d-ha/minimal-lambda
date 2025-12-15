namespace MinimalLambda.SourceGenerators.UnitTests;

public class MethodHandlerVerifyTests
{
    [Fact]
    public async Task Test_MethodHandler_NoInput_NoOutput() =>
        await GeneratorTestHelpers.Verify(
            """
            using System;
            using MinimalLambda;
            using MinimalLambda.Builder;
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
    public async Task Test_MethodHandler_BlockBody_InputDiKeyedServices() =>
        await GeneratorTestHelpers.Verify(
            """
            using Amazon.Lambda.Core;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(HandlerFactory.Handler);

            await lambda.RunAsync();

            public interface IService
            {
                string GetMessage();
            }

            public static class HandlerFactory
            {
                public static string Handler(
                    [FromEvent] string input,
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
    public async Task Test_MethodHandler_BlockBody_TypeCast_Static() =>
        await GeneratorTestHelpers.Verify(
            """
            using System;
            using MinimalLambda;
            using MinimalLambda.Builder;
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
    public async Task Test_MethodHandler_BlockBody_TypeCast() =>
        await GeneratorTestHelpers.Verify(
            """
            using System;
            using MinimalLambda;
            using MinimalLambda.Builder;
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
    public async Task Test_MethodHandler_TypeCast_ExtraParentheses() =>
        await GeneratorTestHelpers.Verify(
            """
            using System;
            using MinimalLambda;
            using MinimalLambda.Builder;
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
    public async Task Test_MethodHandler_ReturnTask() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
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
    public async Task Test_MethodHandler_ReturnTaskString() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
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
    public async Task Test_MethodHandler_Async_ReturnTaskString() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
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
    public async Task Test_MethodHandler_AsyncVoid() =>
        await GeneratorTestHelpers.Verify(
            """
            using System;
            using MinimalLambda;
            using MinimalLambda.Builder;
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
}
