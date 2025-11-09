namespace AwsLambda.Host;

/// <summary>Marks a parameter to receive the deserialized Lambda event object.</summary>
/// <remarks>
///     <para>
///         The <see cref="EventAttribute" /> is used with the source generator to indicate that a
///         handler method parameter should be injected with the deserialized event from the current
///         Lambda invocation. This attribute is applied to the event parameter in handler methods that
///         are registered using <see cref="ILambdaApplication.OnInit" />,
///         <see cref="ILambdaApplication.MapHandler" />, or related extension methods.
///     </para>
///     <para>
///         The event object is retrieved from the <see cref="ILambdaHostContext.Event" /> property
///         and cast to the parameter type. If the event is not of the expected type, a casting
///         exception will be raised.
///     </para>
///     <para>
///         This attribute is only valid on method parameters and is processed at compile-time by the
///         source generator to generate the necessary wiring code.
///     </para>
///     <para>
///         <b>Important:</b> Only one parameter per handler method can be decorated with this
///         attribute. Applying <see cref="EventAttribute" /> to multiple parameters in the same
///         handler will result in a compile-time error from the source generator.
///     </para>
/// </remarks>
/// <example>
///     <code>
///     public async Task HandleEvent([Event] MyEventType myEvent, ILogger logger, CancellationToken ct)
///     {
///         logger.LogInformation("Received event: {EventId}", myEvent.Id);
///         await ProcessEventAsync(@event, ct);
///     }
///     </code>
/// </example>
/// <seealso cref="ILambdaHostContext.Event" />
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class EventAttribute : Attribute;
