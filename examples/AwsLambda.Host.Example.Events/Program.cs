using System;
using System.Collections.Generic;
using System.Text.Json;
using AwsLambda.Host;
using AwsLambda.Host.APIGatewayEvents;
using AwsLambda.Host.SQSEvent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
    ([Event] APIGatewayProxyRequest<Request> request) =>
        new APIGatewayProxyResponse<Response>
        {
            Body = new Response($"Hello {request.Body!.Name}!", DateTime.UtcNow),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
        }
);

// this wont compile as we can only have a single handler per lambda function
lambda.MapHandler(
    ([Event] SQSEvent<Request> sqsEvent, ILogger logger) =>
    {
        var responses = new SQSBatchResponse();

        foreach (var record in sqsEvent.Records)
        {
            // simulate failure if we get bad data
            if (record.Body!.Name == "John")
                responses.BatchItemFailures.Add(
                    new Amazon.Lambda.SQSEvents.SQSBatchResponse.BatchItemFailure
                    {
                        ItemIdentifier = record.MessageId,
                    }
                );

            // otherwise, log the message
            logger.LogInformation("Hello {name}!", record.Body.Name);
        }

        return responses;
    }
);

await lambda.RunAsync();

internal record Response(string Message, DateTime TimestampUtc);

internal record Request(string Name);
