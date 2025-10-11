using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;

namespace Lambda.Host;

public static class LambdaJsonSerializerExtensions
{
    /// <summary>
    /// Adds the specified implementation of <see cref="ILambdaSerializer"/> to the service collection.
    /// </summary>
    /// <param name="serviceCollection">The instance of <see cref="IServiceCollection"/> to add the serializer to.</param>
    /// <param name="lambdaSerializer">An implementation of <see cref="ILambdaSerializer"/> to manage Lambda function serialization.</param>
    /// <returns>The modified <see cref="IServiceCollection"/> instance with the added serializer.</returns>
    public static IServiceCollection AddLambdaJsonSerializer(
        this IServiceCollection serviceCollection,
        ILambdaSerializer lambdaSerializer
    ) => serviceCollection.AddSingleton(lambdaSerializer);

    /// <summary>
    /// Adds a Lambda JSON serializer using the specified <see cref="JsonSerializerContext"/> to the service collection.
    /// </summary>
    /// <param name="serviceCollection">The instance of <see cref="IServiceCollection"/> to add the serializer to.</param>
    /// <typeparam name="T">The type of the <see cref="JsonSerializerContext"/> for the serializer.</typeparam>
    /// <returns>The modified <see cref="IServiceCollection"/> instance with the added serializer.</returns>
    public static IServiceCollection AddLambdaJsonSerializer<T>(
        this IServiceCollection serviceCollection
    )
        where T : JsonSerializerContext =>
        serviceCollection.AddLambdaJsonSerializer(new SourceGeneratorLambdaJsonSerializer<T>());
}
