using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MinimalLambda.UnitTests.Builder;

[TestSubject(typeof(LambdaOnInitBuilder))]
public class LambdaOnInitBuilderTests
{
    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException(
        IServiceScopeFactory scopeFactory,
        IOptions<LambdaHostOptions> options
    )
    {
        // Act
        var act = () => new LambdaOnInitBuilder(null!, scopeFactory, options);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("serviceProvider");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithNullScopeFactory_ThrowsArgumentNullException(
        IServiceProvider serviceProvider,
        IOptions<LambdaHostOptions> options
    )
    {
        // Act
        var act = () => new LambdaOnInitBuilder(serviceProvider, null!, options);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("scopeFactory");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory
    )
    {
        // Act
        var act = () => new LambdaOnInitBuilder(serviceProvider, scopeFactory, null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("options");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithValidParameters_Succeeds(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<LambdaHostOptions> options
    )
    {
        // Act
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, options);

        // Assert
        builder.Should().NotBeNull();
        builder.Services.Should().Be(serviceProvider);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Services_ReturnsServiceProvider(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<LambdaHostOptions> options
    )
    {
        // Arrange
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, options);

        // Act
        var result = builder.Services;

        // Assert
        result.Should().Be(serviceProvider);
    }

    [Theory]
    [AutoNSubstituteData]
    public void InitHandlers_ReturnsReadOnlyList(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<LambdaHostOptions> options
    )
    {
        // Arrange
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, options);

        // Act
        var handlers = builder.InitHandlers;

        // Assert
        handlers.Should().NotBeNull();
        handlers.Should().BeEmpty();
        handlers.Should().BeAssignableTo<IReadOnlyList<LambdaInitDelegate>>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void OnInit_WithNullHandler_ThrowsArgumentNullException(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<LambdaHostOptions> options
    )
    {
        // Arrange
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, options);

        // Act
        var act = () => builder.OnInit(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("handler");
    }

    [Theory]
    [AutoNSubstituteData]
    public void OnInit_WithValidHandler_AddsHandlerAndReturnsBuilder(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<LambdaHostOptions> options
    )
    {
        // Arrange
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, options);
        LambdaInitDelegate handler = (_, _) => Task.FromResult(true);

        // Act
        var result = builder.OnInit(handler);

        // Assert
        result.Should().Be(builder);
        builder.InitHandlers.Should().Contain(handler);
    }

    [Theory]
    [AutoNSubstituteData]
    public void OnInit_MultipleHandlers_AllAdded(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<LambdaHostOptions> options
    )
    {
        // Arrange
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, options);
        LambdaInitDelegate handler1 = (_, _) => Task.FromResult(true);
        LambdaInitDelegate handler2 = (_, _) => Task.FromResult(true);
        LambdaInitDelegate handler3 = (_, _) => Task.FromResult(true);

        // Act
        builder.OnInit(handler1);
        builder.OnInit(handler2);
        builder.OnInit(handler3);

        // Assert
        builder.InitHandlers.Should().HaveCount(3);
        builder.InitHandlers.Should().Equal(handler1, handler2, handler3);
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Build_WithoutHandlers_ReturnsAlwaysTrueFunction(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<LambdaHostOptions> options
    )
    {
        // Arrange
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, options);

        // Act
        var buildFunc = builder.Build();
        var result = await buildFunc(CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Build_WithSingleSuccessfulHandler_ReturnsTrue()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var lambdaHostOptions = Microsoft.Extensions.Options.Options.Create(
            new LambdaHostOptions()
        );
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, lambdaHostOptions);
        var handlerCalled = false;

        LambdaInitDelegate handler = (_, _) =>
        {
            handlerCalled = true;
            return Task.FromResult(true);
        };

        builder.OnInit(handler);

        // Act
        var buildFunc = builder.Build();
        var result = await buildFunc(CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        handlerCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Build_WithHandlerReturnsFalse_ReturnsFalse()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var lambdaHostOptions = Microsoft.Extensions.Options.Options.Create(
            new LambdaHostOptions()
        );
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, lambdaHostOptions);

        LambdaInitDelegate handler = (_, _) => Task.FromResult(false);

        builder.OnInit(handler);

        // Act
        var buildFunc = builder.Build();
        var result = await buildFunc(CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Build_WithMultipleHandlers_AllExecuted()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var lambdaHostOptions = Microsoft.Extensions.Options.Options.Create(
            new LambdaHostOptions()
        );
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, lambdaHostOptions);
        var handler1Called = false;
        var handler2Called = false;
        var handler3Called = false;

        LambdaInitDelegate handler1 = (_, _) =>
        {
            handler1Called = true;
            return Task.FromResult(true);
        };

        LambdaInitDelegate handler2 = (_, _) =>
        {
            handler2Called = true;
            return Task.FromResult(true);
        };

        LambdaInitDelegate handler3 = (_, _) =>
        {
            handler3Called = true;
            return Task.FromResult(true);
        };

        builder.OnInit(handler1);
        builder.OnInit(handler2);
        builder.OnInit(handler3);

        // Act
        var buildFunc = builder.Build();
        var result = await buildFunc(CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        handler1Called.Should().BeTrue();
        handler2Called.Should().BeTrue();
        handler3Called.Should().BeTrue();
    }

    [Fact]
    public async Task Build_WithAnyHandlerReturningFalse_ReturnsFalse()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var lambdaHostOptions = Microsoft.Extensions.Options.Options.Create(
            new LambdaHostOptions()
        );
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, lambdaHostOptions);

        LambdaInitDelegate handler1 = (_, _) => Task.FromResult(true);
        LambdaInitDelegate handler2 = (_, _) => Task.FromResult(false);
        LambdaInitDelegate handler3 = (_, _) => Task.FromResult(true);

        builder.OnInit(handler1);
        builder.OnInit(handler2);
        builder.OnInit(handler3);

        // Act
        var buildFunc = builder.Build();
        var result = await buildFunc(CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Build_WhenHandlerThrowsException_ThrowsAggregateException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var lambdaHostOptions = Microsoft.Extensions.Options.Options.Create(
            new LambdaHostOptions()
        );
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, lambdaHostOptions);
        var testException = new InvalidOperationException("Test error");

        LambdaInitDelegate handler = (_, _) => throw testException;

        builder.OnInit(handler);

        // Act
        var buildFunc = builder.Build();
        Func<Task> act = () => buildFunc(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AggregateException>();
    }

    [Fact]
    public async Task Build_WithMultipleFailures_AggregatesAllErrors()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var lambdaHostOptions = Microsoft.Extensions.Options.Options.Create(
            new LambdaHostOptions()
        );
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, lambdaHostOptions);
        var exception1 = new InvalidOperationException("Error 1");
        var exception2 = new ArgumentException("Error 2");

        LambdaInitDelegate handler1 = (_, _) => throw exception1;

        LambdaInitDelegate handler2 = (_, _) => throw exception2;

        builder.OnInit(handler1);
        builder.OnInit(handler2);

        // Act
        var buildFunc = builder.Build();
        Func<Task> act = () => buildFunc(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AggregateException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Build_RespectsInitTimeout(IServiceScopeFactory scopeFactory)
    {
        // Arrange
        var lambdaHostOptions = Microsoft.Extensions.Options.Options.Create(
            new LambdaHostOptions { InitTimeout = TimeSpan.FromMilliseconds(50) }
        );
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, lambdaHostOptions);

        LambdaInitDelegate slowHandler = async (_, cancellationToken) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            return true;
        };

        builder.OnInit(slowHandler);

        // Act
        var buildFunc = builder.Build();
        var act = () => buildFunc(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Build_CreatesServiceScopeForEachHandler(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory
    )
    {
        // Arrange
        var lambdaHostOptions = Microsoft.Extensions.Options.Options.Create(
            new LambdaHostOptions()
        );
        var builder = new LambdaOnInitBuilder(serviceProvider, scopeFactory, lambdaHostOptions);

        var scopeUsed = false;

        LambdaInitDelegate handler = (scope, _) =>
        {
            scopeUsed = scope != serviceProvider;
            return Task.FromResult(true);
        };

        builder.OnInit(handler);

        // Act
        var buildFunc = builder.Build();
        await buildFunc(CancellationToken.None);

        // Assert
        scopeUsed.Should().BeTrue();
    }
}
