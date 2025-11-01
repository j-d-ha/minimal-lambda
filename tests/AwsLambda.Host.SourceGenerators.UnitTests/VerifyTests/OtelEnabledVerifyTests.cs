namespace AwsLambda.Host.SourceGenerators.UnitTests;

public class OtelEnabledVerifyTests
{
    [Fact]
    public async Task Test_OtelEnabled_EventAndResponse() =>
        await GeneratorTestHelpers.Verify(
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
        await GeneratorTestHelpers.Verify(
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
        await GeneratorTestHelpers.Verify(
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
        await GeneratorTestHelpers.Verify(
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
        await GeneratorTestHelpers.Verify(
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
}
