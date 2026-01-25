using Microsoft.Extensions.DependencyInjection;

namespace MinimalLambda.UnitTests.Core.Context;

[TestSubject(typeof(LambdaLifecycleContext))]
public class LambdaLifecycleContextTests
{
    [Theory]
    [AutoNSubstituteData]
    internal void Constructor_WithValidParameters_SuccessfullyConstructs(
        IServiceScopeFactory serviceScopeFactory,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        // Arrange
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core
        {
            Stopwatch = stopwatch, Region = "us-east-1",
        };

        // Act
        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            properties,
            cancellationToken);

        // Assert
        context.Should().NotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void CancellationToken_ReturnsCancellationTokenPassedToConstructor(
        IServiceScopeFactory serviceScopeFactory,
        IDictionary<string, object?> properties)
    {
        // Arrange
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core { Stopwatch = stopwatch };
        var expectedToken = new CancellationToken();

        // Act
        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            properties,
            expectedToken);

        // Assert
        context.CancellationToken.Should().Be(expectedToken);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Properties_ReturnsPropertiesDictionaryPassedToConstructor(
        IServiceScopeFactory serviceScopeFactory,
        CancellationToken cancellationToken)
    {
        // Arrange
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core { Stopwatch = stopwatch };
        var propertiesDict =
            new Dictionary<string, object?> { { "key1", "value1" }, { "key2", 42 } };

        // Act
        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            propertiesDict,
            cancellationToken);

        // Assert
        context.Properties.Should().BeEquivalentTo(propertiesDict);
        context.Properties.Should().BeSameAs(propertiesDict);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void ServiceProvider_IsCreatedOnFirstAccess(IServiceScopeFactory serviceScopeFactory)
    {
        // Arrange
        var mockScope = Substitute.For<IServiceScope>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockScope.ServiceProvider.Returns(mockServiceProvider);
        serviceScopeFactory.CreateScope().Returns(mockScope);

        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core { Stopwatch = stopwatch };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            CancellationToken.None);

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
        IServiceScopeFactory serviceScopeFactory)
    {
        // Arrange
        var mockScope = Substitute.For<IServiceScope>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockScope.ServiceProvider.Returns(mockServiceProvider);
        serviceScopeFactory.CreateScope().Returns(mockScope);

        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core { Stopwatch = stopwatch };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            CancellationToken.None);

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
    internal void ElapsedTime_ReturnsValueFromContextCore(
        IServiceScopeFactory serviceScopeFactory,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        // Arrange
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core { Stopwatch = stopwatch };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            properties,
            cancellationToken);

        // Act
        var result = context.ElapsedTime;

        // Assert
        (result >= TimeSpan.Zero).Should().BeTrue();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Region_ReturnsValueFromContextCore(
        IServiceScopeFactory serviceScopeFactory,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        // Arrange
        const string expectedValue = "us-west-2";
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core
        {
            Stopwatch = stopwatch, Region = expectedValue,
        };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            properties,
            cancellationToken);

        // Act
        var result = context.Region;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void ExecutionEnvironment_ReturnsValueFromContextCore(
        IServiceScopeFactory serviceScopeFactory,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        // Arrange
        const string expectedValue = "AWS_Lambda_dotnet8";
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core
        {
            Stopwatch = stopwatch, ExecutionEnvironment = expectedValue,
        };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            properties,
            cancellationToken);

        // Act
        var result = context.ExecutionEnvironment;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void FunctionName_ReturnsValueFromContextCore(
        IServiceScopeFactory serviceScopeFactory,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        // Arrange
        const string expectedValue = "my-lambda-function";
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core
        {
            Stopwatch = stopwatch, FunctionName = expectedValue,
        };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            properties,
            cancellationToken);

        // Act
        var result = context.FunctionName;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void FunctionMemorySize_ReturnsValueFromContextCore(
        IServiceScopeFactory serviceScopeFactory,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        // Arrange
        const int expectedValue = 512;
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core
        {
            Stopwatch = stopwatch, FunctionMemorySize = expectedValue,
        };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            properties,
            cancellationToken);

        // Act
        var result = context.FunctionMemorySize;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void FunctionVersion_ReturnsValueFromContextCore(
        IServiceScopeFactory serviceScopeFactory,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        // Arrange
        const string expectedValue = "$LATEST";
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core
        {
            Stopwatch = stopwatch, FunctionVersion = expectedValue,
        };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            properties,
            cancellationToken);

        // Act
        var result = context.FunctionVersion;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void InitializationType_ReturnsValueFromContextCore(
        IServiceScopeFactory serviceScopeFactory,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        // Arrange
        const string expectedValue = "on-demand";
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core
        {
            Stopwatch = stopwatch, InitializationType = expectedValue,
        };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            properties,
            cancellationToken);

        // Act
        var result = context.InitializationType;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void LogGroupName_ReturnsValueFromContextCore(
        IServiceScopeFactory serviceScopeFactory,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        // Arrange
        const string expectedValue = "/aws/lambda/my-function";
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core
        {
            Stopwatch = stopwatch, LogGroupName = expectedValue,
        };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            properties,
            cancellationToken);

        // Act
        var result = context.LogGroupName;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void LogStreamName_ReturnsValueFromContextCore(
        IServiceScopeFactory serviceScopeFactory,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        // Arrange
        const string expectedValue = "2024/12/16/[$LATEST]abcdef123456";
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core
        {
            Stopwatch = stopwatch, LogStreamName = expectedValue,
        };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            properties,
            cancellationToken);

        // Act
        var result = context.LogStreamName;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void TaskRoot_ReturnsValueFromContextCore(
        IServiceScopeFactory serviceScopeFactory,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        // Arrange
        const string expectedValue = "/var/task";
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core
        {
            Stopwatch = stopwatch, TaskRoot = expectedValue,
        };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            properties,
            cancellationToken);

        // Act
        var result = context.TaskRoot;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task DisposeAsync_DisposesServiceScopeWhenAsyncDisposable(
        IServiceScopeFactory serviceScopeFactory)
    {
        // Arrange
        var asyncDisposableScope = Substitute.For<IServiceScope, IAsyncDisposable>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        asyncDisposableScope.ServiceProvider.Returns(mockServiceProvider);
        serviceScopeFactory.CreateScope().Returns(asyncDisposableScope);

        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core { Stopwatch = stopwatch };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            CancellationToken.None);
        _ = context.ServiceProvider; // Trigger lazy initialization

        // Act
        await context.DisposeAsync();

        // Assert
        await ((IAsyncDisposable)asyncDisposableScope).Received(1).DisposeAsync();
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task DisposeAsync_DisposesServiceScopeSynchronouslyWhenNotAsyncDisposable(
        IServiceScopeFactory serviceScopeFactory)
    {
        // Arrange
        var mockScope = Substitute.For<IServiceScope>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockScope.ServiceProvider.Returns(mockServiceProvider);
        serviceScopeFactory.CreateScope().Returns(mockScope);

        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core { Stopwatch = stopwatch };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            CancellationToken.None);
        _ = context.ServiceProvider; // Trigger lazy initialization

        // Act
        await context.DisposeAsync();

        // Assert
        mockScope.Received(1).Dispose();
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task DisposeAsync_DoesNotDisposeIfServiceProviderNeverAccessed(
        IServiceScopeFactory serviceScopeFactory)
    {
        // Arrange
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core { Stopwatch = stopwatch };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            CancellationToken.None);

        // Act
        await context.DisposeAsync();

        // Assert
        serviceScopeFactory.DidNotReceive().CreateScope();
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task DisposeAsync_CanBeCalledMultipleTimes(
        IServiceScopeFactory serviceScopeFactory)
    {
        // Arrange
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core { Stopwatch = stopwatch };

        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            CancellationToken.None);

        // Act
        var act = async () =>
        {
            await context.DisposeAsync();
            await context.DisposeAsync(); // Should not throw
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void ContextImplementsIAsyncDisposable(IServiceScopeFactory serviceScopeFactory)
    {
        // Arrange
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core { Stopwatch = stopwatch };

        // Act
        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            CancellationToken.None);

        // Assert
        context.Should().BeAssignableTo<IAsyncDisposable>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void ContextImplementsILambdaLifecycleContext(IServiceScopeFactory serviceScopeFactory)
    {
        // Arrange
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core { Stopwatch = stopwatch };

        // Act
        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            new Dictionary<string, object?>(),
            CancellationToken.None);

        // Assert
        context.Should().BeAssignableTo<ILambdaLifecycleContext>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void MultipleContextInstances_AreIndependent(IServiceScopeFactory serviceScopeFactory)
    {
        // Arrange
        var stopwatch = new LifetimeStopwatch();
        var contextCore1 = new LambdaLifecycleContext.Core
        {
            Stopwatch = stopwatch, Region = "us-east-1",
        };
        var contextCore2 = new LambdaLifecycleContext.Core
        {
            Stopwatch = stopwatch, Region = "us-west-2",
        };

        var properties1 = new Dictionary<string, object?>();
        var properties2 = new Dictionary<string, object?>();

        // Act
        var context1 = new LambdaLifecycleContext(
            contextCore1,
            serviceScopeFactory,
            properties1,
            CancellationToken.None);
        var context2 = new LambdaLifecycleContext(
            contextCore2,
            serviceScopeFactory,
            properties2,
            CancellationToken.None);

        properties1["key"] = "value1";
        properties2["key"] = "value2";

        // Assert
        context1.Properties["key"].Should().Be("value1");
        context2.Properties["key"].Should().Be("value2");
        context1.Region.Should().Be("us-east-1");
        context2.Region.Should().Be("us-west-2");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void NullableProperties_ReturnNullWhenNotSet(
        IServiceScopeFactory serviceScopeFactory,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        // Arrange
        var stopwatch = new LifetimeStopwatch();
        var contextCore = new LambdaLifecycleContext.Core { Stopwatch = stopwatch };

        // Act
        var context = new LambdaLifecycleContext(
            contextCore,
            serviceScopeFactory,
            properties,
            cancellationToken);

        // Assert
        context.Region.Should().BeNull();
        context.ExecutionEnvironment.Should().BeNull();
        context.FunctionName.Should().BeNull();
        context.FunctionMemorySize.Should().BeNull();
        context.FunctionVersion.Should().BeNull();
        context.InitializationType.Should().BeNull();
        context.LogGroupName.Should().BeNull();
        context.LogStreamName.Should().BeNull();
        context.TaskRoot.Should().BeNull();
    }
}
