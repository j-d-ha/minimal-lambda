namespace MinimalLambda.Testing;

/// <summary>
///     Provides convenience extension methods for <see cref="LambdaTestServer" /> to simplify
///     common invocation patterns.
/// </summary>
/// <remarks>
///     <para>
///         These extension methods wrap the core
///         <see cref="LambdaTestServer.InvokeAsync{TEvent, TResponse}" /> method, providing simplified
///         overloads for common scenarios such as invocations without events or without response
///         bodies.
///     </para>
/// </remarks>
public static class LambdaTestServerExtensions
{
    extension(LambdaTestServer server)
    {
        /// <summary>Invokes the Lambda function with the specified event and waits for the response.</summary>
        /// <typeparam name="TEvent">The type of the Lambda event to send to the function.</typeparam>
        /// <typeparam name="TResponse">The expected type of the Lambda function's response.</typeparam>
        /// <param name="invokeEvent">The event object to pass to the Lambda function.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the
        ///     invocation to complete.
        /// </param>
        /// <returns>
        ///     A <see cref="Task{TResult}" /> that completes with an
        ///     <see cref="InvocationResponse{TResponse}" /> containing either the successful response or error
        ///     information from the Lambda function.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the server is not in a valid state to accept
        ///     invocations.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Thrown if the invocation times out or the
        ///     <paramref name="cancellationToken" /> is cancelled.
        /// </exception>
        /// <remarks>
        ///     This is a convenience method that invokes the Lambda function with an event and expects a
        ///     response body. It is equivalent to calling
        ///     <see cref="LambdaTestServer.InvokeAsync{TEvent, TResponse}" /> with <c>noResponse</c> set to
        ///     <see langword="false" />.
        /// </remarks>
        public Task<InvocationResponse<TResponse>> InvokeAsync<TEvent, TResponse>(
            TEvent invokeEvent,
            CancellationToken cancellationToken = default) =>
            server.InvokeAsync<TEvent, TResponse>(
                invokeEvent,
                false,
                cancellationToken: cancellationToken);

        /// <summary>Invokes the Lambda function without an event and waits for the response.</summary>
        /// <typeparam name="TResponse">The expected type of the Lambda function's response.</typeparam>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the
        ///     invocation to complete.
        /// </param>
        /// <returns>
        ///     A <see cref="Task{TResult}" /> that completes with an
        ///     <see cref="InvocationResponse{TResponse}" /> containing either the successful response or error
        ///     information from the Lambda function.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the server is not in a valid state to accept
        ///     invocations.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Thrown if the invocation times out or the
        ///     <paramref name="cancellationToken" /> is cancelled.
        /// </exception>
        /// <remarks>
        ///     <para>
        ///         This is a convenience method for invoking Lambda functions that do not require an event
        ///         payload, such as handlers that perform scheduled tasks or generate data without input.
        ///     </para>
        ///     <para>
        ///         It is equivalent to calling
        ///         <see cref="LambdaTestServer.InvokeAsync{TEvent, TResponse}" /> with a
        ///         <see langword="null" /> event and <c>noResponse</c> set to <see langword="false" />.
        ///     </para>
        /// </remarks>
        public Task<InvocationResponse<TResponse>> InvokeNoEventAsync<TResponse>(
            CancellationToken cancellationToken = default) =>
            server.InvokeAsync<object, TResponse>(
                null,
                false,
                cancellationToken: cancellationToken);

        /// <summary>
        ///     Invokes the Lambda function with the specified event but does not expect or deserialize a
        ///     response body.
        /// </summary>
        /// <typeparam name="TEvent">The type of the Lambda event to send to the function.</typeparam>
        /// <param name="invokeEvent">The event object to pass to the Lambda function.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the
        ///     invocation to complete.
        /// </param>
        /// <returns>
        ///     A <see cref="Task{TResult}" /> that completes with an <see cref="InvocationResponse" />
        ///     containing either success status or error information from the Lambda function.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the server is not in a valid state to accept
        ///     invocations.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Thrown if the invocation times out or the
        ///     <paramref name="cancellationToken" /> is cancelled.
        /// </exception>
        /// <remarks>
        ///     <para>
        ///         This is a convenience method for invoking Lambda functions that do not return a response
        ///         body, such as handlers that write directly to streams or perform side effects without
        ///         returning data.
        ///     </para>
        ///     <para>
        ///         It is equivalent to calling
        ///         <see cref="LambdaTestServer.InvokeAsync{TEvent, TResponse}" /> with <c>noResponse</c> set
        ///         to <see langword="true" />.
        ///     </para>
        /// </remarks>
        public async Task<InvocationResponse> InvokeNoResponseAsync<TEvent>(
            TEvent invokeEvent,
            CancellationToken cancellationToken = default) =>
            await server.InvokeAsync<TEvent, object>(
                invokeEvent,
                true,
                cancellationToken: cancellationToken);
    }
}
