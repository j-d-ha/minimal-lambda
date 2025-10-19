using Lambda.Host.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lambda.Host;

public sealed class LambdaApplication : IHost, ILambdaApplication, IAsyncDisposable
{
    private readonly DelegateHolder _delegateHolder;
    private readonly IHost _host;

    internal LambdaApplication(IHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _delegateHolder =
            Services.GetRequiredService<DelegateHolder>() ?? throw new InvalidOperationException();
    }

    public ValueTask DisposeAsync() => ((IAsyncDisposable)_host).DisposeAsync();

    public void Dispose() => _host.Dispose();

    public Task StartAsync(CancellationToken cancellationToken = default) =>
        _host.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken = default) =>
        _host.StopAsync(cancellationToken);

    public IServiceProvider Services => _host.Services;

    public ILambdaApplication MapHandler(LambdaInvocationDelegate handler)
    {
        if (_delegateHolder.IsHandlerSet)
            throw new InvalidOperationException("Handler is already set");

        _delegateHolder.Handler = handler ?? throw new ArgumentNullException(nameof(handler));

        return this;
    }

    public ILambdaApplication Use(
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware
    )
    {
        _delegateHolder.Middlewares.Add(
            middleware ?? throw new ArgumentNullException(nameof(middleware))
        );

        return this;
    }

    //  ┌──────────────────────────────────────────────────────────┐
    //  │                 Builder Factory Methods                  │
    //  └──────────────────────────────────────────────────────────┘

    /// <summary>
    ///     Initializes a new instance of the <see cref="LambdaApplicationBuilder" /> class with
    ///     pre-configured defaults.
    /// </summary>
    /// <remarks>
    ///     The following defaults are applied to the returned <see cref="LambdaApplicationBuilder" />:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 set the <see cref="IHostEnvironment.ContentRootPath" /> to the result of
    ///                 <see cref="Directory.GetCurrentDirectory()" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 load host <see cref="IConfiguration" /> from "DOTNET_" prefixed
    ///                 environment variables
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 load app <see cref="IConfiguration" /> from 'appsettings.json' and
    ///                 'appsettings.[<see cref="IHostEnvironment.EnvironmentName" />].json'
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 load app <see cref="IConfiguration" /> from User Secrets when
    ///                 <see cref="IHostEnvironment.EnvironmentName" /> is 'Development' using the entry
    ///                 assembly
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>load app <see cref="IConfiguration" /> from environment variables</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 configure the <see cref="ILoggerFactory" /> to log to the console, debug,
    ///                 and event source output
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 enables scope validation on the dependency injection container when
    ///                 <see cref="IHostEnvironment.EnvironmentName" /> is 'Development'
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <returns>The initialized <see cref="LambdaApplicationBuilder" />.</returns>
    public static LambdaApplicationBuilder CreateBuilder() => new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="LambdaApplicationBuilder" /> class with
    ///     pre-configured defaults.
    /// </summary>
    /// <remarks>
    ///     The following defaults are applied to the returned <see cref="LambdaApplicationBuilder" />:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 set the <see cref="IHostEnvironment.ContentRootPath" /> to the result of
    ///                 <see cref="Directory.GetCurrentDirectory()" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 load host <see cref="IConfiguration" /> from "DOTNET_" prefixed
    ///                 environment variables
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>load host <see cref="IConfiguration" /> from supplied command line args</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 load app <see cref="IConfiguration" /> from 'appsettings.json' and
    ///                 'appsettings.[<see cref="IHostEnvironment.EnvironmentName" />].json'
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 load app <see cref="IConfiguration" /> from User Secrets when
    ///                 <see cref="IHostEnvironment.EnvironmentName" /> is 'Development' using the entry
    ///                 assembly
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>load app <see cref="IConfiguration" /> from environment variables</description>
    ///         </item>
    ///         <item>
    ///             <description>load app <see cref="IConfiguration" /> from supplied command line args</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 configure the <see cref="ILoggerFactory" /> to log to the console, debug,
    ///                 and event source output
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 enables scope validation on the dependency injection container when
    ///                 <see cref="IHostEnvironment.EnvironmentName" /> is 'Development'
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <param name="args">The command line args.</param>
    /// <returns>The initialized <see cref="LambdaApplicationBuilder" />.</returns>
    public static LambdaApplicationBuilder CreateBuilder(string[]? args) => new(args);

    /// <inheritdoc cref="CreateBuilder()" />
    /// <param name="settings">
    ///     Controls the initial configuration and other settings for constructing the
    ///     <see cref="LambdaApplicationBuilder" />.
    /// </param>
    public static LambdaApplicationBuilder CreateBuilder(HostApplicationBuilderSettings settings) =>
        new(settings);

    /// <summary>
    ///     Initializes a new instance of the <see cref="LambdaApplicationBuilder" /> class with no
    ///     pre-configured defaults.
    /// </summary>
    /// <param name="settings">
    ///     Controls the initial configuration and other settings for constructing the
    ///     <see cref="HostApplicationBuilder" />.
    /// </param>
    /// <returns>The initialized <see cref="LambdaApplicationBuilder" />.</returns>
    public static LambdaApplicationBuilder CreateEmptyBuilder(
        HostApplicationBuilderSettings settings
    ) => new(settings, true);
}
