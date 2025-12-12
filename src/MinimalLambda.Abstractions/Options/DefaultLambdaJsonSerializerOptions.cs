using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.Serialization.SystemTextJson.Converters;

namespace MinimalLambda.Options;

/// <summary>Provides the default JSON serializer options used by AWS Lambda.</summary>
public static class DefaultLambdaJsonSerializerOptions
{
    /// <summary>
    ///     Creates a <see cref="JsonSerializerOptions" /> instance that matches the defaults used by
    ///     <see cref="DefaultLambdaJsonSerializer" />.
    /// </summary>
    /// <remarks>
    ///     <para>Configures null-value ignoring, case-insensitive property names, and the AWS naming policy.</para>
    ///     <para>Adds the AWS-provided converters for dates, memory streams, constant classes, and byte arrays.</para>
    /// </remarks>
    /// <returns>Configured JSON serializer options suitable for AWS Lambda payloads.</returns>
    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = new AwsNamingPolicy(),
        };
        options.Converters.Add(new DateTimeConverter());
        options.Converters.Add(new MemoryStreamConverter());
        options.Converters.Add(new ConstantClassConverter());
        options.Converters.Add(new ByteArrayConverter());

        return options;
    }
}
