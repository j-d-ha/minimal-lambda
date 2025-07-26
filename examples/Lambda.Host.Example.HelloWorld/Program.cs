using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;

await LambdaBootstrapBuilder
    .Create(() => "hello world", new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();
