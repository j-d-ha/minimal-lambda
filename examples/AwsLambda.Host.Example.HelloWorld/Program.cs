using AwsLambda.Host;
using AwsLambda.Host.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddSingleton<IService, Service>();

var lambda = builder.Build();

lambda.UseClearLambdaOutputFormatting();

lambda.MapHandler(
    ([Event] Request request, IService service) => new Response(service.GetMessage(request.Name))
);

lambda.UseMiddleware(
    async (context, next) =>
    {
        context.Features.Get<ILambdaHostContext>();
        await next(context);
    }
);

await lambda.RunAsync();

internal record Response(string Message);

internal record Request(string Name);

internal interface IService
{
    string GetMessage(string name);
}

internal class Service : IService
{
    public string GetMessage(string name) => $"hello {name}";
}
