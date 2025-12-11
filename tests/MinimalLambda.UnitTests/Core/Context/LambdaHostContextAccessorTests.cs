namespace MinimalLambda.UnitTests.Core.Context;

[TestSubject(typeof(LambdaHostContextAccessor))]
public class LambdaHostContextAccessorTests
{
    [Fact]
    public void LambdaHostContext_WhenNotSet_ReturnsNull()
    {
        // Arrange
        var accessor = new LambdaHostContextAccessor();

        // Act
        var result = accessor.LambdaHostContext;

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void LambdaHostContext_WhenSet_ReturnsSetValue(ILambdaHostContext context)
    {
        // Arrange
        var accessor = new LambdaHostContextAccessor();

        // Act
        accessor.LambdaHostContext = context;
        var result = accessor.LambdaHostContext;

        // Assert
        result.Should().BeSameAs(context);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void LambdaHostContext_WhenSetToNewValue_ReturnsNewValue(
        ILambdaHostContext context1,
        ILambdaHostContext context2
    )
    {
        // Arrange
        var accessor = new LambdaHostContextAccessor();
        accessor.LambdaHostContext = context1;

        // Act
        accessor.LambdaHostContext = context2;
        var result = accessor.LambdaHostContext;

        // Assert
        result.Should().BeSameAs(context2);
        result.Should().NotBeSameAs(context1);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void LambdaHostContext_WhenSetToNull_BecomesNull(ILambdaHostContext context)
    {
        // Arrange
        var accessor = new LambdaHostContextAccessor();
        accessor.LambdaHostContext = context;

        // Act
        accessor.LambdaHostContext = null;
        var result = accessor.LambdaHostContext;

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void LambdaHostContext_MultipleInstances_ShareAsyncLocalState(
        ILambdaHostContext context1,
        ILambdaHostContext context2
    )
    {
        // Arrange
        var accessor1 = new LambdaHostContextAccessor();
        var accessor2 = new LambdaHostContextAccessor();

        // Act
        accessor1.LambdaHostContext = context1;
        var result1 = accessor2.LambdaHostContext;

        accessor2.LambdaHostContext = context2;
        var result2 = accessor1.LambdaHostContext;

        // Assert
        // Both accessors share the same AsyncLocal storage, so they see the latest value
        result1.Should().BeSameAs(context1);
        result2.Should().BeSameAs(context2);
    }
}
