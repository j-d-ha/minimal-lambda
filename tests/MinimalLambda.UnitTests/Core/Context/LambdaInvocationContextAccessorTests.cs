namespace MinimalLambda.UnitTests.Core.Context;

[TestSubject(typeof(LambdaInvocationContextAccessor))]
public class LambdaInvocationContextAccessorTests
{
    [Fact]
    public void LambdaInvocationContext_WhenNotSet_ReturnsNull()
    {
        // Arrange
        var accessor = new LambdaInvocationContextAccessor();

        // Act
        var result = accessor.LambdaInvocationContext;

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void LambdaInvocationContext_WhenSet_ReturnsSetValue(ILambdaInvocationContext context)
    {
        // Arrange
        var accessor = new LambdaInvocationContextAccessor();

        // Act
        accessor.LambdaInvocationContext = context;
        var result = accessor.LambdaInvocationContext;

        // Assert
        result.Should().BeSameAs(context);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void LambdaInvocationContext_WhenSetToNewValue_ReturnsNewValue(
        ILambdaInvocationContext context1,
        ILambdaInvocationContext context2
    )
    {
        // Arrange
        var accessor = new LambdaInvocationContextAccessor();
        accessor.LambdaInvocationContext = context1;

        // Act
        accessor.LambdaInvocationContext = context2;
        var result = accessor.LambdaInvocationContext;

        // Assert
        result.Should().BeSameAs(context2);
        result.Should().NotBeSameAs(context1);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void LambdaInvocationContext_WhenSetToNull_BecomesNull(
        ILambdaInvocationContext context
    )
    {
        // Arrange
        var accessor = new LambdaInvocationContextAccessor();
        accessor.LambdaInvocationContext = context;

        // Act
        accessor.LambdaInvocationContext = null;
        var result = accessor.LambdaInvocationContext;

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void LambdaInvocationContext_MultipleInstances_ShareAsyncLocalState(
        ILambdaInvocationContext context1,
        ILambdaInvocationContext context2
    )
    {
        // Arrange
        var accessor1 = new LambdaInvocationContextAccessor();
        var accessor2 = new LambdaInvocationContextAccessor();

        // Act
        accessor1.LambdaInvocationContext = context1;
        var result1 = accessor2.LambdaInvocationContext;

        accessor2.LambdaInvocationContext = context2;
        var result2 = accessor1.LambdaInvocationContext;

        // Assert
        // Both accessors share the same AsyncLocal storage, so they see the latest value
        result1.Should().BeSameAs(context1);
        result2.Should().BeSameAs(context2);
    }
}
