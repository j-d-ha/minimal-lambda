using AwsLambda.Host;
using AwsLambda.Host.Example.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddScoped<IService, Service>();
builder.Services.AddSingleton<Instrumentation>();
builder.Services.AddSingleton<NameMetrics>();

builder
    .Services.AddOpenTelemetry()
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

var lambda = builder.Build();

lambda.UseOpenTelemetryTracing();

lambda.MapHandler(Function.Handler);

lambda.OnShutdownFlushOpenTelemetry();

await lambda.RunAsync();
