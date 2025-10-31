namespace AwsLambda.Host.SourceGenerators;

internal static class CommonSources
{
    internal static string Generate()
    {
        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.InterceptsLocationAttributeTemplateFile
        );

        return template.Render();
    }
}
