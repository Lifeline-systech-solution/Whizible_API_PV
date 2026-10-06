using Whizible.PRValidator.Models;
using Whizible.PRValidator.Rules;
using Whizible.PRValidator.Services;

namespace Whizible.PRValidator.Validators;

public sealed class AspxValidator
{
    private static readonly HashSet<string> PageAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Language", "AutoEventWireup", "CodeBehind", "CodeFile", "Inherits", "Src", "ClassName",
        "MasterPageFile", "Title", "Culture", "UICulture", "EnableViewState", "EnableEventValidation",
        "ValidateRequest", "ResponseEncoding", "ContentType", "Debug", "Trace", "ErrorPage",
        "MaintainScrollPositionOnPostback", "StylesheetTheme", "Theme", "EnableTheming",
        "ClientIDMode", "ViewStateMode", "Async", "AsyncTimeout", "CodePage", "LCID",
        "EnableSessionState", "Buffer", "TargetSchema", "SmartNavigation", "CompilerOptions",
        "WarningLevel", "Explicit", "Strict", "LinePragmas", "EnableViewStateMac",
        "ViewStateEncryptionMode", "MaintainScrollPositionOnPostback", "MetaDescription",
        "MetaKeywords", "ClientTarget", "ResponseEncoding", "AspCompat", "Transaction"
    };

    private readonly AspNetControlCatalog _catalog;
    private readonly AspxCodeBehindValidator _codeBehind = new();

    public AspxValidator(AspNetControlCatalog catalog)
    {
        _catalog = catalog;
    }

    public void ValidateMarkup(string filePath, ValidationResult result, bool includeLinkedCodeBehind)
    {
        if (!File.Exists(filePath))
        {
            result.Add(ValidationError.Create(
                RuleIds.AspxMarkup, ValidationSeverity.Error, filePath, null, null, null, null,
                "File was listed as changed but does not exist.",
                "Restore the file or remove it from the changed-file list."));
            return;
        }

        var text = File.ReadAllText(filePath);
        var parser = new AspxMarkupParser(text, filePath, _catalog, enforceUnknownControls: _catalog.LoadedFromAssemblies);
        var (document, issues) = parser.Parse();
        result.AddRange(issues);
        ValidateDirectives(filePath, document, result, includeLinkedCodeBehind);
    }

    public void ValidateCodeBehindFile(string filePath, ValidationResult result, string? markupPath)
    {
        if (!File.Exists(filePath))
        {
            result.Add(ValidationError.Create(
                RuleIds.AspxCSharpSyntax, ValidationSeverity.Error, filePath, null, null, null, null,
                "Code-behind file was listed as changed but does not exist.",
                "Restore the file or remove it from the changed-file list."));
            return;
        }

        var source = File.ReadAllText(filePath);
        result.AddRange(_codeBehind.ValidateSyntax(filePath, source));
        if (markupPath != null && File.Exists(markupPath))
            CheckControlReferences(markupPath, filePath, source, result);
    }

    private void ValidateDirectives(string filePath, AspxDocument document, ValidationResult result, bool includeLinkedCodeBehind)
    {
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        foreach (var directive in document.Directives)
        {
            if (directive.Name.Equals("Page", StringComparison.OrdinalIgnoreCase) ||
                directive.Name.Equals("Control", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var attribute in directive.Attributes.Keys)
                {
                    if (!PageAttributes.Contains(attribute))
                    {
                        result.Add(ValidationError.Create(
                            RuleIds.AspxDirective, ValidationSeverity.Warning, filePath, directive.Line, null, null, null,
                            $"Directive attribute '{attribute}' is not a known @{directive.Name} attribute.",
                            "Remove the attribute or correct its name."));
                    }
                }

                ValidateCodeFile(filePath, directory, directive, result, includeLinkedCodeBehind);
            }
            else if (directive.Name.Equals("Register", StringComparison.OrdinalIgnoreCase))
            {
                var hasPrefix = directive.Attributes.ContainsKey("TagPrefix");
                var hasTag = directive.Attributes.ContainsKey("TagName") && directive.Attributes.ContainsKey("Src");
                var hasAssembly = directive.Attributes.ContainsKey("Namespace") && directive.Attributes.ContainsKey("Assembly");
                if (!hasPrefix || (!hasTag && !hasAssembly))
                {
                    result.Add(ValidationError.Create(
                        RuleIds.AspxDirective, ValidationSeverity.Error, filePath, directive.Line, null, null, null,
                        "<%@ Register %> is missing TagPrefix plus either TagName/Src or Namespace/Assembly.",
                        "Use <%@ Register TagPrefix=\"uc\" TagName=\"Name\" Src=\"File.ascx\" %> or TagPrefix, Namespace, and Assembly."));
                }
            }
        }
    }

    private void ValidateCodeFile(
        string markupPath,
        string directory,
        AspxDirective directive,
        ValidationResult result,
        bool includeLinkedCodeBehind)
    {
        string? relative = null;
        if (directive.Attributes.TryGetValue("CodeBehind", out var codeBehind) && !string.IsNullOrWhiteSpace(codeBehind))
            relative = codeBehind;
        else if (directive.Attributes.TryGetValue("CodeFile", out var codeFile) && !string.IsNullOrWhiteSpace(codeFile))
            relative = codeFile;

        if (directive.Attributes.TryGetValue("Inherits", out var inherits) &&
            !string.IsNullOrWhiteSpace(inherits) &&
            !IsTypeName(inherits))
        {
            result.Add(ValidationError.Create(
                RuleIds.AspxDirective, ValidationSeverity.Error, markupPath, directive.Line, null, inherits, null,
                $"Inherits value '{inherits}' is not a valid type name.",
                "Set Inherits to the code-behind namespace and class, for example Whizible.EmployeePage."));
        }

        if (relative == null)
            return;

        var full = Path.GetFullPath(Path.Combine(directory, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!File.Exists(full))
        {
            result.Add(ValidationError.Create(
                RuleIds.AspxDirective, ValidationSeverity.Error, markupPath, directive.Line, null, null, null,
                $"Code-behind file '{relative}' was not found.",
                "Point CodeBehind or CodeFile at the existing .aspx.cs or .aspx.vb file."));
            return;
        }

        if (!includeLinkedCodeBehind)
            return;

        var source = File.ReadAllText(full);
        result.AddRange(_codeBehind.ValidateSyntax(full, source));
        if (directive.Attributes.TryGetValue("Inherits", out var inherited) && !string.IsNullOrWhiteSpace(inherited))
        {
            var className = _codeBehind.FindClassName(full, source);
            var expected = inherited.Split('.').Last();
            if (!string.IsNullOrWhiteSpace(className) &&
                !className.Equals(expected, StringComparison.Ordinal))
            {
                result.Add(ValidationError.Create(
                    RuleIds.AspxDirective, ValidationSeverity.Error, markupPath, directive.Line, null, className, null,
                    $"Inherits '{inherited}' does not match code-behind class '{className}'.",
                    "Make the Inherits type match the class declared in the code-behind."));
            }
        }

        CheckControlReferences(markupPath, full, source, result);
    }

    private void CheckControlReferences(string markupPath, string codeBehindPath, string source, ValidationResult result)
    {
        var markup = File.ReadAllText(markupPath);
        var parser = new AspxMarkupParser(markup, markupPath, _catalog, enforceUnknownControls: false);
        var (document, _) = parser.Parse();
        var ids = document.ServerElements
            .Where(e => !string.IsNullOrWhiteSpace(e.Id))
            .Select(e => e.Id!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        result.AddRange(_codeBehind.FindMissingServerControls(codeBehindPath, source, ids));
    }

    private static bool IsTypeName(string value)
    {
        var parts = value.Split('.');
        if (parts.Length == 0)
            return false;
        foreach (var part in parts)
        {
            if (part.Length == 0 || !(char.IsLetter(part[0]) || part[0] == '_'))
                return false;
            for (var i = 1; i < part.Length; i++)
            {
                if (!(char.IsLetterOrDigit(part[i]) || part[i] == '_'))
                    return false;
            }
        }

        return true;
    }
}
