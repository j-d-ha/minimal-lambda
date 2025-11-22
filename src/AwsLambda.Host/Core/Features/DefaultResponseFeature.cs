using Amazon.Lambda.Core;

namespace AwsLambda.Host.Core.Features;

/// <summary>
///     Default implementation of <see cref="DefaultResponseFeature{T}" /> that is used by source
///     generated code.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class DefaultResponseFeature<T> : IResponseFeature<T>
{
    private readonly ILambdaSerializer _lambdaSerializer;
    private bool _isSet;
    private T _response;

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

        context.RawInvocationData.Response.SetLength(0L);
        _lambdaSerializer.Serialize<T>(_response, context.RawInvocationData.Response);
        context.RawInvocationData.Response.Position = 0L;
    }
}
