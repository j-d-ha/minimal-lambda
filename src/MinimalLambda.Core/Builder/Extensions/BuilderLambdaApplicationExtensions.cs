using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MinimalLambda.Host.Builder;

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
        ///         <item>set the application name from <c>AWS_LAMBDA_FUNCTION_NAME</c> environment variable</item>
        ///         <item>
        ///             set the <see cref="IHostEnvironment.ContentRootPath" /> by checking environment
        ///             variables in order: <c>DOTNET_CONTENTROOT</c>, then <c>AWS_LAMBDA_TASK_ROOT</c>,
        ///             falling back to <see cref="Directory.GetCurrentDirectory()" />
        ///         </item>
        ///         <item>Load <see cref="IConfiguration" /> from "AWS_" prefixed environment variables</item>
        ///         <item>load host <see cref="IConfiguration" /> from "DOTNET_" prefixed environment variables</item>
        ///         <item>
        ///             load app <see cref="IConfiguration" /> from 'appsettings.json' and 'appsettings.[
        ///             <see cref="IHostEnvironment.EnvironmentName" />].json'
        ///         </item>
        ///         <item>
        ///             load app <see cref="IConfiguration" /> from User Secrets when
        ///             <see cref="IHostEnvironment.EnvironmentName" /> is 'Development' using the entry
        ///             assembly
        ///         </item>
        ///         <item>load app <see cref="IConfiguration" /> from environment variables</item>
        ///         <item>
        ///             configure the <see cref="ILoggerFactory" /> to log out to the console, debug, and
        ///             event source output
        ///         </item>
        ///         <item>
        ///             enables scope validation on the dependency injection container when
        ///             <see cref="IHostEnvironment.EnvironmentName" /> is 'Development'
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <returns>The initialized <see cref="LambdaApplicationBuilder" />.</returns>
        public static LambdaApplicationBuilder CreateBuilder() => new(null);

        /// <inheritdoc cref="CreateBuilder()" />
        /// <param name="options">
        ///     Controls the initial configuration and other settings for constructing the
        ///     <see cref="LambdaApplicationBuilder" />. Use this to override the default settings, such as
        ///     disabling defaults entirely, providing a custom configuration, or setting explicit values for
        ///     the application name and content root path.
        /// </param>
        public static LambdaApplicationBuilder CreateBuilder(LambdaApplicationOptions options) =>
            new(options);
    }
}
