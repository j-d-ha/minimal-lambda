using Amazon.Lambda.Core;
using AwsLambda.Host;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Provides extension methods for integrating OpenTelemetry tracing with AWS Lambda
///     invocations.
/// </summary>
public static class LambdaOpenTelemetryServiceProviderExtensions
{
    extension(IServiceProvider services)
    {
        /// <summary>
        ///     Creates a middleware function that traces Lambda invocations with both event and response
        ///     types.
        /// </summary>
        /// <typeparam name="TEvent">The type of the Lambda event expected in the context.</typeparam>
        /// <typeparam name="TResponse">The type of the Lambda response expected in the context.</typeparam>
        /// <returns>A middleware function that wraps the Lambda invocation with OpenTelemetry tracing.</returns>
        /// <remarks>
        ///     <para>
        ///         <b>Important:</b> These methods are primarily intended to be used by source generators
        ///         and interceptors. Direct usage is not recommended. Source generation and interception are
        ///         the primary use cases for automatic tracing integration.
        ///     </para>
        ///     <para>
        ///         Uses the registered <see cref="TracerProvider" /> to wrap Lambda invocations with
        ///         distributed tracing capabilities through AWS Lambda instrumentation. This method is a
        ///         wrapper around <see cref="OpenTelemetry.Instrumentation.AWSLambda.AWSLambdaWrapper" /> from
        ///         the
        ///         <see href="https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AWSLambda">OpenTelemetry.Instrumentation.AWSLambda</see>
        ///         NuGet package.
        ///     </para>
        ///     <para>
        ///         The context must contain an event of type <typeparamref name="TEvent" /> and the handler
        ///         must set a response of type <typeparamref name="TResponse" />.
        ///     </para>
        ///     <para>
        ///         <b>TracerProvider Registration Required:</b> A <see cref="TracerProvider" /> instance
        ///         must be registered in the dependency injection container before calling these methods.
        ///         Failure to register a <see cref="TracerProvider" /> will result in an
        ///         <see cref="InvalidOperationException" /> being thrown at startup.
        ///     </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the context event is not of type
        ///     <typeparamref name="TEvent" /> or if the context response is not of type
        ///     <typeparamref name="TResponse" />, or if a <see cref="TracerProvider" /> instance is not
        ///     registered in the dependency injection container.
        /// </exception>
        public Func<LambdaInvocationDelegate, LambdaInvocationDelegate> GetOpenTelemetryTracer<
            TEvent,
            TResponse
        >()
        {
            ArgumentNullException.ThrowIfNull(services);

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
                        async Task<TResponse> (_, _) =>
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

        /// <summary>Creates a middleware function that traces Lambda invocations with only a response type.</summary>
        /// <typeparam name="TResponse">The type of the Lambda response expected in the context.</typeparam>
        /// <returns><inheritdoc cref="GetOpenTelemetryTracer{TEvent,TResponse}" path="/returns" /></returns>
        /// <remarks>
        ///     <inheritdoc cref="GetOpenTelemetryTracer{TEvent,TResponse}" path="/remarks" /> The event
        ///     type is not relevant or known when using this overload.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the context response is not of type
        ///     <typeparamref name="TResponse" />.
        /// </exception>
        public Func<
            LambdaInvocationDelegate,
            LambdaInvocationDelegate
        > GetOpenTelemetryTracerNoEvent<TResponse>()
        {
            ArgumentNullException.ThrowIfNull(services);

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

        /// <summary>Creates a middleware function that traces Lambda invocations with only an event type.</summary>
        /// <typeparam name="TEvent">The type of the Lambda event expected in the context.</typeparam>
        /// <returns><inheritdoc cref="GetOpenTelemetryTracer{TEvent,TResponse}" path="/returns" /></returns>
        /// <remarks>
        ///     <inheritdoc cref="GetOpenTelemetryTracer{TEvent,TResponse}" path="/remarks" /> The
        ///     response type is not relevant or known when using this overload.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the context event is not of type
        ///     <typeparamref name="TEvent" />.
        /// </exception>
        public Func<
            LambdaInvocationDelegate,
            LambdaInvocationDelegate
        > GetOpenTelemetryTracerNoResponse<TEvent>()
        {
            ArgumentNullException.ThrowIfNull(services);

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
                        async Task (_, _) => await next(context),
                        inputEvent,
                        context
                    );
                };
            };
        }

        /// <summary>
        ///     Creates a middleware function that traces Lambda invocations without specific event or
        ///     response types.
        /// </summary>
        /// <returns><inheritdoc cref="GetOpenTelemetryTracer{TEvent,TResponse}" path="/returns" /></returns>
        /// <remarks>
        ///     <inheritdoc cref="GetOpenTelemetryTracer{TEvent,TResponse}" path="/remarks" /> Neither
        ///     event nor response types are relevant or known when using this overload.
        /// </remarks>
        public Func<
            LambdaInvocationDelegate,
            LambdaInvocationDelegate
        > GetOpenTelemetryTracerNoEventNoResponse()
        {
            ArgumentNullException.ThrowIfNull(services);

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
}
