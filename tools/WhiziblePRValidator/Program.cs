using Whizible.PRValidator.Models;
using Whizible.PRValidator.Services;

namespace Whizible.PRValidator;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            if (!TryParse(args, out var options, out var error))
            {
                Console.Error.WriteLine(error);
                Console.Error.WriteLine();
                Console.Error.WriteLine("Usage:");
                Console.Error.WriteLine("  WhiziblePRValidator <repository-root> <changed-files.txt>");
                Console.Error.WriteLine("  WhiziblePRValidator --root <repository-root> --changed-files <changed-files.txt> [--format text|json] [--all]");
                return 2;
            }

            var result = ValidationRunner.Run(options);
            Console.WriteLine(ReportWriter.Write(result, options.Format));
            return result.Passed ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Whizible PR Validator failed to run.");
            Console.Error.WriteLine(ex.Message);
            return 2;
        }
    }

    public static bool TryParse(string[] args, out ValidatorOptions options, out string error)
    {
        options = new ValidatorOptions();
        error = string.Empty;
        string? root = null;
        string? changed = null;
        var validateAll = false;
        var format = "text";
        string? config = null;
        bool? acceptClassAttributes = null;

        var positionals = new List<string>();
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg is "--help" or "-h" or "/?")
            {
                error = "Whizible PR Validator 1.0";
                return false;
            }

            if (arg.Equals("--all", StringComparison.OrdinalIgnoreCase))
            {
                validateAll = true;
                continue;
            }

            if (arg.Equals("--method-attributes", StringComparison.OrdinalIgnoreCase))
            {
                acceptClassAttributes = false;
                continue;
            }

            string? value = null;
            if (arg.StartsWith("--root=", StringComparison.OrdinalIgnoreCase))
                value = arg["--root=".Length..];
            else if (arg.Equals("--root", StringComparison.OrdinalIgnoreCase))
                value = Next(args, ref i);
            if (value != null && arg.StartsWith("--root", StringComparison.OrdinalIgnoreCase))
            {
                root = value;
                continue;
            }

            value = null;
            if (arg.StartsWith("--changed-files=", StringComparison.OrdinalIgnoreCase))
                value = arg["--changed-files=".Length..];
            else if (arg.Equals("--changed-files", StringComparison.OrdinalIgnoreCase))
                value = Next(args, ref i);
            if (value != null && arg.StartsWith("--changed-files", StringComparison.OrdinalIgnoreCase))
            {
                changed = value;
                continue;
            }

            value = null;
            if (arg.StartsWith("--format=", StringComparison.OrdinalIgnoreCase))
                value = arg["--format=".Length..];
            else if (arg.Equals("--format", StringComparison.OrdinalIgnoreCase))
                value = Next(args, ref i);
            if (value != null && arg.StartsWith("--format", StringComparison.OrdinalIgnoreCase))
            {
                format = value;
                continue;
            }

            value = null;
            if (arg.StartsWith("--config=", StringComparison.OrdinalIgnoreCase))
                value = arg["--config=".Length..];
            else if (arg.Equals("--config", StringComparison.OrdinalIgnoreCase))
                value = Next(args, ref i);
            if (value != null && arg.StartsWith("--config", StringComparison.OrdinalIgnoreCase))
            {
                config = value;
                continue;
            }

            if (arg.StartsWith('-'))
            {
                error = "Unknown option: " + arg;
                return false;
            }

            positionals.Add(arg);
        }

        if (root == null && positionals.Count > 0)
            root = positionals[0];
        if (changed == null && positionals.Count > 1)
            changed = positionals[1];

        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
        {
            error = "Repository root is missing or is not a directory.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(changed) && !validateAll)
        {
            error = "Pass a changed-file list, or pass --all to validate every ASPX page and API controller under the root.";
            return false;
        }

        if (!format.Equals("text", StringComparison.OrdinalIgnoreCase) &&
            !format.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            error = "Format must be text or json.";
            return false;
        }

        options = new ValidatorOptions
        {
            RepositoryRoot = Path.GetFullPath(root),
            ChangedFilesPath = string.IsNullOrWhiteSpace(changed) ? null : Path.GetFullPath(changed),
            ValidateAll = validateAll,
            Format = format,
            ConfigPath = config
        };
        if (acceptClassAttributes.HasValue)
            options.AcceptControllerLevelAttributes = acceptClassAttributes.Value;
        return true;
    }

    private static string Next(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
            throw new ArgumentException("Missing value for " + args[index]);
        index++;
        return args[index];
    }
}
