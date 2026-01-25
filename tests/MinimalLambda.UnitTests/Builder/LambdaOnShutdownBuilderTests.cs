namespace MinimalLambda.UnitTests.Builder;

[TestSubject(typeof(LambdaOnShutdownBuilder))]
public class LambdaOnShutdownBuilderTests
{
    [Theory]
    [AutoNSubstituteData]
    internal void ShutdownHandlers_ReturnsReadOnlyList(LambdaOnShutdownBuilder builder)
    {
        // Act
        var handlers = builder.ShutdownHandlers;

        // Assert
        handlers.Should().NotBeNull();
        handlers.Should().BeEmpty();
        handlers.Should().BeAssignableTo<IReadOnlyList<LambdaShutdownDelegate>>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void OnShutdown_WithNullHandler_ThrowsArgumentNullException(
        LambdaOnShutdownBuilder builder)
    {
        // Act
        var act = () => builder.OnShutdown(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("handler");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void OnShutdown_WithValidHandler_AddsHandlerAndReturnsBuilder(
        LambdaOnShutdownBuilder builder)
    {
        // Arrange
        LambdaShutdownDelegate handler = _ => Task.CompletedTask;

        // Act
        var result = builder.OnShutdown(handler);

        // Assert
        result.Should().Be(builder);
        builder.ShutdownHandlers.Should().Contain(handler);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void OnShutdown_MultipleHandlers_AllAdded(LambdaOnShutdownBuilder builder)
    {
        // Arrange
        LambdaShutdownDelegate handler1 = _ => Task.CompletedTask;
        LambdaShutdownDelegate handler2 = _ => Task.CompletedTask;
        LambdaShutdownDelegate handler3 = _ => Task.CompletedTask;

        // Act
        builder.OnShutdown(handler1);
        builder.OnShutdown(handler2);
        builder.OnShutdown(handler3);

        // Assert
        builder.ShutdownHandlers.Should().HaveCount(3);
        builder.ShutdownHandlers.Should().Equal(handler1, handler2, handler3);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task Build_WithoutHandlers_ReturnsCompletedTaskFunction(
        LambdaOnShutdownBuilder builder)
    {
        // Act
        var buildFunc = builder.Build();
        var task = buildFunc(CancellationToken.None);

        // Assert
        await task;
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task Build_WithSingleHandler_ExecutesHandler(LambdaOnShutdownBuilder builder)
    {
        // Arrange
        var handlerCalled = false;

        LambdaShutdownDelegate handler = _ =>
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

    [Theory]
    [AutoNSubstituteData]
    internal async Task Build_WithMultipleHandlers_AllExecuted(LambdaOnShutdownBuilder builder)
    {
        // Arrange
        var handler1Called = false;
        var handler2Called = false;
        var handler3Called = false;

        LambdaShutdownDelegate handler1 = _ =>
        {
            handler1Called = true;
            return Task.CompletedTask;
        };

        LambdaShutdownDelegate handler2 = _ =>
        {
            handler2Called = true;
            return Task.CompletedTask;
        };

        LambdaShutdownDelegate handler3 = _ =>
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

    [Theory]
    [AutoNSubstituteData]
    internal async Task Build_WhenHandlerThrowsException_ThrowsAggregateException(
        LambdaOnShutdownBuilder builder)
    {
        // Arrange
        var testException = new InvalidOperationException("Test error");

        LambdaShutdownDelegate handler = _ => throw testException;

        builder.OnShutdown(handler);

        // Act
        var buildFunc = builder.Build();
        var act = () => buildFunc(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AggregateException>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task Build_WithMultipleFailures_AggregatesAllErrors(
        LambdaOnShutdownBuilder builder)
    {
        // Arrange
        var exception1 = new InvalidOperationException("Error 1");
        var exception2 = new ArgumentException("Error 2");

        LambdaShutdownDelegate handler1 = _ => throw exception1;

        LambdaShutdownDelegate handler2 = _ => throw exception2;

        builder.OnShutdown(handler1);
        builder.OnShutdown(handler2);

        // Act
        var buildFunc = builder.Build();
        var act = () => buildFunc(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AggregateException>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task Build_WithMixedSuccessAndFailure_AggregatesOnlyErrors(
        LambdaOnShutdownBuilder builder)
    {
        // Arrange
        var successfulHandlerCalled = false;
        var testException = new InvalidOperationException("Test error");

        LambdaShutdownDelegate successHandler = _ =>
        {
            successfulHandlerCalled = true;
            return Task.CompletedTask;
        };

        LambdaShutdownDelegate failingHandler = _ => throw testException;

        builder.OnShutdown(successHandler);
        builder.OnShutdown(failingHandler);

        // Act
        var buildFunc = builder.Build();
        var act = () => buildFunc(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AggregateException>();
        successfulHandlerCalled.Should().BeTrue();
    }
}
