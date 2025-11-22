namespace AwsLambda.Host;

/// <summary>A callback delegate invoked during the AWS Lambda Function Shutdown phase.</summary>
/// <param name="services">A scoped <see cref="IServiceProvider" /> for resolving dependencies.</param>
/// <param name="cancellationToken">
///     Signals the handler to stop. Expires before the Lambda runtime's
///     hard timeout.
/// </param>
/// <returns>A <see cref="Task" /> representing the shutdown operation.</returns>
/// <seealso cref="ILambdaOnShutdownBuilder.OnShutdown(LambdaShutdownDelegate)" />
public delegate Task LambdaShutdownDelegate(
    IServiceProvider services,
    CancellationToken cancellationToken
);
