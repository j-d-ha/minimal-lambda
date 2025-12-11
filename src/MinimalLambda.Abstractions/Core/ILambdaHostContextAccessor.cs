namespace MinimalLambda;

/// <summary>
///     Provides access to the <see cref="ILambdaHostContext" /> for the current Lambda
///     invocation.
/// </summary>
/// <remarks>
///     This accessor is typically used to retrieve invocation context information from dependency
///     injection containers. It allows components to access the current Lambda context without
///     requiring it to be passed directly through method parameters.
/// </remarks>
public interface ILambdaHostContextAccessor
{
    /// <summary>Gets or sets the <see cref="ILambdaHostContext" /> for the current Lambda invocation.</summary>
    /// <value>
    ///     The <see cref="ILambdaHostContext" /> for the current invocation, or <c>null</c> if no
    ///     invocation context is available.
    /// </value>
    ILambdaHostContext? LambdaHostContext { get; set; }
}
