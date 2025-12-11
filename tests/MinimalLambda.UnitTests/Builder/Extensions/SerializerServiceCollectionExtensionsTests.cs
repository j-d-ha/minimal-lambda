using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalLambda.UnitTests.Builder.Extensions;

[TestSubject(typeof(SerializerServiceCollectionExtensions))]
public class SerializerServiceCollectionExtensionsTests
{
    [Fact]
    public void AddLambdaSerializerWithContext_WithNullServiceCollection_ThrowsArgumentNullException()
    {
        // Act
        var act = () =>
            ((IServiceCollection)null!).AddLambdaSerializerWithContext<TestJsonSerializerContext>();

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void AddLambdaSerializerWithContext_WithValidServiceCollection_ReturnsServiceCollection()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var result = serviceCollection.AddLambdaSerializerWithContext<TestJsonSerializerContext>();

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }

    [Fact]
    public void AddLambdaSerializerWithContext_AddsOnlyLambdaSerializer()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddLambdaSerializerWithContext<TestJsonSerializerContext>();

        // Assert
        serviceCollection.Should().HaveCount(1);
        var descriptor = serviceCollection[0];
        descriptor.ServiceType.Should().Be<ILambdaSerializer>();
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddLambdaSerializerWithContext_RegistersSerializerFactory()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddLambdaSerializerWithContext<TestJsonSerializerContext>();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var serializer = serviceProvider.GetRequiredService<ILambdaSerializer>();
        serializer.Should().NotBeNull();
    }

    [Fact]
    public void AddLambdaSerializerWithContext_EnablesMethodChaining()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var result = serviceCollection
            .AddLambdaSerializerWithContext<TestJsonSerializerContext>()
            .AddLambdaSerializerWithContext<TestJsonSerializerContext>();

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }
}

[JsonSerializable(typeof(object))]
public partial class TestJsonSerializerContext : JsonSerializerContext;
