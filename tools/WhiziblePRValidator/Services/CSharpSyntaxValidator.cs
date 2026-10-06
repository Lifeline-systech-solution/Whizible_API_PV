using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Whizible.PRValidator.Models;
using Whizible.PRValidator.Rules;

namespace Whizible.PRValidator.Services;

public static class CSharpSyntaxValidator
{
    private static readonly HashSet<string> BraceDiagnostics = new(StringComparer.Ordinal)
    {
        "CS1513", "CS1514", "CS1515"
    };

    private static readonly HashSet<string> MethodDiagnostics = new(StringComparer.Ordinal)
    {
        "CS1519", "CS1026", "CS1003", "CS1031", "CS0501", "CS8112"
    };

    private static readonly HashSet<string> ReferenceDiagnostics = new(StringComparer.Ordinal)
    {
        "CS0246", "CS0234", "CS0103", "CS0117", "CS1061", "CS0426", "CS1069"
    };

    public static IEnumerable<ValidationError> AnalyzeSyntax(string filePath, string source, bool forApi)
    {
        var tree = CSharpSyntaxTree.ParseText(source, path: filePath);
        foreach (var diagnostic in tree.GetDiagnostics())
        {
            if (diagnostic.Severity != DiagnosticSeverity.Error)
                continue;
            yield return Map(filePath, diagnostic, forApi, tree);
        }
    }

    public static IEnumerable<ValidationError> AnalyzeCompilation(string filePath, string source, bool forApi)
    {
        var tree = CSharpSyntaxTree.ParseText(source, path: filePath);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            "WhiziblePrValidatorSnippet",
            new[] { tree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        foreach (var diagnostic in compilation.GetDiagnostics())
        {
            if (diagnostic.Severity != DiagnosticSeverity.Error)
                continue;
            if (diagnostic.Location.SourceTree != tree)
                continue;
            yield return Map(filePath, diagnostic, forApi, tree);
        }
    }

    public static IEnumerable<ValidationError> AnalyzeVisualBasic(string filePath, string source)
    {
        var tree = VisualBasicSyntaxTree.ParseText(source, path: filePath);
        foreach (var diagnostic in tree.GetDiagnostics())
        {
            if (diagnostic.Severity != DiagnosticSeverity.Error)
                continue;

            var span = diagnostic.Location.GetLineSpan();
            var line = span.StartLinePosition.Line + 1;
            var column = span.StartLinePosition.Character + 1;
            var rule = diagnostic.Id is "BC30026" or "BC30680" or "BC30087"
                ? RuleIds.AspxMissingBrace
                : RuleIds.AspxCSharpSyntax;
            yield return ValidationError.Create(
                rule,
                ValidationSeverity.Error,
                filePath,
                line,
                column,
                null,
                null,
                diagnostic.GetMessage(),
                "Correct the Visual Basic syntax reported by the compiler.");
        }
    }

    private static ValidationError Map(string filePath, Diagnostic diagnostic, bool forApi, SyntaxTree tree)
    {
        var span = diagnostic.Location.GetLineSpan();
        var line = span.StartLinePosition.Line + 1;
        var column = span.StartLinePosition.Character + 1;
        var rule = forApi ? RuleIds.ApiCSharpSyntax : ClassifyAspx(diagnostic, tree);
        var message = diagnostic.Id == "CS1002"
            ? "Missing ';'."
            : diagnostic.GetMessage();
        var fix = rule switch
        {
            RuleIds.AspxMissingSemicolon => "Add the missing semicolon at the end of the statement.",
            RuleIds.AspxMissingBrace => "Add the missing brace so every { has a matching }.",
            RuleIds.AspxInvalidMethod => "Correct the method signature: parameters, parentheses, and body.",
            RuleIds.AspxInvalidReference => "Add the missing namespace, type, or project reference, or correct the name.",
            _ => "Correct the C# syntax reported by the compiler."
        };

        return ValidationError.Create(rule, ValidationSeverity.Error, filePath, line, column, null, null, message, fix);
    }

    private static string ClassifyAspx(Diagnostic diagnostic, SyntaxTree tree)
    {
        if (diagnostic.Id == "CS1002")
            return RuleIds.AspxMissingSemicolon;
        if (BraceDiagnostics.Contains(diagnostic.Id))
            return RuleIds.AspxMissingBrace;
        if (ReferenceDiagnostics.Contains(diagnostic.Id))
            return RuleIds.AspxInvalidReference;
        if (MethodDiagnostics.Contains(diagnostic.Id))
            return RuleIds.AspxInvalidMethod;

        var root = tree.GetRoot();
        var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
        var method = token.Parent?.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (method != null && diagnostic.Location.SourceSpan.Start <= method.ParameterList.Span.End)
            return RuleIds.AspxInvalidMethod;

        return RuleIds.AspxCSharpSyntax;
    }
}
