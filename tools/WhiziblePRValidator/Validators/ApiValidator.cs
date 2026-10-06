using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Whizible.PRValidator.Models;
using Whizible.PRValidator.Rules;
using Whizible.PRValidator.Services;

namespace Whizible.PRValidator.Validators;

public sealed class ApiValidator
{
    private static readonly HashSet<string> HttpVerbs = new(StringComparer.Ordinal)
    {
        "HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch", "HttpHead", "HttpOptions", "AcceptVerbs"
    };

    private readonly bool _acceptControllerLevel;

    public ApiValidator(bool acceptControllerLevelAttributes)
    {
        _acceptControllerLevel = acceptControllerLevelAttributes;
    }

    public void ValidateFile(string filePath, ValidationResult result, ValidatorOptions options)
    {
        if (!File.Exists(filePath))
        {
            result.Add(ValidationError.Create(
                RuleIds.ApiCSharpSyntax, ValidationSeverity.Error, filePath, null, null, null, null,
                "Controller file was listed as changed but does not exist.",
                "Restore the file or remove it from the changed-file list."));
            return;
        }

        var source = File.ReadAllText(filePath);
        result.AddRange(CSharpSyntaxValidator.AnalyzeSyntax(filePath, source, forApi: true));

        var tree = CSharpSyntaxTree.ParseText(source, path: filePath);
        var root = tree.GetCompilationUnitRoot();
        foreach (var type in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            if (!IsController(type))
                continue;
            if (options.ExcludeControllers.Contains(type.Identifier.ValueText))
                continue;

            var classAttributes = _acceptControllerLevel
                ? MergePartialAttributes(filePath, type)
                : Array.Empty<AttributeSyntax>();

            foreach (var method in type.Members.OfType<MethodDeclarationSyntax>())
            {
                if (!IsEndpoint(method))
                    continue;
                if (options.ExcludeMethods.Contains(method.Identifier.ValueText))
                    continue;

                var attributes = method.AttributeLists.SelectMany(l => l.Attributes).Concat(classAttributes).ToList();
                var (verb, route) = DescribeVerb(attributes);
                var line = method.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                var className = type.Identifier.ValueText;
                var methodName = method.Identifier.ValueText;
                if (!string.IsNullOrWhiteSpace(route) && !route.Equals(methodName, StringComparison.Ordinal))
                    methodName = methodName + " (" + route + ")";

                if (!HasAuthorize(attributes))
                {
                    result.Add(Missing(filePath, line, className, methodName, RuleIds.ApiAuthorize,
                        "[Authorize]",
                        "Add [Authorize] to the endpoint."));
                }

                if (!HasServiceFilter(attributes, "AuthorizeAuditAttribute"))
                {
                    result.Add(Missing(filePath, line, className, methodName, RuleIds.ApiAuthorizeAudit,
                        "[ServiceFilter(typeof(AuthorizeAuditAttribute))]",
                        "Add [ServiceFilter(typeof(AuthorizeAuditAttribute))] to the endpoint."));
                }

                if (!HasServiceFilter(attributes, "ValidateHeadersAttribute"))
                {
                    result.Add(Missing(filePath, line, className, methodName, RuleIds.ApiValidateHeaders,
                        "[ServiceFilter(typeof(ValidateHeadersAttribute))]",
                        "Add the required ValidateHeadersAttribute to the endpoint."));
                }

                _ = verb;
            }
        }
    }

    private static ValidationError Missing(
        string filePath,
        int line,
        string className,
        string methodName,
        string rule,
        string attribute,
        string fix)
    {
        return ValidationError.Create(
            rule,
            ValidationSeverity.Error,
            filePath,
            line,
            null,
            className,
            methodName,
            $"Missing required attribute: {attribute}",
            fix);
    }

    private IReadOnlyList<AttributeSyntax> MergePartialAttributes(string filePath, TypeDeclarationSyntax type)
    {
        var attributes = type.AttributeLists.SelectMany(l => l.Attributes).ToList();
        if (!type.Modifiers.Any(m => m.Text == "partial"))
            return attributes;

        var directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            return attributes;

        var className = type.Identifier.ValueText;
        foreach (var other in Directory.EnumerateFiles(directory, "*.cs"))
        {
            if (string.Equals(Path.GetFullPath(other), Path.GetFullPath(filePath), StringComparison.OrdinalIgnoreCase))
                continue;
            var otherTree = CSharpSyntaxTree.ParseText(File.ReadAllText(other), path: other);
            foreach (var otherType in otherTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (!otherType.Identifier.ValueText.Equals(className, StringComparison.Ordinal))
                    continue;
                if (!otherType.Modifiers.Any(m => m.Text == "partial"))
                    continue;
                attributes.AddRange(otherType.AttributeLists.SelectMany(l => l.Attributes));
            }
        }

        return attributes;
    }

    private static bool IsController(TypeDeclarationSyntax type)
    {
        if (type.Identifier.ValueText.EndsWith("Controller", StringComparison.Ordinal))
            return true;
        if (type.AttributeLists.SelectMany(l => l.Attributes).Any(a => SimpleName(a) is "ApiController" or "ApiControllerAttribute"))
            return true;
        var baseName = type.BaseList?.ToString() ?? string.Empty;
        return baseName.Contains("ControllerBase", StringComparison.Ordinal) ||
               baseName.Contains("Controller", StringComparison.Ordinal);
    }

    private static bool IsEndpoint(MethodDeclarationSyntax method)
    {
        if (method.Modifiers.Any(m => m.Text is "private" or "protected" or "static"))
            return false;
        if (!method.Modifiers.Any(m => m.Text == "public"))
            return false;
        if (method.AttributeLists.SelectMany(l => l.Attributes).Any(a => SimpleName(a) is "NonAction" or "NonActionAttribute"))
            return false;

        return method.AttributeLists.SelectMany(l => l.Attributes).Any(IsHttpAttribute);
    }

    private static bool IsHttpAttribute(AttributeSyntax attribute)
    {
        return HttpVerbs.Contains(SimpleName(attribute));
    }

    private static (string Verb, string? Route) DescribeVerb(IEnumerable<AttributeSyntax> attributes)
    {
        foreach (var attribute in attributes)
        {
            var name = SimpleName(attribute);
            if (!HttpVerbs.Contains(name))
                continue;
            string? route = null;
            var first = attribute.ArgumentList?.Arguments.FirstOrDefault();
            if (first?.Expression is LiteralExpressionSyntax literal)
                route = literal.Token.ValueText;
            return (name, route);
        }

        return (string.Empty, null);
    }

    private static bool HasAuthorize(IEnumerable<AttributeSyntax> attributes)
    {
        return attributes.Any(a => SimpleName(a) is "Authorize" or "AuthorizeAttribute");
    }

    private static bool HasServiceFilter(IEnumerable<AttributeSyntax> attributes, string typeName)
    {
        foreach (var attribute in attributes)
        {
            var name = SimpleName(attribute);
            if (name is not ("ServiceFilter" or "ServiceFilterAttribute"))
                continue;
            var argument = attribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
            if (argument is not TypeOfExpressionSyntax typeOf)
                continue;
            var type = typeOf.Type.ToString();
            if (type.Equals(typeName, StringComparison.Ordinal) ||
                type.EndsWith("." + typeName, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static string SimpleName(AttributeSyntax attribute)
    {
        var text = attribute.Name.ToString();
        var dot = text.LastIndexOf('.');
        if (dot >= 0)
            text = text[(dot + 1)..];
        return text.EndsWith("Attribute", StringComparison.Ordinal) ? text[..^9] : text;
    }
}
