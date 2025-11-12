using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

/// <summary>
///     Provides JSON serialization for Lambda requests and responses with configurable
///     <see cref="System.Text.Json.JsonSerializerOptions" /> and
///     <see cref="System.Text.Json.JsonWriterOptions" />.
/// </summary>
/// <remarks>
///     This implementation enables deeper control over JSON serialization behavior, allowing
///     customization of naming policies, converters, writer formatting, and other serialization
///     settings through the <see cref="LambdaHostOptions" /> configuration.
/// </remarks>
internal class DefaultLambdaHostJsonSerializer : ILambdaSerializer
{
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly JsonWriterOptions _writerOptions;

    /// <summary>Initializes a new instance of the <see cref="DefaultLambdaHostJsonSerializer" /> class.</summary>
    /// <param name="lambdaHostSettings">
    ///     The Lambda host options containing serializer and writer
    ///     configuration.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="lambdaHostSettings" /> is null.</exception>
    public DefaultLambdaHostJsonSerializer(IOptions<LambdaHostOptions> lambdaHostSettings)
    {
        ArgumentNullException.ThrowIfNull(lambdaHostSettings);

        var settings = lambdaHostSettings.Value;

        _serializerOptions = settings.JsonSerializerOptions;
        _writerOptions = settings.JsonWriterOptions;
    }

    /// <summary>Deserializes JSON from a stream to an object of type <typeparamref name="T" />.</summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="requestStream">The stream containing JSON data to deserialize.</param>
    /// <returns>The deserialized object, or null if the JSON is null.</returns>
    /// <exception cref="JsonSerializerException">Thrown when deserialization fails.</exception>
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

    /// <summary>Serializes an object of type <typeparamref name="T" /> to JSON and writes it to a stream.</summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="response">The object to serialize.</param>
    /// <param name="responseStream">The stream to write the JSON data to.</param>
    /// <exception cref="JsonSerializerException">Thrown when serialization fails.</exception>
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
