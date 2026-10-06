namespace Whizible.PRValidator.Services;

public static class ChangedFileProvider
{
    private static readonly HashSet<string> SkipDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "obj", ".vs", ".git", "node_modules", "packages"
    };

    public static IReadOnlyList<string> Resolve(string repositoryRoot, string? changedFilesPath, bool validateAll)
    {
        if (!string.IsNullOrWhiteSpace(changedFilesPath))
        {
            if (!File.Exists(changedFilesPath))
                throw new FileNotFoundException("Changed-file list was not found.", changedFilesPath);

            var list = new List<string>();
            foreach (var raw in File.ReadAllLines(changedFilesPath))
            {
                var line = raw.Trim().Trim('"');
                if (line.Length == 0 || line.StartsWith('#'))
                    continue;

                var full = Path.IsPathRooted(line)
                    ? Path.GetFullPath(line)
                    : Path.GetFullPath(Path.Combine(repositoryRoot, line));
                list.Add(full);
            }

            return list;
        }

        if (!validateAll)
            return Array.Empty<string>();

        var found = new List<string>();
        if (!Directory.Exists(repositoryRoot))
            return found;

        foreach (var file in Directory.EnumerateFiles(repositoryRoot, "*.*", SearchOption.AllDirectories))
        {
            if (IsSkippedPath(file) || !IsCandidate(file))
                continue;
            found.Add(Path.GetFullPath(file));
        }

        return found;
    }

    public static bool IsCandidate(string path)
    {
        var ext = Path.GetExtension(path);
        if (ext.Equals(".aspx", StringComparison.OrdinalIgnoreCase) ||
            ext.Equals(".ascx", StringComparison.OrdinalIgnoreCase))
            return true;

        if (ext.Equals(".cs", StringComparison.OrdinalIgnoreCase) ||
            ext.Equals(".vb", StringComparison.OrdinalIgnoreCase))
        {
            var name = Path.GetFileName(path);
            if (name.EndsWith(".aspx.cs", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith(".aspx.vb", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith(".ascx.cs", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith(".ascx.vb", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("Controller.cs", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static bool IsSkippedPath(string path)
    {
        var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        foreach (var part in parts)
        {
            if (SkipDirectoryNames.Contains(part))
                return true;
            if (part.Equals("WhiziblePRValidator", StringComparison.OrdinalIgnoreCase) ||
                part.Equals("WhiziblePRValidator.Tests", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        var name = Path.GetFileName(path);
        return name.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase);
    }
}
