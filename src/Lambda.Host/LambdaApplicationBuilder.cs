using Lambda.Host.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lambda.Host;

public sealed class LambdaApplicationBuilder : IHostApplicationBuilder
{
    private const string LambdaHostAppSettingsSectionName = "LambdaHost";
    private readonly HostApplicationBuilder _hostBuilder;

    private LambdaApplicationBuilder(HostApplicationBuilder hostBuilder)
    {
        _hostBuilder = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

        // Configure LambdaHostSettings from appsettings.json
        Services.Configure<LambdaHostSettings>(
            Configuration.GetSection(LambdaHostAppSettingsSectionName)
        );
    }

    internal LambdaApplicationBuilder()
        : this(new HostApplicationBuilder()) { }

    internal LambdaApplicationBuilder(string[]? args)
        : this(new HostApplicationBuilder(args)) { }

    internal LambdaApplicationBuilder(HostApplicationBuilderSettings settings, bool empty = false)
        : this(
            empty
                ? Microsoft.Extensions.Hosting.Host.CreateEmptyApplicationBuilder(settings)
                : new HostApplicationBuilder(settings)
        ) { }

    public IDictionary<object, object> Properties =>
        ((IHostApplicationBuilder)_hostBuilder).Properties;

    public IConfigurationManager Configuration => _hostBuilder.Configuration;
    public IHostEnvironment Environment => _hostBuilder.Environment;
    public ILoggingBuilder Logging => _hostBuilder.Logging;
    public IMetricsBuilder Metrics => _hostBuilder.Metrics;
    public IServiceCollection Services => _hostBuilder.Services;

    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null
    )
        where TContainerBuilder : notnull => _hostBuilder.ConfigureContainer(factory, configure);

    public LambdaApplication Build()
    {
        // register LambdaHostedService as IHostedService
        Services.AddSingleton<IHostedService, LambdaHostedService>();

        // Register DelegateHolder to pass the handler delegate to the generated LambdaApplication
        Services.AddSingleton<DelegateHolder>();

        // Attempt to add a default cancellation token source factory if one is not already
        // registered.
        Services.TryAddSingleton<ILambdaCancellationTokenSourceFactory>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<LambdaHostSettings>>().Value;

            return new LambdaCancellationTokenSourceFactory(settings.InvocationCancellationBuffer);
        });

        var host = _hostBuilder.Build();

        return new LambdaApplication(host);
    }
}
