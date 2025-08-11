using System.Threading.Tasks;
using Lambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

lambda.MapHandler(Task () =>
{
    return Task.CompletedTask;
});

await lambda.RunAsync();
