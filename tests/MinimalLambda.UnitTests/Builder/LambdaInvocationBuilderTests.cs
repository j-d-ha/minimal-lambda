namespace MinimalLambda.UnitTests.Builder;

[TestSubject(typeof(LambdaInvocationBuilder))]
public class LambdaInvocationBuilderTests
{
    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LambdaInvocationBuilder(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("services");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithValidServiceProvider_Succeeds(IServiceProvider serviceProvider)
    {
        // Act
        var builder = new LambdaInvocationBuilder(serviceProvider);

        // Assert
        builder.Should().NotBeNull();
        builder.Services.Should().Be(serviceProvider);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Services_ReturnsServiceProvider(IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);

        // Act
        var result = builder.Services;

        // Assert
        result.Should().Be(serviceProvider);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Properties_IsInitializedAsEmptyDictionary(IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);

        // Act
        var properties = builder.Properties;

        // Assert
        properties.Should().NotBeNull();
        properties.Should().BeEmpty();
    }

    [Theory]
    [AutoNSubstituteData]
    public void Properties_CanAddAndRetrieveValues(IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);
        const string key = "testKey";
        const string value = "testValue";

        // Act
        builder.Properties[key] = value;

        // Assert
        builder.Properties.Should().ContainKey(key);
        builder.Properties[key].Should().Be(value);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Middlewares_ReturnsReadOnlyList(IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);

        // Act
        var middlewares = builder.Middlewares;

        // Assert
        middlewares.Should().NotBeNull();
        middlewares.Should().BeEmpty();
        middlewares
            .Should()
            .BeAssignableTo<
                IReadOnlyList<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void Handle_WithNullHandler_ThrowsArgumentNullException(IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);

        // Act
        var act = () => builder.Handle(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("handler");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Handle_WithValidHandler_SetsHandlerAndReturnsBuilder(
        IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);
        LambdaInvocationDelegate handler = _ => Task.CompletedTask;

        // Act
        var result = builder.Handle(handler);

        // Assert
        result.Should().Be(builder);
        builder.Handler.Should().Be(handler);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Handle_WhenHandlerAlreadySet_ThrowsInvalidOperationException(
        IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);
        LambdaInvocationDelegate handler1 = _ => Task.CompletedTask;
        LambdaInvocationDelegate handler2 = _ => Task.CompletedTask;

        builder.Handle(handler1);

        // Act
        var act = () => builder.Handle(handler2);

        // Assert
        act
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Lambda Handler has already been set.");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Use_WithNullMiddleware_ThrowsArgumentNullException(IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);

        // Act
        var act = () => builder.Use(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("middleware");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Use_WithValidMiddleware_AddsToListAndReturnsBuilder(
        IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware = next => next;

        // Act
        var result = builder.Use(middleware);

        // Assert
        result.Should().Be(builder);
        builder.Middlewares.Should().Contain(middleware);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Use_MultipleMiddleware_AllAdded(IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware1 = next => next;
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware2 = next => next;
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware3 = next => next;

        // Act
        builder.Use(middleware1);
        builder.Use(middleware2);
        builder.Use(middleware3);

        // Assert
        builder.Middlewares.Should().HaveCount(3);
        builder.Middlewares.Should().Equal(middleware1, middleware2, middleware3);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Build_WithoutHandler_ThrowsInvalidOperationException(
        IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);

        // Act
        var act = () => builder.Build();

        // Assert
        act
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Lambda Handler has not been set.");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Build_WithHandler_ReturnsHandler(IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);
        LambdaInvocationDelegate handler = _ => Task.CompletedTask;
        builder.Handle(handler);

        // Act
        var result = builder.Build();

        // Assert
        result.Should().Be(handler);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Build_WithMiddleware_BuildsCorrectChain(IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);
        LambdaInvocationDelegate handler = _ => Task.CompletedTask;

        // Create middleware that actually wraps the handler
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware1 = next =>
            async context =>
            {
                await next(context);
            };
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware2 = next =>
            async context =>
            {
                await next(context);
            };

        builder.Handle(handler);
        builder.Use(middleware1);
        builder.Use(middleware2);

        // Act
        var built = builder.Build();

        // Assert - should not be null and should be different from the original handler
        // (because it's been wrapped by middleware)
        built.Should().NotBeNull();
        built.Should().NotBe(handler);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Build_WithMultipleMiddleware_AllMiddlewareIncorporated(
        IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);
        LambdaInvocationDelegate handler = _ => Task.CompletedTask;

        // Add three middleware
        builder.Use(next => next);
        builder.Use(next => next);
        builder.Use(next => next);

        builder.Handle(handler);

        // Act
        var built = builder.Build();

        // Assert - all middleware should be in the chain
        builder.Middlewares.Should().HaveCount(3);
        built.Should().NotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void Handler_PropertyInitiallyNull(IServiceProvider serviceProvider)
    {
        // Arrange
        var builder = new LambdaInvocationBuilder(serviceProvider);

        // Act
        var handler = builder.Handler;

        // Assert
        handler.Should().BeNull();
    }
}
