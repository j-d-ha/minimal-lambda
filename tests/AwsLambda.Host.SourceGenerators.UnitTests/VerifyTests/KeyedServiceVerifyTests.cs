namespace AwsLambda.Host.SourceGenerators.UnitTests;

public class KeyedServiceVerifyTests
{
    [Fact]
    public async Task Test_KeyedService_StringAndEnumKeys() =>
        await GeneratorTestHelpers.Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddKeyedSingleton<IService, Service>("myKey");
            builder.Services.AddKeyedSingleton<IService, Service>(ServiceType.Secondary);

            var lambda = builder.Build();

            lambda.MapHandler(
                (
                    [FromKeyedServices("myKey")] IService serviceA,
                    [FromKeyedServices(ServiceType.Secondary)] IService serviceB
                ) => { }
            );

            await lambda.RunAsync();

            public enum ServiceType
            {
                Primary,
                Secondary,
            }

            public interface IService
            {
                string GetMessage();
            }

            public class Service : IService
            {
                public string GetMessage() => "Hello";
            }
            """
        );

    [Fact]
    public async Task Test_KeyedService_IntAndLongKeys() =>
        await GeneratorTestHelpers.Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddKeyedSingleton<IService, Service>(42);
            builder.Services.AddKeyedSingleton<IService, Service>(42L);

            var lambda = builder.Build();

            lambda.MapHandler(
                ([FromKeyedServices(42)] IService serviceA, [FromKeyedServices(42L)] IService serviceB) => { }
            );

            await lambda.RunAsync();

            public interface IService
            {
                string GetMessage();
            }

            public class Service : IService
            {
                public string GetMessage() => "Hello";
            }
            """
        );

    [Fact]
    public async Task Test_KeyedService_SmallIntegerTypes() =>
        await GeneratorTestHelpers.Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddKeyedSingleton<IService, Service>((short)42);
            builder.Services.AddKeyedSingleton<IService, Service>((byte)42);
            builder.Services.AddKeyedSingleton<IService, Service>((sbyte)42);

            var lambda = builder.Build();

            lambda.MapHandler(
                (
                    [FromKeyedServices((short)42)] IService serviceA,
                    [FromKeyedServices((byte)42)] IService serviceB,
                    [FromKeyedServices((sbyte)42)] IService serviceC
                ) => { }
            );

            await lambda.RunAsync();

            public interface IService
            {
                string GetMessage();
            }

            public class Service : IService
            {
                public string GetMessage() => "Hello";
            }
            """
        );

    [Fact]
    public async Task Test_KeyedService_UnsignedIntegerTypes() =>
        await GeneratorTestHelpers.Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddKeyedSingleton<IService, Service>(42u);
            builder.Services.AddKeyedSingleton<IService, Service>(42ul);
            builder.Services.AddKeyedSingleton<IService, Service>((ushort)42);

            var lambda = builder.Build();

            lambda.MapHandler(
                (
                    [FromKeyedServices(42u)] IService serviceA,
                    [FromKeyedServices(42ul)] IService serviceB,
                    [FromKeyedServices((ushort)42)] IService serviceC
                ) => { }
            );

            await lambda.RunAsync();

            public interface IService
            {
                string GetMessage();
            }

            public class Service : IService
            {
                public string GetMessage() => "Hello";
            }
            """
        );

    [Fact]
    public async Task Test_KeyedService_FloatingPointTypes() =>
        await GeneratorTestHelpers.Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddKeyedSingleton<IService, Service>(3.14);
            builder.Services.AddKeyedSingleton<IService, Service>(3.14f);
            builder.Services.AddKeyedSingleton<IService, Service>(3.14m);

            var lambda = builder.Build();

            lambda.MapHandler(
                (
                    [FromKeyedServices(3.14)] IService serviceA,
                    [FromKeyedServices(3.14f)] IService serviceB,
                    [FromKeyedServices(3.14m)] IService serviceC
                ) => { }
            );

            await lambda.RunAsync();

            public interface IService
            {
                string GetMessage();
            }

            public class Service : IService
            {
                public string GetMessage() => "Hello";
            }
            """
        );

    [Fact]
    public async Task Test_KeyedService_OtherTypes() =>
        await GeneratorTestHelpers.Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddKeyedSingleton<IService, Service>(true);
            builder.Services.AddKeyedSingleton<IService, Service>('A');
            builder.Services.AddKeyedSingleton<IService, Service>(typeof(Service));
            builder.Services.AddKeyedSingleton<IService, Service>(null);

            var lambda = builder.Build();

            lambda.MapHandler(
                (
                    [FromKeyedServices(true)] IService serviceA,
                    [FromKeyedServices('A')] IService serviceB,
                    [FromKeyedServices(typeof(Service))] IService serviceC,
                    [FromKeyedServices(null)] IService serviceD
                ) => { }
            );

            await lambda.RunAsync();

            public interface IService
            {
                string GetMessage();
            }

            public class Service : IService
            {
                public string GetMessage() => "Hello";
            }
            """
        );

    [Fact]
    public async Task Test_KeyedService_ArrayKey() =>
        await GeneratorTestHelpers.Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            var key = new[] { "a", "b" };
            builder.Services.AddKeyedSingleton<IService, Service>(key);

            var lambda = builder.Build();

            lambda.MapHandler(([FromKeyedServices(new[] { "a", "b" })] IService service) => { });

            await lambda.RunAsync();

            public interface IService
            {
                string GetMessage();
            }

            public class Service : IService
            {
                public string GetMessage() => "Hello";
            }
            """
        );
}
