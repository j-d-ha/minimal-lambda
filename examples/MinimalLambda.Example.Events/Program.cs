using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinimalLambda.Host.Builder;

var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.ClearLambdaOutputFormatting = true;
});

builder.Services.ConfigureEnvelopeOptions(options =>
{
    options.JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.JsonOptions.TypeInfoResolver = SerializerContext.Default;
});

builder.Services.AddLambdaSerializerWithContext<SerializerContext>();

var lambda = builder.Build();

lambda.MapHandler(
    ([Event] ApiGatewayRequestEnvelope<Request> request, ILogger<Program> logger) =>
    {
        logger.LogInformation("In Handler. Payload: {Payload}", request.Body);

        return new ApiGatewayResponseEnvelope<Response>
        {
            BodyContent = new Response($"Hello {request.BodyContent?.Name}!", DateTime.UtcNow),
            StatusCode = 201,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
        };
    }
);

await lambda.RunAsync();

internal record Response(string Message, DateTime TimestampUtc);

internal record Request(string Name);

[JsonSerializable(typeof(ApiGatewayRequestEnvelope<Request>))]
[JsonSerializable(typeof(ApiGatewayResponseEnvelope<Response>))]
[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(Response))]
internal partial class SerializerContext : JsonSerializerContext;
