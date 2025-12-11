namespace MinimalLambda.Core;

internal interface IInvocationDataFeatureFactory
{
    IInvocationDataFeature Create(Stream eventStream);
}
