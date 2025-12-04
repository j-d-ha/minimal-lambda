namespace AwsLambda.Host.UnitTests.Core.Features;

[TestSubject(typeof(ResponseFeatureProviderFactory))]
public class ResponseFeatureProviderFactoryTest
{
    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReturnsDefaultEventFeatureProvider(ResponseFeatureProviderFactory sut)
    {
        // Act
        var provider = sut.Create<string>();

        // Assert
        provider.Should().BeOfType<DefaultResponseFeatureProvider<string>>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReturnsIFeatureProvider(ResponseFeatureProviderFactory sut)
    {
        // Act
        var provider = sut.Create<string>();

        // Assert
        provider.Should().BeAssignableTo<IFeatureProvider>();
    }
}
