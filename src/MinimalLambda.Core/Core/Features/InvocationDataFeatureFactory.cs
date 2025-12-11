namespace MinimalLambda.Host.Core;

internal class InvocationDataFeatureFactory : IInvocationDataFeatureFactory
{
    public IInvocationDataFeature Create(Stream eventStream) =>
        new InvocationDataFeature { EventStream = eventStream };
}
