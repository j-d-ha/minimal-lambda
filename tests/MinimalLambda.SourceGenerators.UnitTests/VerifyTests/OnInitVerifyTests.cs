namespace MinimalLambda.SourceGenerators.UnitTests;

public class OnInitVerifyTests
{
    [Fact]
    public async Task Test_OnInit_BaseMethodCall() =>
        await GeneratorTestHelpers.Verify(
            """
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(
                async (services, token) =>
                {
                    return true;
                }
            );

            await lambda.RunAsync();
            """,
            0
        );

    [Fact]
    public async Task Test_OnInit_ExplicitReturnTypeAsync() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(async Task<bool> () =>
            {
                return true;
            });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnInit_NoInput_NoOutput() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(() => { });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnInit_NoInput_ReturnBool() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(() => true);

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnInit_NoInput_ReturnTaskBool() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(() => Task.FromResult(true));

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnInit_NoInput_ReturnAsyncBool() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(async () => true);

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnInit_NoInput_ReturnNotExpectedType() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(() => "string");

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnInit_NoInput_ReturnNotExpectedTypeTask() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(() => Task.FromResult("string"));

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnInit_NoInput_ReturnNotExpectedTypeAsync() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(async () => "string");

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnInit_PrimitiveInput() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(
                Task<bool> (string x, int y) =>
                {
                    return Task.FromResult(true);
                }
            );

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnInit_NullableValueAndReferenceInputs() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(
                Task<bool> (string? x, IService? y) =>
                {
                    return Task.FromResult(true);
                }
            );

            await lambda.RunAsync();

            public interface IService
            {
                Task<string> GetMessage();
            }
            """
        );

    [Fact]
    public async Task Test_OnInit_OneOfEachPossibleKindOfInput() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading;
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(
                Task<bool> (
                    CancellationToken token,
                    [FromKeyedServices("key1")] IService service1,
                    [FromKeyedServices("key2")] IService? service2,
                    IService service3,
                    IService? service4
                ) =>
                {
                    return Task.FromResult(true);
                }
            );

            await lambda.RunAsync();

            public interface IService
            {
                Task<string> GetMessage();
            }
            """
        );

    [Fact]
    public async Task Test_OnInit_MultipleCalls() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(Task<bool> () =>
            {
                return Task.FromResult(true);
            });

            lambda.OnInit(
                (string? x, IService? y) =>
                {
                    return Task.FromResult(true);
                }
            );

            lambda.OnInit(
                Task<bool> (string x, int y) =>
                {
                    return Task.FromResult(true);
                }
            );

            await lambda.RunAsync();

            public interface IService
            {
                Task<string> GetMessage();
            }
            """
        );

    [Fact]
    public async Task Test_OnInit_MethodHandler_NoDi() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            await lambda.RunAsync();

            lambda.OnInit(Function.OnInitHandler);

            public static class Function
            {
                public static Task<bool> OnInitHandler()
                {
                    return Task.FromResult(true);
                }
            }
            """
        );

    [Fact]
    public async Task Test_OnInit_MethodHandler_AsyncAndDi() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(Function.OnInitHandler);

            await lambda.RunAsync();

            public static class Function
            {
                public static async Task<bool> OnInitHandler(IService service)
                {
                    return await service.ShouldContinue();
                }
            }

            public interface IService
            {
                Task<bool> ShouldContinue();
            }
            """
        );

    [Fact]
    public async Task Test_OnInit_MethodHandler_AsyncAndDiAndReturnUnexpectedType() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using MinimalLambda;
            using MinimalLambda.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnInit(Function.OnInitHandler);

            await lambda.RunAsync();

            public static class Function
            {
                public static async Task<string> OnInitHandler(IService service)
                {
                    return await service.ShouldContinue();
                }
            }

            public interface IService
            {
                Task<string> ShouldContinue();
            }
            """
        );
}
