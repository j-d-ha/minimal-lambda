#region

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinimalLambda.Builder;
using MinimalLambda.Envelopes.ApiGateway;

#endregion

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
    ApiGatewayResult<Response, ErrorDetails, BadErrorDetails> (
        [Event] ApiGatewayRequestEnvelope<Request> request,
        ILogger<Program> logger
    ) =>
    {
        logger.LogInformation("In Handler. Payload: {Payload}", request.Body);

        if (request.BodyContent == null)
            return ApiGatewayResult.InternalServerError(new BadErrorDetails("Bad error"));

        if (request.BodyContent.Name == "error")
            return ApiGatewayResult.BadRequest(new ErrorDetails("bummer"));

        return ApiGatewayResult.Ok(
            new Response($"Hello {request.BodyContent?.Name}!", DateTime.UtcNow)
        );
    }
);

await lambda.RunAsync();

public static class ApiGatewayResultExtensions
{
    extension(ApiGatewayResult)
    {
        public static ApiGatewayResult<T> Ok<T>(T bodyContent) =>
            new()
            {
                BodyContent = bodyContent,
                StatusCode = 200,
                Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
            };

        public static ApiGatewayResult<T> BadRequest<T>(T bodyContent) =>
            new()
            {
                BodyContent = bodyContent,
                StatusCode = 400,
                Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
            };

        public static ApiGatewayResult<T> InternalServerError<T>(T bodyContent) =>
            new()
            {
                BodyContent = bodyContent,
                StatusCode = 500,
                Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
            };
    }
}

internal record Response(string Message, DateTime TimestampUtc);

internal record ErrorDetails(string Message);

internal record BadErrorDetails(string Message);

internal record Request(string Name);

[JsonSerializable(typeof(ApiGatewayRequestEnvelope<Request>))]
[JsonSerializable(typeof(ApiGatewayResult<Response, ErrorDetails>))]
[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(ErrorDetails))]
[JsonSerializable(typeof(Response))]
internal partial class SerializerContext : JsonSerializerContext;
