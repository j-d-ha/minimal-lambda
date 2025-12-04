using Microsoft.Extensions.DependencyInjection;

namespace AwsLambda.Host.UnitTests.Core.Context;

[TestSubject(typeof(LambdaHostContextFactory))]
public class LambdaHostContextFactoryTests
{
    [Fact]
    public void Constructor_WithNullServiceScopeFactory_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () =>
        {
            _ = new LambdaHostContextFactory(null!, Substitute.For<IFeatureCollectionFactory>());
        };
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullFeatureCollectionFactory_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () =>
        {
            _ = new LambdaHostContextFactory(Substitute.For<IServiceScopeFactory>(), null!);
        };
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Constructor_WithValidDependencies_SuccessfullyConstructs(
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollectionFactory featureCollectionFactory
    )
    {
        // Act
        var factory = new LambdaHostContextFactory(serviceScopeFactory, featureCollectionFactory);

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullContextAccessor_SuccessfullyConstructs()
    {
        // Act
        var factory = new LambdaHostContextFactory(
            Substitute.For<IServiceScopeFactory>(),
            Substitute.For<IFeatureCollectionFactory>()
        );

        // Assert
        factory.Should().NotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_CallsFeatureCollectionFactoryCreate(
        [Frozen] IFeatureCollectionFactory featureCollectionFactory,
        IServiceScopeFactory serviceScopeFactory,
        ILambdaContext lambdaContext,
        IDictionary<string, object?> properties
    )
    {
        // Arrange
        var factory = new LambdaHostContextFactory(serviceScopeFactory, featureCollectionFactory);

        // Act
        _ = factory.Create(lambdaContext, properties, CancellationToken.None);

        // Assert
        featureCollectionFactory.Received(1).Create(Arg.Any<IEnumerable<IFeatureProvider>>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_WithContextAccessor_SetsContextOnAccessor(
        [Frozen] ILambdaHostContextAccessor? contextAccessor,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollectionFactory featureCollectionFactory,
        ILambdaContext lambdaContext,
        IDictionary<string, object?> properties
    )
    {
        // Arrange
        var factory = new LambdaHostContextFactory(
            serviceScopeFactory,
            featureCollectionFactory,
            contextAccessor
        );

        // Act
        _ = factory.Create(lambdaContext, properties, CancellationToken.None);

        // Assert
        contextAccessor!.LambdaHostContext.Should().NotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_GetsFeaturesFromProperties(
        [Frozen] IFeatureCollectionFactory featureCollectionFactory,
        IFeatureProvider eventFeatureProvider,
        IFeatureProvider responseFeatureProvider,
        ILambdaHostContext lambdaContext,
        LambdaHostContextFactory factory
    )
    {
        // Arrange
        var properties = new Dictionary<string, object?>
        {
            [LambdaInvocationBuilder.EventFeatureProviderKey] = eventFeatureProvider,
            [LambdaInvocationBuilder.ResponseFeatureProviderKey] = responseFeatureProvider,
        };

        // Act
        _ = factory.Create(lambdaContext, properties, CancellationToken.None);

        // Assert
        featureCollectionFactory
            .Received(1)
            .Create(
                Arg.Is<IEnumerable<IFeatureProvider>>(providers =>
                    providers.Count() == 2
                    && providers.Contains(eventFeatureProvider)
                    && providers.Contains(responseFeatureProvider)
                )
            );
    }
}
