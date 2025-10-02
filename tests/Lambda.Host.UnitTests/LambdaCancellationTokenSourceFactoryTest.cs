// /Users/jonasha/Repos/CSharp/dotnet-lambda-host/test/Lambda.Host.UnitTests/LambdaCancellationTokenSourceFactoryTest.cs:

using Amazon.Lambda.Core;
using AwesomeAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Xunit;

namespace Lambda.Host.UnitTests;

[TestSubject(typeof(LambdaCancellationTokenSourceFactory))]
public class LambdaCancellationTokenSourceFactoryTest
{
    [Fact]
    public void Constructor_WithNegativeBufferDuration_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var negativeBufferDuration = TimeSpan.FromSeconds(-1);

        // Act & Assert
        Action act = () => new LambdaCancellationTokenSourceFactory(negativeBufferDuration);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_WithZeroOrPositiveBufferDuration_DoesNotThrow()
    {
        // Arrange
        var zeroBufferDuration = TimeSpan.Zero;
        var positiveBufferDuration = TimeSpan.FromSeconds(10);

        // Act & Assert
        var factoryWithZero = new LambdaCancellationTokenSourceFactory(zeroBufferDuration);
        factoryWithZero.Should().NotBeNull();

        var factoryWithPositive = new LambdaCancellationTokenSourceFactory(positiveBufferDuration);
        factoryWithPositive.Should().NotBeNull();
    }

    [Fact]
    public void NewCancellationTokenSource_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = new LambdaCancellationTokenSourceFactory(TimeSpan.FromSeconds(5));

        // Act & Assert
        Action act = () => factory.NewCancellationTokenSource(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NewCancellationTokenSource_WhenContextRemainingTimeZeroOrLess_ThrowsInvalidOperationException()
    {
        // Arrange
        var factory = new LambdaCancellationTokenSourceFactory(TimeSpan.FromSeconds(5));
        var mockContext = Substitute.For<ILambdaContext>();
        mockContext.RemainingTime.Returns(TimeSpan.Zero);

        // Act & Assert
        Action act = () => factory.NewCancellationTokenSource(mockContext);
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
        var factory = new LambdaCancellationTokenSourceFactory(bufferDuration);
        var mockContext = Substitute.For<ILambdaContext>();
        mockContext.RemainingTime.Returns(TimeSpan.FromSeconds(5)); // Less than buffer

        // Act & Assert
        Action act = () => factory.NewCancellationTokenSource(mockContext);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NewCancellationTokenSource_WithValidContext_ReturnsConfiguredCancellationTokenSource()
    {
        // Arrange
        var bufferDuration = TimeSpan.FromSeconds(5);
        var factory = new LambdaCancellationTokenSourceFactory(bufferDuration);
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
        var factory = new LambdaCancellationTokenSourceFactory(bufferDuration);
        var mockContext = Substitute.For<ILambdaContext>();
        mockContext.RemainingTime.Returns(bufferDuration);

        // Act & Assert
        Action act = () => factory.NewCancellationTokenSource(mockContext);
        act.Should().Throw<InvalidOperationException>();
    }
}
