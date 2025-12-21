namespace MinimalLambda;

/// <summary>Defines a factory for creating middleware instances.</summary>
/// <remarks>
///     <para>
///         Use an <see cref="ILambdaMiddlewareFactory" /> when middleware construction needs to be
///         customized or deferred. Register the factory in the DI container and use
///         <c>UseMiddleware&lt;TFactory&gt;()</c> to resolve it per invocation.
///     </para>
/// </remarks>
public interface ILambdaMiddlewareFactory
{
    /// <summary>Creates a middleware instance.</summary>
    /// <returns>The <see cref="ILambdaMiddleware" /> instance to execute in the pipeline.</returns>
    ILambdaMiddleware Create();
}
