using Amazon.Lambda.Core;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Trace;

namespace AwsLambda.Host.OpenTelemetry;

public static class LambdaOpenTelemetryAdapters
{
    public static Task<TResult> TraceAsync<TInput, TResult>(
        TracerProvider? tracerProvider,
        Func<TInput, ILambdaContext, Task<TResult>> lambdaHandler,
        TInput input,
        ILambdaContext context
    ) =>
        lambdaHandler is null
            ? throw new ArgumentNullException(nameof(lambdaHandler), "Must not be null")
            : AWSLambdaWrapper.TraceAsync(tracerProvider, lambdaHandler, input, context);

    public static Task TraceAsync<TInput>(
        TracerProvider? tracerProvider,
        Func<TInput, ILambdaContext, Task> lambdaHandler,
        TInput input,
        ILambdaContext context
    ) =>
        lambdaHandler is null
            ? throw new ArgumentNullException(nameof(lambdaHandler), "Must not be null")
            : AWSLambdaWrapper.TraceAsync(tracerProvider, lambdaHandler, input, context);
}
