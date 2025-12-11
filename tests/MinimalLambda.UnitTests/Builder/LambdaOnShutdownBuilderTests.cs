using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalLambda.UnitTests.Builder;

[TestSubject(typeof(LambdaOnShutdownBuilder))]
public class LambdaOnShutdownBuilderTests
{
    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException(
        IServiceScopeFactory scopeFactory
    )
    {
        // Act
        var act = () => new LambdaOnShutdownBuilder(null!, scopeFactory);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("serviceProvider");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithNullScopeFactory_ThrowsArgumentNullException(
        IServiceProvider serviceProvider
    )
    {
        // Act
        var act = () => new LambdaOnShutdownBuilder(serviceProvider, null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("scopeFactory");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithValidParameters_Succeeds(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory
    )
    {
        // Act
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);

        // Assert
        builder.Should().NotBeNull();
        builder.Services.Should().Be(serviceProvider);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Services_ReturnsServiceProvider(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory
    )
    {
        // Arrange
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);

        // Act
        var result = builder.Services;

        // Assert
        result.Should().Be(serviceProvider);
    }

    [Theory]
    [AutoNSubstituteData]
    public void ShutdownHandlers_ReturnsReadOnlyList(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory
    )
    {
        // Arrange
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);

        // Act
        var handlers = builder.ShutdownHandlers;

        // Assert
        handlers.Should().NotBeNull();
        handlers.Should().BeEmpty();
        handlers.Should().BeAssignableTo<IReadOnlyList<LambdaShutdownDelegate>>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void OnShutdown_WithNullHandler_ThrowsArgumentNullException(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory
    )
    {
        // Arrange
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);

        // Act
        var act = () => builder.OnShutdown(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("handler");
    }

    [Theory]
    [AutoNSubstituteData]
    public void OnShutdown_WithValidHandler_AddsHandlerAndReturnsBuilder(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory
    )
    {
        // Arrange
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);
        LambdaShutdownDelegate handler = (_, _) => Task.CompletedTask;

        // Act
        var result = builder.OnShutdown(handler);

        // Assert
        result.Should().Be(builder);
        builder.ShutdownHandlers.Should().Contain(handler);
    }

    [Theory]
    [AutoNSubstituteData]
    public void OnShutdown_MultipleHandlers_AllAdded(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory
    )
    {
        // Arrange
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);
        LambdaShutdownDelegate handler1 = (_, _) => Task.CompletedTask;
        LambdaShutdownDelegate handler2 = (_, _) => Task.CompletedTask;
        LambdaShutdownDelegate handler3 = (_, _) => Task.CompletedTask;

        // Act
        builder.OnShutdown(handler1);
        builder.OnShutdown(handler2);
        builder.OnShutdown(handler3);

        // Assert
        builder.ShutdownHandlers.Should().HaveCount(3);
        builder.ShutdownHandlers.Should().Equal(handler1, handler2, handler3);
    }

    [Fact]
    public async Task Build_WithoutHandlers_ReturnsCompletedTaskFunction()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);

        // Act
        var buildFunc = builder.Build();
        var task = buildFunc(CancellationToken.None);

        // Assert
        await task;
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task Build_WithSingleHandler_ExecutesHandler()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);
        var handlerCalled = false;

        LambdaShutdownDelegate handler = (_, _) =>
        {
            handlerCalled = true;
            return Task.CompletedTask;
        };

        builder.OnShutdown(handler);

        // Act
        var buildFunc = builder.Build();
        await buildFunc(CancellationToken.None);

        // Assert
        handlerCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Build_WithMultipleHandlers_AllExecuted()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);
        var handler1Called = false;
        var handler2Called = false;
        var handler3Called = false;

        LambdaShutdownDelegate handler1 = (_, _) =>
        {
            handler1Called = true;
            return Task.CompletedTask;
        };

        LambdaShutdownDelegate handler2 = (_, _) =>
        {
            handler2Called = true;
            return Task.CompletedTask;
        };

        LambdaShutdownDelegate handler3 = (_, _) =>
        {
            handler3Called = true;
            return Task.CompletedTask;
        };

        builder.OnShutdown(handler1);
        builder.OnShutdown(handler2);
        builder.OnShutdown(handler3);

        // Act
        var buildFunc = builder.Build();
        await buildFunc(CancellationToken.None);

        // Assert
        handler1Called.Should().BeTrue();
        handler2Called.Should().BeTrue();
        handler3Called.Should().BeTrue();
    }

    [Fact]
    public async Task Build_WhenHandlerThrowsException_ThrowsAggregateException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);
        var testException = new InvalidOperationException("Test error");

        LambdaShutdownDelegate handler = (_, _) => throw testException;

        builder.OnShutdown(handler);

        // Act
        var buildFunc = builder.Build();
        var act = () => buildFunc(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AggregateException>();
    }

    [Fact]
    public async Task Build_WithMultipleFailures_AggregatesAllErrors()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);
        var exception1 = new InvalidOperationException("Error 1");
        var exception2 = new ArgumentException("Error 2");

        LambdaShutdownDelegate handler1 = (_, _) => throw exception1;

        LambdaShutdownDelegate handler2 = (_, _) => throw exception2;

        builder.OnShutdown(handler1);
        builder.OnShutdown(handler2);

        // Act
        var buildFunc = builder.Build();
        var act = () => buildFunc(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AggregateException>();
    }

    [Fact]
    public async Task Build_WithMixedSuccessAndFailure_AggregatesOnlyErrors()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);
        var successfulHandlerCalled = false;
        var testException = new InvalidOperationException("Test error");

        LambdaShutdownDelegate successHandler = (_, _) =>
        {
            successfulHandlerCalled = true;
            return Task.CompletedTask;
        };

        LambdaShutdownDelegate failingHandler = (_, _) => throw testException;

        builder.OnShutdown(successHandler);
        builder.OnShutdown(failingHandler);

        // Act
        var buildFunc = builder.Build();
        var act = () => buildFunc(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AggregateException>();
        successfulHandlerCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Build_CreatesServiceScopeForEachHandler()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);

        var scopeUsed = false;

        LambdaShutdownDelegate handler = (scope, _) =>
        {
            scopeUsed = scope != serviceProvider;
            return Task.CompletedTask;
        };

        builder.OnShutdown(handler);

        // Act
        var buildFunc = builder.Build();
        await buildFunc(CancellationToken.None);

        // Assert
        scopeUsed.Should().BeTrue();
    }

    [Fact]
    public async Task Build_RespectsCancellationToken()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);

        var cancellationTokenReceived = false;

        LambdaShutdownDelegate handler = (_, cancellationToken) =>
        {
            cancellationTokenReceived = !cancellationToken.IsCancellationRequested;
            return Task.CompletedTask;
        };

        builder.OnShutdown(handler);

        // Act
        var buildFunc = builder.Build();
        using var cts = new CancellationTokenSource();
        await buildFunc(cts.Token);

        // Assert
        cancellationTokenReceived.Should().BeTrue();
    }

    [Fact]
    public async Task Build_WithCancelledToken_RespectsCancellation()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var builder = new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);

        LambdaShutdownDelegate handler = async (_, cancellationToken) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        };

        builder.OnShutdown(handler);

        // Act
        var buildFunc = builder.Build();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var act = () => buildFunc(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
