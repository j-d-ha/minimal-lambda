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
    public async Task Test_MiddlewareClass_MultipleConstructorsAndOneWithMiddlewareConstructor() =>
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

                [MiddlewareConstructor]
                internal MyLambdaMiddleware()
                {
                    _service = null!;
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
    public async Task Test_MiddlewareClass_FromServicesAttribute() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using Microsoft.Extensions.Hosting;
            using Microsoft.Extensions.DependencyInjection;
            using MinimalLambda;
            using MinimalLambda.Builder;

            var builder = LambdaApplication.CreateBuilder();

            await using var lambda = builder.Build();

            lambda.UseMiddleware<MyLambdaMiddleware>();

            lambda.MapHandler(() => { });

            await lambda.RunAsync();

            internal class MyLambdaMiddleware(
                [FromServices] IService service
            ) : ILambdaMiddleware
            {
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
    public async Task Test_MiddlewareClass_FromKeyedServicesAttribute() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using Microsoft.Extensions.Hosting;
            using Microsoft.Extensions.DependencyInjection;
            using MinimalLambda;
            using MinimalLambda.Builder;

            var builder = LambdaApplication.CreateBuilder();

            await using var lambda = builder.Build();

            lambda.UseMiddleware<MyLambdaMiddleware>();

            lambda.MapHandler(() => { });

            await lambda.RunAsync();

            internal class MyLambdaMiddleware(
                [FromKeyedServices("myKey")] IService service
            ) : ILambdaMiddleware
            {
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
    public async Task Test_MiddlewareClass_FromArgumentsAttribute() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using Microsoft.Extensions.Hosting;
            using Amazon.Lambda.Core;
            using MinimalLambda;
            using MinimalLambda.Builder;

            var builder = LambdaApplication.CreateBuilder();

            await using var lambda = builder.Build();

            lambda.UseMiddleware<MyLambdaMiddleware>();

            lambda.MapHandler(() => { });

            await lambda.RunAsync();

            internal class MyLambdaMiddleware(
                [FromArguments] string apiKey
            ) : ILambdaMiddleware
            {
                public async Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
                {
                    await next(context);
                }
            }
            """
        );

    [Fact]
    public async Task Test_MiddlewareClass_NullableParameter() =>
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

            internal class MyLambdaMiddleware(
                IService? service
            ) : ILambdaMiddleware
            {
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
    public async Task Test_MiddlewareClass_OptionalParameterWithDefaultValue() =>
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
                internal MyLambdaMiddleware(string name = "default")
                {
                }

                public async Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
                {
                    await next(context);
                }
            }
            """
        );

    [Fact]
    public async Task Test_MiddlewareClass_MixedParameterSources() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using Microsoft.Extensions.Hosting;
            using Microsoft.Extensions.DependencyInjection;
            using MinimalLambda;
            using MinimalLambda.Builder;

            var builder = LambdaApplication.CreateBuilder();

            await using var lambda = builder.Build();

            lambda.UseMiddleware<MyLambdaMiddleware>();

            lambda.MapHandler(() => { });

            await lambda.RunAsync();

            internal class MyLambdaMiddleware(
                [FromServices] ILogger logger,
                [FromKeyedServices("cache")] ICache cache,
                [FromArguments] string apiKey,
                IMetrics? metrics
            ) : ILambdaMiddleware
            {
                public async Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
                {
                    await next(context);
                }
            }

            internal interface ILogger { }
            internal interface ICache { }
            internal interface IMetrics { }
            """
        );

    [Fact]
    public async Task Test_MiddlewareClass_WithArgsArray() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using Microsoft.Extensions.Hosting;
            using MinimalLambda;
            using MinimalLambda.Builder;

            var builder = LambdaApplication.CreateBuilder();

            await using var lambda = builder.Build();

            lambda.UseMiddleware<MyLambdaMiddleware>("myApiKey");

            lambda.MapHandler(() => { });

            await lambda.RunAsync();

            internal class MyLambdaMiddleware(
                string apiKey,
                IService service
            ) : ILambdaMiddleware
            {
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
    public async Task Test_MiddlewareClass_ComplexRealWorldScenario() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using Microsoft.Extensions.Hosting;
            using Microsoft.Extensions.DependencyInjection;
            using Amazon.Lambda.Core;
            using MinimalLambda;
            using MinimalLambda.Builder;

            var builder = LambdaApplication.CreateBuilder();

            await using var lambda = builder.Build();

            lambda.UseMiddleware<MyLambdaMiddleware>();

            lambda.MapHandler(() => { });

            await lambda.RunAsync();

            internal class MyLambdaMiddleware(
                [FromArguments] string name,
                [FromKeyedServices("primary")] ILogger logger,
                [FromServices] IMetrics metrics,
                IDataService? dataService
            ) : ILambdaMiddleware
            {
                public async Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
                {
                    await next(context);
                }
            }

            internal interface ILogger { }
            internal interface IMetrics { }
            internal interface IDataService { }
            """
        );

    [Fact]
    public async Task Test_MiddlewareClass_IDisposable() =>
        await GeneratorTestHelpers.Verify(
            """
            using System;
            using System.Threading.Tasks;
            using Microsoft.Extensions.Hosting;
            using MinimalLambda;
            using MinimalLambda.Builder;

            var builder = LambdaApplication.CreateBuilder();

            await using var lambda = builder.Build();

            lambda.UseMiddleware<MyLambdaMiddleware>();

            lambda.MapHandler(() => { });

            await lambda.RunAsync();

            internal class MyLambdaMiddleware : ILambdaMiddleware, IDisposable
            {
                public async Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
                {
                    await next(context);
                }

                public void Dispose() { }
            }
            """
        );

    [Fact]
    public async Task Test_MiddlewareClass_IAsyncDisposable() =>
        await GeneratorTestHelpers.Verify(
            """
            using System;
            using System.Threading.Tasks;
            using Microsoft.Extensions.Hosting;
            using MinimalLambda;
            using MinimalLambda.Builder;

            var builder = LambdaApplication.CreateBuilder();

            await using var lambda = builder.Build();

            lambda.UseMiddleware<MyLambdaMiddleware>();

            lambda.MapHandler(() => { });

            await lambda.RunAsync();

            internal class MyLambdaMiddleware : ILambdaMiddleware, IAsyncDisposable
            {
                public async Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
                {
                    await next(context);
                }

                public async ValueTask DisposeAsync() { }
            }
            """
        );
}
