using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinimalLambda.Builder;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddSingleton<ILifecycleService, LifecycleService>();
builder.Services.AddSingleton<IService, Service>();

await using var lambda = builder.Build();

lambda.OnInit(
    (ILifecycleService service, ILogger<DiLambda> logger) =>
    {
        logger.LogInformation("Init 1");
        return service.OnStart();
    }
);

lambda.UseMiddleware(
    async (context, next) =>
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<DiLambda>>();

        logger.LogInformation("Middleware 1: Before");
        await next(context);
        logger.LogInformation("Middleware 1: After");
    }
);

lambda.MapHandler(
    ([FromEvent] DiLambdaRequest diLambdaRequest, IService service, ILogger<DiLambda> logger) =>
    {
        logger.LogInformation("Lambda handler");
        return new DiLambdaResponse(service.GetMessage(diLambdaRequest.Name), DateTime.UtcNow);
    }
);

lambda.OnShutdown(
    (ILifecycleService service, ILogger<DiLambda> logger) =>
    {
        logger.LogInformation("Shutdown 1");
        service.OnStop();
    }
);

await lambda.RunAsync();

public class DiLambda;

internal record DiLambdaRequest(string Name);

internal record DiLambdaResponse(string Message, DateTime TimestampUtc);

internal interface IService
{
    string GetMessage(string name);
}

internal class Service : IService
{
    public string GetMessage(string name) => $"Hello {name}!";
}

internal interface ILifecycleService
{
    bool OnStart();
    void OnStop();
}

internal class LifecycleService : ILifecycleService
{
    public bool OnStart() => true;

    public void OnStop() { }
}
