namespace AwsLambda.Host;

/// <summary>Provides access to the deserialized Lambda invocation event of a specific type.</summary>
/// <typeparam name="T">The type of the event object.</typeparam>
/// <remarks>
///     <para>
///         <see cref="IEventFeature{T}" /> extends <see cref="IEventFeature" /> to provide type-safe
///         access to the deserialized event. Use this interface when you need to work with
///         strongly-typed events.
///     </para>
/// </remarks>
public interface IEventFeature<out T> : IEventFeature
{
    /// <summary>Gets the deserialized Lambda invocation event of type <typeparamref name="T" />.</summary>
    /// <param name="context">The <see cref="ILambdaHostContext" /> for the current invocation.</param>
    /// <returns>The deserialized event object of type <typeparamref name="T" />.</returns>
    new T GetEvent(ILambdaHostContext context);
}
