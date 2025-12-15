namespace MinimalLambda.Builder;

/// <summary>Marks a parameter to receive the deserialized Lambda event object.</summary>
/// <remarks>
///     <para>
///         The <see cref="FromEventAttribute" /> is used with the source generator to indicate that
///         a handler method parameter should be injected with the deserialized event from the current
///         Lambda invocation. This attribute is applied to the event parameter in handler methods that
///         are registered using <see cref="ILambdaOnInitBuilder.OnInit" />,
///         <see cref="ILambdaInvocationBuilder.Handle" />, or related extension methods.
///     </para>
///     <para>
///         This attribute is only valid on method parameters and is processed at compile-time by the
///         source generator to generate the necessary wiring code.
///     </para>
///     <para>
///         <b>Important:</b> Only one parameter per handler method can be decorated with this
///         attribute. Applying <see cref="FromEventAttribute" /> to multiple parameters in the same
///         handler will result in a compile-time error from the source generator.
///     </para>
/// </remarks>
/// <example>
///     <code>
///     lambda.MapHandler(([FromEvent] MyEvent myEvent) => $"Hello, {myEvent.Name}!");
///     </code>
/// </example>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromEventAttribute : Attribute;
