using Amazon.Lambda.Core;
using AwsLambda.Host.Middleware;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

public static class LambdaOpenTelemetryServiceProviderExtensions
{
    public static Func<LambdaInvocationDelegate, LambdaInvocationDelegate> GetTracer<
        TEvent,
        TResponse
    >(this IServiceProvider services)
    {
        var tracerProvider = services.GetRequiredService<TracerProvider>();

        return next =>
        {
            return async context =>
            {
                if (context.Event is not TEvent inputEvent)
                    throw new InvalidOperationException(
                        $"Lambda event of type '{typeof(TEvent).FullName}' is not available in the context."
                    );

                await AWSLambdaWrapper.TraceAsync(
                    tracerProvider,
                    async Task<TResponse> (TEvent _, ILambdaContext _) =>
                    {
                        await next(context);

                        if (context.Response is not TResponse result)
                            throw new InvalidOperationException(
                                $"Lambda response of type '{typeof(TResponse).FullName}' is not available in the context."
                            );

                        return result;
                    },
                    inputEvent,
                    context
                );
            };
        };
    }

    public static Func<
        LambdaInvocationDelegate,
        LambdaInvocationDelegate
    > GetTracerNoEvent<TResponse>(this IServiceProvider services)
    {
        var tracerProvider = services.GetRequiredService<TracerProvider>();

        return next =>
        {
            return async context =>
            {
                await AWSLambdaWrapper.TraceAsync(
                    tracerProvider,
                    async Task<TResponse> (object? _, ILambdaContext _) =>
                    {
                        await next(context);

                        if (context.Response is not TResponse result)
                            throw new InvalidOperationException(
                                $"Lambda response of type '{typeof(TResponse).FullName}' is not available in the context."
                            );

                        return result;
                    },
                    null,
                    context
                );
            };
        };
    }

    public static Func<
        LambdaInvocationDelegate,
        LambdaInvocationDelegate
    > GetTracerNoResponse<TEvent>(this IServiceProvider services)
    {
        var tracerProvider = services.GetRequiredService<TracerProvider>();

        return next =>
        {
            return async context =>
            {
                if (context.Event is not TEvent inputEvent)
                    throw new InvalidOperationException(
                        $"Lambda event of type '{typeof(TEvent).FullName}' is not available in the context."
                    );

                await AWSLambdaWrapper.TraceAsync(
                    tracerProvider,
                    async Task (TEvent _, ILambdaContext _) => await next(context),
                    inputEvent,
                    context
                );
            };
        };
    }

    public static Func<
        LambdaInvocationDelegate,
        LambdaInvocationDelegate
    > GetTracerNoEventNoResponse(this IServiceProvider services)
    {
        var tracerProvider = services.GetRequiredService<TracerProvider>();

        return next =>
        {
            return async context =>
            {
                await AWSLambdaWrapper.TraceAsync(
                    tracerProvider,
                    async Task (object? _, ILambdaContext _) => await next(context),
                    null,
                    context
                );
            };
        };
    }
}
