using Amazon.Lambda.Core;
using AutoFixture.Xunit3;
using AwesomeAssertions;
using AwsLambda.Host.Core.Features;
using JetBrains.Annotations;
using NSubstitute;
using Xunit;

namespace AwsLambda.Host.UnitTests.Core.Features;

[TestSubject(typeof(DefaultEventFeatureProvider<>))]
public class DefaultEventFeatureProviderTests
{
    #region Constructor Validation Tests

    [Theory]
    [AutoNSubstituteData]
    internal void Constructor_WithValidSerializer_SuccessfullyConstructs(
        DefaultEventFeatureProvider<string> provider
    )
    {
        // Assert
        provider.Should().NotBeNull();
    }

    #endregion

    #region Interface Implementation Tests

    [Theory]
    [AutoNSubstituteData]
    internal void Provider_ImplementsIFeatureProvider(DefaultEventFeatureProvider<string> provider)
    {
        // Assert
        provider.Should().BeAssignableTo<IFeatureProvider>();
    }

    #endregion

    #region Test Data Classes

    internal sealed class TestEvent
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    #endregion

    #region TryCreate Tests - IEventFeature Type

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithIEventFeatureType_ReturnsTrue(
        DefaultEventFeatureProvider<string> provider
    )
    {
        // Act
        var result = provider.TryCreate(typeof(IEventFeature), out var feature);

        // Assert
        result.Should().Be(true);
        feature.Should().NotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithIEventFeatureType_CreatesDefaultEventFeature(
        DefaultEventFeatureProvider<string> provider
    )
    {
        // Act
        provider.TryCreate(typeof(IEventFeature), out var feature);

        // Assert
        feature.Should().BeOfType<DefaultEventFeature<string>>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithIEventFeatureType_CreatesNewInstanceEachCall(
        DefaultEventFeatureProvider<string> provider
    )
    {
        // Act
        provider.TryCreate(typeof(IEventFeature), out var feature1);
        provider.TryCreate(typeof(IEventFeature), out var feature2);

        // Assert
        feature1.Should().NotBeSameAs(feature2);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithIEventFeatureType_InitializesFeatureWithSerializer(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeatureProvider<string> provider,
        ILambdaHostContext context
    )
    {
        // Arrange
        const string expectedEvent = "test-event";
        serializer.Deserialize<string>(Arg.Any<Stream>()).Returns(expectedEvent);

        // Act
        provider.TryCreate(typeof(IEventFeature), out var feature);
        var result = ((IEventFeature)feature!).GetEvent(context);

        // Assert
        result.Should().Be(expectedEvent);
    }

    #endregion

    #region TryCreate Tests - Wrong Type

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithWrongType_ReturnsFalse(DefaultEventFeatureProvider<string> provider)
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
        DefaultEventFeatureProvider<string> provider
    )
    {
        // Act
        var result = provider.TryCreate(typeof(string), out var feature);

        // Assert
        result.Should().Be(false);
        feature.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithNullType_ReturnsFalse(DefaultEventFeatureProvider<string> provider)
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
        DefaultEventFeatureProvider<TestEvent> provider
    )
    {
        // Act
        provider.TryCreate(typeof(IEventFeature), out var feature);

        // Assert
        feature.Should().BeOfType<DefaultEventFeature<TestEvent>>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithListGenericType_CreatesCorrectFeature(
        DefaultEventFeatureProvider<List<string>> provider
    )
    {
        // Act
        provider.TryCreate(typeof(IEventFeature), out var feature);

        // Assert
        feature.Should().BeOfType<DefaultEventFeature<List<string>>>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void TryCreate_WithNullableType_CreatesCorrectFeature(
        DefaultEventFeatureProvider<int?> provider
    )
    {
        // Act
        provider.TryCreate(typeof(IEventFeature), out var feature);

        // Assert
        feature.Should().BeOfType<DefaultEventFeature<int?>>();
    }

    #endregion

    #region Dependency Injection Tests

    [Theory]
    [AutoNSubstituteData]
    internal void Provider_CanBeUsedAsIFeatureProviderDependency(
        DefaultEventFeatureProvider<string> provider
    )
    {
        // Act
        IFeatureProvider featureProvider = provider;
        var result = featureProvider.TryCreate(typeof(IEventFeature), out var feature);

        // Assert
        result.Should().Be(true);
        feature.Should().NotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Provider_WithDifferentGenericTypes_CreatesTypedFeatures(
        DefaultEventFeatureProvider<string> stringProvider,
        DefaultEventFeatureProvider<int> intProvider
    )
    {
        // Act
        stringProvider.TryCreate(typeof(IEventFeature), out var stringFeature);
        intProvider.TryCreate(typeof(IEventFeature), out var intFeature);

        // Assert
        stringFeature.Should().BeOfType<DefaultEventFeature<string>>();
        intFeature.Should().BeOfType<DefaultEventFeature<int>>();
        stringFeature.Should().NotBeSameAs(intFeature);
    }

    #endregion
}
