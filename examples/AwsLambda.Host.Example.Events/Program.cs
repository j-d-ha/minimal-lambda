using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using AwsLambda.Host;
using AwsLambda.Host.Envelopes.APIGateway;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
    ([Event] APIGatewayRequestEnvelope<Request> request, ILogger<Program> logger) =>
    {
        logger.LogInformation("In Handler. Payload: {payload}", request.Body);

        return new APIGatewayResponseEnvelope<Response>
        {
            BodyContent = new Response($"Hello {request.BodyContent?.Name}!", DateTime.UtcNow),
            StatusCode = 201,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
        };
    }
);

// // this wont compile as we can only have a single handler per lambda function
// lambda.MapHandler(
//     ([Event] SQSEnvelope<Request> sqsEnvelope, ILogger<Program> logger) =>
//     {
//         var responses = new SQSBatchResponse();
//
//         foreach (var record in sqsEnvelope.Records)
//         {
//             // simulate failure if we get bad data
//             if (record.BodyContent?.Name is null or "John")
//             {
//                 responses.BatchItemFailures.Add(
//                     new SQSBatchResponse.BatchItemFailure { ItemIdentifier = record.MessageId }
//                 );
//
//                 continue;
//             }
//
//             // otherwise, log the message
//             logger.LogInformation("Hello {name}!", record.BodyContent.Name);
//         }
//
//         return responses;
//     }
// );

await lambda.RunAsync();

internal record Response(string Message, DateTime TimestampUtc);

internal record Request(string Name);

[JsonSerializable(typeof(APIGatewayRequestEnvelope<Request>))]
[JsonSerializable(typeof(APIGatewayResponseEnvelope<Response>))]
[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(Response))]
internal partial class SerializerContext : JsonSerializerContext;
