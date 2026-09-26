namespace ValorVDC_FamilyBrowser.Services;

public enum SettingsScope
{
    Project, // Stored in the Revit document via ExtensibleStorage — shared with the team
    User     // Stored in %APPDATA%\ValorVDC\FamilyBrowser\settings.json — applies to all projects
}
