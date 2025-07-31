using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lambda.Host;

public sealed class LambdaApplicationBuilder : IHostApplicationBuilder
{
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
        var hostedServiceTypes = Assembly
            .GetCallingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IHostedService).IsAssignableFrom(t))
            .ToArray();

        if (!hostedServiceTypes.Any())
            throw new InvalidOperationException("No hosted services found.");

        foreach (var serviceType in hostedServiceTypes)
        {
            Services.AddSingleton(serviceType);

            Services.AddSingleton<IHostedService>(serviceProvider =>
                (IHostedService)serviceProvider.GetRequiredService(serviceType)
            );
        }

        Services.AddSingleton<DelegateHolder>();

        var host = _hostBuilder.Build();

        return new LambdaApplication(host);
    }

    public static LambdaApplicationBuilder CreateBuilder() => new();

    public static LambdaApplicationBuilder CreateBuilder(string[] args) => new(args);

    public static LambdaApplicationBuilder CreateBuilder(HostApplicationBuilderSettings settings) =>
        new(settings);
}
