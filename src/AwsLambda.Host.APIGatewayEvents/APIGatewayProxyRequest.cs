using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace AwsLambda.Host.APIGatewayEvents;

public class APIGatewayProxyRequest<T>
    : APIGatewayProxyRequest,
        ILambdaRequest<APIGatewayProxyRequest<T>>
{
    public new required T? Body { get; set; }

    public static APIGatewayProxyRequest<T> Deserialize(
        Stream stream,
        ILambdaSerializer serializer,
        JsonSerializerOptions? jsonSerializerOptions
    )
    {
        var baseResponse = serializer.Deserialize<APIGatewayProxyRequest>(stream);

        var body = JsonSerializer.Deserialize<T>(baseResponse.Body, jsonSerializerOptions);

        return new APIGatewayProxyRequest<T>
        {
            Body = body,
            Resource = baseResponse.Resource,
            Path = baseResponse.Path,
            HttpMethod = baseResponse.HttpMethod,
            Headers = baseResponse.Headers,
            MultiValueHeaders = baseResponse.MultiValueHeaders,
            QueryStringParameters = baseResponse.QueryStringParameters,
            MultiValueQueryStringParameters = baseResponse.MultiValueQueryStringParameters,
            PathParameters = baseResponse.PathParameters,
            StageVariables = baseResponse.StageVariables,
            RequestContext = baseResponse.RequestContext,
            IsBase64Encoded = baseResponse.IsBase64Encoded,
        };
    }
}
