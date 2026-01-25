using Microsoft.Extensions.DependencyInjection;
using MinimalLambda.Builder.Extensions;

namespace MinimalLambda.UnitTests.Builder.Extensions;

[TestSubject(typeof(LambdaHttpClientServiceCollectionExtensions))]
public class LambdaHttpClientServiceCollectionExtensionsTests
{
    [Fact]
    public void AddLambdaBootstrapHttpClient_WithNullServiceCollection_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((IServiceCollection)null!).AddLambdaBootstrapHttpClient(new HttpClient());

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void AddLambdaBootstrapHttpClient_WithNullClient_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var act = () => serviceCollection.AddLambdaBootstrapHttpClient((HttpClient)null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void AddLambdaBootstrapHttpClient_WithValidClient_ReturnsServiceCollection()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var httpClient = new HttpClient();

        // Act
        var result = serviceCollection.AddLambdaBootstrapHttpClient(httpClient);

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }

    [Fact]
    public void AddLambdaBootstrapHttpClient_WithValidClient_RegistersKeyedSingleton()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var httpClient = new HttpClient();

        // Act
        serviceCollection.AddLambdaBootstrapHttpClient(httpClient);
        var provider = serviceCollection.BuildServiceProvider();

        // Assert
        var registeredClient =
            provider.GetKeyedService<HttpClient>(typeof(ILambdaBootstrapOrchestrator));
        registeredClient.Should().BeSameAs(httpClient);
    }

    [Fact]
    public void AddLambdaBootstrapHttpClient_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var act = () => serviceCollection.AddLambdaBootstrapHttpClient(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void AddLambdaBootstrapHttpClient_WithValidFactory_ReturnsServiceCollection()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var result = serviceCollection.AddLambdaBootstrapHttpClient((_, _) => new HttpClient());

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }

    [Fact]
    public void AddLambdaBootstrapHttpClient_WithValidFactory_RegistersKeyedSingleton()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var httpClient = new HttpClient();

        // Act
        serviceCollection.AddLambdaBootstrapHttpClient((_, _) => httpClient);
        var provider = serviceCollection.BuildServiceProvider();

        // Assert
        var registeredClient =
            provider.GetKeyedService<HttpClient>(typeof(ILambdaBootstrapOrchestrator));
        registeredClient.Should().BeSameAs(httpClient);
    }

    [Fact]
    public void
        TryAddLambdaBootstrapHttpClient_WithNullServiceCollection_ThrowsArgumentNullException()
    {
        // Act
        var act = () =>
            ((IServiceCollection)null!).TryAddLambdaBootstrapHttpClient(new HttpClient());

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void TryAddLambdaBootstrapHttpClient_WithNullClient_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var act = () => serviceCollection.TryAddLambdaBootstrapHttpClient((HttpClient)null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void TryAddLambdaBootstrapHttpClient_WithValidClient_RegistersKeyedSingleton()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var httpClient = new HttpClient();

        // Act
        serviceCollection.TryAddLambdaBootstrapHttpClient(httpClient);
        var provider = serviceCollection.BuildServiceProvider();

        // Assert
        var registeredClient =
            provider.GetKeyedService<HttpClient>(typeof(ILambdaBootstrapOrchestrator));
        registeredClient.Should().BeSameAs(httpClient);
    }

    [Fact]
    public void TryAddLambdaBootstrapHttpClient_WhenAlreadyRegistered_DoesNotOverwrite()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var firstClient = new HttpClient();
        var secondClient = new HttpClient();

        // Act
        serviceCollection.TryAddLambdaBootstrapHttpClient(firstClient);
        serviceCollection.TryAddLambdaBootstrapHttpClient(secondClient);
        var provider = serviceCollection.BuildServiceProvider();

        // Assert
        var registeredClient =
            provider.GetKeyedService<HttpClient>(typeof(ILambdaBootstrapOrchestrator));
        registeredClient.Should().BeSameAs(firstClient);
    }

    [Fact]
    public void TryAddLambdaBootstrapHttpClient_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var act = () => serviceCollection.TryAddLambdaBootstrapHttpClient(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void TryAddLambdaBootstrapHttpClient_WithValidFactory_RegistersKeyedSingleton()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var httpClient = new HttpClient();

        // Act
        serviceCollection.TryAddLambdaBootstrapHttpClient((_, _) => httpClient);
        var provider = serviceCollection.BuildServiceProvider();

        // Assert
        var registeredClient =
            provider.GetKeyedService<HttpClient>(typeof(ILambdaBootstrapOrchestrator));
        registeredClient.Should().BeSameAs(httpClient);
    }

    [Fact]
    public void TryAddLambdaBootstrapHttpClient_WithFactory_WhenAlreadyRegistered_DoesNotOverwrite()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var firstClient = new HttpClient();
        var secondClient = new HttpClient();

        // Act
        serviceCollection.TryAddLambdaBootstrapHttpClient((_, _) => firstClient);
        serviceCollection.TryAddLambdaBootstrapHttpClient((_, _) => secondClient);
        var provider = serviceCollection.BuildServiceProvider();

        // Assert
        var registeredClient =
            provider.GetKeyedService<HttpClient>(typeof(ILambdaBootstrapOrchestrator));
        registeredClient.Should().BeSameAs(firstClient);
    }

    [Fact]
    public void AddLambdaBootstrapHttpClient_EnablesMethodChaining()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var firstClient = new HttpClient();
        var secondClient = new HttpClient();

        // Act
        var result = serviceCollection
            .AddLambdaBootstrapHttpClient(firstClient)
            .AddLambdaBootstrapHttpClient((_, _) => secondClient);

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }
}
