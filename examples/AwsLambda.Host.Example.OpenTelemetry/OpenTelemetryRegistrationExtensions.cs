using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AwsLambda.Host.Example.OpenTelemetry;

internal static class OpenTelemetryRegistrationExtensions
{
    internal static IServiceCollection AddOtel(this IServiceCollection services)
    {
        services
            .AddOpenTelemetry()
            .WithTracing(configure =>
                configure
                    .AddAWSLambdaConfigurations()
                    .AddSource(Instrumentation.ActivitySourceName)
                    .SetResourceBuilder(
                        ResourceBuilder
                            .CreateDefault()
                            .AddService(
                                Instrumentation.ActivitySourceName,
                                serviceVersion: Instrumentation.ActivitySourceVersion
                            )
                    )
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://localhost:4318/v1/traces");
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    })
            )
            .WithMetrics(configure =>
                configure
                    .AddMeter(Instrumentation.ActivitySourceName)
                    .SetResourceBuilder(
                        ResourceBuilder
                            .CreateDefault()
                            .AddService(
                                Instrumentation.ActivitySourceName,
                                serviceVersion: Instrumentation.ActivitySourceVersion
                            )
                    )
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://localhost:4318/v1/metrics");
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    })
            );

        return services;
    }
}
