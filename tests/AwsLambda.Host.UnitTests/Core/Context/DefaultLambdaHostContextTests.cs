using Microsoft.Extensions.DependencyInjection;

namespace AwsLambda.Host.UnitTests.Core.Context;

[TestSubject(typeof(DefaultLambdaHostContext))]
public class DefaultLambdaHostContextTests
{
    [Theory]
    [AutoNSubstituteData]
    internal void Constructor_WithNullLambdaContext_ThrowsArgumentNullException(
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
    internal void Constructor_WithNullServiceScopeFactory_ThrowsArgumentNullException(
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
    internal void Constructor_WithNullProperties_ThrowsArgumentNullException(
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
    internal void Constructor_WithNullFeaturesCollection_ThrowsArgumentNullException(
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
    internal void Constructor_WithValidParameters_SuccessfullyConstructs(
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

    [Theory]
    [AutoNSubstituteData]
    internal void AwsRequestId_ReturnsValueFromDelegatedContext(
        string expectedValue,
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        lambdaContext.AwsRequestId.Returns(expectedValue);
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.AwsRequestId;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void ClientContext_ReturnsValueFromDelegatedContext(
        IClientContext expectedValue,
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        lambdaContext.ClientContext.Returns(expectedValue);
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.ClientContext;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void FunctionName_ReturnsValueFromDelegatedContext(
        string expectedValue,
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        lambdaContext.FunctionName.Returns(expectedValue);
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.FunctionName;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void FunctionVersion_ReturnsValueFromDelegatedContext(
        string expectedValue,
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        lambdaContext.FunctionVersion.Returns(expectedValue);
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.FunctionVersion;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Identity_ReturnsValueFromDelegatedContext(
        ICognitoIdentity expectedValue,
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        lambdaContext.Identity.Returns(expectedValue);
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.Identity;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void InvokedFunctionArn_ReturnsValueFromDelegatedContext(
        string expectedValue,
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        lambdaContext.InvokedFunctionArn.Returns(expectedValue);
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.InvokedFunctionArn;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Logger_ReturnsValueFromDelegatedContext(
        ILambdaLogger expectedValue,
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        lambdaContext.Logger.Returns(expectedValue);
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.Logger;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void LogGroupName_ReturnsValueFromDelegatedContext(
        string expectedValue,
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        lambdaContext.LogGroupName.Returns(expectedValue);
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.LogGroupName;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void LogStreamName_ReturnsValueFromDelegatedContext(
        string expectedValue,
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        lambdaContext.LogStreamName.Returns(expectedValue);
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.LogStreamName;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void MemoryLimitInMB_ReturnsValueFromDelegatedContext(
        int expectedValue,
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        lambdaContext.MemoryLimitInMB.Returns(expectedValue);
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.MemoryLimitInMB;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void RemainingTime_ReturnsValueFromDelegatedContext(
        TimeSpan expectedValue,
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        lambdaContext.RemainingTime.Returns(expectedValue);
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.RemainingTime;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Properties_ReturnsPropertiesDictionaryPassedToConstructor(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var propertiesDict = new Dictionary<string, object?>
        {
            { "key1", "value1" },
            { "key2", 42 },
        };
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            propertiesDict,
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.Properties;

        // Assert
        result.Should().BeEquivalentTo(propertiesDict);
        result.Should().BeSameAs(propertiesDict);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Items_ReturnsEmptyDictionaryInitially(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange & Act
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Assert
        context.Items.Should().NotBeNull();
        context.Items.Should().BeEmpty();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Items_AllowsAddingAndRetrievingValues(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );
        var key = new object();
        var value = "test-value";

        // Act
        context.Items[key] = value;
        var result = context.Items[key];

        // Assert
        result.Should().Be(value);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Items_AllowsClearingValues(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );
        context.Items["key1"] = "value1";
        context.Items["key2"] = "value2";

        // Act
        context.Items.Clear();

        // Assert
        context.Items.Should().BeEmpty();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void CancellationToken_ReturnsCancellationTokenPassedToConstructor(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var expectedToken = new CancellationToken();
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            expectedToken
        );

        // Act
        var result = context.CancellationToken;

        // Assert
        result.Should().Be(expectedToken);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Features_ReturnsFeaturesCollectionPassedToConstructor(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.Features;

        // Assert
        result.Should().BeSameAs(featuresCollection);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void RawInvocationData_ReturnsRawDataPassedToConstructor(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection
    )
    {
        // Arrange
        var rawData = new RawInvocationData
        {
            Event = new MemoryStream(),
            Response = new MemoryStream(),
        };
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.RawInvocationData;

        // Assert
        result.Should().BeSameAs(rawData);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void ServiceProvider_IsCreatedOnFirstAccess(
        ILambdaContext lambdaContext,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var mockScope = Substitute.For<IServiceScope>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockScope.ServiceProvider.Returns(mockServiceProvider);

        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactory.CreateScope().Returns(mockScope);

        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result = context.ServiceProvider;

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(mockServiceProvider);
        serviceScopeFactory.Received(1).CreateScope();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void ServiceProvider_ReturnsSameScopeOnSubsequentAccess(
        ILambdaContext lambdaContext,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var mockScope = Substitute.For<IServiceScope>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockScope.ServiceProvider.Returns(mockServiceProvider);

        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactory.CreateScope().Returns(mockScope);

        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        var result1 = context.ServiceProvider;
        var result2 = context.ServiceProvider;
        var result3 = context.ServiceProvider;

        // Assert
        result1.Should().BeSameAs(result2);
        result2.Should().BeSameAs(result3);
        serviceScopeFactory.Received(1).CreateScope();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void ServiceProvider_UsesServiceScopeFactory(
        ILambdaContext lambdaContext,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        var mockScope = Substitute.For<IServiceScope>();
        serviceScopeFactory.CreateScope().Returns(mockScope);

        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        _ = context.ServiceProvider;

        // Assert
        serviceScopeFactory.Received(1).CreateScope();
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task DisposeAsync_DisposesServiceScopeWhenAsyncDisposable(
        ILambdaContext lambdaContext,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var asyncDisposableScope = Substitute.For<IServiceScope, IAsyncDisposable>();
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactory.CreateScope().Returns(asyncDisposableScope);

        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );
        _ = context.ServiceProvider; // Trigger lazy initialization

        // Act
        await context.DisposeAsync();

        // Assert
        await ((IAsyncDisposable)asyncDisposableScope).Received(1).DisposeAsync();
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task DisposeAsync_DisposesServiceScopeSynchronouslyWhenNotAsyncDisposable(
        ILambdaContext lambdaContext,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var mockScope = Substitute.For<IServiceScope>();
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactory.CreateScope().Returns(mockScope);

        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );
        _ = context.ServiceProvider; // Trigger lazy initialization

        // Act
        await context.DisposeAsync();

        // Assert
        mockScope.Received(1).Dispose();
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task DisposeAsync_DisposesRawInvocationEventStream(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection
    )
    {
        // Arrange
        var eventStream = new MemoryStream();
        var responseStream = new MemoryStream();
        var rawData = new RawInvocationData { Event = eventStream, Response = responseStream };

        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        await context.DisposeAsync();

        // Assert
        eventStream.CanRead.Should().BeFalse();
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task DisposeAsync_ClearsItemsCollection(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );
        context.Items["key1"] = "value1";
        context.Items["key2"] = "value2";

        // Act
        await context.DisposeAsync();

        // Assert
        context.Items.Should().BeEmpty();
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task DisposeAsync_DisposesServiceScopeBeforeNullingServiceProvider(
        ILambdaContext lambdaContext,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var mockScope = Substitute.For<IServiceScope>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockScope.ServiceProvider.Returns(mockServiceProvider);

        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactory.CreateScope().Returns(mockScope);

        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );
        _ = context.ServiceProvider; // Trigger initialization

        // Act
        await context.DisposeAsync();

        // Assert
        // Verify the scope was disposed
        mockScope.Received(1).Dispose();
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task DisposeAsync_CanBeCalledMultipleTimes(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act & Assert
        await context.DisposeAsync();
        await context.DisposeAsync(); // Should not throw
    }

    [Theory]
    [AutoNSubstituteData]
    internal void ContextImplementsIAsyncDisposable(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange & Act
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Assert
        context.Should().BeAssignableTo<IAsyncDisposable>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void ContextImplementsILambdaHostContext(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange & Act
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Assert
        context.Should().BeAssignableTo<ILambdaHostContext>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void ContextImplementsILambdaContext(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange & Act
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Assert
        context.Should().BeAssignableTo<ILambdaContext>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void MultipleContextInstances_AreIndependent(
        ILambdaContext lambdaContext1,
        ILambdaContext lambdaContext2,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData1,
        RawInvocationData rawData2
    )
    {
        // Arrange
        var context1 = new DefaultLambdaHostContext(
            lambdaContext1,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData1,
            CancellationToken.None
        );
        var context2 = new DefaultLambdaHostContext(
            lambdaContext2,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData2,
            CancellationToken.None
        );

        context1.Items["key"] = "value1";
        context2.Items["key"] = "value2";

        // Act & Assert
        context1.Items["key"].Should().Be("value1");
        context2.Items["key"].Should().Be("value2");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void FeaturesCollection_CanBeUsedToStoreAndRetrieveFeaturesViaItems(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            featuresCollection,
            rawData,
            CancellationToken.None
        );
        var featureType = typeof(string);
        var featureValue = "test-feature";

        // Act
        context.Items[featureType] = featureValue;
        var retrievedValue = context.Items[featureType];

        // Assert
        retrievedValue.Should().Be(featureValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void PropertiesDictionary_PersistsAcrossMultipleInvocations(
        ILambdaContext lambdaContext,
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollection featuresCollection,
        RawInvocationData rawData
    )
    {
        // Arrange
        var properties = new Dictionary<string, object?> { { "persistent", "value" } };
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            serviceScopeFactory,
            properties,
            featuresCollection,
            rawData,
            CancellationToken.None
        );

        // Act
        properties["persistent"] = "updated-value";
        var result = context.Properties["persistent"];

        // Assert
        result.Should().Be("updated-value");
    }
}
