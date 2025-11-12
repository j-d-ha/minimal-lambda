using System;
using System.Collections.Generic;
using System.Text.Json;
using AwsLambda.Host;
using AwsLambda.Host.APIGatewayEnvelops;
using AwsLambda.Host.Envelopes.APIGateway;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.ClearLambdaOutputFormatting = true;
});

builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNameCaseInsensitive = true;
    options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

var lambda = builder.Build();

lambda.MapHandler(
    ([Event] ApiGatewayRequestEnvelope<Request> request) =>
        new ApiGatewayResponseEnvelope<Response>
        {
            Body = new Response($"Hello {request.Body!.Name}!", DateTime.UtcNow),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
        }
);

// // this wont compile as we can only have a single handler per lambda function
// lambda.MapHandler(
//     ([Event] SqsEnvelope<Request> sqsEnvelope, ILogger logger) =>
//     {
//         var responses = new SQSBatchResponse();
//
//         foreach (var record in sqsEnvelope.Records)
//         {
//             // simulate failure if we get bad data
//             if (record.Body!.Name == "John")
//                 responses.BatchItemFailures.Add(
//                     new SQSBatchResponse.BatchItemFailure { ItemIdentifier = record.MessageId }
//                 );
//
//             // otherwise, log the message
//             logger.LogInformation("Hello {name}!", record.Body.Name);
//         }
//
//         return responses;
//     }
// );
//
// lambda.MapHandler(
//     ([Event] SQSEvent sqsEnvelope, ILogger logger, ILambdaHostContext context) =>
//     {
//         var responses = new SQSBatchResponse();
//
//         foreach (var record in sqsEnvelope.Records)
//         {
//             // simulate failure if we get bad data
//             var body = JsonSerializer.Deserialize<Request>(record.Body);
//
//             if (body!.Name == "John")
//                 responses.BatchItemFailures.Add(
//                     new SQSBatchResponse.BatchItemFailure { ItemIdentifier = record.MessageId }
//                 );
//
//             // otherwise, log the message
//             logger.LogInformation("Hello {name}!", body.Name);
//         }
//
//         return responses;
//     }
// );

await lambda.RunAsync();

internal record Response(string Message, DateTime TimestampUtc);

internal record Request(string Name);
