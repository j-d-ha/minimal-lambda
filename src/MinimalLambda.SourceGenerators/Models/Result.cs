using System;
using Microsoft.CodeAnalysis;

namespace MinimalLambda.SourceGenerators.Models;

internal class Result<T, TError>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public TError? Error { get; }

    protected Result(bool isSuccess, T? value, TError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T, TError> Success(T value) => new(true, value, default);

    public static Result<T, TError> Failure(TError error) => new(false, default, error);

    // Map transforms the success value
    public Result<TNew, TError> Map<TNew>(Func<T, TNew> map) =>
        IsSuccess
            ? Result<TNew, TError>.Success(map(Value!))
            : Result<TNew, TError>.Failure(Error!);

    // Bind chains operations that return Results
    public Result<TNew, TError> Bind<TNew>(Func<T, Result<TNew, TError>> bind) =>
        IsSuccess ? bind(Value!) : Result<TNew, TError>.Failure(Error!);

    // Match for pattern matching
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<TError, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}

internal class DiagnosticResult<T> : Result<T, DiagnosticInfo?>
{
    private DiagnosticResult(bool isSuccess, T? value, DiagnosticInfo? error)
        : base(isSuccess, value, error) { }

    public static new DiagnosticResult<T> Success(T value) => new(true, value, null);

    public static DiagnosticResult<T> Failure(DiagnosticInfo error) => new(false, default, error);

    public static DiagnosticResult<T> Failure(
        DiagnosticDescriptor diagnosticDescriptor,
        LocationInfo? locationInfo = null,
        params object?[] messageArgs
    ) => new(false, default, new DiagnosticInfo(diagnosticDescriptor, locationInfo, messageArgs));
}
