using Amazon.Lambda.Core;

namespace MinimalLambda.Core;

/// <summary>
///     Default implementation of <see cref="DefaultResponseFeature{T}" /> that is used by source
///     generated code.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class DefaultResponseFeature<T> : IResponseFeature<T>
{
    private readonly ILambdaSerializer _lambdaSerializer;
    private bool _isSet;
    private T _response = default!;

    public DefaultResponseFeature(ILambdaSerializer lambdaSerializer)
    {
        ArgumentNullException.ThrowIfNull(lambdaSerializer);

        _lambdaSerializer = lambdaSerializer;
    }

    public void SetResponse(T response)
    {
        _response = response;
        _isSet = true;
    }

    object? IResponseFeature.GetResponse() => GetResponse();

    public T? GetResponse() => _isSet ? _response : default;

    public void SerializeToStream(ILambdaHostContext context)
    {
        if (!_isSet)
            return;

        var invocationDataFeature = context.Features.GetRequired<IInvocationDataFeature>();
        invocationDataFeature.ResponseStream.SetLength(0L);
        _lambdaSerializer.Serialize(_response, invocationDataFeature.ResponseStream);
        invocationDataFeature.ResponseStream.Position = 0L;
    }
}
