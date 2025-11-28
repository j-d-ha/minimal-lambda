using Microsoft.Extensions.Options;

namespace AwsLambda.Host.Options;

internal class EnvelopeOptionsPostConfiguration : IPostConfigureOptions<EnvelopeOptions>
{
    public void PostConfigure(string? name, EnvelopeOptions options) =>
        options.LambdaDefaultJsonOptions.Value.TypeInfoResolver = options
            .JsonOptions
            .TypeInfoResolver;
}
