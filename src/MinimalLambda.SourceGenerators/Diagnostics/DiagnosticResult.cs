using System;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;

// ReSharper disable MemberCanBePrivate.Global

namespace MinimalLambda.SourceGenerators;

internal sealed class DiagnosticResult<T>
{
    internal bool IsSuccess { get; }
    internal T? Value { get; }
    internal DiagnosticInfo? Error { get; }

    private DiagnosticResult(bool isSuccess, T? value, DiagnosticInfo? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static DiagnosticResult<T> Success(T value) => new(true, value, null);

    public static implicit operator DiagnosticResult<T>(T value) => Success(value);

    public static DiagnosticResult<T> Failure(DiagnosticInfo error) => new(false, default, error);

    public static DiagnosticResult<T> Failure(
        DiagnosticDescriptor diagnosticDescriptor,
        LocationInfo? locationInfo = null,
        params object?[] messageArgs) =>
        new(false, default, new DiagnosticInfo(diagnosticDescriptor, locationInfo, messageArgs));

    public DiagnosticResult<TNew> Map<TNew>(Func<T, TNew> map) =>
        IsSuccess
            ? DiagnosticResult<TNew>.Success(map(Value!))
            : DiagnosticResult<TNew>.Failure(Error!);

    public DiagnosticResult<TNew> Bind<TNew>(Func<T, DiagnosticResult<TNew>> bind) =>
        IsSuccess ? bind(Value!) : DiagnosticResult<TNew>.Failure(Error!);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<DiagnosticInfo, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error!);

    public void Switch(Action<T> onSuccess, Action<DiagnosticInfo> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value!);
        else
            onFailure(Error!);
    }
}
