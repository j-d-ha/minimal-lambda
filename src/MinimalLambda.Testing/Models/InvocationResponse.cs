namespace MinimalLambda.Testing;

public class InvocationResponse<TResponse>
{
    public ErrorResponse? Error { get; internal set; }
    public TResponse? Response { get; internal set; }
    public bool WasSuccess { get; internal set; }
}
