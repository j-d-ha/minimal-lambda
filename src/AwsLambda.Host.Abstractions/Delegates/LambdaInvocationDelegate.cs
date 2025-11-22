namespace AwsLambda.Host;

/// <summary>A delegate that represents an asynchronous handler for AWS Lambda invocations.</summary>
/// <remarks>
///     <para>
///         The <see cref="LambdaInvocationDelegate" /> is the core handler for processing Lambda
///         invocations. It receives an <see cref="ILambdaHostContext" /> that contains the
///         deserialized event, response storage, service provider, and other invocation-specific
///         information.
///     </para>
///     <para>
///         The handler is responsible for processing the request and setting the response on the
///         context. It runs within the invocation phase after the Function Init phase and before the
///         Function Shutdown phase.
///     </para>
///     <para>
///         Handlers can be registered using the <see cref="ILambdaInvocationBuilder.Handle" /> method
///         and overloads. Multiple middleware can wrap handlers using the
///         <see cref="ILambdaInvocationBuilder.Use" /> method to implement cross-cutting concerns.
///     </para>
/// </remarks>
/// <param name="context">
///     The <see cref="ILambdaHostContext" /> containing invocation information,
///     services, event data, and a location to store the response.
/// </param>
/// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
/// <seealso cref="LambdaInitDelegate" />
/// <seealso cref="LambdaShutdownDelegate" />
/// <seealso cref="ILambdaInvocationBuilder.Handle" />
/// <seealso cref="ILambdaInvocationBuilder.Use" />
public delegate Task LambdaInvocationDelegate(ILambdaHostContext context);
