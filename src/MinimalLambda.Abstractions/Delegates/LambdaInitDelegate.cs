namespace MinimalLambda;

/// <summary>
///     A callback delegate invoked during the AWS Lambda Function Init phase, before any handler
///     invocation.
/// </summary>
/// <param name="context">
///     The <see cref="ILambdaLifecycleContext" /> providing access to the scoped
///     <see cref="IServiceProvider" /> and <see cref="CancellationToken" /> for the Function Init phase.
/// </param>
/// <returns>
///     A <see cref="Task{TResult}" /> that completes with <c>true</c> to allow the Function Init
///     phase to continue, or <c>false</c> to abort the Function Init phase. Exceptions thrown are
///     collected and rethrown after all delegates complete.
/// </returns>
/// <seealso cref="ILambdaOnInitBuilder.OnInit(LambdaInitDelegate)" />
public delegate Task<bool> LambdaInitDelegate(ILambdaLifecycleContext context);
