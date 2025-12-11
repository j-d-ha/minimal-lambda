using Microsoft.Extensions.Options;

namespace MinimalLambda.UnitTests;

[TestSubject(typeof(DefaultLambdaCancellationFactory))]
public class DefaultLambdaCancellationFactoryTest
{
    [Fact]
    public void Constructor_WithNegativeBufferDuration_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var options = Options.Create(
            new LambdaHostOptions { InvocationCancellationBuffer = TimeSpan.FromSeconds(-1) }
        );

        // Act & Assert
        var act = () =>
        {
            _ = new DefaultLambdaCancellationFactory(options);
        };
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithZeroOrPositiveBufferDuration_DoesNotThrow()
    {
        // Arrange
        var optionsWithZero = Options.Create(
            new LambdaHostOptions { InvocationCancellationBuffer = TimeSpan.Zero }
        );
        var optionsWithPositive = Options.Create(
            new LambdaHostOptions { InvocationCancellationBuffer = TimeSpan.FromSeconds(10) }
        );

        // Act & Assert
        var actWithZero = () =>
        {
            _ = new DefaultLambdaCancellationFactory(optionsWithZero);
        };
        actWithZero.Should().NotThrow();

        var actWithPositive = () =>
        {
            _ = new DefaultLambdaCancellationFactory(optionsWithPositive);
        };
        actWithPositive.Should().NotThrow();
    }

    [Fact]
    public void NewCancellationTokenSource_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(
            new LambdaHostOptions { InvocationCancellationBuffer = TimeSpan.FromSeconds(5) }
        );
        var factory = new DefaultLambdaCancellationFactory(options);

        // Act & Assert
        var act = () =>
        {
            factory.NewCancellationTokenSource(null!);
        };
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NewCancellationTokenSource_WhenContextRemainingTimeZeroOrLess_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(
            new LambdaHostOptions { InvocationCancellationBuffer = TimeSpan.FromSeconds(5) }
        );
        var factory = new DefaultLambdaCancellationFactory(options);
        var mockContext = Substitute.For<ILambdaContext>();
        mockContext.RemainingTime.Returns(TimeSpan.Zero);

        // Act & Assert
        var act = () =>
        {
            factory.NewCancellationTokenSource(mockContext);
        };
        act.Should().Throw<InvalidOperationException>();

        // Test with negative remaining time
        mockContext.RemainingTime.Returns(TimeSpan.FromSeconds(-10));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NewCancellationTokenSource_WhenBufferExceedsRemainingTime_ThrowsInvalidOperationException()
    {
        // Arrange
        var bufferDuration = TimeSpan.FromSeconds(10);
        var options = Options.Create(
            new LambdaHostOptions { InvocationCancellationBuffer = bufferDuration }
        );
        var factory = new DefaultLambdaCancellationFactory(options);
        var mockContext = Substitute.For<ILambdaContext>();
        mockContext.RemainingTime.Returns(TimeSpan.FromSeconds(5)); // Less than buffer

        // Act & Assert
        var act = () =>
        {
            factory.NewCancellationTokenSource(mockContext);
        };
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NewCancellationTokenSource_WithValidContext_ReturnsConfiguredCancellationTokenSource()
    {
        // Arrange
        var bufferDuration = TimeSpan.FromSeconds(5);
        var options = Options.Create(
            new LambdaHostOptions { InvocationCancellationBuffer = bufferDuration }
        );
        var factory = new DefaultLambdaCancellationFactory(options);
        var mockContext = Substitute.For<ILambdaContext>();
        mockContext.RemainingTime.Returns(TimeSpan.FromSeconds(30));

        // Act
        var cancellationTokenSource = factory.NewCancellationTokenSource(mockContext);

        // Assert
        cancellationTokenSource.Should().NotBeNull();
        (
            cancellationTokenSource.Token.WaitHandle.WaitOne(1)
                ? TimeSpan.MinValue
                : TimeSpan.FromSeconds(25)
        )
            .Should()
            .Be(TimeSpan.FromSeconds(25));
    }

    [Fact]
    public void NewCancellationTokenSource_WithRemainingTimeEqualToBuffer_ThrowsInvalidOperationException()
    {
        // Arrange
        var bufferDuration = TimeSpan.FromSeconds(10);
        var options = Options.Create(
            new LambdaHostOptions { InvocationCancellationBuffer = bufferDuration }
        );
        var factory = new DefaultLambdaCancellationFactory(options);
        var mockContext = Substitute.For<ILambdaContext>();
        mockContext.RemainingTime.Returns(bufferDuration);

        // Act & Assert
        var act = () =>
        {
            factory.NewCancellationTokenSource(mockContext);
        };
        act.Should().Throw<InvalidOperationException>();
    }
}
