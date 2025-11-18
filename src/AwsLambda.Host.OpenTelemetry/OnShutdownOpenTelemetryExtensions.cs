using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

    extension(ILambdaOnShutdownBuilder application)
    {
        /// <summary>
        ///     Registers shutdown handlers to force flush both OpenTelemetry tracers and meters on Lambda
        ///     shutdown.
        /// </summary>
        /// <param name="timeoutMilliseconds">
        ///     The timeout in milliseconds for flush operations. Defaults to
        ///     <see cref="Timeout.Infinite" />.
        /// </param>
        /// <returns>The same <see cref="ILambdaOnShutdownBuilder" /> instance for method chaining.</returns>
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
        public ILambdaOnShutdownBuilder OnShutdownFlushOpenTelemetry(
            int timeoutMilliseconds = Timeout.Infinite
        )
        {
            ArgumentNullException.ThrowIfNull(application);

            application.OnShutdownFlushTracer(timeoutMilliseconds);

            application.OnShutdownFlushMeter(timeoutMilliseconds);

            return application;
        }

        /// <summary>
        ///     Registers a shutdown handler to force flush the OpenTelemetry tracer provider on Lambda
        ///     shutdown.
        /// </summary>
        /// <param name="timeoutMilliseconds">
        ///     The timeout in milliseconds for the flush operation. Defaults to
        ///     <see cref="Timeout.Infinite" />.
        /// </param>
        /// <returns>The same <see cref="ILambdaOnShutdownBuilder" /> instance for method chaining.</returns>
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
        public ILambdaOnShutdownBuilder OnShutdownFlushTracer(
            int timeoutMilliseconds = Timeout.Infinite
        )
        {
            ArgumentNullException.ThrowIfNull(application);

            var tracerProvider = application.Services.GetRequiredService<TracerProvider>();
            var logger =
                application.Services.GetService<ILoggerFactory>()?.CreateLogger(LogCategory)
                ?? NullLogger.Instance;

            application.ShutdownHandlers.Add(
                (_, cancellationToken) =>
                    RunForceFlush(
                        "tracer",
                        tracerProvider.ForceFlush,
                        timeoutMilliseconds,
                        logger,
                        cancellationToken
                    )
            );

            return application;
        }

        /// <summary>
        ///     Registers a shutdown handler to force flush the OpenTelemetry meter provider on Lambda
        ///     shutdown.
        /// </summary>
        /// <param name="timeoutMilliseconds">
        ///     The timeout in milliseconds for the flush operation. Defaults to
        ///     <see cref="Timeout.Infinite" />.
        /// </param>
        /// <returns>The same <see cref="ILambdaOnShutdownBuilder" /> instance for method chaining.</returns>
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
        public ILambdaOnShutdownBuilder OnShutdownFlushMeter(
            int timeoutMilliseconds = Timeout.Infinite
        )
        {
            ArgumentNullException.ThrowIfNull(application);

            var meterProvider = application.Services.GetRequiredService<MeterProvider>();
            var logger =
                application.Services.GetService<ILoggerFactory>()?.CreateLogger(LogCategory)
                ?? NullLogger.Instance;

            application.ShutdownHandlers.Add(
                (_, cancellationToken) =>
                    RunForceFlush(
                        "meter",
                        meterProvider.ForceFlush,
                        timeoutMilliseconds,
                        logger,
                        cancellationToken
                    )
            );

            return application;
        }

        /// <summary>Executes a force flush operation with timeout handling and logging.</summary>
        private static async Task RunForceFlush(
            string providerName,
            Func<int, bool> flusher,
            int timeoutMilliseconds,
            ILogger logger,
            CancellationToken cancellationToken
        )
        {
            var flusherTask = Task.Run(() => flusher(timeoutMilliseconds), cancellationToken);

            await Task.WhenAny(flusherTask, Task.Delay(Timeout.Infinite, cancellationToken));

            if (flusherTask.Status != TaskStatus.RanToCompletion)
            {
                logger.LogWarning(
                    "OpenTelemetry {ProviderName} provider force flush failed to complete within allocated time",
                    providerName
                );

                return;
            }

            logger.LogInformation(
                "OpenTelemetry {ProviderName} provider force flush {Status}",
                providerName,
                flusherTask.Result ? "succeeded" : "failed"
            );
        }
    }
}
