namespace MinimalLambda.Host.Core;

internal sealed class InvocationDataFeature : IInvocationDataFeature
{
    public required Stream EventStream { get; init; }
    public Stream ResponseStream { get; set; } = new MemoryStream();

    /// <summary>
    ///     Dispose the underlying stream. We only dispose of the event stream, not the response
    ///     stream as the Lambda bootstrap will dispose of it.
    /// </summary>
    public void Dispose() => EventStream.Dispose();
}
