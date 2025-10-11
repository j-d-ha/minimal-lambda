using System.Reflection;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Lambda.Host.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lambda.Host;

public sealed class LambdaApplicationBuilder : IHostApplicationBuilder
{
    private readonly TimeSpan _defaultCancellationBuffer = TimeSpan.FromSeconds(3);
    private readonly HostApplicationBuilder _hostBuilder;

    public LambdaApplicationBuilder() => _hostBuilder = new HostApplicationBuilder();

    public LambdaApplicationBuilder(string[] args) =>
        _hostBuilder = new HostApplicationBuilder(args);

    public LambdaApplicationBuilder(HostApplicationBuilderSettings settings) =>
        _hostBuilder = new HostApplicationBuilder(settings);

    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null
    )
        where TContainerBuilder : notnull => _hostBuilder.ConfigureContainer(factory, configure);

    public IDictionary<object, object> Properties =>
        ((IHostApplicationBuilder)_hostBuilder).Properties;

    public IConfigurationManager Configuration => _hostBuilder.Configuration;
    public IHostEnvironment Environment => _hostBuilder.Environment;
    public ILoggingBuilder Logging => _hostBuilder.Logging;
    public IMetricsBuilder Metrics => _hostBuilder.Metrics;
    public IServiceCollection Services => _hostBuilder.Services;

    public LambdaApplication Build()
    {
#if !NATIVEAOT

        if (Services.All(x => x.ServiceType != typeof(LambdaHostedService)))
        {
            var hostedServiceTypes = Assembly
                .GetCallingAssembly()
                .GetTypes()
                .Where(t =>
                    t is { IsClass: true, IsAbstract: false }
                    && typeof(LambdaHostedService).IsAssignableFrom(t)
                )
                .ToArray();

            switch (hostedServiceTypes.Length)
            {
                case 0:
                    throw new InvalidOperationException(
                        $"No instances of {typeof(LambdaHostedService)} found for DI registration."
                    );
                case > 1:
                    throw new InvalidOperationException(
                        $"Multiple instances of {typeof(LambdaHostedService)} found."
                    );
                default:
                    Services.AddSingleton(hostedServiceTypes.First());
                    break;
            }
        }

#else
        // check if an instance of LambdaHostedService registered
        if (Services.All(x => x.ServiceType != typeof(LambdaHostedService)))
            throw new InvalidOperationException($"No {typeof(LambdaHostedService)} registered.");

#endif

        // register LambdaHostedService as IHostedService
        Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<LambdaHostedService>());

        Services.AddSingleton<DelegateHolder>();

        // Attempt to add a default cancellation token source factory if one is not already
        // registered.
        Services.TryAddSingleton<ILambdaCancellationTokenSourceFactory>(
            _ => new LambdaCancellationTokenSourceFactory(_defaultCancellationBuffer)
        );

        Services.TryAddSingleton<ILambdaSerializer, DefaultLambdaJsonSerializer>();

        var host = _hostBuilder.Build();

        return new LambdaApplication(host);
    }

    public static LambdaApplicationBuilder CreateBuilder() => new();

    public static LambdaApplicationBuilder CreateBuilder(string[] args) => new(args);

    public static LambdaApplicationBuilder CreateBuilder(HostApplicationBuilderSettings settings) =>
        new(settings);
}
