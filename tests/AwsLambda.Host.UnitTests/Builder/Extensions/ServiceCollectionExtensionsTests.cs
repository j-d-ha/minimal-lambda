using Amazon.Lambda.Core;
using AwesomeAssertions;
using AwsLambda.Host.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace AwsLambda.Host.UnitTests.Builder.Extensions;

[TestSubject(typeof(ServiceCollectionExtensions))]
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddLambdaHostCoreServices_WithNullServiceCollection_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((IServiceCollection)null!).AddLambdaHostCoreServices();

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void AddLambdaHostCoreServices_WithValidServiceCollection_ReturnsServiceCollection()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var result = serviceCollection.AddLambdaHostCoreServices();

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }

    [Fact]
    public void AddLambdaHostCoreServices_RegistersExactlyEightServices()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddLambdaHostCoreServices();

        // Assert
        // Should register exactly 8 services (with hosted service being the 8th)
        serviceCollection.Should().HaveCount(8);
    }

    [Fact]
    public void AddLambdaHostCoreServices_RegistersLambdaInvocationBuilderFactory()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddLambdaHostCoreServices();

        // Assert
        var descriptor = serviceCollection.FirstOrDefault(d =>
            d.ServiceType == typeof(ILambdaInvocationBuilderFactory)
        );
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(DefaultLambdaInvocationBuilderFactory));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddLambdaHostCoreServices_RegistersLambdaOnInitBuilderFactory()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddLambdaHostCoreServices();

        // Assert
        var descriptor = serviceCollection.FirstOrDefault(d =>
            d.ServiceType == typeof(ILambdaOnInitBuilderFactory)
        );
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(DefaultLambdaOnInitBuilderFactory));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddLambdaHostCoreServices_RegistersLambdaOnShutdownBuilderFactory()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddLambdaHostCoreServices();

        // Assert
        var descriptor = serviceCollection.FirstOrDefault(d =>
            d.ServiceType == typeof(ILambdaOnShutdownBuilderFactory)
        );
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(DefaultLambdaOnShutdownBuilderFactory));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddLambdaHostCoreServices_RegistersFeatureCollectionFactory()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddLambdaHostCoreServices();

        // Assert
        var descriptor = serviceCollection.FirstOrDefault(d =>
            d.ServiceType == typeof(IFeatureCollectionFactory)
        );
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(DefaultFeatureCollectionFactory));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddLambdaHostCoreServices_RegistersLambdaHandlerFactory()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddLambdaHostCoreServices();

        // Assert
        var descriptor = serviceCollection.FirstOrDefault(d =>
            d.ServiceType == typeof(ILambdaHandlerFactory)
        );
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(LambdaHandlerComposer));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddLambdaHostCoreServices_RegistersLambdaBootstrapOrchestrator()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddLambdaHostCoreServices();

        // Assert
        var descriptor = serviceCollection.FirstOrDefault(d =>
            d.ServiceType == typeof(ILambdaBootstrapOrchestrator)
        );
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(LambdaBootstrapAdapter));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddLambdaHostCoreServices_RegistersHostedService()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddLambdaHostCoreServices();

        // Assert
        var descriptor = serviceCollection.FirstOrDefault(d =>
            d.ServiceType == typeof(IHostedService)
            && d.ImplementationType == typeof(LambdaHostedService)
        );
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddLambdaHostCoreServices_RegistersHostOptionsPostConfiguration()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddLambdaHostCoreServices();

        // Assert
        var descriptor = serviceCollection.FirstOrDefault(d =>
            d.ServiceType == typeof(IPostConfigureOptions<HostOptions>)
        );
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(HostOptionsPostConfiguration));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddLambdaHostCoreServices_EnablesMethodChaining()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var result = serviceCollection.AddLambdaHostCoreServices().AddLambdaHostCoreServices();

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }

    [Fact]
    public void TryAddLambdaHostDefaultServices_WithNullServiceCollection_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((IServiceCollection)null!).TryAddLambdaHostDefaultServices();

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void TryAddLambdaHostDefaultServices_WithValidServiceCollection_ReturnsServiceCollection()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var result = serviceCollection.TryAddLambdaHostDefaultServices();

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }

    [Fact]
    public void TryAddLambdaHostDefaultServices_RegistersExactlyTwoServices()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.TryAddLambdaHostDefaultServices();

        // Assert
        serviceCollection.Should().HaveCount(2);
    }

    [Fact]
    public void TryAddLambdaHostDefaultServices_RegistersLambdaSerializer()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.TryAddLambdaHostDefaultServices();

        // Assert
        var descriptor = serviceCollection.FirstOrDefault(d =>
            d.ServiceType == typeof(ILambdaSerializer)
        );
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void TryAddLambdaHostDefaultServices_RegistersLambdaCancellationFactory()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.TryAddLambdaHostDefaultServices();

        // Assert
        var descriptor = serviceCollection.FirstOrDefault(d =>
            d.ServiceType == typeof(ILambdaCancellationFactory)
        );
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(DefaultLambdaCancellationFactory));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void TryAddLambdaHostDefaultServices_DoesNotAddIfAlreadyRegistered()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ILambdaSerializer, CustomLambdaSerializer>();

        // Act
        serviceCollection.TryAddLambdaHostDefaultServices();

        // Assert
        var descriptors = serviceCollection
            .Where(d => d.ServiceType == typeof(ILambdaSerializer))
            .ToList();
        descriptors.Should().HaveCount(1);
        descriptors[0].ImplementationType.Should().Be(typeof(CustomLambdaSerializer));
    }

    [Fact]
    public void TryAddLambdaHostDefaultServices_EnablesMethodChaining()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var result = serviceCollection
            .TryAddLambdaHostDefaultServices()
            .TryAddLambdaHostDefaultServices();

        // Assert
        result.Should().BeSameAs(serviceCollection);
    }
}

// Test implementation for verifying TryAdd behavior
internal class CustomLambdaSerializer : ILambdaSerializer
{
    public T Deserialize<T>(Stream requestStream)
    {
        throw new NotImplementedException();
    }

    public void Serialize<T>(T response, Stream responseStream)
    {
        throw new NotImplementedException();
    }
}
