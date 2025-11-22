using AwesomeAssertions;
using AwsLambda.Host.Core;
using JetBrains.Annotations;
using NSubstitute;
using Xunit;

namespace AwsLambda.Host.UnitTests.Core.Features;

[TestSubject(typeof(FeatureCollectionExtensions))]
public class FeatureCollectionExtensionsTest
{
    [Fact]
    public void TryGet_ReturnsTrueWhenFeatureExists()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        var expectedFeature = "test-feature";
        collection.Get<string>().Returns(expectedFeature);

        // Act
        var result = collection.TryGet(out string? feature);

        // Assert
        result.Should().BeTrue();
        feature.Should().Be(expectedFeature);
    }

    [Fact]
    public void TryGet_ReturnsFalseWhenFeatureNotFound()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        collection.Get<string>().Returns((string?)null);

        // Act
        var result = collection.TryGet(out string? feature);

        // Assert
        result.Should().BeFalse();
        feature.Should().BeNull();
    }

    [Fact]
    public void TryGet_WorksWithIntFeatures()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        var expectedValue = 42;
        collection.Get<int>().Returns(expectedValue);

        // Act
        var result = collection.TryGet(out int feature);

        // Assert
        result.Should().BeTrue();
        feature.Should().Be(expectedValue);
    }

    [Fact]
    public void TryGet_WorksWithInterfaceFeatures()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        var mockProvider = Substitute.For<IFeatureProvider>();
        collection.Get<IFeatureProvider>().Returns(mockProvider);

        // Act
        var result = collection.TryGet(out IFeatureProvider? feature);

        // Assert
        result.Should().BeTrue();
        feature.Should().BeSameAs(mockProvider);
    }

    [Fact]
    public void TryGet_WorksWithCustomObjectFeatures()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        var customObject = new TestFeature { Value = "test" };
        collection.Get<TestFeature>().Returns(customObject);

        // Act
        var result = collection.TryGet(out TestFeature? feature);

        // Assert
        result.Should().BeTrue();
        feature.Should().BeSameAs(customObject);
    }

    [Fact]
    public void TryGet_ReturnsFalseAndNullForMissingInterface()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        collection.Get<IFeatureProvider>().Returns((IFeatureProvider?)null);

        // Act
        var result = collection.TryGet(out IFeatureProvider? feature);

        // Assert
        result.Should().BeFalse();
        feature.Should().BeNull();
    }

    [Fact]
    public void TryGet_CallsGetOnCollection()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        collection.Get<string>().Returns("feature");

        // Act
        _ = collection.TryGet(out string? _);

        // Assert
        collection.Received(1).Get<string>();
    }

    [Fact]
    public void GetRequired_ReturnsFeatureWhenExists()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        var expectedFeature = "required-feature";
        collection.Get<string>().Returns(expectedFeature);

        // Act
        var result = collection.GetRequired<string>();

        // Assert
        result.Should().Be(expectedFeature);
    }

    [Fact]
    public void GetRequired_ThrowsInvalidOperationExceptionWhenNotFound()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        collection.Get<string>().Returns((string?)null);

        // Act & Assert
        var act = () => collection.GetRequired<string>();
        act.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void GetRequired_ExceptionMessageIncludesFeatureTypeName()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        collection.Get<TestFeature>().Returns((TestFeature?)null);

        // Act & Assert
        var act = () => collection.GetRequired<TestFeature>();
        var exception = act.Should().ThrowExactly<InvalidOperationException>().Which;
        exception.Message.Should().Contain(typeof(TestFeature).FullName);
    }

    [Fact]
    public void GetRequired_WorksWithIntFeatures()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        var expectedValue = 42;
        collection.Get<int>().Returns(expectedValue);

        // Act
        var result = collection.GetRequired<int>();

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void GetRequired_WorksWithInterfaceFeatures()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        var mockProvider = Substitute.For<IFeatureProvider>();
        collection.Get<IFeatureProvider>().Returns(mockProvider);

        // Act
        var result = collection.GetRequired<IFeatureProvider>();

        // Assert
        result.Should().BeSameAs(mockProvider);
    }

    [Fact]
    public void GetRequired_WorksWithCustomObjectFeatures()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        var customObject = new TestFeature { Value = "test" };
        collection.Get<TestFeature>().Returns(customObject);

        // Act
        var result = collection.GetRequired<TestFeature>();

        // Assert
        result.Should().BeSameAs(customObject);
    }

    [Fact]
    public void GetRequired_CallsGetOnCollection()
    {
        // Arrange
        var collection = Substitute.For<IFeatureCollection>();
        collection.Get<string>().Returns("feature");

        // Act
        _ = collection.GetRequired<string>();

        // Assert
        collection.Received(1).Get<string>();
    }

    #region Test Fixtures

    private class TestFeature
    {
        public string? Value { get; set; }
    }

    #endregion
}
