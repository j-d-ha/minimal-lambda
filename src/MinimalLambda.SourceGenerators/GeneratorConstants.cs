namespace MinimalLambda.SourceGenerators;

/// <summary>Constants for attribute names used in source generation.</summary>
internal static class AttributeConstants
{
    internal const string MiddlewareConstructor =
        "MinimalLambda.Builder.MiddlewareConstructorAttribute";
}

internal static class GeneratorConstants
{
    internal const string InterceptsLocationAttributeTemplateFile =
        "Templates/InterceptsLocationAttribute.scriban";

    internal const string LambdaHostMapHandlerExtensionsTemplateFile =
        "Templates/MapHandler.scriban";

    internal const string UseMiddlewareTTemplateFile = "Templates/UseMiddlewareT.scriban";

    internal const string GenericHandlerTemplateFile = "Templates/GenericHandler.scriban";
}
