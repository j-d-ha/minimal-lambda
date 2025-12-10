using AwsLambda.Host.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Create the application builder
var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.ClearLambdaOutputFormatting = true;
});

// Build the Lambda application
var lambda = builder.Build();

// throw new Exception("Init failed");

// lambda.OnInit(() =>
// {
//     // throw new Exception("Init failed");
//     // return false;
// });

// Map your handler - the event is automatically injected
lambda.MapHandler(
    ([Event] string name) =>
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name), "Name is required.");

        return $"Hello {name}!";
    }
);

// Run the Lambda
await lambda.RunAsync();

public partial class Program;
