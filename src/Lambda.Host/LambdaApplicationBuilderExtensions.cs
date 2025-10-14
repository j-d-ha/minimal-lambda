using Microsoft.Extensions.DependencyInjection;

namespace Lambda.Host;

public static class LambdaApplicationBuilderExtensions
{
    public static LambdaApplicationBuilder UseLambdaHost<T>(this LambdaApplicationBuilder builder)
        where T : LambdaHostedService
    {
        builder.Services.AddSingleton<LambdaHostedService, T>();

        return builder;
    }
}
