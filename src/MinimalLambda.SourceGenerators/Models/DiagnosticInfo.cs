using System;
using LayeredCraft.SourceGeneratorTools.Utilities;
using Microsoft.CodeAnalysis;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly struct DiagnosticInfo(
    DiagnosticDescriptor diagnosticDescriptor,
    LocationInfo? locationInfo = null,
    params object?[] messageArgs
) : IEquatable<DiagnosticInfo>
{
    public DiagnosticDescriptor DiagnosticDescriptor { get; } = diagnosticDescriptor;
    public LocationInfo? LocationInfo { get; } = locationInfo;
    public object?[] MessageArgs { get; } = messageArgs;

    public bool Equals(DiagnosticInfo other) =>
        DiagnosticDescriptor.Id == other.DiagnosticDescriptor.Id
        && LocationInfo == other.LocationInfo;

    public override bool Equals(object? obj) => obj is DiagnosticInfo other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(DiagnosticDescriptor.Id.GetHashCode(), LocationInfo.GetHashCode());
}

internal static class DiagnosticInfoExtensions
{
    extension(DiagnosticInfo diagnosticInfo)
    {
        internal static DiagnosticInfo Create(
            DiagnosticDescriptor diagnosticDescriptor,
            LocationInfo? locationInfo,
            object?[] messageArgs
        ) => new(diagnosticDescriptor, locationInfo, messageArgs);

        internal Diagnostic ToDiagnostic() =>
            Diagnostic.Create(
                diagnosticInfo.DiagnosticDescriptor,
                diagnosticInfo.LocationInfo?.ToLocation(),
                diagnosticInfo.MessageArgs
            );

        internal void ReportDiagnostic(SourceProductionContext context) =>
            context.ReportDiagnostic(diagnosticInfo.ToDiagnostic());
    }
}
