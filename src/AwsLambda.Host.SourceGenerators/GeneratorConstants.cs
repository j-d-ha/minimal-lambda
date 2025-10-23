namespace AwsLambda.Host.SourceGenerators;

/// <summary>
///     Constants for common .NET and AWS Lambda types used in source generation.
/// </summary>
internal static class TypeConstants
{
    internal const string ILambdaContext = "global::Amazon.Lambda.Core.ILambdaContext";

    internal const string ILambdaHostContext = "global::AwsLambda.Host.ILambdaHostContext";

    internal const string CancellationToken = "global::System.Threading.CancellationToken";

    internal const string Task = "global::System.Threading.Tasks.Task";

    internal const string Void = "void";

    internal const string Action = "global::System.Action";

    internal const string Func = "global::System.Func";

    internal const string Stream = "global::System.IO.Stream";
}

/// <summary>
///     Constants for attribute names used in source generation.
/// </summary>
internal static class AttributeConstants
{
    internal const string EventAttribute = "AwsLambda.Host.EventAttribute";

    internal const string FromKeyedService =
        "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute";
}

internal static class GeneratorConstants
{
    internal const string MapHandlerMethodName = "MapHandler";

    internal const string LambdaHandlerTemplateFile = "Templates/LambdaHandler.scriban";
}
