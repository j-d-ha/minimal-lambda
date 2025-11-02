namespace AwsLambda.Host;

/// <summary>
///     A callback delegate invoked when the AWS Lambda runtime initiates shutdown.
/// </summary>
/// <param name="services">
///     A scoped <see cref="IServiceProvider"/> for resolving dependencies.
/// </param>
/// <param name="cancellationToken">
///     Signals the handler to stop. Expires before the Lambda runtime's hard timeout.
/// </param>
/// <returns>A <see cref="Task"/> representing the shutdown operation.</returns>
/// <seealso cref="ILambdaApplication.OnShutdown(LambdaShutdownDelegate)"/>
public delegate Task LambdaShutdownDelegate(
    IServiceProvider services,
    CancellationToken cancellationToken
);
