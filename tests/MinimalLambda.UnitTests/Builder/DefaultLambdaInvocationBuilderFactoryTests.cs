namespace MinimalLambda.UnitTests.Builder;

[TestSubject(typeof(DefaultLambdaInvocationBuilderFactory))]
public class DefaultLambdaInvocationBuilderFactoryTests
{
    [Fact]
    public void CreateBuilder_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = new DefaultLambdaInvocationBuilderFactory(null!);

        // Act & Assert - NullReferenceException will be thrown when CreateBuilder tries to use it
        var act = () => factory.CreateBuilder();
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidServiceProvider_SuccessfullyConstructs()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();

        // Act
        var factory = new DefaultLambdaInvocationBuilderFactory(serviceProvider);

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void CreateBuilder_ReturnsLambdaInvocationBuilder()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var factory = new DefaultLambdaInvocationBuilderFactory(serviceProvider);

        // Act
        var builder = factory.CreateBuilder();

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<ILambdaInvocationBuilder>();
    }

    [Fact]
    public void CreateBuilder_PassesServiceProviderToBuilder()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var factory = new DefaultLambdaInvocationBuilderFactory(serviceProvider);

        // Act
        var builder = factory.CreateBuilder();

        // Assert
        builder.Services.Should().Be(serviceProvider);
    }

    [Fact]
    public void CreateBuilder_CreatesNewInstanceEachCall()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var factory = new DefaultLambdaInvocationBuilderFactory(serviceProvider);

        // Act
        var builder1 = factory.CreateBuilder();
        var builder2 = factory.CreateBuilder();

        // Assert
        builder1.Should().NotBeSameAs(builder2);
    }
}
