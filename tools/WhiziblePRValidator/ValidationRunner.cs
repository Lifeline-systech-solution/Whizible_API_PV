using Whizible.PRValidator.Models;
using Whizible.PRValidator.Rules;
using Whizible.PRValidator.Services;
using Whizible.PRValidator.Validators;

namespace Whizible.PRValidator;

public static class ValidationRunner
{
    public static ValidationResult Run(ValidatorOptions options)
    {
        ValidatorConfigLoader.Apply(options);
        var files = ChangedFileProvider.Resolve(options.RepositoryRoot, options.ChangedFilesPath, options.ValidateAll);
        var result = new ValidationResult();
        var catalog = AspNetControlCatalog.Load(options.ProbeSystemWeb);
        if (!catalog.LoadedFromAssemblies)
        {
            result.Add(ValidationError.Create(
                RuleIds.AspxServerControl,
                ValidationSeverity.Info,
                options.RepositoryRoot,
                null,
                null,
                null,
                null,
                "System.Web was not loaded, so unknown <asp:> control types were not rejected. Registered controls and markup syntax are still checked.",
                "Install .NET Framework 4.x so System.Web.dll can be reflected, or keep probeSystemWeb enabled on a Windows build agent."));
        }

        var aspx = new AspxValidator(catalog);
        var api = new ApiValidator(options.AcceptControllerLevelAttributes);
        var linkedCodeBehind = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files.Where(IsMarkup))
        {
            aspx.ValidateMarkup(file, result, includeLinkedCodeBehind: true);
            var linked = LinkedCodeBehind(file);
            if (linked != null)
                linkedCodeBehind.Add(Path.GetFullPath(linked));
        }

        foreach (var file in files.Where(IsCodeBehind))
        {
            if (linkedCodeBehind.Contains(Path.GetFullPath(file)))
                continue;
            aspx.ValidateCodeBehindFile(file, result, SiblingMarkup(file));
        }

        foreach (var file in files.Where(IsController))
            api.ValidateFile(file, result, options);

        return result;
    }

    private static bool IsMarkup(string path)
    {
        var ext = Path.GetExtension(path);
        return ext.Equals(".aspx", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".ascx", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCodeBehind(string path)
    {
        var name = Path.GetFileName(path);
        return name.EndsWith(".aspx.cs", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".aspx.vb", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".ascx.cs", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".ascx.vb", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsController(string path)
    {
        return Path.GetFileName(path).EndsWith("Controller.cs", StringComparison.OrdinalIgnoreCase);
    }

    private static string? SiblingMarkup(string codeBehindPath)
    {
        if (codeBehindPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
            codeBehindPath.EndsWith(".vb", StringComparison.OrdinalIgnoreCase))
        {
            var markup = codeBehindPath[..^3];
            return File.Exists(markup) ? markup : null;
        }

        return null;
    }

    private static string? LinkedCodeBehind(string markupPath)
    {
        var directory = Path.GetDirectoryName(markupPath);
        if (string.IsNullOrEmpty(directory) || !File.Exists(markupPath))
            return null;

        foreach (var line in File.ReadLines(markupPath))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("<%@", StringComparison.Ordinal))
                continue;
            var match = System.Text.RegularExpressions.Regex.Match(
                trimmed,
                @"Code(?:Behind|File)\s*=\s*""([^""]+)""",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            return Path.GetFullPath(Path.Combine(directory, match.Groups[1].Value));
        }

        return null;
    }
}
