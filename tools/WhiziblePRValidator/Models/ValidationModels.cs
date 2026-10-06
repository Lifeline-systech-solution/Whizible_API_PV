namespace Whizible.PRValidator.Models;

public enum ValidationSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2
}

public sealed class ValidationError
{
    public string RuleId { get; init; } = string.Empty;
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Error;
    public string FilePath { get; init; } = string.Empty;
    public int? LineNumber { get; init; }
    public int? ColumnNumber { get; init; }
    public string? ClassName { get; init; }
    public string? MethodName { get; init; }
    public string Message { get; init; } = string.Empty;
    public string SuggestedFix { get; init; } = string.Empty;

    public static ValidationError Create(
        string ruleId,
        ValidationSeverity severity,
        string filePath,
        int? line,
        int? column,
        string? className,
        string? methodName,
        string message,
        string suggestedFix)
    {
        return new ValidationError
        {
            RuleId = ruleId,
            Severity = severity,
            FilePath = filePath,
            LineNumber = line,
            ColumnNumber = column,
            ClassName = className,
            MethodName = methodName,
            Message = message,
            SuggestedFix = suggestedFix
        };
    }
}

public sealed class ValidationResult
{
    public List<ValidationError> Issues { get; } = new();

    public bool Passed => Issues.All(i => i.Severity != ValidationSeverity.Error);

    public int ErrorCount => Issues.Count(i => i.Severity == ValidationSeverity.Error);

    public void Add(ValidationError issue) => Issues.Add(issue);

    public void AddRange(IEnumerable<ValidationError> issues) => Issues.AddRange(issues);
}

public sealed class ValidatorOptions
{
    public string RepositoryRoot { get; init; } = string.Empty;
    public string? ChangedFilesPath { get; init; }
    public bool ValidateAll { get; init; }
    public string Format { get; init; } = "text";
    public string? ConfigPath { get; init; }
    public bool AcceptControllerLevelAttributes { get; set; } = true;
    public HashSet<string> ExcludeControllers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ExcludeMethods { get; } = new(StringComparer.OrdinalIgnoreCase);
    public bool ProbeSystemWeb { get; set; } = true;
}
