namespace MinimalLambda.SourceGenerators.UnitTests;

public class BlockLambdaVerifyTests
{
    [Fact]
    public async Task Test_BlockLambda_ReturnExplicitType() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host.Core;
            using AwsLambda.Host.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
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
    public async Task Test_BlockLambda_ReturnString() =>
        await GeneratorTestHelpers.Verify(
            """
            using AwsLambda.Host.Core;
            using AwsLambda.Host.Builder;
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
    public async Task Test_BlockLambda_TypeCast_InputFromKeyedServices() =>
        await GeneratorTestHelpers.Verify(
            """
            using System;
            using System.Threading.Tasks;
            using AwsLambda.Host.Core;
            using AwsLambda.Host.Builder;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
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
            """
        );

    [Fact]
    public async Task Test_BlockLambda_ReturnsTask_TypeCast() =>
        await GeneratorTestHelpers.Verify(
            """
            using System;
            using System.Threading.Tasks;
            using AwsLambda.Host.Core;
            using AwsLambda.Host.Builder;

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
    public async Task Test_BlockLambda_Async_ReturnTaskString_TypeCast() =>
        await GeneratorTestHelpers.Verify(
            """
            using System;
            using System.Threading.Tasks;
            using AwsLambda.Host.Core;
            using AwsLambda.Host.Builder;

            var builder = LambdaApplication.CreateBuilder();
            var lambda = builder.Build();

            lambda.MapHandler((Func<Task<string>>)(async () => "hello world"));
            """
        );

    [Fact]
    public async Task Test_BlockLambda_TypeCast() =>
        await GeneratorTestHelpers.Verify(
            """
            using System;
            using AwsLambda.Host.Core;
            using AwsLambda.Host.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
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
            """
        );

    [Fact]
    public async Task Test_BlockLambda_NoTypeInfo_TypeCast() =>
        await GeneratorTestHelpers.Verify(
            """
            using System;
            using AwsLambda.Host.Core;
            using AwsLambda.Host.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
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
            """
        );

    [Fact]
    public async Task Test_BlockLambda_NoReturn_TypeCast() =>
        await GeneratorTestHelpers.Verify(
            """
            using System;
            using System.Threading.Tasks;
            using AwsLambda.Host.Core;
            using AwsLambda.Host.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
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
            """
        );

    [Fact]
    public async Task Test_BlockLambda_ReturnImplicitNullable() =>
        await GeneratorTestHelpers.Verify(
            """
            using AwsLambda.Host.Core;
            using AwsLambda.Host.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
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
            """
        );

    [Fact]
    public async Task Test_BlockLambda_StreamResponse() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.IO;
            using AwsLambda.Host.Core;
            using AwsLambda.Host.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.MapHandler(Stream () => new FileStream("hello.txt", FileMode.Open));

            await lambda.RunAsync();
            """
        );

    [Fact]
    public async Task Test_BlockLambda_AllInputSources() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading;
            using Amazon.Lambda.Core;
            using AwsLambda.Host.Core;
            using AwsLambda.Host.Builder;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.UseClearLambdaOutputFormatting();

            lambda.MapHandler(
                (
                    [Event] string request,
                    ILambdaContext context,
                    CancellationToken cancellationToken,
                    [FromKeyedServices("key0")] IService service0,
                    [FromKeyedServices("key1")] IService? service1,
                    IService service2,
                    IService? service3
                ) => { }
            );

            await lambda.RunAsync();

            internal interface IService
            {
                string GetMessage(string name);
            }
            """
        );

    [Fact]
    public async Task Test_BlockLambda_ReturnTaskString() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host.Core;
            using AwsLambda.Host.Builder;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.UseClearLambdaOutputFormatting();

            lambda.MapHandler(() =>
            {
                return Task.FromResult("Hello World");
            });

            await lambda.RunAsync();
            """
        );
}
