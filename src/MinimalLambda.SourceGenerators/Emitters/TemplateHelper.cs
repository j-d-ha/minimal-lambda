using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using Scriban;

namespace MinimalLambda.SourceGenerators.Emitters;

/// <summary>
///     Helper class for loading, caching, and rendering Scriban templates from embedded
///     resources.
/// </summary>
internal static class TemplateHelper
{
    internal static readonly Lazy<string> GeneratedCodeAttribute = new(() =>
    {
        var assembly = Assembly.GetExecutingAssembly();
        var generatorName = assembly.GetName().Name;
        var generatorVersion = assembly.GetName().Version;

        return $"""[global::System.CodeDom.Compiler.GeneratedCode("{generatorName}", "{generatorVersion}")]""";
    });

    private static readonly ConcurrentDictionary<string, Template> Cache = new();

    /// <summary>
    ///     Renders a Scriban template with the provided model. Templates are cached after first load
    ///     for performance.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to render</typeparam>
    /// <param name="resourceName">
    ///     Relative path to the template resource (e.g.,
    ///     "Templates.Common.InterceptsLocationAttribute.scriban")
    /// </param>
    /// <param name="model">The model object to render with the template</param>
    /// <returns>Rendered template as a string</returns>
    /// <exception cref="InvalidOperationException">Thrown if template is not found or has parsing errors</exception>
    internal static string Render<TModel>(string resourceName, TModel model)
    {
        var template = Cache.GetOrAdd(resourceName, LoadTemplate);
        return template.Render(model);
    }

    /// <summary>Loads a Scriban template from embedded resources.</summary>
    /// <param name="relativePath">
    ///     Relative path to the template resource (e.g.,
    ///     "Templates.Common.InterceptsLocationAttribute.scriban")
    /// </param>
    /// <returns>Parsed Scriban template ready for rendering</returns>
    /// <exception cref="InvalidOperationException">Thrown if template is not found or has parsing errors</exception>
    private static Template LoadTemplate(string relativePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var baseName = assembly.GetName().Name;

        // Convert relative path to resource name format
        var templateName = relativePath
            .TrimStart('.')
            .Replace(Path.DirectorySeparatorChar, '.')
            .Replace(Path.AltDirectorySeparatorChar, '.');

        // Find the manifest resource name that ends with our template name
        var manifestTemplateName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(x => x.EndsWith(templateName, StringComparison.InvariantCulture));

        if (string.IsNullOrEmpty(manifestTemplateName))
        {
            var availableResources = string.Join(", ", assembly.GetManifestResourceNames());
            throw new InvalidOperationException(
                $"Did not find required resource ending in '{templateName}' in assembly '{baseName}'. "
                    + $"Available resources: {availableResources}"
            );
        }

        // Load the template content
        using var stream = assembly.GetManifestResourceStream(manifestTemplateName);
        if (stream == null)
            throw new FileNotFoundException(
                $"Template '{relativePath}' not found in embedded resources. "
                    + $"Manifest resource name: '{manifestTemplateName}'"
            );

        using var reader = new StreamReader(stream);
        var templateContent = reader.ReadToEnd();

        // Parse and validate the template
        var template = Template.Parse(templateContent, relativePath);
        if (!template.HasErrors)
            return template;
        var errors = string.Join(
            "\n",
            template.Messages.Select(m =>
                $"{relativePath}({m.Span.Start.Line},{m.Span.Start.Column}): {m.Message}"
            )
        );
        throw new InvalidOperationException(
            $"Failed to parse template '{relativePath}':\n{errors}"
        );
    }
}
