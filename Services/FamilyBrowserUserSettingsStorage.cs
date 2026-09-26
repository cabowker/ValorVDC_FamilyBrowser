using System;
using System.IO;
using Newtonsoft.Json;

namespace ValorVDC_FamilyBrowser.Services;

public static class FamilyBrowserUserSettingsStorage
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ValorVDC", "FamilyBrowser", "settings.json");

    private class UserSettingsFile
    {
        public SettingsScope       Scope        { get; set; } = SettingsScope.Project;
        public FamilyBrowserSettings UserSettings { get; set; } = new();
    }

    public static SettingsScope LoadScope()
    {
        try
        {
            if (!File.Exists(FilePath)) return SettingsScope.Project;
            var file = Read();
            return file?.Scope ?? SettingsScope.Project;
        }
        catch { return SettingsScope.Project; }
    }

    public static FamilyBrowserSettings LoadUserSettings()
    {
        try
        {
            if (!File.Exists(FilePath)) return new FamilyBrowserSettings();
            return Read()?.UserSettings ?? new FamilyBrowserSettings();
        }
        catch { return new FamilyBrowserSettings(); }
    }

    public static void SaveScope(SettingsScope scope)
    {
        try
        {
            var file = SafeRead();
            file.Scope = scope;
            Write(file);
        }
        catch { }
    }

    public static void SaveUserSettings(FamilyBrowserSettings settings)
    {
        try
        {
            var file = SafeRead();
            file.UserSettings = settings;
            Write(file);
        }
        catch { }
    }

    private static UserSettingsFile Read()
        => JsonConvert.DeserializeObject<UserSettingsFile>(File.ReadAllText(FilePath));

    private static UserSettingsFile SafeRead()
    {
        try { return File.Exists(FilePath) ? Read() ?? new UserSettingsFile() : new UserSettingsFile(); }
        catch  { return new UserSettingsFile(); }
    }

    private static void Write(UserSettingsFile file)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonConvert.SerializeObject(file, Formatting.Indented));
    }
}
