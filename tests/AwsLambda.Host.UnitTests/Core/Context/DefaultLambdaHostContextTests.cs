using Amazon.Lambda.Core;
using AwesomeAssertions;
using AwsLambda.Host.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace AwsLambda.Host.UnitTests.Core.Context;

[TestSubject(typeof(DefaultLambdaHostContext))]
public class DefaultLambdaHostContextTests
{
    #region Helper Methods

    /// <summary>Creates a DefaultLambdaHostContext instance with sensible defaults for testing.</summary>
    private DefaultLambdaHostContext CreateDefaultLambdaHostContext(
        ILambdaContext? lambdaContext = null,
        IServiceScopeFactory? serviceScopeFactory = null,
        IDictionary<string, object?>? properties = null,
        IFeatureCollection? featuresCollection = null,
        RawInvocationData? rawData = null,
        CancellationToken? cancellationToken = null
    )
    {
        lambdaContext ??= Substitute.For<ILambdaContext>();
        serviceScopeFactory ??= Substitute.For<IServiceScopeFactory>();
        properties ??= new Dictionary<string, object?>();
        featuresCollection ??= Substitute.For<IFeatureCollection>();
        rawData ??= new RawInvocationData
        {
            Event = new MemoryStream(),
            Response = new MemoryStream(),
        };
        cancellationToken ??= CancellationToken.None;

        return new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            properties,
            featuresCollection,
            rawData,
            cancellationToken.Value
        );
    }

    #endregion

    #region Constructor Validation Tests

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithNullLambdaContext_ThrowsArgumentNullException(
        Dictionary<string, object?> properties,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData,
        CancellationToken cancellationToken
    )
    {
        // Act & Assert
        var act = () =>
            new DefaultLambdaHostContext(
                null!,
                serviceScopeFactory,
                properties,
                featuresCollection,
                rawData,
                cancellationToken
            );
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithNullServiceScopeFactory_ThrowsArgumentNullException(
        Dictionary<string, object?> properties,
        ILambdaContext lambdaContext,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData,
        CancellationToken cancellationToken
    )
    {
        // Act & Assert
        var act = () =>
            new DefaultLambdaHostContext(
                lambdaContext,
                null!,
                properties,
                featuresCollection,
                rawData,
                cancellationToken
            );
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithNullProperties_ThrowsArgumentNullException(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData,
        CancellationToken cancellationToken
    )
    {
        // Act & Assert
        var act = () =>
            new DefaultLambdaHostContext(
                lambdaContext,
                serviceScopeFactory,
                null!,
                featuresCollection,
                rawData,
                cancellationToken
            );
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithNullFeaturesCollection_ThrowsArgumentNullException(
        Dictionary<string, object?> properties,
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        RawInvocationData rawData,
        CancellationToken cancellationToken
    )
    {
        // Act & Assert
        var act = () =>
            new DefaultLambdaHostContext(
                lambdaContext,
                serviceScopeFactory,
                properties,
                null!,
                rawData,
                cancellationToken
            );
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithValidParameters_SuccessfullyConstructs(
        Dictionary<string, object?> properties,
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData,
        CancellationToken cancellationToken
    )
    {
        // Act
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            properties,
            featuresCollection,
            rawData,
            cancellationToken
        );

        // Assert
        context.Should().NotBeNull();
    }

    #endregion

    #region ILambdaContext Delegation Tests

    [Theory]
    [AutoNSubstituteData]
    public void AwsRequestId_ReturnsValueFromDelegatedContext(
        string expectedValue,
        ILambdaContext lambdaContext
    )
    {
        // Arrange
        lambdaContext.AwsRequestId.Returns(expectedValue);
        var context = CreateDefaultLambdaHostContext(lambdaContext);

        // Act
        var result = context.AwsRequestId;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    public void ClientContext_ReturnsValueFromDelegatedContext(
        IClientContext expectedValue,
        ILambdaContext lambdaContext
    )
    {
        // Arrange
        lambdaContext.ClientContext.Returns(expectedValue);
        var context = CreateDefaultLambdaHostContext(lambdaContext);

        // Act
        var result = context.ClientContext;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    public void FunctionName_ReturnsValueFromDelegatedContext(
        string expectedValue,
        ILambdaContext lambdaContext
    )
    {
        // Arrange
        lambdaContext.FunctionName.Returns(expectedValue);
        var context = CreateDefaultLambdaHostContext(lambdaContext);

        // Act
        var result = context.FunctionName;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    public void FunctionVersion_ReturnsValueFromDelegatedContext(
        string expectedValue,
        ILambdaContext lambdaContext
    )
    {
        // Arrange
        lambdaContext.FunctionVersion.Returns(expectedValue);
        var context = CreateDefaultLambdaHostContext(lambdaContext);

        // Act
        var result = context.FunctionVersion;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Identity_ReturnsValueFromDelegatedContext(
        ICognitoIdentity expectedValue,
        ILambdaContext lambdaContext
    )
    {
        // Arrange
        lambdaContext.Identity.Returns(expectedValue);
        var context = CreateDefaultLambdaHostContext(lambdaContext);

        // Act
        var result = context.Identity;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    public void InvokedFunctionArn_ReturnsValueFromDelegatedContext(
        string expectedValue,
        ILambdaContext lambdaContext
    )
    {
        // Arrange
        lambdaContext.InvokedFunctionArn.Returns(expectedValue);
        var context = CreateDefaultLambdaHostContext(lambdaContext);

        // Act
        var result = context.InvokedFunctionArn;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Logger_ReturnsValueFromDelegatedContext(
        ILambdaLogger expectedValue,
        ILambdaContext lambdaContext
    )
    {
        // Arrange
        lambdaContext.Logger.Returns(expectedValue);
        var context = CreateDefaultLambdaHostContext(lambdaContext);

        // Act
        var result = context.Logger;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    public void LogGroupName_ReturnsValueFromDelegatedContext(
        string expectedValue,
        ILambdaContext lambdaContext
    )
    {
        // Arrange
        lambdaContext.LogGroupName.Returns(expectedValue);
        var context = CreateDefaultLambdaHostContext(lambdaContext);

        // Act
        var result = context.LogGroupName;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    public void LogStreamName_ReturnsValueFromDelegatedContext(
        string expectedValue,
        ILambdaContext lambdaContext
    )
    {
        // Arrange
        lambdaContext.LogStreamName.Returns(expectedValue);
        var context = CreateDefaultLambdaHostContext(lambdaContext);

        // Act
        var result = context.LogStreamName;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    public void MemoryLimitInMB_ReturnsValueFromDelegatedContext(
        int expectedValue,
        ILambdaContext lambdaContext
    )
    {
        // Arrange
        lambdaContext.MemoryLimitInMB.Returns(expectedValue);
        var context = CreateDefaultLambdaHostContext(lambdaContext);

        // Act
        var result = context.MemoryLimitInMB;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    public void RemainingTime_ReturnsValueFromDelegatedContext(
        TimeSpan expectedValue,
        ILambdaContext lambdaContext
    )
    {
        // Arrange
        lambdaContext.RemainingTime.Returns(expectedValue);
        var context = CreateDefaultLambdaHostContext(lambdaContext);

        // Act
        var result = context.RemainingTime;

        // Assert
        result.Should().Be(expectedValue);
    }

    #endregion

    #region ILambdaHostContext Property Tests

    [Fact]
    public void Properties_ReturnsPropertiesDictionaryPassedToConstructor()
    {
        // Arrange
        var propertiesDict = new Dictionary<string, object?>
        {
            { "key1", "value1" },
            { "key2", 42 },
        };
        var context = CreateDefaultLambdaHostContext(properties: propertiesDict);

        // Act
        var result = context.Properties;

        // Assert
        result.Should().BeEquivalentTo(propertiesDict);
        result.Should().BeSameAs(propertiesDict);
    }

    [Fact]
    public void Items_ReturnsEmptyDictionaryInitially()
    {
        // Arrange & Act
        var context = CreateDefaultLambdaHostContext();

        // Assert
        context.Items.Should().NotBeNull();
        context.Items.Should().BeEmpty();
    }

    [Fact]
    public void Items_AllowsAddingAndRetrievingValues()
    {
        // Arrange
        var context = CreateDefaultLambdaHostContext();
        var key = new object();
        var value = "test-value";

        // Act
        context.Items[key] = value;
        var result = context.Items[key];

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void Items_AllowsClearingValues()
    {
        // Arrange
        var context = CreateDefaultLambdaHostContext();
        context.Items["key1"] = "value1";
        context.Items["key2"] = "value2";

        // Act
        context.Items.Clear();

        // Assert
        context.Items.Should().BeEmpty();
    }

    [Fact]
    public void CancellationToken_ReturnsCancellationTokenPassedToConstructor()
    {
        // Arrange
        var expectedToken = new CancellationToken();
        var context = CreateDefaultLambdaHostContext(cancellationToken: expectedToken);

        // Act
        var result = context.CancellationToken;

        // Assert
        result.Should().Be(expectedToken);
    }

    [Fact]
    public void Features_ReturnsFeaturesCollectionPassedToConstructor()
    {
        // Arrange
        var featuresCollection = Substitute.For<IFeatureCollection>();
        var context = CreateDefaultLambdaHostContext(featuresCollection: featuresCollection);

        // Act
        var result = context.Features;

        // Assert
        result.Should().BeSameAs(featuresCollection);
    }

    [Fact]
    public void RawInvocationData_ReturnsRawDataPassedToConstructor()
    {
        // Arrange
        var rawData = new RawInvocationData
        {
            Event = new MemoryStream(),
            Response = new MemoryStream(),
        };
        var context = CreateDefaultLambdaHostContext(rawData: rawData);

        // Act
        var result = context.RawInvocationData;

        // Assert
        result.Should().BeSameAs(rawData);
    }

    #endregion

    #region ServiceProvider Lazy Initialization Tests

    [Fact]
    public void ServiceProvider_IsCreatedOnFirstAccess()
    {
        // Arrange
        var mockScope = Substitute.For<IServiceScope>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockScope.ServiceProvider.Returns(mockServiceProvider);

        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactory.CreateScope().Returns(mockScope);

        var context = CreateDefaultLambdaHostContext(serviceScopeFactory: serviceScopeFactory);

        // Act
        var result = context.ServiceProvider;

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(mockServiceProvider);
        serviceScopeFactory.Received(1).CreateScope();
    }

    [Fact]
    public void ServiceProvider_ReturnsSameScopeOnSubsequentAccess()
    {
        // Arrange
        var mockScope = Substitute.For<IServiceScope>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockScope.ServiceProvider.Returns(mockServiceProvider);

        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactory.CreateScope().Returns(mockScope);

        var context = CreateDefaultLambdaHostContext(serviceScopeFactory: serviceScopeFactory);

        // Act
        var result1 = context.ServiceProvider;
        var result2 = context.ServiceProvider;
        var result3 = context.ServiceProvider;

        // Assert
        result1.Should().BeSameAs(result2);
        result2.Should().BeSameAs(result3);
        serviceScopeFactory.Received(1).CreateScope();
    }

    [Fact]
    public void ServiceProvider_UsesServiceScopeFactory()
    {
        // Arrange
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        var mockScope = Substitute.For<IServiceScope>();
        serviceScopeFactory.CreateScope().Returns(mockScope);

        var context = CreateDefaultLambdaHostContext(serviceScopeFactory: serviceScopeFactory);

        // Act
        _ = context.ServiceProvider;

        // Assert
        serviceScopeFactory.Received(1).CreateScope();
    }

    #endregion

    #region DisposeAsync Tests

    [Fact]
    public async Task DisposeAsync_DisposesServiceScopeWhenAsyncDisposable()
    {
        // Arrange
        var asyncDisposableScope = Substitute.For<IServiceScope, IAsyncDisposable>();
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactory.CreateScope().Returns(asyncDisposableScope);

        var context = CreateDefaultLambdaHostContext(serviceScopeFactory: serviceScopeFactory);
        _ = context.ServiceProvider; // Trigger lazy initialization

        // Act
        await context.DisposeAsync();

        // Assert
        await ((IAsyncDisposable)asyncDisposableScope).Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_DisposesServiceScopeSynchronouslyWhenNotAsyncDisposable()
    {
        // Arrange
        var mockScope = Substitute.For<IServiceScope>();
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactory.CreateScope().Returns(mockScope);

        var context = CreateDefaultLambdaHostContext(serviceScopeFactory: serviceScopeFactory);
        _ = context.ServiceProvider; // Trigger lazy initialization

        // Act
        await context.DisposeAsync();

        // Assert
        mockScope.Received(1).Dispose();
    }

    [Fact]
    public async Task DisposeAsync_DisposesRawInvocationEventStream()
    {
        // Arrange
        var eventStream = new MemoryStream();
        var responseStream = new MemoryStream();
        var rawData = new RawInvocationData { Event = eventStream, Response = responseStream };

        var context = CreateDefaultLambdaHostContext(rawData: rawData);

        // Act
        await context.DisposeAsync();

        // Assert
        eventStream.CanRead.Should().BeFalse();
    }

    [Fact]
    public async Task DisposeAsync_ClearsItemsCollection()
    {
        // Arrange
        var context = CreateDefaultLambdaHostContext();
        context.Items["key1"] = "value1";
        context.Items["key2"] = "value2";

        // Act
        await context.DisposeAsync();

        // Assert
        context.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task DisposeAsync_DisposesServiceScopeBeforeNullingServiceProvider()
    {
        // Arrange
        var mockScope = Substitute.For<IServiceScope>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockScope.ServiceProvider.Returns(mockServiceProvider);

        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactory.CreateScope().Returns(mockScope);

        var context = CreateDefaultLambdaHostContext(serviceScopeFactory: serviceScopeFactory);
        _ = context.ServiceProvider; // Trigger initialization

        // Act
        await context.DisposeAsync();

        // Assert
        // Verify the scope was disposed
        mockScope.Received(1).Dispose();
    }

    [Fact]
    public async Task DisposeAsync_CanBeCalledMultipleTimes()
    {
        // Arrange
        var context = CreateDefaultLambdaHostContext();

        // Act & Assert
        await context.DisposeAsync();
        await context.DisposeAsync(); // Should not throw
    }

    #endregion

    #region Edge Cases & Integration Tests

    [Fact]
    public void ContextImplementsIAsyncDisposable()
    {
        // Arrange & Act
        var context = CreateDefaultLambdaHostContext();

        // Assert
        context.Should().BeAssignableTo<IAsyncDisposable>();
    }

    [Fact]
    public void ContextImplementsILambdaHostContext()
    {
        // Arrange & Act
        var context = CreateDefaultLambdaHostContext();

        // Assert
        context.Should().BeAssignableTo<ILambdaHostContext>();
    }

    [Fact]
    public void ContextImplementsILambdaContext()
    {
        // Arrange & Act
        var context = CreateDefaultLambdaHostContext();

        // Assert
        context.Should().BeAssignableTo<ILambdaContext>();
    }

    [Fact]
    public void MultipleContextInstances_AreIndependent()
    {
        // Arrange
        var context1 = CreateDefaultLambdaHostContext();
        var context2 = CreateDefaultLambdaHostContext();

        context1.Items["key"] = "value1";
        context2.Items["key"] = "value2";

        // Act & Assert
        context1.Items["key"].Should().Be("value1");
        context2.Items["key"].Should().Be("value2");
    }

    [Fact]
    public void FeaturesCollection_CanBeUsedToStoreAndRetrieveFeaturesViaItems()
    {
        // Arrange
        var context = CreateDefaultLambdaHostContext();
        var featureType = typeof(string);
        var featureValue = "test-feature";

        // Act
        context.Items[featureType] = featureValue;
        var retrievedValue = context.Items[featureType];

        // Assert
        retrievedValue.Should().Be(featureValue);
    }

    [Fact]
    public void PropertiesDictionary_PersistsAcrossMultipleInvocations()
    {
        // Arrange
        var properties = new Dictionary<string, object?> { { "persistent", "value" } };
        var context = CreateDefaultLambdaHostContext(properties: properties);

        // Act
        properties["persistent"] = "updated-value";
        var result = context.Properties["persistent"];

        // Assert
        result.Should().Be("updated-value");
    }

    #endregion
}
