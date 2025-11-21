using AwesomeAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AwsLambda.Host.UnitTests.Builder.Extensions;

// Source file is in Microsoft.Extensions.DependencyInjection namespace

[TestSubject(typeof(ConfigurationServiceCollectionExtensions))]
public class ConfigurationServiceCollectionExtensionsTests
{
    [Fact]
    public void ConfigureEnvelopeOptions_WithNullServiceCollection_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((IServiceCollection)null!).ConfigureEnvelopeOptions(_ => { });

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void ConfigureEnvelopeOptions_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var act = () => serviceCollection.ConfigureEnvelopeOptions(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void ConfigureEnvelopeOptions_WithValidParameters_ReturnsServiceCollection()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var result = serviceCollection.ConfigureEnvelopeOptions(_ => { });

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }

    [Fact]
    public void ConfigureLambdaHostOptions_WithNullServiceCollection_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((IServiceCollection)null!).ConfigureLambdaHostOptions(_ => { });

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void ConfigureLambdaHostOptions_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var act = () => serviceCollection.ConfigureLambdaHostOptions(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void ConfigureLambdaHostOptions_WithValidParameters_ReturnsServiceCollection()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var result = serviceCollection.ConfigureLambdaHostOptions(_ => { });

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }

    [Fact]
    public void ConfigureEnvelopeOptions_EnablesMethodChaining()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act - chain multiple extension methods
        var result = serviceCollection
            .ConfigureEnvelopeOptions(_ => { })
            .ConfigureEnvelopeOptions(_ => { });

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }

    [Fact]
    public void ConfigureLambdaHostOptions_EnablesMethodChaining()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act - chain multiple extension methods
        var result = serviceCollection
            .ConfigureLambdaHostOptions(_ => { })
            .ConfigureLambdaHostOptions(_ => { });

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }

    [Fact]
    public void Extensions_CanChainDifferentMethods()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act - chain both extension methods together
        var result = serviceCollection
            .ConfigureEnvelopeOptions(_ => { })
            .ConfigureLambdaHostOptions(_ => { });

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }
}
