namespace Whizible.PRValidator.Services;

/// <summary>
/// Discovers ASP.NET server-control types from System.Web when that assembly
/// is installed. A hard-coded control list is not used as the source of truth.
/// </summary>
public sealed class AspNetControlCatalog
{
    public bool LoadedFromAssemblies { get; set; }
    public HashSet<string> ServerControlNames { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> NamingContainerNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    public static readonly string[] FrameworkProbePaths =
    {
        @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Web.dll",
        @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Web.dll",
        @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Web.Extensions.dll",
        @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Web.Extensions.dll"
    };

    public static readonly string[] FallbackNamingContainers =
    {
        "Repeater", "DataList", "GridView", "FormView", "DetailsView", "ListView", "DataGrid",
        "CheckBoxList", "RadioButtonList", "ContentPlaceHolder", "Wizard", "MultiView",
        "LoginView", "Menu", "RadioButtonList", "ChangePassword", "CreateUserWizard"
    };

    public static AspNetControlCatalog Load(bool probeSystemWeb)
    {
        var catalog = new AspNetControlCatalog();
        foreach (var name in FallbackNamingContainers)
            catalog.NamingContainerNames.Add(name);

        if (!probeSystemWeb)
            return catalog;

        foreach (var path in FrameworkProbePaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(path))
                continue;
            try
            {
                var assembly = System.Reflection.Assembly.LoadFrom(path);
                catalog.Absorb(assembly);
            }
            catch (Exception ex) when (ex is IOException or BadImageFormatException or System.Reflection.ReflectionTypeLoadException or FileLoadException)
            {
                // The next probe path, or a per-page @Register directive, can still identify controls.
            }
        }

        return catalog;
    }

    public void Absorb(System.Reflection.Assembly assembly)
    {
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (System.Reflection.ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
        }

        var controlType = types.FirstOrDefault(t => t.FullName == "System.Web.UI.Control");
        var namingType = types.FirstOrDefault(t => t.FullName == "System.Web.UI.INamingContainer");
        if (controlType == null && namingType == null)
        {
            // A custom or extension assembly may not define Control itself.
            controlType = typeof(object);
        }

        foreach (var type in types)
        {
            if (type.IsAbstract || !type.IsPublic || type.Name.Length == 0)
                continue;

            var inWebUi = type.Namespace != null &&
                          type.Namespace.StartsWith("System.Web.UI", StringComparison.Ordinal);
            var isControl = controlType != null &&
                            controlType != typeof(object) &&
                            controlType.IsAssignableFrom(type);
            if (!isControl && !inWebUi)
                continue;

            if (isControl || inWebUi)
            {
                ServerControlNames.Add(type.Name);
                LoadedFromAssemblies = true;
            }

            if (namingType != null && namingType.IsAssignableFrom(type))
                NamingContainerNames.Add(type.Name);
        }
    }
}
