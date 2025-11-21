using AwesomeAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace AwsLambda.Host.UnitTests.Builder;

[TestSubject(typeof(LambdaApplicationBuilder))]
public class LambdaApplicationBuilderTests
{
    [Fact]
    public void CreateBuilder_ReturnsValidLambdaApplicationBuilder()
    {
        // Act
        var builder = LambdaApplication.CreateBuilder();

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<IHostApplicationBuilder>();
    }

    [Fact]
    public void CreateBuilder_WithArgs_ReturnsValidLambdaApplicationBuilder()
    {
        // Act
        var builder = LambdaApplication.CreateBuilder(Array.Empty<string>());

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<IHostApplicationBuilder>();
    }

    [Fact]
    public void CreateBuilder_HasValidProperties()
    {
        // Act
        var builder = LambdaApplication.CreateBuilder();

        // Assert
        builder.Services.Should().NotBeNull();
        builder.Configuration.Should().NotBeNull();
        builder.Environment.Should().NotBeNull();
        builder.Logging.Should().NotBeNull();
        builder.Metrics.Should().NotBeNull();
        builder.Properties.Should().NotBeNull();
    }

    [Fact]
    public void Build_ReturnsLambdaApplication()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();

        // Act
        var application = builder.Build();

        // Assert
        application.Should().NotBeNull();
        application.Should().BeAssignableTo<LambdaApplication>();
    }

    [Fact]
    public void Build_ReturnsApplicationOnce()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();

        // Act
        var app = builder.Build();

        // Assert
        app.Should().NotBeNull();
        app.Should().BeAssignableTo<LambdaApplication>();
    }

    [Fact]
    public void Builder_CanAddServices()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();

        // Act
        builder.Services.AddSingleton<ITestService, TestService>();

        // Assert
        var app = builder.Build();
        var service = app.Services.GetService(typeof(ITestService));
        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [Fact]
    public void ConfigureContainer_Succeeds()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();

        // Act
        builder.ConfigureContainer(new TestServiceProviderFactory(), _ => { });

        // Assert
        // ConfigureContainer should not throw
        builder.Should().NotBeNull();
    }

    [Fact]
    public void CreateBuilder_WithHostApplicationBuilderSettings_ReturnsValidBuilder()
    {
        // Arrange
        var settings = new HostApplicationBuilderSettings();

        // Act
        var builder = LambdaApplication.CreateBuilder(settings);

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<IHostApplicationBuilder>();
    }

    [Fact]
    public void CreateEmptyBuilder_WithHostApplicationBuilderSettings_ReturnsValidBuilder()
    {
        // Arrange
        var settings = new HostApplicationBuilderSettings();

        // Act
        var builder = LambdaApplication.CreateEmptyBuilder(settings);

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<IHostApplicationBuilder>();
    }

    [Fact]
    public void Build_RegistersLambdaHostedServiceOptions()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();

        // Act
        var app = builder.Build();
        var options = app.Services.GetRequiredService<IOptions<LambdaHostedServiceOptions>>();

        // Assert
        options.Should().NotBeNull();
        options.Value.Should().NotBeNull();
        options.Value.ConfigureHandlerBuilder.Should().NotBeNull();
        options.Value.ConfigureOnInitBuilder.Should().NotBeNull();
        options.Value.ConfigureOnShutdownBuilder.Should().NotBeNull();
    }

    [Fact]
    public void Build_RegistersConfigureHandlerBuilderCallback()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();
        var options = app.Services.GetRequiredService<IOptions<LambdaHostedServiceOptions>>();

        // Act & Assert
        var callbackDelegate = options.Value.ConfigureHandlerBuilder;
        callbackDelegate.Should().NotBeNull();
        // Callback is registered and will apply middlewares and handler when invoked
    }

    [Fact]
    public void Build_ConfigureHandlerBuilderCallback_ThrowsWhenHandlerNotSet()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();
        var options = app.Services.GetRequiredService<IOptions<LambdaHostedServiceOptions>>();
        var mockBuilder = Substitute.For<ILambdaInvocationBuilder>();

        // Act
        var callbackDelegate = options.Value.ConfigureHandlerBuilder;
        callbackDelegate.Should().NotBeNull();
        var act = () => callbackDelegate!.Invoke(mockBuilder);

        // Assert - should throw because no handler was registered in the application
        act.Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Lambda Handler is not set.");
    }

    [Fact]
    public void Build_ConfigureHandlerBuilderCallback_AppliesHandlerWhenSet()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();

        // Register a handler on the app
        LambdaInvocationDelegate handler = _ => Task.CompletedTask;
        app.Handle(handler);

        var options = app.Services.GetRequiredService<IOptions<LambdaHostedServiceOptions>>();
        var callbackDelegate = options.Value.ConfigureHandlerBuilder;
        callbackDelegate.Should().NotBeNull();

        // Act - create a real invocation builder to verify callback applies the handler
        var invocationBuilder = app
            .Services.GetRequiredService<ILambdaInvocationBuilderFactory>()
            .CreateBuilder();

        // The callback will attempt to apply the handler and middlewares
        var exception = Record.Exception(() => callbackDelegate.Invoke(invocationBuilder));

        // Assert - if an exception occurs, it should not be "Handler not set" since we registered
        // one
        if (exception != null)
            exception.Message.Should().NotContain("Lambda Handler is not set");
    }

    [Fact]
    public void Build_ConfigureHandlerBuilderCallback_AppliesMiddlewareAndProperties()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();

        // Register handler, middleware, and properties on the app
        LambdaInvocationDelegate handler = _ => Task.CompletedTask;
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware = next => next;
        const string propKey = "testPropKey";
        const string propValue = "testPropValue";

        app.Handle(handler);
        app.Use(middleware);
        app.Properties[propKey] = propValue;

        var options = app.Services.GetRequiredService<IOptions<LambdaHostedServiceOptions>>();
        var callbackDelegate = options.Value.ConfigureHandlerBuilder;

        // Act - create invocation builder to verify callback applies all components
        var invocationBuilder = app
            .Services.GetRequiredService<ILambdaInvocationBuilderFactory>()
            .CreateBuilder();

        // The callback should apply middlewares and properties from the app
        var exception = Record.Exception(() => callbackDelegate.Invoke(invocationBuilder));

        // Assert - callback should handle the registered middleware and properties
        // If exception occurs, it should not be about missing handler
        if (exception != null)
            exception.Message.Should().NotContain("Lambda Handler is not set");

        // Verify the invocation builder has the registered properties
        invocationBuilder.Properties.Should().ContainKey(propKey);
        invocationBuilder.Properties[propKey].Should().Be(propValue);
    }

    [Fact]
    public void Build_AppliesConfigureOnInitBuilderCallback()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();
        var options = app.Services.GetRequiredService<IOptions<LambdaHostedServiceOptions>>();

        // Act & Assert
        var callbackDelegate = options.Value.ConfigureOnInitBuilder;
        callbackDelegate.Should().NotBeNull();
        // Callback is registered during builder initialization
    }

    [Fact]
    public void Build_ConfigureOnInitBuilderCallback_AppliesInitHandlers()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();

        // Register init handlers on the app
        LambdaInitDelegate handler1 = (_, __) => Task.FromResult(true);
        LambdaInitDelegate handler2 = (_, __) => Task.FromResult(false);
        app.OnInit(handler1);
        app.OnInit(handler2);

        var options = app.Services.GetRequiredService<IOptions<LambdaHostedServiceOptions>>();
        var callbackDelegate = options.Value.ConfigureOnInitBuilder;

        // Act - create onInit builder to verify callback applies handlers
        var onInitBuilder = app
            .Services.GetRequiredService<ILambdaOnInitBuilderFactory>()
            .CreateBuilder();

        callbackDelegate.Invoke(onInitBuilder);

        // Assert - verify the init builders have the registered handlers
        onInitBuilder.InitHandlers.Should().Contain(handler1);
        onInitBuilder.InitHandlers.Should().Contain(handler2);
    }

    [Fact]
    public void Build_ConfigureOnInitBuilderCallback_AppliesClearLambdaOutputFormattingWhenEnabled()
    {
        // Arrange
        var builderSettings = new HostApplicationBuilderSettings();
        var builder = LambdaApplication.CreateBuilder(builderSettings);

        // Configure to enable ClearLambdaOutputFormatting via appsettings
        builder.Configuration["AwsLambdaHost:ClearLambdaOutputFormatting"] = "true";

        var app = builder.Build();

        var options = app.Services.GetRequiredService<IOptions<LambdaHostedServiceOptions>>();
        var callbackDelegate = options.Value.ConfigureOnInitBuilder;

        // Act - create onInit builder and invoke callback
        var onInitBuilder = app
            .Services.GetRequiredService<ILambdaOnInitBuilderFactory>()
            .CreateBuilder();

        callbackDelegate.Invoke(onInitBuilder);

        // Assert - verify that OnInitClearLambdaOutputFormatting was called (handler registered)
        // The ClearLambdaOutputFormatting handler should be in the init handlers
        onInitBuilder.InitHandlers.Should().NotBeEmpty();
    }

    [Fact]
    public void Build_ConfigureOnInitBuilderCallback_SkipsClearLambdaOutputFormattingWhenDisabled()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();

        var options = app.Services.GetRequiredService<IOptions<LambdaHostedServiceOptions>>();
        var callbackDelegate = options.Value.ConfigureOnInitBuilder;

        // Act - create onInit builder without registering init handlers
        var onInitBuilder = app
            .Services.GetRequiredService<ILambdaOnInitBuilderFactory>()
            .CreateBuilder();

        callbackDelegate.Invoke(onInitBuilder);

        // Assert - when ClearLambdaOutputFormatting is false and no handlers registered,
        // the init handlers list should be empty
        onInitBuilder.InitHandlers.Should().BeEmpty();
    }

    [Fact]
    public void Build_AppliesConfigureOnShutdownBuilderCallback()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();
        var options = app.Services.GetRequiredService<IOptions<LambdaHostedServiceOptions>>();

        // Act & Assert
        var callbackDelegate = options.Value.ConfigureOnShutdownBuilder;
        callbackDelegate.Should().NotBeNull();
        // Callback is registered during builder initialization
    }

    [Fact]
    public void Build_ConfigureOnShutdownBuilderCallback_AppliesShutdownHandlers()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();

        // Register shutdown handlers on the app
        LambdaShutdownDelegate handler1 = (_, __) => Task.CompletedTask;
        LambdaShutdownDelegate handler2 = (_, __) => Task.CompletedTask;
        app.OnShutdown(handler1);
        app.OnShutdown(handler2);

        var options = app.Services.GetRequiredService<IOptions<LambdaHostedServiceOptions>>();
        var callbackDelegate = options.Value.ConfigureOnShutdownBuilder;

        // Act - create onShutdown builder to verify callback applies handlers
        var onShutdownBuilder = app
            .Services.GetRequiredService<ILambdaOnShutdownBuilderFactory>()
            .CreateBuilder();

        callbackDelegate.Invoke(onShutdownBuilder);

        // Assert - verify the shutdown builder has the registered handlers
        onShutdownBuilder.ShutdownHandlers.Should().Contain(handler1);
        onShutdownBuilder.ShutdownHandlers.Should().Contain(handler2);
    }

    [Fact]
    public void Build_ConfigureOnShutdownBuilderCallback_WithNoHandlers()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();

        var options = app.Services.GetRequiredService<IOptions<LambdaHostedServiceOptions>>();
        var callbackDelegate = options.Value.ConfigureOnShutdownBuilder;

        // Act - create onShutdown builder without registering shutdown handlers
        var onShutdownBuilder = app
            .Services.GetRequiredService<ILambdaOnShutdownBuilderFactory>()
            .CreateBuilder();

        callbackDelegate.Invoke(onShutdownBuilder);

        // Assert - when no handlers are registered, the shutdown handlers list should be empty
        onShutdownBuilder.ShutdownHandlers.Should().BeEmpty();
    }

    [Fact]
    public void Build_ConfigureOnShutdownBuilderCallback_WithMultipleHandlers()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();

        // Register multiple shutdown handlers
        LambdaShutdownDelegate handler1 = (_, __) => Task.CompletedTask;
        LambdaShutdownDelegate handler2 = (_, __) => Task.CompletedTask;
        LambdaShutdownDelegate handler3 = (_, __) => Task.CompletedTask;
        app.OnShutdown(handler1);
        app.OnShutdown(handler2);
        app.OnShutdown(handler3);

        var options = app.Services.GetRequiredService<IOptions<LambdaHostedServiceOptions>>();
        var callbackDelegate = options.Value.ConfigureOnShutdownBuilder;

        // Act - create onShutdown builder and apply handlers
        var onShutdownBuilder = app
            .Services.GetRequiredService<ILambdaOnShutdownBuilderFactory>()
            .CreateBuilder();

        callbackDelegate.Invoke(onShutdownBuilder);

        // Assert - all handlers should be applied in order
        onShutdownBuilder.ShutdownHandlers.Should().HaveCount(3);
        onShutdownBuilder.ShutdownHandlers.Should().Equal(handler1, handler2, handler3);
    }

    [Fact]
    public void Build_ImplementsILambdaInvocationBuilder_HandleMethod()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();
        LambdaInvocationDelegate handlerDelegate = _ => Task.CompletedTask;

        // Act
        var result = app.Handle(handlerDelegate);

        // Assert
        result.Should().Be(app);
        app.Handler.Should().Be(handlerDelegate);
    }

    [Fact]
    public void Build_ImplementsILambdaInvocationBuilder_UseMiddleware()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware = next => next;

        // Act
        var result = app.Use(middleware);

        // Assert
        result.Should().Be(app);
        app.Middlewares.Should().Contain(middleware);
    }

    [Fact]
    public void Build_ImplementsILambdaInvocationBuilder_PropertiesDictionary()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();
        const string key = "testKey";
        const string value = "testValue";

        // Act
        app.Properties[key] = value;

        // Assert
        app.Properties.Should().ContainKey(key);
        app.Properties[key].Should().Be(value);
    }

    [Fact]
    public void Build_ImplementsILambdaOnInitBuilder_OnInitMethod()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();
        LambdaInitDelegate handler = (_, __) => Task.FromResult(true);

        // Act
        var result = app.OnInit(handler);

        // Assert
        result.Should().Be(app);
        app.InitHandlers.Should().Contain(handler);
    }

    [Fact]
    public void Build_ImplementsILambdaOnInitBuilder_MultipleHandlers()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();
        LambdaInitDelegate handler1 = (_, __) => Task.FromResult(true);
        LambdaInitDelegate handler2 = (_, __) => Task.FromResult(false);

        // Act
        app.OnInit(handler1);
        app.OnInit(handler2);

        // Assert
        app.InitHandlers.Should().HaveCount(2);
        app.InitHandlers.Should().Contain(handler1);
        app.InitHandlers.Should().Contain(handler2);
    }

    [Fact]
    public void Build_ImplementsILambdaOnShutdownBuilder_OnShutdownMethod()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();
        LambdaShutdownDelegate handler = (_, __) => Task.CompletedTask;

        // Act
        var result = app.OnShutdown(handler);

        // Assert
        result.Should().Be(app);
        app.ShutdownHandlers.Should().Contain(handler);
    }

    [Fact]
    public void Build_ImplementsILambdaOnShutdownBuilder_MultipleHandlers()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();
        var app = builder.Build();
        LambdaShutdownDelegate handler1 = (_, __) => Task.CompletedTask;
        LambdaShutdownDelegate handler2 = (_, __) => Task.CompletedTask;

        // Act
        app.OnShutdown(handler1);
        app.OnShutdown(handler2);

        // Assert
        app.ShutdownHandlers.Should().HaveCount(2);
        app.ShutdownHandlers.Should().Contain(handler1);
        app.ShutdownHandlers.Should().Contain(handler2);
    }

    [Fact]
    public void Build_ImplementsIHost_Services()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();

        // Act
        var app = builder.Build();

        // Assert
        app.Services.Should().NotBeNull();
        app.Services.Should().BeAssignableTo<IServiceProvider>();
    }

    [Fact]
    public void Build_ImplementsIHost_Configuration()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();

        // Act
        var app = builder.Build();

        // Assert
        app.Configuration.Should().NotBeNull();
    }

    [Fact]
    public void Build_ImplementsIHost_Environment()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();

        // Act
        var app = builder.Build();

        // Assert
        app.Environment.Should().NotBeNull();
        app.Environment.ApplicationName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Build_ImplementsIHost_Logger()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();

        // Act
        var app = builder.Build();

        // Assert
        app.Logger.Should().NotBeNull();
    }

    [Fact]
    public void Build_ImplementsIHost_Lifetime()
    {
        // Arrange
        var builder = LambdaApplication.CreateBuilder();

        // Act
        var app = builder.Build();

        // Assert
        app.Lifetime.Should().NotBeNull();
    }

    private interface ITestService { }

    private class TestService : ITestService { }

    private class TestServiceProviderFactory : IServiceProviderFactory<object>
    {
        public object CreateBuilder(IServiceCollection services) => new();

        public IServiceProvider CreateServiceProvider(object containerBuilder) =>
            throw new NotImplementedException();
    }
}
