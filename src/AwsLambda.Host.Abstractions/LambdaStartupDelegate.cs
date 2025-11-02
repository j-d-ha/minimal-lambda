namespace AwsLambda.Host;

/// <summary>
///     A callback delegate invoked during the AWS Lambda Function Init phase, before any handler invocation.
/// </summary>
/// <param name="services">
///     A scoped <see cref="IServiceProvider"/> for resolving dependencies.
/// </param>
/// <param name="cancellationToken">
///     Signals the handler to stop during the Function Init phase.
/// </param>
/// <returns>
///     A <see cref="Task{TResult}"/> that completes with <c>true</c> to allow the Function Init phase to continue,
///     or <c>false</c> to abort the Function Init phase. Exceptions thrown are collected and rethrown after all delegates complete.
/// </returns>
/// <seealso cref="ILambdaApplication.OnInit(LambdaInitDelegate)"/>
public delegate Task<bool> LambdaInitDelegate(
    IServiceProvider services,
    CancellationToken cancellationToken
);
