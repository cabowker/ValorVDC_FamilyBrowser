using System;
using Autodesk.Revit.UI;
using ValorVDC_FamilyBrowser.Services;

namespace ValorVDC_FamilyBrowser.Commands.FamilyBrowser.ExternalEvents;

public class SaveFamilyBrowserSettingsHandler : IExternalEventHandler
{
    private FamilyBrowserViewModel _viewModel;

    public FamilyBrowserSettings SettingsToSave { get; set; }

    public void SetViewModel(FamilyBrowserViewModel vm) => _viewModel = vm;

    public void Execute(UIApplication app)
    {
        if (SettingsToSave == null || _viewModel == null) return;

        try
        {
            if (_viewModel.CurrentScope == SettingsScope.User)
            {
                FamilyBrowserUserSettingsStorage.SaveUserSettings(SettingsToSave);
            }
            else
            {
                var doc = app.ActiveUIDocument?.Document;
                if (doc == null) return;
                FamilyBrowserSettingsStorage.Save(doc, SettingsToSave);
            }

            _viewModel.OnSettingsSaved();
        }
        catch (Exception ex)
        {
            _viewModel.SetStatus($"Error saving settings: {ex.Message}");
        }
    }

    public string GetName() => "Save Family Browser Settings";
}
