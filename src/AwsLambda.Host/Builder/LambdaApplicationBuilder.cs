using System.Diagnostics;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

/// <summary>A builder for configuring and constructing an AWS Lambda Host application.</summary>
/// <remarks>
///     <para>
///         The <see cref="LambdaApplicationBuilder" /> is used to configure services, logging,
///         configuration, and other aspects of a Lambda application before building it into a
///         <see cref="LambdaApplication" /> instance.
///     </para>
///     <para>
///         This builder implements the <see cref="IHostApplicationBuilder" /> interface, providing
///         access to the standard .NET host builder APIs for dependency injection, configuration,
///         logging, and metrics.
///     </para>
///     <para>
///         Instances are created using factory methods on <see cref="LambdaApplication" />, such as
///         <see cref="BuilderLambdaApplicationExtensions.CreateBuilder()" />,
///         <see cref="BuilderLambdaApplicationExtensions.CreateBuilder(string[])" />, and
///         <see
///             cref="BuilderLambdaApplicationExtensions.CreateBuilder(HostApplicationBuilderSettings)" />
///         .
///     </para>
/// </remarks>
/// <seealso cref="BuilderLambdaApplicationExtensions.CreateBuilder()" />
/// <seealso cref="IHostApplicationBuilder" />
public sealed class LambdaApplicationBuilder : IHostApplicationBuilder
{
    private const string LambdaHostAppSettingsSectionName = "AwsLambdaHost";
    private readonly HostApplicationBuilder _hostBuilder;

    private LambdaApplication? _builtApplication = null;

    private LambdaApplicationBuilder(HostApplicationBuilder hostBuilder)
    {
        _hostBuilder = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

        // Configure LambdaHostSettings from appsettings.json
        Services.Configure<LambdaHostOptions>(
            Configuration.GetSection(LambdaHostAppSettingsSectionName)
        );

        // Configure LambdaHostedServiceOptions with callbacks
        Services.Configure<LambdaHostedServiceOptions>(options =>
        {
            options.ConfigureHandlerBuilder = ConfigureHandlerBuilder;
            options.ConfigureOnInitBuilder = ConfigureOnInitBuilder;
            options.ConfigureOnShutdownBuilder = ConfigureOnShutdownBuilder;
        });

        // Register DelegateHolder to pass the handler delegate to the generated LambdaApplication
        Services.AddSingleton<DelegateHolder>();

        Services.AddLambdaHostCoreServices();
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

    /// <inheritdoc />
    public IDictionary<object, object> Properties =>
        ((IHostApplicationBuilder)_hostBuilder).Properties;

    /// <inheritdoc />
    public IConfigurationManager Configuration => _hostBuilder.Configuration;

    /// <inheritdoc />
    public IHostEnvironment Environment => _hostBuilder.Environment;

    /// <inheritdoc />
    public ILoggingBuilder Logging => _hostBuilder.Logging;

    /// <inheritdoc />
    public IMetricsBuilder Metrics => _hostBuilder.Metrics;

    /// <inheritdoc />
    public IServiceCollection Services => _hostBuilder.Services;

    /// <inheritdoc />
    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null
    )
        where TContainerBuilder : notnull => _hostBuilder.ConfigureContainer(factory, configure);

    /// <summary>Builds the Lambda application with the configured services and settings.</summary>
    /// <remarks>
    ///     <para>
    ///         The <c>Build</c> method finalizes the configuration and creates a
    ///         <see cref="LambdaApplication" /> instance that is ready to run. It performs the following
    ///         operations:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Registers a default <see cref="ILambdaCancellationTokenSourceFactory" />
    ///                 if one is not already registered, which manages cancellation tokens for Lambda
    ///                 invocations.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Applies shutdown timeout settings from <see cref="LambdaHostOptions" /> to
    ///                 ensure proper graceful shutdown before the Lambda runtime timeout.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Constructs the underlying host and wraps it in a
    ///                 <see cref="LambdaApplication" /> instance.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <returns>A configured <see cref="LambdaApplication" /> ready to be started and run.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the build process fails due to service
    ///     configuration issues.
    /// </exception>
    public LambdaApplication Build()
    {
        // Get LambdaHostOptions from DI for final configuration.
        var lambdaHostOptions = Services
            .BuildServiceProvider()
            .GetRequiredService<IOptions<LambdaHostOptions>>()
            .Value;

        // Set the shutdown timeout to the configured value minus the buffer.
        var shutdownTimeout =
            lambdaHostOptions.ShutdownDuration - lambdaHostOptions.ShutdownDurationBuffer;

        Services.PostConfigure<HostOptions>(options =>
            options.ShutdownTimeout =
                shutdownTimeout >= TimeSpan.Zero ? shutdownTimeout : TimeSpan.Zero
        );

        Services.TryAddLambdaHostDefaultServices();

        _builtApplication = new LambdaApplication(_hostBuilder.Build());

        return _builtApplication;
    }

    private void ConfigureHandlerBuilder(ILambdaHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Debug.Assert(_builtApplication is not null);

        if (_builtApplication.Handler is null)
            throw new InvalidOperationException("Lambda Handler is not set.");

        foreach (var middleware in _builtApplication.Middlewares)
            builder.Use(middleware);

        // add default middleware to the end of the middleware pipeline
        builder.UseExtractAndPackEnvelope();

        builder.Handle(_builtApplication.Handler);

        foreach (var property in _builtApplication.Properties)
            builder.Properties[property.Key] = property.Value;
    }

    private void ConfigureOnInitBuilder(ILambdaOnInitBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Debug.Assert(_builtApplication is not null);

        foreach (var handlers in _builtApplication.InitHandlers)
            builder.InitHandlers.Add(handlers);
    }

    private void ConfigureOnShutdownBuilder(ILambdaOnShutdownBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Debug.Assert(_builtApplication is not null);

        foreach (var handlers in _builtApplication.ShutdownHandlers)
            builder.ShutdownHandlers.Add(handlers);
    }
}
