using System.Text.Json;
using Whizible.PRValidator.Models;

namespace Whizible.PRValidator.Services;

public static class ValidatorConfigLoader
{
    public static void Apply(ValidatorOptions options)
    {
        var path = options.ConfigPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            var beside = Path.Combine(AppContext.BaseDirectory, "Configuration", "validator.json");
            if (File.Exists(beside))
                path = beside;
        }

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return;

        using var stream = File.OpenRead(path);
        using var doc = JsonDocument.Parse(stream);
        var root = doc.RootElement;
        if (root.TryGetProperty("acceptControllerLevelAttributes", out var accept) &&
            (accept.ValueKind == JsonValueKind.True || accept.ValueKind == JsonValueKind.False))
        {
            options.AcceptControllerLevelAttributes = accept.GetBoolean();
        }

        if (root.TryGetProperty("probeSystemWeb", out var probe) &&
            (probe.ValueKind == JsonValueKind.True || probe.ValueKind == JsonValueKind.False))
        {
            options.ProbeSystemWeb = probe.GetBoolean();
        }

        AddNames(root, "excludeControllers", options.ExcludeControllers);
        AddNames(root, "excludeMethods", options.ExcludeMethods);
    }

    private static void AddNames(JsonElement root, string property, HashSet<string> target)
    {
        if (!root.TryGetProperty(property, out var array) || array.ValueKind != JsonValueKind.Array)
            return;

        foreach (var item in array.EnumerateArray())
        {
            var name = item.GetString();
            if (!string.IsNullOrWhiteSpace(name))
                target.Add(name.Trim());
        }
    }
}
