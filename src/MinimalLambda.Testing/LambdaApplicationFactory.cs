// Portions of this file are derived from aspnetcore
// Source:
// https://github.com/dotnet/aspnetcore/blob/v10.0.0/src/Mvc/Mvc.Testing/src/LambdaApplicationFactory.cs
// Copyright (c) .NET Foundation and Contributors
// Licensed under the MIT License
// See THIRD-PARTY-LICENSES.txt file in the project root or visit
// https://github.com/dotnet/aspnetcore/blob/v10.0.0/LICENSE.txt

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using MinimalLambda.Builder.Extensions;
using MinimalLambda.Options;

namespace MinimalLambda.Testing;

/// <summary>
/// Factory for bootstrapping an application in memory for functional end to end tests.
/// </summary>
/// <typeparam name="TEntryPoint">A type in the entry point assembly of the application.
/// Typically the Startup or Program classes can be used.</typeparam>
public class LambdaApplicationFactory<TEntryPoint> : IDisposable, IAsyncDisposable
    where TEntryPoint : class
{
    private readonly List<LambdaApplicationFactory<TEntryPoint>> _derivedFactories = [];
    private Action<IHostBuilder> _configuration;
    private bool _disposed;
    private bool _disposedAsync;
    private IHost? _host;
    private LambdaTestServer? _server;

    /// <summary>
    /// <para>
    /// Creates an instance of <see cref="LambdaApplicationFactory{TEntryPoint}"/>. This factory can be used to
    /// create a <see cref="LambdaTestServer"/> instance using the MVC application defined by <typeparamref name="TEntryPoint"/>
    /// and one or more <see cref="HttpClient"/> instances used to send <see cref="HttpRequestMessage"/> to the <see cref="LambdaTestServer"/>.
    /// The <see cref="LambdaApplicationFactory{TEntryPoint}"/> will find the entry point class of <typeparamref name="TEntryPoint"/>
    /// assembly and initialize the application by calling <c>IHostBuilder CreateWebHostBuilder(string [] args)</c>
    /// on <typeparamref name="TEntryPoint"/>.
    /// </para>
    /// <para>
    /// This constructor will infer the application content root path by searching for a
    /// <see cref="LambdaApplicationFactoryContentRootAttribute"/> on the assembly containing the functional tests with
    /// a key equal to the <typeparamref name="TEntryPoint"/> assembly <see cref="Assembly.FullName"/>.
    /// In case an attribute with the right key can't be found, <see cref="LambdaApplicationFactory{TEntryPoint}"/>
    /// will fall back to searching for a solution file (*.sln) and then appending <typeparamref name="TEntryPoint"/> assembly name
    /// to the solution directory. The application root directory will be used to discover views and content files.
    /// </para>
    /// <para>
    /// The application assemblies will be loaded from the dependency context of the assembly containing
    /// <typeparamref name="TEntryPoint" />. This means that project dependencies of the assembly containing
    /// <typeparamref name="TEntryPoint" /> will be loaded as application assemblies.
    /// </para>
    /// </summary>
    public LambdaApplicationFactory() => _configuration = ConfigureWebHost;

    /// <summary>
    /// Gets the <see cref="WebApplicationFactoryClientOptions"/> used by <see cref="CreateClient()"/>.
    /// </summary>
    public LambdaApplicationFactoryClientOptions ClientOptions { get; private set; } = new();

    /// <summary>
    /// Gets the <see cref="IReadOnlyList{LambdaApplicationFactory}"/> of factories created from this factory
    /// by further customizing the <see cref="IHostBuilder"/> when calling
    /// <see cref="WithHostBuilder"/>.
    /// </summary>
    public IReadOnlyList<LambdaApplicationFactory<TEntryPoint>> Factories =>
        _derivedFactories.AsReadOnly();

    /// <summary>
    /// Gets the <see cref="LambdaTestServer"/> created by this <see cref="LambdaApplicationFactory{TEntryPoint}"/>.
    /// </summary>
    public LambdaTestServer TestServer
    {
        get
        {
            EnsureServer();
            return _server;
        }
    }

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> created by the server associated with this <see cref="LambdaApplicationFactory{TEntryPoint}"/>.
    /// </summary>
    public virtual IServiceProvider Services => TestServer.Services;

    /// <inheritdoc />
    public virtual async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        if (_disposedAsync)
            return;

        foreach (var factory in _derivedFactories)
            await ((IAsyncDisposable)factory).DisposeAsync().ConfigureAwait(false);

        // TestServer handles disposing both processor and host
        if (_server != null)
            await _server.DisposeAsync().ConfigureAwait(false);

        _disposedAsync = true;

        Dispose(true);

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="LambdaApplicationFactory{TEntryPoint}"/> class.
    /// </summary>
    ~LambdaApplicationFactory() => Dispose(false);

    /// <summary>
    /// Creates a new <see cref="LambdaApplicationFactory{TEntryPoint}"/> with a <see cref="IHostBuilder"/>
    /// that is further customized by <paramref name="configuration"/>.
    /// </summary>
    /// <param name="configuration">
    /// An <see cref="Action{IHostBuilder}"/> to configure the <see cref="IHostBuilder"/>.
    /// </param>
    /// <returns>A new <see cref="LambdaApplicationFactory{TEntryPoint}"/>.</returns>
    public LambdaApplicationFactory<TEntryPoint> WithHostBuilder(
        Action<IHostBuilder> configuration
    ) => WithHostBuilderCore(configuration);

    internal virtual LambdaApplicationFactory<TEntryPoint> WithHostBuilderCore(
        Action<IHostBuilder> configuration
    )
    {
        var factory = new DelegatedLambdaApplicationFactory(
            ClientOptions,
            CreateHost,
            CreateHostBuilder,
            GetTestAssemblies,
            builder =>
            {
                _configuration(builder);
                configuration(builder);
            }
        );

        _derivedFactories.Add(factory);

        return factory;
    }

    [MemberNotNull(nameof(_server))]
    private void EnsureServer()
    {
        if (_server != null)
            return;

        EnsureDepsFile();

        var deferredHostBuilder = new DeferredHostBuilder();
        deferredHostBuilder.UseEnvironment(Environments.Development);
        // There's no helper for UseApplicationName, but we need to
        // set the application name to the target entry point
        // assembly name.
        deferredHostBuilder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    {
                        HostDefaults.ApplicationKey,
                        typeof(TEntryPoint).Assembly.GetName().Name ?? string.Empty
                    },
                }
            );
        });
        // This helper call does the hard work to determine if we can fallback to diagnostic
        // source events to get the host instance
        var factory = HostFactoryResolver.ResolveHostFactory(
            typeof(TEntryPoint).Assembly,
            stopApplication: false,
            configureHostBuilder: deferredHostBuilder.ConfigureHostBuilder,
            entrypointCompleted: deferredHostBuilder.EntryPointCompleted
        );

        if (factory is not null)
        {
            // If we have a valid factory it means the specified entry point's assembly can
            // potentially resolve the IHost
            // so we set the factory on the DeferredHostBuilder so we can invoke it on the call
            // to IHostBuilder.Build.
            deferredHostBuilder.SetHostFactory(factory);

            ConfigureHostBuilder(deferredHostBuilder, deferredHostBuilder.EntryPointCompletion);
            return;
        }

        throw new InvalidOperationException("Unable to create IHostBuilder instance.");
    }

    [MemberNotNull(nameof(_server))]
    private void ConfigureHostBuilder(
        IHostBuilder hostBuilder,
        Task<Exception?> entryPointCompletion
    )
    {
        SetContentRoot(hostBuilder);
        _configuration(hostBuilder);

        _server = new LambdaTestServer(entryPointCompletion);

        // set Lambda Bootstrap Http Client
        hostBuilder.ConfigureServices(services =>
        {
            services.AddLambdaBootstrapHttpClient(new HttpClient(_server.CreateHandler()));

            services.PostConfigure<LambdaHostOptions>(options =>
            {
                if (string.IsNullOrEmpty(options.BootstrapOptions.RuntimeApiEndpoint))
                    options.BootstrapOptions.RuntimeApiEndpoint = "localhost";
            });
        });

        // Build the host but DON'T start it - server will start it
        _host = CreateHost(hostBuilder);
        _server.SetHost(_host);

        // Create the public server with the built (but not started) host
        // _server = new LambdaTestServer(_host, processor, entryPointCompletion:
        // entryPointCompletion);
    }

    private void SetContentRoot(IHostBuilder builder)
    {
        var contentRoot = GetContentRootFromAssembly();

        if (contentRoot != null)
            builder.UseContentRoot(contentRoot);
        else
            UseSolutionRelativeContentRoot(builder, typeof(TEntryPoint).Assembly.GetName().Name!);
    }

    private static void UseSolutionRelativeContentRoot(
        IHostBuilder builder,
        string solutionRelativePath
    )
    {
        ArgumentNullException.ThrowIfNull(solutionRelativePath);

        var applicationBasePath = AppContext.BaseDirectory;
        string[] solutionNames = ["*.sln", "*.slnx"];

        var directoryInfo = new DirectoryInfo(applicationBasePath);
        do
        {
            foreach (var solutionName in solutionNames)
            {
                var solutionPath = Directory
                    .EnumerateFiles(directoryInfo.FullName, solutionName)
                    .FirstOrDefault();
                if (solutionPath != null)
                {
                    builder.UseContentRoot(
                        Path.GetFullPath(Path.Combine(directoryInfo.FullName, solutionRelativePath))
                    );
                    return;
                }
            }

            directoryInfo = directoryInfo.Parent;
        } while (directoryInfo is not null);

        throw new InvalidOperationException(
            $"Solution root could not be located using application root {applicationBasePath}."
        );
    }

    private string? GetContentRootFromAssembly()
    {
        var metadataAttributes = GetContentRootMetadataAttributes(
            typeof(TEntryPoint).Assembly.FullName!,
            typeof(TEntryPoint).Assembly.GetName().Name!
        );

        string? contentRoot = null;
        foreach (var contentRootAttribute in metadataAttributes)
        {
            var contentRootCandidate = Path.Combine(
                AppContext.BaseDirectory,
                contentRootAttribute.ContentRootPath
            );

            var contentRootMarker = Path.Combine(
                contentRootCandidate,
                Path.GetFileName(contentRootAttribute.ContentRootTest)
            );

            if (File.Exists(contentRootMarker))
            {
                contentRoot = contentRootCandidate;
                break;
            }
        }

        return contentRoot;
    }

    private LambdaApplicationFactoryContentRootAttribute[] GetContentRootMetadataAttributes(
        string tEntryPointAssemblyFullName,
        string tEntryPointAssemblyName
    )
    {
        var testAssembly = GetTestAssemblies();
        var metadataAttributes = testAssembly
            .SelectMany(a => a.GetCustomAttributes<LambdaApplicationFactoryContentRootAttribute>())
            .Where(a =>
                string.Equals(
                    a.Key,
                    tEntryPointAssemblyFullName,
                    StringComparison.OrdinalIgnoreCase
                )
                || string.Equals(a.Key, tEntryPointAssemblyName, StringComparison.OrdinalIgnoreCase)
            )
            .OrderBy(a => a.Priority)
            .ToArray();

        return metadataAttributes;
    }

    /// <summary>
    /// Gets the assemblies containing the functional tests. The
    /// <see cref="LambdaApplicationFactoryContentRootAttribute"/> applied to these
    /// assemblies defines the content root to use for the given
    /// <typeparamref name="TEntryPoint"/>.
    /// </summary>
    /// <returns>The list of <see cref="Assembly"/> containing tests.</returns>
    protected virtual IEnumerable<Assembly> GetTestAssemblies()
    {
        try
        {
            // The default dependency context will be populated in .net core applications.
            var context = DependencyContext.Default;
            if (context == null || context.CompileLibraries.Count == 0)
                // The app domain friendly name will be populated in full framework.
                return [Assembly.Load(AppDomain.CurrentDomain.FriendlyName)];

            var runtimeProjectLibraries = context.RuntimeLibraries.ToDictionary(
                r => r.Name,
                r => r,
                StringComparer.Ordinal
            );

            // Find the list of projects
            var projects = context.CompileLibraries.Where(l => l.Type == "project");

            var entryPointAssemblyName = typeof(TEntryPoint).Assembly.GetName().Name;

            // Find the list of projects referencing TEntryPoint.
            var candidates = context.CompileLibraries.Where(library =>
                library.Dependencies.Any(d =>
                    string.Equals(d.Name, entryPointAssemblyName, StringComparison.Ordinal)
                )
            );

            var testAssemblies = new List<Assembly>();
            foreach (var candidate in candidates)
                if (runtimeProjectLibraries.TryGetValue(candidate.Name, out var runtimeLibrary))
                {
                    var runtimeAssemblies = runtimeLibrary.GetDefaultAssemblyNames(context);
                    testAssemblies.AddRange(runtimeAssemblies.Select(Assembly.Load));
                }

            return testAssemblies;
        }
        catch (Exception)
        {
            // Ignore
        }

        return [];
    }

    private static void EnsureDepsFile()
    {
        if (typeof(TEntryPoint).Assembly.EntryPoint == null)
            throw new InvalidOperationException(
                $"Invalid assembly entry point: {typeof(TEntryPoint).Assembly.FullName}"
            );

        var depsFileName = $"{typeof(TEntryPoint).Assembly.GetName().Name}.deps.json";
        var depsFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, depsFileName));
        if (!depsFile.Exists)
            throw new InvalidOperationException($"Missing deps file: {depsFile.FullName}");
    }

    /// <summary>
    /// Creates a <see cref="IHostBuilder"/> used to set up <see cref="LambdaTestServer"/>.
    /// </summary>
    /// <remarks>
    /// The default implementation of this method looks for a <c>public static IHostBuilder CreateHostBuilder(string[] args)</c>
    /// method defined on the entry point of the assembly of <typeparamref name="TEntryPoint" /> and invokes it passing an empty string
    /// array as arguments.
    /// </remarks>
    /// <returns>A <see cref="IHostBuilder"/> instance.</returns>
    protected virtual IHostBuilder? CreateHostBuilder()
    {
        var hostBuilder = HostFactoryResolver
            .ResolveHostBuilderFactory<IHostBuilder>(typeof(TEntryPoint).Assembly)
            ?.Invoke(Array.Empty<string>());

        hostBuilder?.UseEnvironment(Environments.Development);
        return hostBuilder;
    }

    /// <summary>
    /// Creates the <see cref="IHost"/> with the bootstrapped application in <paramref name="builder"/>.
    /// This is only called for applications using <see cref="IHostBuilder"/>. Applications based on
    /// <see cref="IHostBuilder"/> will use <see cref="CreateServer"/> instead.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder"/> used to create the host.</param>
    /// <returns>The <see cref="IHost"/> with the bootstrapped application.</returns>
    protected virtual IHost CreateHost(IHostBuilder builder) =>
        // Build the host but DON'T start it - LambdaTestServer.StartAsync() will start it
        builder.Build();

    /// <summary>
    /// Gives a fixture an opportunity to configure the application before it gets built.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder"/> for the application.</param>
    protected virtual void ConfigureWebHost(IHostBuilder builder) { }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true" /> to release both managed and unmanaged resources;
    /// <see langword="false" /> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing && !_disposedAsync)
            DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();

        _disposed = true;
    }

    private sealed class DelegatedLambdaApplicationFactory : LambdaApplicationFactory<TEntryPoint>
    {
        private readonly Func<IHostBuilder, IHost> _createHost;
        private readonly Func<IHostBuilder?> _createHostBuilder;
        private readonly Func<IEnumerable<Assembly>> _getTestAssemblies;

        public DelegatedLambdaApplicationFactory(
            LambdaApplicationFactoryClientOptions options,
            Func<IHostBuilder, IHost> createHost,
            Func<IHostBuilder?> createHostBuilder,
            Func<IEnumerable<Assembly>> getTestAssemblies,
            Action<IHostBuilder> configureWebHost
        )
        {
            ClientOptions = options;
            _createHost = createHost;
            _createHostBuilder = createHostBuilder;
            _getTestAssemblies = getTestAssemblies;
            _configuration = configureWebHost;
        }

        protected override IHost CreateHost(IHostBuilder builder) => _createHost(builder);

        protected override IHostBuilder? CreateHostBuilder() => _createHostBuilder();

        protected override IEnumerable<Assembly> GetTestAssemblies() => _getTestAssemblies();

        protected override void ConfigureWebHost(IHostBuilder builder) => _configuration(builder);

        internal override LambdaApplicationFactory<TEntryPoint> WithHostBuilderCore(
            Action<IHostBuilder> configuration
        ) =>
            new DelegatedLambdaApplicationFactory(
                ClientOptions,
                _createHost,
                _createHostBuilder,
                _getTestAssemblies,
                builder =>
                {
                    _configuration(builder);
                    configuration(builder);
                }
            );
    }
}
