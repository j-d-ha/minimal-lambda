namespace MinimalLambda.SourceGenerators;

internal static class CommonSources
{
    internal static string Generate() =>
        TemplateHelper.Render(
            GeneratorConstants.InterceptsLocationAttributeTemplateFile,
            new { MinimalLambdaEmitter.GeneratedCodeAttribute }
        );
}
