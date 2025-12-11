namespace MinimalLambda.UnitTests.Core.Features;

[TestSubject(typeof(EventFeatureProviderFactory))]
public class EventFeatureProviderFactoryTest
{
    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReturnsDefaultEventFeatureProvider(EventFeatureProviderFactory sut)
    {
        // Act
        var provider = sut.Create<string>();

        // Assert
        provider.Should().BeOfType<DefaultEventFeatureProvider<string>>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReturnsIFeatureProvider(EventFeatureProviderFactory sut)
    {
        // Act
        var provider = sut.Create<string>();

        // Assert
        provider.Should().BeAssignableTo<IFeatureProvider>();
    }
}
