using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalLambda;

internal class LambdaLifecycleContextFactory(
    IServiceScopeFactory scopeFactory,
    ILifetimeStopwatch stopwatch,
    IConfiguration configuration) : ILambdaLifecycleContextFactory
{
    private LambdaLifecycleContext.Core? _contextCore;

    public ILambdaLifecycleContext Create(
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken)
    {
        _contextCore ??= new LambdaLifecycleContext.Core
        {
            Stopwatch = stopwatch,
            Region = configuration["AWS_REGION"] ?? configuration["AWS_DEFAULT_REGION"],
            ExecutionEnvironment = configuration["AWS_EXECUTION_ENV"],
            FunctionName = configuration["AWS_LAMBDA_FUNCTION_NAME"],
            FunctionMemorySize =
                int.TryParse(
                    configuration["AWS_LAMBDA_FUNCTION_MEMORY_SIZE"],
                    out var memorySize)
                    ? memorySize
                    : null,
            FunctionVersion = configuration["AWS_LAMBDA_FUNCTION_VERSION"],
            InitializationType = configuration["AWS_LAMBDA_INITIALIZATION_TYPE"],
            LogGroupName = configuration["AWS_LAMBDA_LOG_GROUP_NAME"],
            LogStreamName = configuration["AWS_LAMBDA_LOG_STREAM_NAME"],
            TaskRoot = configuration["LAMBDA_TASK_ROOT"],
        };

        return new LambdaLifecycleContext(
            _contextCore,
            scopeFactory,
            properties,
            cancellationToken);
    }
}
