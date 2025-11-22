using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AwsLambda.Host.Builder;

/// <summary>
///     Provides extension methods on <see cref="LambdaApplication" /> for creating and
///     configuring <see cref="LambdaApplicationBuilder" /> instances.
/// </summary>
/// <remarks>
///     <para>
///         These extensions are the primary entry points for initializing a Lambda application. Use
///         <c>CreateBuilder()</c> to create a builder with standard defaults (configuration, logging,
///         dependency injection), or <c>CreateEmptyBuilder()</c> for minimal setup.
///     </para>
/// </remarks>
public static class BuilderLambdaApplicationExtensions
{
    extension(LambdaApplication)
    {
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
        ///         <item><description>load app <see cref="IConfiguration" /> from environment variables</description></item>
        ///         <item>
        ///             <description>
        ///                 configure the <see cref="ILoggerFactory" /> to log out to the console,
        ///                 debug, and event source output
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
        ///         <item><description>load host <see cref="IConfiguration" /> from supplied command line args</description></item>
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
        ///         <item><description>load app <see cref="IConfiguration" /> from environment variables</description></item>
        ///         <item><description>load app <see cref="IConfiguration" /> from supplied command line args</description></item>
        ///         <item>
        ///             <description>
        ///                 configure the <see cref="ILoggerFactory" /> to log to console, debug, and
        ///                 event source output
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
        public static LambdaApplicationBuilder CreateBuilder(
            HostApplicationBuilderSettings settings
        ) => new(settings);

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
}
