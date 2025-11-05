namespace AwsLambda.Host.SourceGenerators.UnitTests;

public class OnShutdownVerifyTests
{
    [Fact]
    public async Task Test_OnShutdown_BaseMethodCall() =>
        await GeneratorTestHelpers.Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnShutdown(
                async (services, token) =>
                {
                    return;
                }
            );

            await lambda.RunAsync();
            """,
            0
        );

    [Fact]
    public async Task Test_OnShutdown_NoInput() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnShutdown(Task () =>
            {
                return Task.CompletedTask;
            });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnShutdown_NoInput_ReturnUnexpectedType() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnShutdown(() =>
            {
                return "test"; 
            });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnShutdown_NoInput_ReturnUnexpectedTypeTask() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnShutdown(() =>
            {
                return Task.FromResult("test"); 
            });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnShutdown_NoInput_ReturnUnexpectedTypeAsync() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnShutdown(async () =>
            {
                return "test"; 
            });

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnShutdown_PrimitiveInput() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnShutdown(
                Task (string x, int y) =>
                {
                    return Task.CompletedTask;
                }
            );

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_OnShutdown_NullableValueAndReferenceInputs() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnShutdown(
                Task (string? x, IService? y) =>
                {
                    return Task.CompletedTask;
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
    public async Task Test_OnShutdown_OneOfEachPossibleKindOfInput() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading;
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnShutdown(
                Task (
                    CancellationToken token,
                    [FromKeyedServices("key1")] IService service1,
                    [FromKeyedServices("key2")] IService? service2,
                    IService service3,
                    IService? service4
                ) =>
                {
                    return Task.CompletedTask;
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
    public async Task Test_OnShutdown_MultipleCalls() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnShutdown(Task () =>
            {
                return Task.CompletedTask;
            });

            lambda.OnShutdown(
                Task (string? x, IService? y) =>
                {
                    return Task.CompletedTask;
                }
            );

            lambda.OnShutdown(
                Task (string x, int y) =>
                {
                    return Task.CompletedTask;
                }
            );

            await lambda.RunAsync();

            public interface IService
            {
                Task<string> GetMessage();
            }
            """
        );
}
