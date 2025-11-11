using System.Text.Json.Serialization;

namespace AwsLambda.Host;

/// <summary>Provides a mechanism for types to register custom JSON serialization converters.</summary>
public interface IJsonSerializable
{
    /// <summary>Registers type information and converters for JSON serialization.</summary>
    /// <remarks>
    ///     <para>
    ///         Implement this method to register custom <see cref="JsonConverter" /> instances required
    ///         for serializing and deserializing the type. This allows types to define their own JSON
    ///         serialization behavior independently.
    ///     </para>
    ///     <para>
    ///         The Lambda host automatically discovers and invokes this method for any type implementing
    ///         <see cref="IJsonSerializable" />, eliminating the need for manual converter registration.
    ///     </para>
    /// </remarks>
    /// <param name="converters">
    ///     The list of <see cref="JsonConverter" /> instances to populate with custom
    ///     converters.
    /// </param>
    static abstract void RegisterTypeInfo(IList<JsonConverter> converters);
}
