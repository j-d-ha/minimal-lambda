using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MinimalLambda.UnitTests.Builder;

[TestSubject(typeof(DefaultLambdaOnInitBuilderFactory))]
public class DefaultLambdaOnInitBuilderFactoryTests
{
    private readonly IOptions<LambdaHostOptions> _options =
        Microsoft.Extensions.Options.Options.Create(new LambdaHostOptions());

    private readonly IServiceScopeFactory _scopeFactory = Substitute.For<IServiceScopeFactory>();
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();

    private readonly ILambdaLifecycleContextFactory _contextFactory =
        Substitute.For<ILambdaLifecycleContextFactory>();

    [Theory]
    [InlineData(0)] // ServiceProvider
    [InlineData(1)] // ScopeFactory
    [InlineData(2)] // Options
    [InlineData(3)] // ContextFactory
    public void CreateBuilder_WithNullParameter_ThrowsArgumentNullException(int parameterIndex)
    {
        // Arrange
        var serviceProvider = parameterIndex == 0 ? null : _serviceProvider;
        var scopeFactory = parameterIndex == 1 ? null : _scopeFactory;
        var options = parameterIndex == 2 ? null : _options;
        var contextFactory = parameterIndex == 3 ? null : _contextFactory;

        var factory = new DefaultLambdaOnInitBuilderFactory(
            serviceProvider!,
            scopeFactory!,
            options!,
            contextFactory!);

        // Act & Assert - validation happens in LambdaOnInitBuilder constructor
        var act = () => factory.CreateBuilder();
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidParameters_SuccessfullyConstructs()
    {
        // Act
        var factory = new DefaultLambdaOnInitBuilderFactory(
            _serviceProvider,
            _scopeFactory,
            _options,
            _contextFactory);

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void CreateBuilder_ReturnsLambdaOnInitBuilder()
    {
        // Arrange
        var factory = new DefaultLambdaOnInitBuilderFactory(
            _serviceProvider,
            _scopeFactory,
            _options,
            _contextFactory);

        // Act
        var builder = factory.CreateBuilder();

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<ILambdaOnInitBuilder>();
    }

    [Fact]
    public void CreateBuilder_PassesServiceProviderToBuilder()
    {
        // Arrange
        var factory = new DefaultLambdaOnInitBuilderFactory(
            _serviceProvider,
            _scopeFactory,
            _options,
            _contextFactory);

        // Act
        var builder = factory.CreateBuilder();

        // Assert
        builder.Services.Should().Be(_serviceProvider);
    }

    [Fact]
    public void CreateBuilder_CreatesNewInstanceEachCall()
    {
        // Arrange
        var factory = new DefaultLambdaOnInitBuilderFactory(
            _serviceProvider,
            _scopeFactory,
            _options,
            _contextFactory);

        // Act
        var builder1 = factory.CreateBuilder();
        var builder2 = factory.CreateBuilder();

        // Assert
        builder1.Should().NotBeSameAs(builder2);
    }
}
