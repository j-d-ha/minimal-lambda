namespace AwsLambda.Host.Core;

/// <summary>Provides access to the deserialized Lambda invocation event.</summary>
/// <remarks>
///     <para>
///         <see cref="IEventFeature" /> allows retrieving the strongly-typed event object for the
///         current Lambda invocation. This feature is typically registered as a feature provider in
///         <see cref="IFeatureCollection" /> to enable lazy deserialization of events.
///     </para>
/// </remarks>
public interface IEventFeature
{
    /// <summary>Gets the deserialized Lambda invocation event.</summary>
    /// <param name="context">The <see cref="ILambdaHostContext" /> for the current invocation.</param>
    /// <returns>The deserialized event object, or <c>null</c> if the event cannot be deserialized.</returns>
    object? GetEvent(ILambdaHostContext context);
}
