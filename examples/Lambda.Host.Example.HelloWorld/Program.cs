using System;
using System.Threading;
using System.Threading.Tasks;
using Lambda.Host;
using Lambda.Host.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddSingleton<IService, Service>();

var lambda = builder.Build();

lambda.UseMiddleware(DefaultMiddleware.ClearLambdaOutputFormatting);

lambda.UseMiddleware(
    async (context, next) =>
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("[Middleware 1] Before");
        await next(context);
        logger.LogInformation("[Middleware 1] After");
    }
);

lambda.UseMiddleware(
    async (context, next) =>
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("[Middleware 2] Before");
        await next(context);
        logger.LogInformation("[Middleware 2] After");
    }
);

lambda.MapHandler(
    async (
        [Event] Request request,
        IService service,
        ILogger<Program> logger,
        CancellationToken cancellationToken
    ) =>
    {
        logger.LogInformation("Handler called");
        var message = await service.SayHello(request.Name, cancellationToken);
        return new Response(message, DateTime.Now);
    }
);

await lambda.RunAsync();

internal record Request(string Name);

internal record Response(string Message, DateTime Now);

internal interface IService
{
    Task<string> SayHello(string name, CancellationToken cancellationToken);
}

internal class Service : IService
{
    public Task<string> SayHello(string name, CancellationToken cancellationToken) =>
        Task.FromResult($"hello {name}");
}
