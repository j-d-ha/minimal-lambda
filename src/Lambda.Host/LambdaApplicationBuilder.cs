using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lambda.Host;

public sealed class LambdaApplicationBuilder : IHostApplicationBuilder
{
    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null
    )
        where TContainerBuilder : notnull => throw new NotImplementedException();

    public IDictionary<object, object> Properties { get; }
    public IConfigurationManager Configuration { get; }
    public IHostEnvironment Environment { get; }
    public ILoggingBuilder Logging { get; }
    public IMetricsBuilder Metrics { get; }
    public IServiceCollection Services { get; }
}
