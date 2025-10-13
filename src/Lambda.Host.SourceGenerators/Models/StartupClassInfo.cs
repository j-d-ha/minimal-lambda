namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct StartupClassInfo(
    string Namespace,
    string ClassName,
    LocationInfo? LocationInfo,
    string? Accessibility
);
