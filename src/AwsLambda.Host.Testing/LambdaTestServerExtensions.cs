namespace AwsLambda.Host.Testing;

public static class LambdaTestServerExtensions
{
    extension(LambdaTestServer server)
    {
        public Task<InvocationResponse<TResponse>> InvokeAsync<TResponse, TEvent>(
            TEvent invokeEvent,
            CancellationToken cancellationToken = default
        ) =>
            server.InvokeAsync<TResponse, TEvent>(
                invokeEvent,
                cancellationToken: cancellationToken
            );
    }
}
