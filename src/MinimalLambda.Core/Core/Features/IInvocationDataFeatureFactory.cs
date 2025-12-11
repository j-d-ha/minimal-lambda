namespace MinimalLambda.Host.Core;

internal interface IInvocationDataFeatureFactory
{
    IInvocationDataFeature Create(Stream eventStream);
}
