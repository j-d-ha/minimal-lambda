namespace AwsLambda.Host.SourceGenerators.Models;

/// <summary>Represents generic type information.</summary>
/// <param name="Argument">The generic argument name as a global, Ex. `global::My.Namespace.IService`.</param>
/// <param name="Parameter">The generic parameter name, Ex. `T1`.</param>
internal readonly record struct GenericInfo(string Argument, string Parameter);
