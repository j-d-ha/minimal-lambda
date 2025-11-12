using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

public class DefaultLambdaHostJsonSerializer : ILambdaSerializer
{
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly JsonWriterOptions _writerOptions;

    public DefaultLambdaHostJsonSerializer(IOptions<LambdaHostOptions> lambdaHostSettings)
    {
        ArgumentNullException.ThrowIfNull(lambdaHostSettings);

        var settings = lambdaHostSettings.Value;

        _serializerOptions = settings.LambdaJsonSerializerOptions;
        _writerOptions = settings.LambdaJsonWriterOptions;
    }

    public T? Deserialize<T>(Stream requestStream)
    {
        try
        {
            byte[] utf8Json;
            if (requestStream is MemoryStream memoryStream)
            {
                utf8Json = memoryStream.ToArray();
            }
            else
            {
                using var destination = new MemoryStream();
                requestStream.CopyTo(destination);
                utf8Json = destination.ToArray();
            }

            return JsonSerializer.Deserialize<T>((ReadOnlySpan<byte>)utf8Json, _serializerOptions);
        }
        catch (Exception ex)
        {
            throw new JsonSerializerException(
                $"Error converting the Lambda event JSON payload to type {typeof(T).FullName}: {ex.Message}",
                ex
            );
        }
    }

    public void Serialize<T>(T response, Stream responseStream)
    {
        try
        {
            using var writer = new Utf8JsonWriter(responseStream, _writerOptions);
            JsonSerializer.Serialize(writer, response, _serializerOptions);
        }
        catch (Exception ex)
        {
            throw new JsonSerializerException(
                $"Error converting the response object of type {typeof(T).FullName} from the Lambda function to JSON: {ex.Message}",
                ex
            );
        }
    }
}
