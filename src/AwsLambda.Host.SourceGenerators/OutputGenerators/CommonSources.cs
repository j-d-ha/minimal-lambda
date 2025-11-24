namespace AwsLambda.Host.SourceGenerators;

internal static class CommonSources
{
    internal static string Generate(string generatedCodeAttribute)
    {
        var model = new { GeneratedCodeAttribute = generatedCodeAttribute };

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.InterceptsLocationAttributeTemplateFile
        );

        return template.Render(model);
    }
}
