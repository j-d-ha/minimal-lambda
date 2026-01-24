namespace MinimalLambda.UnitTests.Core.Features;

[TestSubject(typeof(DefaultResponseFeatureProvider<>))]
public class DefaultResponseFeatureProviderTests
{
    #region Constructor Validation Tests

    [Theory]
    [AutoNSubstituteData]
    internal void Constructor_WithValidSerializer_SuccessfullyConstructs(
        DefaultResponseFeatureProvider<string> provider) =>
        // Assert
        provider.Should().NotBeNull();

    #endregion

    #region Interface Implementation Tests

    [Theory]
    [AutoNSubstituteData]
    internal void Provider_ImplementsIFeatureProvider(
        DefaultResponseFeatureProvider<string> provider) =>
        // Assert
        provider.Should().BeAssignableTo<IFeatureProvider>();

    #endregion

    #region Test Data Classes

    internal sealed class TestResponse
    {
        public int Id { get; set; }
        public string Message { get; set; } = "";
    }

    #endregion

    #region TryCreate Tests - IResponseFeature Type

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithIResponseFeatureType_ReturnsTrue(
        DefaultResponseFeatureProvider<string> provider)
    {
        // Act
        var result = provider.TryCreate(typeof(IResponseFeature), out var feature);

        // Assert
        result.Should().Be(true);
        feature.Should().NotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithIResponseFeatureType_CreatesDefaultResponseFeature(
        DefaultResponseFeatureProvider<string> provider)
    {
        // Act
        provider.TryCreate(typeof(IResponseFeature), out var feature);

        // Assert
        feature.Should().BeOfType<DefaultResponseFeature<string>>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithIResponseFeatureType_CreatesNewInstanceEachCall(
        DefaultResponseFeatureProvider<string> provider)
    {
        // Act
        provider.TryCreate(typeof(IResponseFeature), out var feature1);
        provider.TryCreate(typeof(IResponseFeature), out var feature2);

        // Assert
        feature1.Should().NotBeSameAs(feature2);
    }

    #endregion

    #region TryCreate Tests - Wrong Type

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithWrongType_ReturnsFalse(
        DefaultResponseFeatureProvider<string> provider)
    {
        // Act
        var result = provider.TryCreate(typeof(ILambdaSerializer), out var feature);

        // Assert
        result.Should().Be(false);
        feature.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithStringType_ReturnsFalse(
        DefaultResponseFeatureProvider<string> provider)
    {
        // Act
        var result = provider.TryCreate(typeof(string), out var feature);

        // Assert
        result.Should().Be(false);
        feature.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithNullType_ReturnsFalse(
        DefaultResponseFeatureProvider<string> provider)
    {
        // Act
        var result = provider.TryCreate(null!, out var feature);

        // Assert
        result.Should().Be(false);
        feature.Should().BeNull();
    }

    #endregion

    #region Generic Type Handling Tests

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithComplexGenericType_CreatesCorrectFeature(
        DefaultResponseFeatureProvider<TestResponse> provider)
    {
        // Act
        provider.TryCreate(typeof(IResponseFeature), out var feature);

        // Assert
        feature.Should().BeOfType<DefaultResponseFeature<TestResponse>>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithListGenericType_CreatesCorrectFeature(
        DefaultResponseFeatureProvider<List<string>> provider)
    {
        // Act
        provider.TryCreate(typeof(IResponseFeature), out var feature);

        // Assert
        feature.Should().BeOfType<DefaultResponseFeature<List<string>>>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithNullableType_CreatesCorrectFeature(
        DefaultResponseFeatureProvider<int?> provider)
    {
        // Act
        provider.TryCreate(typeof(IResponseFeature), out var feature);

        // Assert
        feature.Should().BeOfType<DefaultResponseFeature<int?>>();
    }

    #endregion

    #region Dependency Injection Tests

    [Theory]
    [AutoNSubstituteData]
    internal void Provider_CanBeUsedAsIFeatureProviderDependency(
        DefaultResponseFeatureProvider<string> provider)
    {
        // Act
        IFeatureProvider featureProvider = provider;
        var result = featureProvider.TryCreate(typeof(IResponseFeature), out var feature);

        // Assert
        result.Should().Be(true);
        feature.Should().NotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Provider_WithDifferentGenericTypes_CreatesTypedFeatures(
        DefaultResponseFeatureProvider<string> stringProvider,
        DefaultResponseFeatureProvider<int> intProvider)
    {
        // Act
        stringProvider.TryCreate(typeof(IResponseFeature), out var stringFeature);
        intProvider.TryCreate(typeof(IResponseFeature), out var intFeature);

        // Assert
        stringFeature.Should().BeOfType<DefaultResponseFeature<string>>();
        intFeature.Should().BeOfType<DefaultResponseFeature<int>>();
        stringFeature.Should().NotBeSameAs(intFeature);
    }

    #endregion
}
