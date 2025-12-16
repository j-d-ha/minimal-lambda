namespace MinimalLambda;

/// <summary>A callback delegate invoked during the AWS Lambda Function Shutdown phase.</summary>
/// <param name="context">
///     The <see cref="ILambdaLifecycleContext" /> providing access to the scoped
///     <see cref="IServiceProvider" /> and <see cref="CancellationToken" /> for the Function Shutdown phase.
/// </param>
/// <returns>A <see cref="Task" /> representing the shutdown operation.</returns>
/// <seealso cref="ILambdaOnShutdownBuilder.OnShutdown(LambdaShutdownDelegate)" />
public delegate Task LambdaShutdownDelegate(ILambdaLifecycleContext context);
