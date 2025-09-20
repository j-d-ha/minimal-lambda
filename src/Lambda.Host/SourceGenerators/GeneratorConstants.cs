namespace Lambda.Host.SourceGenerators;

/// <summary>
///     Constants for common .NET and AWS Lambda types used in source generation.
/// </summary>
internal static class TypeConstants
{
    public const string ILambdaContext = "global::Amazon.Lambda.Core.ILambdaContext";
    public const string Task = "global::System.Threading.Tasks.Task";
    public const string Void = "void";
}

/// <summary>
///     Constants for attribute names used in source generation.
/// </summary>
internal static class AttributeConstants
{
    public const string Request = "Lambda.Host.RequestAttribute";

    public const string FromKeyedService =
        "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute";
}
