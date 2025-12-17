using Microsoft.Extensions.DependencyInjection;

namespace MinimalLambda.UnitTests.Builder;

[TestSubject(typeof(DefaultLambdaOnShutdownBuilderFactory))]
public class DefaultLambdaOnShutdownBuilderFactoryTests
{
    private readonly IServiceScopeFactory _scopeFactory = Substitute.For<IServiceScopeFactory>();
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();

    private readonly ILambdaLifecycleContextFactory _contextFactory =
        Substitute.For<ILambdaLifecycleContextFactory>();

    [Theory]
    [InlineData(0)] // ServiceProvider
    [InlineData(1)] // ScopeFactory
    [InlineData(2)] // ContextFactory
    public void CreateBuilder_WithNullParameter_ThrowsArgumentNullException(int parameterIndex)
    {
        // Arrange
        var serviceProvider = parameterIndex == 0 ? null : _serviceProvider;
        var scopeFactory = parameterIndex == 1 ? null : _scopeFactory;
        var contextFactory = parameterIndex == 2 ? null : _contextFactory;

        var factory = new DefaultLambdaOnShutdownBuilderFactory(
            serviceProvider!,
            scopeFactory!,
            contextFactory!
        );

        // Act & Assert - validation happens in LambdaOnShutdownBuilder constructor
        var act = () => factory.CreateBuilder();
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidParameters_SuccessfullyConstructs()
    {
        // Act
        var factory = new DefaultLambdaOnShutdownBuilderFactory(
            _serviceProvider,
            _scopeFactory,
            _contextFactory
        );

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void CreateBuilder_ReturnsLambdaOnShutdownBuilder()
    {
        // Arrange
        var factory = new DefaultLambdaOnShutdownBuilderFactory(
            _serviceProvider,
            _scopeFactory,
            _contextFactory
        );

        // Act
        var builder = factory.CreateBuilder();

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<ILambdaOnShutdownBuilder>();
    }

    [Fact]
    public void CreateBuilder_PassesServiceProviderToBuilder()
    {
        // Arrange
        var factory = new DefaultLambdaOnShutdownBuilderFactory(
            _serviceProvider,
            _scopeFactory,
            _contextFactory
        );

        // Act
        var builder = factory.CreateBuilder();

        // Assert
        builder.Services.Should().Be(_serviceProvider);
    }

    [Fact]
    public void CreateBuilder_CreatesNewInstanceEachCall()
    {
        // Arrange
        var factory = new DefaultLambdaOnShutdownBuilderFactory(
            _serviceProvider,
            _scopeFactory,
            _contextFactory
        );

        // Act
        var builder1 = factory.CreateBuilder();
        var builder2 = factory.CreateBuilder();

        // Assert
        builder1.Should().NotBeSameAs(builder2);
    }
}
