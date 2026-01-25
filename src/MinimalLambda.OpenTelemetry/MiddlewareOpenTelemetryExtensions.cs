using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Trace;

namespace MinimalLambda.Builder;

/// <summary>
///     Provides extension methods for enabling OpenTelemetry tracing in the Lambda invocation
///     pipeline.
/// </summary>
public static class MiddlewareOpenTelemetryExtensions
{
    extension(ILambdaInvocationBuilder builder)
    {
        /// <summary>Enables OpenTelemetry tracing for AWS Lambda handler invocations.</summary>
        /// <remarks>
        ///     <para>
        ///         Adds middleware that wraps each Lambda invocation with distributed tracing using the
        ///         <see cref="AWSLambdaWrapper" /> from the OpenTelemetry AWS Lambda instrumentation package.
        ///         A root span is created for each invocation with Lambda context information.
        ///     </para>
        ///     <para>
        ///         <b>Middleware Placement:</b> Call this method early in the middleware pipeline to capture
        ///         the execution time of all subsequent middleware and handler logic.
        ///     </para>
        ///     <para>
        ///         <b>TracerProvider Registration Required:</b> A <see cref="TracerProvider" /> must be
        ///         registered in the dependency injection container. If not found, an
        ///         <see cref="InvalidOperationException" /> is thrown at startup.
        ///     </para>
        /// </remarks>
        /// <returns>The same <see cref="ILambdaInvocationBuilder" /> instance for method chaining.</returns>
        public ILambdaInvocationBuilder UseOpenTelemetryTracing()
        {
            ArgumentNullException.ThrowIfNull(builder);

            var tracerProvider = builder.Services.GetRequiredService<TracerProvider>();

            return builder.Use(next => context => AWSLambdaWrapper.TraceAsync(
                tracerProvider,
                async Task<object?> (_, _) =>
                {
                    await next(context);

                    return context.Features.Get<IResponseFeature>()?.GetResponse();
                },
                context.Features.Get<IEventFeature>()?.GetEvent(context),
                context));
        }
    }
}
