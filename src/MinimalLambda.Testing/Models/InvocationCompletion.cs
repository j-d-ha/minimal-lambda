namespace MinimalLambda.Testing;

/// <summary>
/// Represents the completion of a Lambda invocation with metadata about the outcome.
/// </summary>
internal class InvocationCompletion
{
    internal required HttpRequestMessage Request { get; init; }
    internal required RequestType RequestType { get; init; }
}
