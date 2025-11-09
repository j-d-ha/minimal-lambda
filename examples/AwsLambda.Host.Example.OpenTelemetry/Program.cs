using AwsLambda.Host;
using AwsLambda.Host.Example.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AWSLambda;
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
    );

var lambda = builder.Build();

lambda.UseOpenTelemetryTracing();

lambda.MapHandler(
    async (
        [Event] Request request,
        IService service,
        Instrumentation instrumentation,
        CancellationToken cancellationToken
    ) =>
    {
        using var activity = instrumentation.ActivitySource.StartActivity("Handler");

        var message = await service.GetMessage(request.Name, cancellationToken);

        return new Response(message, DateTime.UtcNow);
    }
);

await lambda.RunAsync();

internal record Request(string Name);

internal record Response(string Message, DateTime Timestamp);
