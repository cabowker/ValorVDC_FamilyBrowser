using System.Collections.Generic;

namespace ValorVDC_FamilyBrowser.Services;

public class FamilyBrowserSettings
{
    public HashSet<string> VisibleCategoryNames { get; set; } = new();

    // Project scope: ElementId values — fast, exact, but project-specific.
    public HashSet<long> VisibleSymbolIds { get; set; } = new();

    // User scope: "FamilyName|TypeName" keys — stable across projects as long as
    // the same families are loaded. Used instead of VisibleSymbolIds for user settings.
    public HashSet<string> VisibleSymbolKeys { get; set; } = new();

    // False on first-run / unconfigured — show all so the palette is immediately useful.
    public bool IsConfigured { get; set; } = false;

    public bool IsCategoryVisible(string categoryName)
        => !IsConfigured || VisibleCategoryNames.Contains(categoryName);

    public bool IsSymbolVisible(long symbolId, string familyName, string typeName)
    {
        if (!IsConfigured) return true;
        // Prefer string keys when present (user scope); fall back to ElementId (project scope).
        if (VisibleSymbolKeys.Count > 0)
            return VisibleSymbolKeys.Contains(MakeKey(familyName, typeName));
        return VisibleSymbolIds.Contains(symbolId);
    }

    public static string MakeKey(string familyName, string typeName)
        => $"{familyName}|{typeName}";
}
