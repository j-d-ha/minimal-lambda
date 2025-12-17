using System.Linq;
using MinimalLambda.SourceGenerators.Models;
using MinimalLambda.SourceGenerators.Types;

namespace MinimalLambda.SourceGenerators;

public class UseMiddlewareTSource
{
    internal static string Generate(
        EquatableArray<UseMiddlewareTInfo> useMiddlewareTInfos,
        string generatedCodeAttribute
    )
    {
        var useMiddlewareTCalls = useMiddlewareTInfos.Select(useMiddlewareTInfo =>
        {
            return new { };
        });

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.LambdaHostMapHandlerExtensionsTemplateFile
        );

        return template.Render(
            new
            {
                GeneratedCodeAttribute = generatedCodeAttribute,
                UseMiddlewareCalls = useMiddlewareTCalls,
            }
        );
    }
}
