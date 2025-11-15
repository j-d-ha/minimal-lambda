using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;

namespace AwsLambda.Host;

/// <summary>Provides extension methods for configuring Lambda host services.</summary>
public static class SerializerServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>Adds a Lambda JSON serializer configured with a source-generated serialization context.</summary>
        /// <remarks>
        ///     <para>
        ///         Registers a <see cref="SourceGeneratorLambdaJsonSerializer{TContext}" /> as the
        ///         <see cref="ILambdaSerializer" /> in the service collection. This method is designed to work
        ///         with source-generated JSON serialization contexts (derived from
        ///         <see cref="System.Text.Json.Serialization.JsonSerializerContext" />), which provide
        ///         compile-time serialization metadata and improved performance.
        ///     </para>
        /// </remarks>
        /// <typeparam name="TContext">
        ///     A <see cref="JsonSerializerContext" /> type that contains the
        ///     source-generated serialization metadata for your Lambda event and response types.
        /// </typeparam>
        /// <returns>The <see cref="IServiceCollection" /> for method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <see cref="IServiceCollection" /> is
        ///     <c>null</c>.
        /// </exception>
        /// <seealso cref="SourceGeneratorLambdaJsonSerializer{TContext}" />
        /// <seealso cref="JsonSerializerContext" />
        public IServiceCollection AddLambdaSerializerWithContext<TContext>()
            where TContext : JsonSerializerContext
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddSingleton<ILambdaSerializer>(
                _ => new SourceGeneratorLambdaJsonSerializer<TContext>()
            );

            return serviceCollection;
        }
    }
}
