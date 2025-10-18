using System.Threading;
using System.Threading.Tasks;
using Lambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddSingleton<IService, Service>();

var lambda = builder.Build();

lambda.MapHandler(
    async ([Event] string request, IService service, CancellationToken cancellationToken) =>
        await service.SayHello(cancellationToken)
);

await lambda.RunAsync();

internal interface IService
{
    Task<string> SayHello(CancellationToken cancellationToken);
}

internal class Service : IService
{
    public Task<string> SayHello(CancellationToken cancellationToken) =>
        Task.FromResult("hello world");
}
