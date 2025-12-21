using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinimalLambda;
using MinimalLambda.Builder;

var builder = LambdaApplication.CreateBuilder();

var lambda = builder.Build();

lambda.MapHandler(
    ([FromEvent] Request request) => new Response($"Hello {request.Name}!", DateTime.UtcNow)
);

await lambda.RunAsync();

internal record Response(string Message, DateTime TimestampUtc);

internal record Request(string Name);

internal class Middleware(ILogger<Middleware> logger) : ILambdaMiddleware
{
    public async Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
    {
        var request = context.GetRequiredEvent<Request>();
        logger.LogInformation("Event received with name: {Name}", request.Name);

        await next(context);

        var response = context.GetRequiredResponse<Response>();
        logger.LogInformation("Response sent with message: {message}", response.Message);
    }
}
