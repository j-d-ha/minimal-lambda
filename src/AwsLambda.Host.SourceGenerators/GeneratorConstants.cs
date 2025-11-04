namespace AwsLambda.Host.SourceGenerators;

/// <summary>Constants for common .NET and AWS Lambda types used in source generation.</summary>
internal static class TypeConstants
{
    internal const string ILambdaContext = "global::Amazon.Lambda.Core.ILambdaContext";

    internal const string ILambdaHostContext = "global::AwsLambda.Host.ILambdaHostContext";

    internal const string CancellationToken = "global::System.Threading.CancellationToken";

    internal const string Task = "global::System.Threading.Tasks.Task";

    internal const string ValueTask = "global::System.Threading.Tasks.ValueTask";

    internal const string Void = "void";

    internal const string Action = "global::System.Action";

    internal const string Func = "global::System.Func";

    internal const string Stream = "global::System.IO.Stream";

    internal const string IServiceProvider = "global::System.IServiceProvider";
}

/// <summary>Constants for attribute names used in source generation.</summary>
internal static class AttributeConstants
{
    internal const string EventAttribute = "AwsLambda.Host.EventAttribute";

    internal const string FromKeyedService =
        "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute";
}

internal static class GeneratorConstants
{
    internal const string MapHandlerMethodName = "MapHandler";

    internal const string OnShutdownMethodName = "OnShutdown";

    internal const string UseOpenTelemetryTracingMethodName = "UseOpenTelemetryTracing";

    internal const string InterceptsLocationAttributeTemplateFile =
        "Templates/InterceptsLocationAttribute.scriban";

    internal const string LambdaHostMapHandlerExtensionsTemplateFile =
        "Templates/LambdaHostMapHandlerExtensions.scriban";

    internal const string LambdaHostOnShutdownExtensionsTemplateFile =
        "Templates/LambdaHostOnShutdownExtensions.scriban";

    internal const string LambdaHostUseOpenTelemetryTracingExtensionsTemplateFile =
        "Templates/LambdaHostUseOpenTelemetryTracingExtensions.scriban";
}
