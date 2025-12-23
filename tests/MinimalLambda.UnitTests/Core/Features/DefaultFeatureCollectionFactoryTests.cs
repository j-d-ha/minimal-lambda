namespace MinimalLambda.UnitTests.Core.Features;

[TestSubject(typeof(DefaultFeatureCollectionFactory))]
public class DefaultFeatureCollectionFactoryTest
{
    [Theory]
    [AutoNSubstituteData]
    public void Create_ReturnsIFeatureCollection(IEnumerable<IFeatureProvider> featureProviders)
    {
        // Arrange
        var factory = new DefaultFeatureCollectionFactory([]);

        // Act
        var collection = factory.Create(featureProviders);

        // Assert
        collection.Should().BeAssignableTo<IFeatureCollection>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void Create_ReturnsNewInstanceEachCall(IEnumerable<IFeatureProvider> featureProviders)
    {
        // Arrange
        var factory = new DefaultFeatureCollectionFactory([]);

        // Act
        var enumerable = featureProviders as IFeatureProvider[] ?? featureProviders.ToArray();
        var collection1 = factory.Create(enumerable);
        var collection2 = factory.Create(enumerable);

        // Assert
        collection1.Should().NotBeSameAs(collection2);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Create_PassesProvidersToCollection(IEnumerable<IFeatureProvider> featureProviders)
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
        var collection = factory.Create(featureProviders);
        var result = collection.Get<string>();

        // Assert
        result.Should().Be("test-value");
    }
}
