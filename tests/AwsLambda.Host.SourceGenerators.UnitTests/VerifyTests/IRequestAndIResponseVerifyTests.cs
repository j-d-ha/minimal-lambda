namespace AwsLambda.Host.SourceGenerators.UnitTests;

public class IRequestAndIResponseVerifyTests
{
    [Fact]
    public async Task Test_IRequestAndIResponse_ExpressionLambda_BothRequestAndResponse() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Collections.Generic;
            using AwsLambda.Host;
            using AwsLambda.Host.Envelopes.APIGateway;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            builder.Services.AddSingleton<IService, Service>();

            var lambda = builder.Build();

            lambda.MapHandler(
                ([Event] ApiGatewayResponseEnvelope<Request> request, IService service) =>
                    new ApiGatewayResponseEnvelope<Response>
                    {
                        Body = new Response(service.GetMessage(request.Body!.Name)),
                        StatusCode = 200,
                        Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
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
            """
        );
}
