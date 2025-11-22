using AwesomeAssertions;
using AwsLambda.Host.Core;
using JetBrains.Annotations;
using NSubstitute;
using Xunit;

namespace AwsLambda.Host.UnitTests.Core.Features;

[TestSubject(typeof(DefaultFeatureCollectionFactory))]
public class DefaultFeatureCollectionFactoryTest
{
    [Fact]
    public void Create_ReturnsIFeatureCollection()
    {
        // Arrange
        var factory = new DefaultFeatureCollectionFactory([]);

        // Act
        var collection = factory.Create();

        // Assert
        collection.Should().BeAssignableTo<IFeatureCollection>();
    }

    [Fact]
    public void Create_ReturnsNewInstanceEachCall()
    {
        // Arrange
        var factory = new DefaultFeatureCollectionFactory([]);

        // Act
        var collection1 = factory.Create();
        var collection2 = factory.Create();

        // Assert
        collection1.Should().NotBeSameAs(collection2);
    }

    [Fact]
    public void Create_PassesProvidersToCollection()
    {
        // Arrange
        var provider = Substitute.For<IFeatureProvider>();
        var factory = new DefaultFeatureCollectionFactory([provider]);

        provider
            .TryCreate(typeof(string), out _)
            .Returns(x =>
            {
                x[1] = "test-value";
                return true;
            });

        // Act
        var collection = factory.Create();
        var result = collection.Get<string>();

        // Assert
        result.Should().Be("test-value");
    }
}
