using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Scriban;

namespace Lambda.Host.SourceGenerators;

internal static class TemplateHelper
{
    /// <summary>
    ///     Loads a Scriban template from embedded resources
    /// </summary>
    /// <param name="relativePath">Name of the template file (without .scriban extension)</param>
    /// <returns>Parsed Scriban template</returns>
    internal static Template LoadTemplate(string relativePath)
    {
        var baseName = Assembly.GetExecutingAssembly().GetName().Name;
        var templateName = relativePath
            .TrimStart('.')
            .Replace(Path.DirectorySeparatorChar, '.')
            .Replace(Path.AltDirectorySeparatorChar, '.');

        var manifestTemplateName = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceNames()
            .FirstOrDefault(x => x!.EndsWith(templateName, StringComparison.InvariantCulture));

        if (string.IsNullOrEmpty(manifestTemplateName))
            throw new InvalidOperationException(
                $"Did not find required resource ending in '{templateName}' in assembly '{baseName}'."
            );

        using var stream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream(manifestTemplateName);
        if (stream == null)
            throw new FileNotFoundException(
                $"Template '{relativePath}' not found in embedded resources."
            );

        using var reader = new StreamReader(stream);
        var templateContent = reader.ReadToEnd();

        var template = Template.Parse(templateContent);
        if (template.HasErrors)
        {
            var errors = string.Join("; ", template.Messages.Select(m => m.ToString()));
            throw new InvalidOperationException(
                $"Template parsing errors in '{templateName}': {errors}"
            );
        }

        return template;
    }
}
