using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace AwsLambda.Host;

/// <summary>
///     Extension methods for configuring OpenTelemetry force flush behavior during Lambda
///     shutdown.
/// </summary>
public static class OnShutdownOpenTelemetryExtensions
{
    private const string LogCategory = "AwsLambda.Host.OpenTelemetry";

    /// <summary>
    ///     Registers shutdown handlers to force flush both OpenTelemetry tracers and meters on Lambda
    ///     shutdown.
    /// </summary>
    /// <param name="application">The <see cref="ILambdaApplication" /> instance.</param>
    /// <param name="timeoutMilliseconds">
    ///     The timeout in milliseconds for flush operations. Defaults to
    ///     <see cref="Timeout.Infinite" />.
    /// </param>
    /// <returns>The same <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <remarks>
    ///     <para>
    ///         This method registers shutdown handlers that force flush both tracer and meter providers
    ///         to ensure all telemetry data is exported before the Lambda container stops.
    ///     </para>
    ///     <para>
    ///         The flush operations respect Lambda's timeout constraints and log warnings if they cannot
    ///         complete within the allocated shutdown time.
    ///     </para>
    /// </remarks>
    public static ILambdaApplication OnShutdownFlushOpenTelemetry(
        this ILambdaApplication application,
        int timeoutMilliseconds = Timeout.Infinite
    )
    {
        ArgumentNullException.ThrowIfNull(application);

        application.OnShutdownFlushMeter(timeoutMilliseconds);

        application.OnShutdownFlushTracer(timeoutMilliseconds);

        return application;
    }

    /// <summary>
    ///     Registers a shutdown handler to force flush the OpenTelemetry tracer provider on Lambda
    ///     shutdown.
    /// </summary>
    /// <param name="application">The <see cref="ILambdaApplication" /> instance.</param>
    /// <param name="timeoutMilliseconds">
    ///     The timeout in milliseconds for the flush operation. Defaults to
    ///     <see cref="Timeout.Infinite" />.
    /// </param>
    /// <returns>The same <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <remarks>
    ///     <para>
    ///         This method registers a shutdown handler that force flushes the tracer provider to ensure
    ///         all distributed traces are exported before Lambda shutdown completes.
    ///     </para>
    ///     <para>
    ///         If no <see cref="TracerProvider" /> is registered in the dependency injection container,
    ///         this method safely returns without error.
    ///     </para>
    /// </remarks>
    public static ILambdaApplication OnShutdownFlushTracer(
        this ILambdaApplication application,
        int timeoutMilliseconds = Timeout.Infinite
    )
    {
        ArgumentNullException.ThrowIfNull(application);

        application.OnShutdown(
            async Task (services, cancellationToken) =>
            {
                var tracerProvider = services.GetService<TracerProvider>();
                if (tracerProvider is null)
                    return;

                var logger = services.GetService<ILoggerFactory>()?.CreateLogger(LogCategory);

                var flusher = Task.Run(
                    () => tracerProvider.ForceFlush(timeoutMilliseconds),
                    cancellationToken
                );

                await Task.WhenAny(flusher, Task.Delay(Timeout.Infinite, cancellationToken));

                if (flusher.Status != TaskStatus.RanToCompletion)
                {
                    logger?.LogWarning(
                        "OpenTelemetry tracer provider force flush failed to complete within allocated time"
                    );

                    return;
                }

                logger?.LogInformation(
                    "OpenTelemetry tracer provider force flush {status}",
                    flusher.Result ? "succeeded" : "failed"
                );
            }
        );

        return application;
    }

    /// <summary>
    ///     Registers a shutdown handler to force flush the OpenTelemetry meter provider on Lambda
    ///     shutdown.
    /// </summary>
    /// <param name="application">The <see cref="ILambdaApplication" /> instance.</param>
    /// <param name="timeoutMilliseconds">
    ///     The timeout in milliseconds for the flush operation. Defaults to
    ///     <see cref="Timeout.Infinite" />.
    /// </param>
    /// <returns>The same <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <remarks>
    ///     <para>
    ///         This method registers a shutdown handler that force flushes the meter provider to ensure
    ///         all metrics are exported before Lambda shutdown completes.
    ///     </para>
    ///     <para>
    ///         If no <see cref="MeterProvider" /> is registered in the dependency injection container,
    ///         this method safely returns without error.
    ///     </para>
    /// </remarks>
    public static ILambdaApplication OnShutdownFlushMeter(
        this ILambdaApplication application,
        int timeoutMilliseconds = Timeout.Infinite
    )
    {
        ArgumentNullException.ThrowIfNull(application);

        application.OnShutdown(
            async Task (services, cancellationToken) =>
            {
                var meterProvider = services.GetService<MeterProvider>();
                if (meterProvider is null)
                    return;

                var logger = services.GetService<ILoggerFactory>()?.CreateLogger(LogCategory);

                var flusher = Task.Run(
                    () => meterProvider.ForceFlush(timeoutMilliseconds),
                    cancellationToken
                );

                await Task.WhenAny(flusher, Task.Delay(Timeout.Infinite, cancellationToken));

                if (flusher.Status != TaskStatus.RanToCompletion)
                {
                    logger?.LogWarning(
                        "OpenTelemetry meter provider force flush failed to complete within allocated time"
                    );

                    return;
                }

                logger?.LogInformation(
                    "OpenTelemetry meter provider force flush {status}",
                    flusher.Result ? "succeeded" : "failed"
                );
            }
        );

        return application;
    }
}
