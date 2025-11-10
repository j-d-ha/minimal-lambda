using System;
using System.Collections.Generic;
using System.Text.Json;
using AwsLambda.Host;
using AwsLambda.Host.APIGatewayEvents;
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
    ([Event] APIGatewayProxyRequest<Request> request) =>
        new APIGatewayProxyResponse<Response>
        {
            Body = new Response($"Hello {request.Body!.Name}!", DateTime.UtcNow),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
        }
);

await lambda.RunAsync();

internal record Response(string Message, DateTime TimestampUtc);

internal record Request(string Name);
