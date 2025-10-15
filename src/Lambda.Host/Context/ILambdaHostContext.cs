using Amazon.Lambda.Core;

namespace Lambda.Host;

public interface ILambdaHostContext : ILambdaContext
{
    object? Request { get; set; }
    object? Response { get; set; }

    IServiceProvider ServiceProvider { get; }

    CancellationToken CancellationToken { get; }
}
