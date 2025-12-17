namespace MinimalLambda.SourceGenerators.UnitTests;

public class UseMiddlewareTVerifyTests
{
    [Fact]
    public async Task Test_MiddlewareClass_Simple() =>
        await GeneratorTestHelpers.Verify(
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
                public async Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
                {
                    await next(context);
                }
            }
            """
        );

    [Fact]
    public async Task Test_MiddlewareClass_AbstractMiddleware() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using Microsoft.Extensions.Hosting;
            using MinimalLambda;
            using MinimalLambda.Builder;

            var builder = LambdaApplication.CreateBuilder();

            await using var lambda = builder.Build();

            lambda.UseMiddleware<MyLambdaMiddleware2>();

            lambda.MapHandler(() => { });

            await lambda.RunAsync();

            internal abstract class MyLambdaMiddleware : ILambdaMiddleware
            {
                public async Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
                {
                    await next(context);
                }
            }

            internal class MyLambdaMiddleware2 : MyLambdaMiddleware
            {
                public string Do() => "Hello World!";
            }
            """
        );

    [Fact]
    public async Task Test_MiddlewareClass_ConstructorWithArgs() =>
        await GeneratorTestHelpers.Verify(
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
                private readonly IService _service;

                internal MyLambdaMiddleware(IService service)
                {
                    _service = service;
                }

                public async Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
                {
                    await next(context);
                }
            }

            internal interface IService
            {
                string GetMessage();
            }
            """
        );

    [Fact]
    public async Task Test_MiddlewareClass_() =>
        await GeneratorTestHelpers.Verify(
            """

            """
        );
}
