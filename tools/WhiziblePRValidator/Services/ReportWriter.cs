using System.Text;
using System.Text.Json;
using Whizible.PRValidator.Models;

namespace Whizible.PRValidator.Services;

public static class ReportWriter
{
    public static string Write(ValidationResult result, string format)
    {
        return format.Equals("json", StringComparison.OrdinalIgnoreCase)
            ? WriteJson(result)
            : WriteText(result);
    }

    public static string WriteText(ValidationResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("==================================================");
        sb.AppendLine("WHIZIBLE PR VALIDATOR");
        sb.AppendLine("==================================================");

        var groups = result.Issues
            .GroupBy(i => i.RuleId.StartsWith("API", StringComparison.Ordinal) ? "API VALIDATION" : "ASPX VALIDATION")
            .OrderBy(g => g.Key);

        if (!result.Issues.Any())
        {
            sb.AppendLine();
            sb.AppendLine("No validation issues.");
        }

        foreach (var group in groups)
        {
            sb.AppendLine();
            sb.AppendLine(group.Key);
            foreach (var issue in group.OrderBy(i => i.FilePath, StringComparer.OrdinalIgnoreCase).ThenBy(i => i.LineNumber))
            {
                var mark = issue.Severity switch
                {
                    ValidationSeverity.Error => "FAILED",
                    ValidationSeverity.Warning => "WARNING",
                    _ => "INFO"
                };
                sb.AppendLine();
                sb.AppendLine($"{mark} {issue.RuleId}");
                sb.AppendLine("File:");
                sb.AppendLine(Path.GetFileName(issue.FilePath));
                sb.AppendLine();
                sb.AppendLine("Line:");
                sb.AppendLine(issue.LineNumber?.ToString() ?? "-");
                if (!string.IsNullOrWhiteSpace(issue.ClassName))
                {
                    sb.AppendLine();
                    sb.AppendLine("Controller:");
                    sb.AppendLine(issue.ClassName);
                }

                if (!string.IsNullOrWhiteSpace(issue.MethodName))
                {
                    sb.AppendLine();
                    sb.AppendLine("Endpoint:");
                    sb.AppendLine(issue.MethodName);
                }

                sb.AppendLine();
                sb.AppendLine(issue.RuleId.StartsWith("API-00", StringComparison.Ordinal) && issue.RuleId != "API-004"
                    ? "Missing:"
                    : "Detail:");
                sb.AppendLine(issue.Message);
                sb.AppendLine();
                sb.AppendLine("Suggested Fix:");
                sb.AppendLine(issue.SuggestedFix);
                sb.AppendLine();
                sb.AppendLine("==================================================");
            }
        }

        sb.AppendLine();
        sb.AppendLine(result.Passed ? "FINAL RESULT: PASSED" : "FINAL RESULT: FAILED");
        sb.AppendLine("==================================================");
        return sb.ToString();
    }

    private static string WriteJson(ValidationResult result)
    {
        var payload = new
        {
            tool = "Whizible PR Validator",
            version = "1.0",
            passed = result.Passed,
            errorCount = result.ErrorCount,
            issues = result.Issues.Select(i => new
            {
                ruleId = i.RuleId,
                severity = i.Severity.ToString(),
                filePath = i.FilePath,
                lineNumber = i.LineNumber,
                columnNumber = i.ColumnNumber,
                className = i.ClassName,
                methodName = i.MethodName,
                message = i.Message,
                suggestedFix = i.SuggestedFix
            })
        };
        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }
}
