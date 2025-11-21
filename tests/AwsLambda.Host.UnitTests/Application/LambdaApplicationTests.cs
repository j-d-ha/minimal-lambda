using AwesomeAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AwsLambda.Host.UnitTests.Application;

[TestSubject(typeof(LambdaApplication))]
public class LambdaApplicationTests
{
    private static IHost CreateHostWithServices()
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.ConfigureLambdaHostOptions(_ => { });
            services.AddLambdaHostCoreServices();
            services.TryAddLambdaHostDefaultServices();
        });

        return builder.Build();
    }

    [Fact]
    public void Constructor_WithNullHost_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LambdaApplication(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("host");
    }

    [Fact]
    public void Constructor_WithValidHost_Succeeds()
    {
        // Arrange
        var host = CreateHostWithServices();

        // Act
        var app = new LambdaApplication(host);

        // Assert
        app.Should().NotBeNull();
        app.Services.Should().Be(host.Services);
    }

    [Fact]
    public void Services_ReturnsHostServices()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var services = app.Services;

        // Assert
        services.Should().Be(host.Services);
    }

    [Fact]
    public void Configuration_ReturnsConfiguration()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var result = app.Configuration;

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Configuration_CachesInstance()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var config1 = app.Configuration;
        var config2 = app.Configuration;

        // Assert
        config1.Should().BeSameAs(config2);
    }

    [Fact]
    public void Environment_ReturnsHostEnvironment()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var result = app.Environment;

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Environment_CachesInstance()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var env1 = app.Environment;
        var env2 = app.Environment;

        // Assert
        env1.Should().BeSameAs(env2);
    }

    [Fact]
    public void Lifetime_ReturnsHostApplicationLifetime()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var result = app.Lifetime;

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Lifetime_CachesInstance()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var lifetime1 = app.Lifetime;
        var lifetime2 = app.Lifetime;

        // Assert
        lifetime1.Should().BeSameAs(lifetime2);
    }

    [Fact]
    public void Logger_ReturnsLogger()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var result = app.Logger;

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Logger_CachesInstance()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var logger1 = app.Logger;
        var logger2 = app.Logger;

        // Assert
        logger1.Should().BeSameAs(logger2);
    }

    [Fact]
    public void Logger_WithoutLoggerFactory_ReturnsNullLogger()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var logger = app.Logger;

        // Assert
        logger.Should().NotBeNull();
    }

    [Fact]
    public void StartAsync_ReturnsAwaitableTask()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var task = app.StartAsync();

        // Assert
        task.Should().NotBeNull();
    }

    [Fact]
    public void StartAsync_WithCancellationToken_ReturnsAwaitableTask()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        using var cts = new CancellationTokenSource();
        var task = app.StartAsync(cts.Token);

        // Assert
        task.Should().NotBeNull();
    }

    [Fact]
    public void StopAsync_ReturnsAwaitableTask()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var task = app.StopAsync();

        // Assert
        task.Should().NotBeNull();
    }

    [Fact]
    public void StopAsync_WithCancellationToken_ReturnsAwaitableTask()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        using var cts = new CancellationTokenSource();
        var task = app.StopAsync(cts.Token);

        // Assert
        task.Should().NotBeNull();
    }

    [Fact]
    public void Dispose_DelegatesToHost()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act & Assert
        app.Dispose();
    }

    [Fact]
    public async Task DisposeAsync_DelegatesToHost()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act & Assert
        await app.DisposeAsync();
    }

    [Fact]
    public void Properties_ReturnsInvocationBuilderProperties()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var properties = app.Properties;

        // Assert
        properties.Should().NotBeNull();
        properties.Should().BeAssignableTo<IDictionary<string, object?>>();
    }

    [Fact]
    public void Properties_AllowsAddingItems()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        app.Properties["key"] = "value";

        // Assert
        app.Properties["key"].Should().Be("value");
    }

    [Fact]
    public void Middlewares_ReturnsInvocationBuilderMiddlewares()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var middlewares = app.Middlewares;

        // Assert
        middlewares.Should().NotBeNull();
        middlewares
            .Should()
            .BeAssignableTo<
                IReadOnlyList<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>
            >();
    }

    [Fact]
    public void Middlewares_IsReadOnly()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var middlewares = app.Middlewares;

        // Assert
        middlewares
            .Should()
            .BeAssignableTo<
                IReadOnlyList<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>
            >();
    }

    [Fact]
    public void Handler_InitiallyNull()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var handler = app.Handler;

        // Assert
        handler.Should().BeNull();
    }

    [Fact]
    public void Handler_ReturnsSetHandler()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        LambdaInvocationDelegate expectedHandler = async _ => await Task.CompletedTask;

        // Act
        app.Handle(expectedHandler);
        var handler = app.Handler;

        // Assert
        handler.Should().Be(expectedHandler);
    }

    [Fact]
    public void Handle_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var act = () => app.Handle(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Handle_ReturnsBuilder()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        LambdaInvocationDelegate handler = async _ => await Task.CompletedTask;

        // Act
        var result = app.Handle(handler);

        // Assert
        result.Should().Be(app);
    }

    [Fact]
    public void Use_WithNullMiddleware_ThrowsArgumentNullException()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var act = () => app.Use(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Use_WithValidMiddleware_AddsMiddleware()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware = next =>
            async context => await next(context);

        // Act
        app.Use(middleware);

        // Assert
        app.Middlewares.Should().Contain(middleware);
    }

    [Fact]
    public void Use_ReturnsBuilder()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware = next =>
            async context => await next(context);

        // Act
        var result = app.Use(middleware);

        // Assert
        result.Should().Be(app);
    }

    [Fact]
    public void Use_EnablesMethodChaining()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware = next =>
            async context => await next(context);

        // Act
        var result = app.Use(middleware).Use(middleware);

        // Assert
        result.Should().Be(app);
        app.Middlewares.Should().HaveCount(2);
    }

    [Fact]
    public void Build_ReturnsHandler()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        LambdaInvocationDelegate handler = async _ => await Task.CompletedTask;
        app.Handle(handler);

        // Act
        var result = app.Build();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void InitHandlers_ReturnsHandlersList()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var handlers = app.InitHandlers;

        // Assert
        handlers.Should().NotBeNull();
        handlers.Should().BeAssignableTo<IReadOnlyList<LambdaInitDelegate>>();
    }

    [Fact]
    public void InitHandlers_IsReadOnly()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var handlers = app.InitHandlers;

        // Assert
        handlers.Should().BeAssignableTo<IReadOnlyList<LambdaInitDelegate>>();
    }

    [Fact]
    public void OnInit_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var act = () => app.OnInit(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void OnInit_WithValidHandler_AddsHandler()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        LambdaInitDelegate handler = async (_, _) => true;

        // Act
        app.OnInit(handler);

        // Assert
        app.InitHandlers.Should().Contain(handler);
    }

    [Fact]
    public void OnInit_ReturnsBuilder()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        LambdaInitDelegate handler = async (_, _) => true;

        // Act
        var result = app.OnInit(handler);

        // Assert
        result.Should().Be(app);
    }

    [Fact]
    public void OnInit_EnablesMethodChaining()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        LambdaInitDelegate handler = async (_, _) => true;

        // Act
        var result = app.OnInit(handler).OnInit(handler);

        // Assert
        result.Should().Be(app);
        app.InitHandlers.Should().HaveCount(2);
    }

    [Fact]
    public void ShutdownHandlers_ReturnsHandlersList()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var handlers = app.ShutdownHandlers;

        // Assert
        handlers.Should().NotBeNull();
        handlers.Should().BeAssignableTo<IReadOnlyList<LambdaShutdownDelegate>>();
    }

    [Fact]
    public void ShutdownHandlers_IsReadOnly()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var handlers = app.ShutdownHandlers;

        // Assert
        handlers.Should().BeAssignableTo<IReadOnlyList<LambdaShutdownDelegate>>();
    }

    [Fact]
    public void OnShutdown_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);

        // Act
        var act = () => app.OnShutdown(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void OnShutdown_WithValidHandler_AddsHandler()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        LambdaShutdownDelegate handler = async (_, _) => await Task.CompletedTask;

        // Act
        app.OnShutdown(handler);

        // Assert
        app.ShutdownHandlers.Should().Contain(handler);
    }

    [Fact]
    public void OnShutdown_ReturnsBuilder()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        LambdaShutdownDelegate handler = async (_, _) => await Task.CompletedTask;

        // Act
        var result = app.OnShutdown(handler);

        // Assert
        result.Should().Be(app);
    }

    [Fact]
    public void OnShutdown_EnablesMethodChaining()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        LambdaShutdownDelegate handler = async (_, _) => await Task.CompletedTask;

        // Act
        var result = app.OnShutdown(handler).OnShutdown(handler);

        // Assert
        result.Should().Be(app);
        app.ShutdownHandlers.Should().HaveCount(2);
    }

    [Fact]
    public void AllBuilders_CanBeChainedTogether()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        LambdaInvocationDelegate invocationHandler = async _ => await Task.CompletedTask;
        LambdaInitDelegate initHandler = async (_, _) => true;
        LambdaShutdownDelegate shutdownHandler = async (_, _) => await Task.CompletedTask;

        // Act
        app.Handle(invocationHandler);
        app.OnInit(initHandler);
        app.OnShutdown(shutdownHandler);

        // Assert
        app.Handler.Should().Be(invocationHandler);
        app.InitHandlers.Should().Contain(initHandler);
        app.ShutdownHandlers.Should().Contain(shutdownHandler);
    }
}
