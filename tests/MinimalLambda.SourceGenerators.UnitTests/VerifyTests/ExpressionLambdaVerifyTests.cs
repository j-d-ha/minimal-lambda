namespace MinimalLambda.SourceGenerators.UnitTests;

public class ExpressionLambdaVerifyTests
{
    [Fact]
    public async Task Test_ExpressionLambda_MainOverload_NoOp() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.Handle(Task (ILambdaHostContext context) => Task.CompletedTask);

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_MainOverload_DeserializerSerializer_NoOp() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.IO;
            using System.Threading.Tasks;
            using Amazon.Lambda.Core;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.Handle(Task (ILambdaHostContext context) => Task.CompletedTask);

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_NoInput_NoOutput() =>
        await GeneratorTestHelpers.Verify(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(() => { });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_NoInput_ReturnString() =>
        await GeneratorTestHelpers.Verify(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(() => "hello world");

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_NoInput_ReturnNullablePrimitive() =>
        await GeneratorTestHelpers.Verify(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(int? () => 1);

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_NoInput_ReturnGenericObject() =>
        await GeneratorTestHelpers.Verify(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
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
    public async Task Test_ExpressionLambda_InputDi_Async() =>
        await GeneratorTestHelpers.Verify(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(
                async ([FromEvent] string input, IService service) => service.GetMessage().ToUpper()
            );

            await lambda.RunAsync();

            public interface IService
            {
                string GetMessage();
            }
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_InputDi_AsyncAndAwait() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(
                async ([FromEvent] string input, IService service) => (await service.GetMessage()).ToUpper()
            );

            await lambda.RunAsync();

            public interface IService
            {
                Task<string> GetMessage();
            }
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_InputDi_AsyncAndAwait_EventAndResponseDifferentNamespace() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;
            using MyNamespace;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(
                async ([FromEvent] Event input, IService service) =>
                    new Response((await service.GetMessage()).ToUpper())
            );

            await lambda.RunAsync();

            public interface IService
            {
                Task<string> GetMessage();
            }

            namespace MyNamespace
            {
                public record Event(string Input);

                public record Response(string Message);
            }
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_ReturnExplicitType() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(
                IService ([FromEvent] string input) => input == "other" ? new Service() : new OtherService()
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
    public async Task Test_ExpressionLambda_NullableInput_ReturnImplicitNullable() =>
        await GeneratorTestHelpers.Verify(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(([FromEvent] string? input, IService service) => service.GetMessage());

            await lambda.RunAsync();

            public interface IService
            {
                string? GetMessage();
            }
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_NullableInput_ReturnExplicitNullable() =>
        await GeneratorTestHelpers.Verify(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(string? ([FromEvent] int? input, IService service) => service.GetMessage());

            await lambda.RunAsync();

            public interface IService
            {
                string? GetMessage();
            }
            """
        );

    // Additional handler type not shown in the examples - generic handlers with complex custom
    // types
    [Fact]
    public async Task Test_ExpressionLambda_ComplexInput_ComplexOutput() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using Amazon.Lambda.Core;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(
                async ([FromEvent] CustomRequest request, IService service, ILambdaContext context) =>
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
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_Async_ReturnExplicitTask() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler(async Task () => { });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_ReturnExplicitTask() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
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
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler((CancellationToken cancellationToken) => "hello world");

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_AsksForCancellationTokenAndLambdaContext() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading;
            using Amazon.Lambda.Core;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler((CancellationToken ct, ILambdaContext ctx) => "hello world");

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_AsksForCancellationTokenAndLambdaHostContext() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading;
            using Amazon.Lambda.Core;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler((CancellationToken ct, ILambdaHostContext ctx) => "hello world");

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_InputStream() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.IO;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(([FromEvent] Stream input) => { });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_ExpressionLambda_ExplicitVoid() =>
        await GeneratorTestHelpers.Verify(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.UseClearLambdaOutputFormatting();

            lambda.MapHandler(void () => { });

            await lambda.RunAsync();
            """
        );
}
