using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Cs = Microsoft.CodeAnalysis.CSharp.Syntax;
using Vb = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Whizible.PRValidator.Models;
using Whizible.PRValidator.Rules;
using Whizible.PRValidator.Services;

namespace Whizible.PRValidator.Validators;

public sealed class AspxCodeBehindValidator
{
    private static readonly HashSet<string> ControlMembers = new(StringComparer.Ordinal)
    {
        "Text", "Visible", "Enabled", "DataSource", "DataBind", "CssClass", "Checked",
        "SelectedValue", "SelectedIndex", "Items", "Attributes", "Style", "ToolTip",
        "DataSourceID", "CommandArgument", "CommandName"
    };

    private static readonly HashSet<string> PageIntrinsics = new(StringComparer.OrdinalIgnoreCase)
    {
        "Response", "Request", "Server", "Session", "Application", "ViewState", "Page",
        "User", "Context", "Trace", "Cache", "Master", "PreviousPage", "ClientScript",
        "ScriptManager", "Form", "Header", "Title", "MyBase", "Me"
    };

    public IEnumerable<ValidationError> ValidateSyntax(string filePath, string source)
    {
        if (filePath.EndsWith(".vb", StringComparison.OrdinalIgnoreCase))
            return CSharpSyntaxValidator.AnalyzeVisualBasic(filePath, source);
        return CSharpSyntaxValidator.AnalyzeSyntax(filePath, source, forApi: false);
    }

    public IEnumerable<ValidationError> FindMissingServerControls(
        string codeBehindPath,
        string source,
        IReadOnlySet<string> serverIds)
    {
        if (codeBehindPath.EndsWith(".vb", StringComparison.OrdinalIgnoreCase))
            return FindMissingVisualBasic(codeBehindPath, source, serverIds);
        return FindMissingCSharp(codeBehindPath, source, serverIds);
    }

    public string? FindClassName(string filePath, string source)
    {
        if (filePath.EndsWith(".vb", StringComparison.OrdinalIgnoreCase))
        {
            var tree = VisualBasicSyntaxTree.ParseText(source);
            return tree.GetRoot().DescendantNodes()
                .OfType<Vb.ClassBlockSyntax>()
                .Select(c => c.ClassStatement.Identifier.ValueText)
                .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));
        }

        var csharp = CSharpSyntaxTree.ParseText(source);
        return csharp.GetRoot().DescendantNodes()
            .OfType<Cs.ClassDeclarationSyntax>()
            .Select(c => c.Identifier.ValueText)
            .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));
    }

    private static IEnumerable<ValidationError> FindMissingCSharp(
        string filePath,
        string source,
        IReadOnlySet<string> serverIds)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();
        var declared = new HashSet<string>(StringComparer.Ordinal);
        foreach (var local in root.DescendantNodes().OfType<Cs.VariableDeclaratorSyntax>())
        {
            if (local.Parent?.Parent is Cs.LocalDeclarationStatementSyntax or Cs.ForStatementSyntax)
                declared.Add(local.Identifier.ValueText);
        }

        foreach (var parameter in root.DescendantNodes().OfType<Cs.ParameterSyntax>())
            declared.Add(parameter.Identifier.ValueText);
        foreach (var each in root.DescendantNodes().OfType<Cs.ForEachStatementSyntax>())
            declared.Add(each.Identifier.ValueText);

        foreach (var access in root.DescendantNodes().OfType<Cs.MemberAccessExpressionSyntax>())
        {
            if (access.Expression is not Cs.IdentifierNameSyntax identifier)
                continue;
            var member = access.Name.Identifier.ValueText;
            if (!ControlMembers.Contains(member))
                continue;
            var name = identifier.Identifier.ValueText;
            if (declared.Contains(name) || PageIntrinsics.Contains(name) || serverIds.Contains(name))
                continue;

            var span = identifier.Identifier.GetLocation().GetLineSpan();
            yield return ValidationError.Create(
                RuleIds.AspxRunat,
                ValidationSeverity.Error,
                filePath,
                span.StartLinePosition.Line + 1,
                span.StartLinePosition.Character + 1,
                null,
                null,
                $"Code-behind uses '{name}' as a server control, but the ASPX markup has no runat=\"server\" control with that ID.",
                $"Add <asp:... ID=\"{name}\" runat=\"server\" /> or remove the code-behind reference.");
        }
    }

    private static IEnumerable<ValidationError> FindMissingVisualBasic(
        string filePath,
        string source,
        IReadOnlySet<string> serverIds)
    {
        var tree = VisualBasicSyntaxTree.ParseText(source);
        var root = tree.GetRoot();
        var declared = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var local in root.DescendantNodes().OfType<Vb.ModifiedIdentifierSyntax>())
            declared.Add(local.Identifier.ValueText);
        foreach (var parameter in root.DescendantNodes().OfType<Vb.ParameterSyntax>())
            declared.Add(parameter.Identifier.Identifier.ValueText);

        foreach (var access in root.DescendantNodes().OfType<Vb.MemberAccessExpressionSyntax>())
        {
            if (access.Expression is not Vb.IdentifierNameSyntax identifier)
                continue;
            var member = access.Name.Identifier.ValueText;
            if (!ControlMembers.Contains(member))
                continue;
            var name = identifier.Identifier.ValueText;
            if (declared.Contains(name) || PageIntrinsics.Contains(name) || serverIds.Contains(name))
                continue;

            var span = identifier.GetLocation().GetLineSpan();
            yield return ValidationError.Create(
                RuleIds.AspxRunat,
                ValidationSeverity.Error,
                filePath,
                span.StartLinePosition.Line + 1,
                span.StartLinePosition.Character + 1,
                null,
                null,
                $"Code-behind uses '{name}' as a server control, but the ASPX markup has no runat=\"server\" control with that ID.",
                $"Add a server control with ID=\"{name}\" or remove the code-behind reference.");
        }
    }
}
