namespace AwsLambda.Host.Core;

internal class LambdaHostContextAccessor : ILambdaHostContextAccessor
{
    private static readonly AsyncLocal<LambdaHostContextHolder> ContextHolder = new();

    public ILambdaHostContext? LambdaHostContext
    {
        get => ContextHolder.Value?.Context;
        set
        {
            ContextHolder.Value?.Context = null;
            if (value is not null)
                ContextHolder.Value = new LambdaHostContextHolder { Context = value };
        }
    }

    private sealed class LambdaHostContextHolder
    {
        internal ILambdaHostContext? Context;
    }
}
