using Microsoft.Extensions.Options;

namespace MinimalLambda.Options;

internal class EnvelopeOptionsPostConfiguration : IPostConfigureOptions<EnvelopeOptions>
{
    public void PostConfigure(string? name, EnvelopeOptions options) =>
        options.LambdaDefaultJsonOptions.TypeInfoResolver ??= options.JsonOptions.TypeInfoResolver;
}
