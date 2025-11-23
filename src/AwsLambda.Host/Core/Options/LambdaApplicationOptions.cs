// Portions of this file are derived from dotnet
// Source:
// https://github.com/dotnet/dotnet/blob/v10.0.100/src/runtime/src/libraries/Microsoft.Extensions.Hosting/src/HostApplicationBuilderSettings.cs
// Copyright (c) .NET Foundation
// Licensed under the MIT License
// See THIRD-PARTY-LICENSES.txt file in the project root or visit
// https://github.com/dotnet/dotnet/blob/v10.0.100/LICENSE.TXT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AwsLambda.Host.Builder;

/// <summary>Options for configuring the <see cref="LambdaApplicationBuilder" />.</summary>
public class LambdaApplicationOptions
{
    /// <summary>Gets or sets the application name.</summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    ///     Gets or sets the command-line arguments to add to the
    ///     <see cref="LambdaApplicationBuilder.Configuration" />.
    /// </summary>
    public string[]? Args { get; set; }

    /// <summary>
    ///     Gets or sets the initial configuration sources to be added to the
    ///     <see cref="LambdaApplicationBuilder.Configuration" />. These sources can influence the
    ///     <see cref="LambdaApplicationBuilder.Environment" /> through the use of
    ///     <see cref="HostDefaults" /> keys. Disposing the built <see cref="IHost" /> disposes the
    ///     <see cref="ConfigurationManager" />.
    /// </summary>
    public ConfigurationManager? Configuration { get; set; }

    /// <summary>Gets or sets the content root path.</summary>
    public string? ContentRootPath { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the <see cref="LambdaApplicationBuilder" />
    ///     instance is configured with pre-configured defaults.
    /// </summary>
    /// <value>
    ///     <see langword="false" /> if the <see cref="LambdaApplicationBuilder" /> instance is configured
    ///     with pre-configured defaults. This is the default value.
    /// </value>
    /// <remarks>
    ///     The following defaults are applied to the <see cref="IHostBuilder" />:
    ///     <list type="bullet">
    ///         <item>
    ///             Set the <see cref="IHostEnvironment.ContentRootPath" /> to the result of
    ///             <see cref="Directory.GetCurrentDirectory()" />
    ///         </item>
    ///         <item>Load <see cref="IConfiguration" /> from "AWS_" prefixed environment variables</item>
    ///         <item>Load <see cref="IConfiguration" /> from "DOTNET_" prefixed environment variables</item>
    ///         <item>
    ///             Load <see cref="IConfiguration" /> from 'appsettings.json' and 'appsettings.[
    ///             <see cref="IHostEnvironment.EnvironmentName" />].json'
    ///         </item>
    ///         <item>
    ///             Load <see cref="IConfiguration" /> from User Secrets when
    ///             <see cref="IHostEnvironment.EnvironmentName" /> is 'Development' using the entry
    ///             assembly
    ///         </item>
    ///         <item>Load <see cref="IConfiguration" /> from environment variables</item>
    ///         <item>Load <see cref="IConfiguration" /> from supplied command line args</item>
    ///         <item>
    ///             Configure the <see cref="ILoggerFactory" /> to log to the console
    ///         </item>
    ///         <item>
    ///             Enable scope validation on the dependency injection container when
    ///             <see cref="IHostEnvironment.EnvironmentName" /> is 'Development'
    ///         </item>
    ///     </list>
    /// </remarks>
    public bool DisableDefaults { get; set; } = false;

    /// <summary>Gets or sets the environment name.</summary>
    public string? EnvironmentName { get; set; }
}
