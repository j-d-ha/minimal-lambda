using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalLambda.UnitTests.Core.Context;

[TestSubject(typeof(LambdaLifecycleContextFactory))]
public class LambdaLifecycleContextFactoryTests
{
    [Theory]
    [AutoNSubstituteData]
    internal void Constructor_WithValidDependencies_SuccessfullyConstructs(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch,
        IConfiguration configuration)
    {
        // Act
        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Assert
        factory.Should().NotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReturnsLambdaLifecycleContext(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch,
        IConfiguration configuration,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        // Arrange
        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(properties, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<ILambdaLifecycleContext>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_PassesPropertiesAndCancellationTokenToContext(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch,
        IConfiguration configuration)
    {
        // Arrange
        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);
        var properties = new Dictionary<string, object?> { { "key", "value" } };
        var cancellationToken = new CancellationToken();

        // Act
        var result = factory.Create(properties, cancellationToken);

        // Assert
        result.Properties.Should().BeSameAs(properties);
        result.CancellationToken.Should().Be(cancellationToken);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReadsRegionFromAwsRegionConfiguration(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?> { { "AWS_REGION", "us-east-1" } };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.Region.Should().Be("us-east-1");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReadsRegionFromAwsDefaultRegionWhenAwsRegionNotSet(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            { "AWS_DEFAULT_REGION", "us-west-2" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.Region.Should().Be("us-west-2");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_PrefersAwsRegionOverAwsDefaultRegion(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            { "AWS_REGION", "us-east-1" }, { "AWS_DEFAULT_REGION", "us-west-2" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.Region.Should().Be("us-east-1");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReadsExecutionEnvironmentFromConfiguration(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            { "AWS_EXECUTION_ENV", "AWS_Lambda_dotnet8" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.ExecutionEnvironment.Should().Be("AWS_Lambda_dotnet8");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReadsFunctionNameFromConfiguration(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            { "AWS_LAMBDA_FUNCTION_NAME", "my-lambda-function" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.FunctionName.Should().Be("my-lambda-function");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReadsFunctionMemorySizeFromConfiguration(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            { "AWS_LAMBDA_FUNCTION_MEMORY_SIZE", "512" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.FunctionMemorySize.Should().Be(512);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReturnsNullForFunctionMemorySizeWhenNotParseable(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            { "AWS_LAMBDA_FUNCTION_MEMORY_SIZE", "not-a-number" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.FunctionMemorySize.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReadsFunctionVersionFromConfiguration(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            { "AWS_LAMBDA_FUNCTION_VERSION", "$LATEST" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.FunctionVersion.Should().Be("$LATEST");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReadsInitializationTypeFromConfiguration(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            { "AWS_LAMBDA_INITIALIZATION_TYPE", "on-demand" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.InitializationType.Should().Be("on-demand");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReadsLogGroupNameFromConfiguration(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            { "AWS_LAMBDA_LOG_GROUP_NAME", "/aws/lambda/my-function" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.LogGroupName.Should().Be("/aws/lambda/my-function");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReadsLogStreamNameFromConfiguration(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            { "AWS_LAMBDA_LOG_STREAM_NAME", "2024/12/16/[$LATEST]abcdef123456" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.LogStreamName.Should().Be("2024/12/16/[$LATEST]abcdef123456");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReadsTaskRootFromConfiguration(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?> { { "LAMBDA_TASK_ROOT", "/var/task" } };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.TaskRoot.Should().Be("/var/task");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_UsesStopwatchFromConstructor(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch,
        IConfiguration configuration)
    {
        // Arrange
        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        (result.ElapsedTime >= TimeSpan.Zero).Should().BeTrue();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReusesContextCoreAcrossMultipleCalls(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            { "AWS_REGION", "us-east-1" }, { "AWS_LAMBDA_FUNCTION_NAME", "my-function" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result1 = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);
        var result2 = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result1.Region.Should().Be("us-east-1");
        result2.Region.Should().Be("us-east-1");
        result1.FunctionName.Should().Be("my-function");
        result2.FunctionName.Should().Be("my-function");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_ReturnsNullForUnconfiguredProperties(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.Region.Should().BeNull();
        result.ExecutionEnvironment.Should().BeNull();
        result.FunctionName.Should().BeNull();
        result.FunctionMemorySize.Should().BeNull();
        result.FunctionVersion.Should().BeNull();
        result.InitializationType.Should().BeNull();
        result.LogGroupName.Should().BeNull();
        result.LogStreamName.Should().BeNull();
        result.TaskRoot.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Create_WithAllConfigurationValues_PopulatesAllProperties(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch)
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            { "AWS_REGION", "us-east-1" },
            { "AWS_EXECUTION_ENV", "AWS_Lambda_dotnet8" },
            { "AWS_LAMBDA_FUNCTION_NAME", "my-lambda-function" },
            { "AWS_LAMBDA_FUNCTION_MEMORY_SIZE", "1024" },
            { "AWS_LAMBDA_FUNCTION_VERSION", "$LATEST" },
            { "AWS_LAMBDA_INITIALIZATION_TYPE", "provisioned-concurrency" },
            { "AWS_LAMBDA_LOG_GROUP_NAME", "/aws/lambda/my-function" },
            { "AWS_LAMBDA_LOG_STREAM_NAME", "2024/12/16/[$LATEST]abcdef123456" },
            { "LAMBDA_TASK_ROOT", "/var/task" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Act
        var result = factory.Create(new Dictionary<string, object?>(), CancellationToken.None);

        // Assert
        result.Region.Should().Be("us-east-1");
        result.ExecutionEnvironment.Should().Be("AWS_Lambda_dotnet8");
        result.FunctionName.Should().Be("my-lambda-function");
        result.FunctionMemorySize.Should().Be(1024);
        result.FunctionVersion.Should().Be("$LATEST");
        result.InitializationType.Should().Be("provisioned-concurrency");
        result.LogGroupName.Should().Be("/aws/lambda/my-function");
        result.LogStreamName.Should().Be("2024/12/16/[$LATEST]abcdef123456");
        result.TaskRoot.Should().Be("/var/task");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void FactoryImplementsILambdaLifecycleContextFactory(
        IServiceScopeFactory serviceScopeFactory,
        LifetimeStopwatch stopwatch,
        IConfiguration configuration)
    {
        // Act
        var factory = new LambdaLifecycleContextFactory(
            serviceScopeFactory,
            stopwatch,
            configuration);

        // Assert
        factory.Should().BeAssignableTo<ILambdaLifecycleContextFactory>();
    }
}
