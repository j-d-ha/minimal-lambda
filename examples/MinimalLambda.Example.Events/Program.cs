using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinimalLambda.Builder;
using MinimalLambda.Envelopes;
using MinimalLambda.Envelopes.ApiGateway;

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
    ([FromEvent] ApiGatewayRequestEnvelope<Request> request, ILogger<Program> logger) =>
    {
        logger.LogInformation("In Handler. Payload: {Payload}", request.Body);

        if (request.BodyContent == null)
            return ApiGatewayResult.InternalServerError(new BadErrorDetails("Bad error"));

        if (request.BodyContent.Name == "error")
            return ApiGatewayResult
                .BadRequest(new ErrorDetails("bummer"))
                .Customize(response =>
                {
                    response.Headers.Add("X-Custom-Header", "Custom Value");
                    response.MultiValueHeaders.Add("X-Custom-Header", ["Custom Value 2"]);
                });

        return ApiGatewayResult
            .Ok(new Response($"Hello {request.BodyContent?.Name}!", DateTime.UtcNow))
            .Customize(result => result.Headers.Add("X-Custom-Header", "Custom Value"));
    }
);

await lambda.RunAsync();

internal record Response(string Message, DateTime TimestampUtc);

internal record ErrorDetails(string Message);

internal record BadErrorDetails(string Message);

internal record Request(string Name);

[JsonSerializable(typeof(ApiGatewayRequestEnvelope<Request>))]
[JsonSerializable(typeof(ApiGatewayResult))]
[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(ErrorDetails))]
[JsonSerializable(typeof(Response))]
internal partial class SerializerContext : JsonSerializerContext;
