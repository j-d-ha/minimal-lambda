using System.Threading.Channels;

namespace AwsLambda.Host.Testing;

public class LambdaTestingHttpHandler(
    Channel<HttpRequestMessage> requestChanel,
    Channel<HttpResponseMessage> responseChanel
) : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        // pass the request out to the client
        await requestChanel.Writer.WriteAsync(request, cancellationToken);

        // block here until the client sends a response. There may not be a response.
        return await responseChanel.Reader.ReadAsync(cancellationToken);
    }
}
