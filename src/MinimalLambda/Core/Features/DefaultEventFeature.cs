using Amazon.Lambda.Core;

namespace MinimalLambda;

/// <summary>
///     Default implementation of <see cref="IEventFeature{T}" /> that is used by source generated
///     code.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class DefaultEventFeature<T> : IEventFeature<T>
{
    private readonly ILambdaSerializer _lambdaSerializer;
    private T _data = default!;
    private bool _isDeserialized;

    public DefaultEventFeature(ILambdaSerializer lambdaSerializer)
    {
        ArgumentNullException.ThrowIfNull(lambdaSerializer);

        _lambdaSerializer = lambdaSerializer;
    }

    public T GetEvent(ILambdaHostContext context)
    {
        if (!_isDeserialized)
        {
            var invocationDataFeature = context.Features.GetRequired<IInvocationDataFeature>();
            _data = _lambdaSerializer.Deserialize<T>(invocationDataFeature.EventStream);
            _isDeserialized = true;
        }

        return _data;
    }

    object? IEventFeature.GetEvent(ILambdaHostContext context) => GetEvent(context);
}
