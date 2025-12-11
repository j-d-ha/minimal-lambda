namespace MinimalLambda;

internal interface IInvocationDataFeatureFactory
{
    IInvocationDataFeature Create(Stream eventStream);
}
