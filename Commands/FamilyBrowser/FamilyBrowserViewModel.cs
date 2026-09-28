using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ValorVDC_FamilyBrowser.Commands.FamilyBrowser.ExternalEvents;
using ValorVDC_FamilyBrowser.Commands.FamilyBrowser.Models;
using ValorVDC_FamilyBrowser.Services;

namespace ValorVDC_FamilyBrowser.Commands.FamilyBrowser;

public partial class FamilyBrowserViewModel : ObservableObject
{
    private readonly ExternalEvent _loadEvent;
    private readonly ExternalEvent _placeEvent;
    private readonly ExternalEvent _saveSettingsEvent;
    private readonly LoadFamilyBrowserDataHandler _loadHandler;
    private readonly PlaceFamilyTypeHandler _placeHandler;
    private readonly SaveFamilyBrowserSettingsHandler _saveSettingsHandler;

    [ObservableProperty]
    private ObservableCollection<FamilyBrowserCategoryItem> _allCategories = new();

    [ObservableProperty]
    private ObservableCollection<BrowseCategoryItem> _browseCategories = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isConfigureMode;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private string _statusMessage = "Open a project to load the palette.";

    [ObservableProperty]
    private SettingsScope _currentScope;

    [ObservableProperty]
    private string? _updateTag;     // e.g. "v1.0.2" when newer release found, else null

    public bool IsUpdateAvailable => UpdateTag != null;

    public string VersionString =>
        "v" + (Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?");

    partial void OnUpdateTagChanged(string? value)
        => OnPropertyChanged(nameof(IsUpdateAvailable));

    // RadioButton-friendly bool wrappers for the scope toggle
    public bool IsScopeProject
    {
        get => CurrentScope == SettingsScope.Project;
        set { if (value) CurrentScope = SettingsScope.Project; }
    }

    public bool IsScopeUser
    {
        get => CurrentScope == SettingsScope.User;
        set { if (value) CurrentScope = SettingsScope.User; }
    }

    public bool HasData => AllCategories.Count > 0;

    public FamilyBrowserViewModel(
        LoadFamilyBrowserDataHandler loadHandler, ExternalEvent loadEvent,
        PlaceFamilyTypeHandler placeHandler, ExternalEvent placeEvent,
        SaveFamilyBrowserSettingsHandler saveSettingsHandler, ExternalEvent saveSettingsEvent)
    {
        _loadHandler         = loadHandler;
        _loadEvent           = loadEvent;
        _placeHandler        = placeHandler;
        _placeEvent          = placeEvent;
        _saveSettingsHandler = saveSettingsHandler;
        _saveSettingsEvent   = saveSettingsEvent;

        _loadHandler.SetViewModel(this);
        _saveSettingsHandler.SetViewModel(this);

        _currentScope = FamilyBrowserUserSettingsStorage.LoadScope();

        _ = CheckForUpdateAsync();
    }

    private async System.Threading.Tasks.Task CheckForUpdateAsync()
    {
        var tag = await UpdateChecker.GetLatestTagIfNewerAsync().ConfigureAwait(false);
        if (tag != null)
            System.Windows.Application.Current?.Dispatcher.Invoke(() => UpdateTag = tag);
    }

    partial void OnCurrentScopeChanged(SettingsScope value)
    {
        OnPropertyChanged(nameof(IsScopeProject));
        OnPropertyChanged(nameof(IsScopeUser));
        FamilyBrowserUserSettingsStorage.SaveScope(value);
        RequestDataLoad();
    }

    partial void OnSearchTextChanged(string value)
    {
        if (!IsConfigureMode)
            RebuildBrowseCategories();
    }

    public void OnViewActivated()
    {
        IsLoading = true;
        _loadEvent.Raise();
    }

    [RelayCommand]
    private void Refresh()
    {
        IsLoading = true;
        _loadEvent.Raise();
    }

    [RelayCommand]
    private void ToggleConfigureMode()
    {
        IsConfigureMode = !IsConfigureMode;
        if (!IsConfigureMode)
            RebuildBrowseCategories();
    }

    [RelayCommand]
    private void SaveSettings()
    {
        var settings = new FamilyBrowserSettings { IsConfigured = true };

        foreach (var cat in AllCategories)
        {
            if (!cat.IsVisible) continue;
            settings.VisibleCategoryNames.Add(cat.CategoryName);
            foreach (var fam in cat.Families)
                foreach (var type in fam.Types)
                {
                    if (!type.IsVisible) continue;
                    if (CurrentScope == SettingsScope.User)
                        // String key — stable across projects
                        settings.VisibleSymbolKeys.Add(FamilyBrowserSettings.MakeKey(type.FamilyName, type.TypeName));
                    else
                        // ElementId — project-specific, stored in the .rvt file
                        settings.VisibleSymbolIds.Add(type.SymbolId.Value);
                }
        }

        _saveSettingsHandler.SettingsToSave = settings;
        _saveSettingsEvent.Raise();
    }

    [RelayCommand]
    private void CancelConfigure()
    {
        IsConfigureMode = false;
        IsLoading = true;
        _loadEvent.Raise();
    }

    [RelayCommand]
    private void PlaceElement(FamilyBrowserTypeItem item)
    {
        if (item == null) return;
        _placeHandler.SymbolId = item.SymbolId;
        _placeEvent.Raise();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var cat in AllCategories)
        {
            cat.IsVisible = true;
            foreach (var fam in cat.Families)
                foreach (var type in fam.Types)
                    type.IsVisible = true;
        }
    }

    [RelayCommand]
    private void SelectNone()
    {
        foreach (var cat in AllCategories)
        {
            cat.IsVisible = false;
            foreach (var fam in cat.Families)
                foreach (var type in fam.Types)
                    type.IsVisible = false;
        }
    }

    [RelayCommand]
    private void SelectAllInCategory(FamilyBrowserCategoryItem cat)
    {
        if (cat == null) return;
        foreach (var fam in cat.Families)
            foreach (var type in fam.Types)
                type.IsVisible = true;
    }

    [RelayCommand]
    private void SelectNoneInCategory(FamilyBrowserCategoryItem cat)
    {
        if (cat == null) return;
        foreach (var fam in cat.Families)
            foreach (var type in fam.Types)
                type.IsVisible = false;
    }

    // Called by LoadFamilyBrowserDataHandler — already on Revit's main (= WPF UI) thread.
    public void UpdateData(List<FamilyBrowserCategoryItem> categories, FamilyBrowserSettings settings)
    {
        AllCategories.Clear();
        foreach (var cat in categories) AllCategories.Add(cat);
        OnPropertyChanged(nameof(HasData));

        RebuildBrowseCategories();

        StatusMessage = AllCategories.Count > 0
            ? $"{AllCategories.Count} categories, {AllCategories.Sum(c => c.TotalTypeCount)} types loaded."
            : "No loadable families found in this project.";
        IsLoading = false;
    }

    public void OnSettingsSaved()
    {
        IsConfigureMode = false;
        StatusMessage = "Settings saved.";
        IsLoading = true;
        _loadEvent.Raise();
    }

    public void SetStatus(string message)
    {
        StatusMessage = message;
        IsLoading = false;
    }

    public void RequestDataLoad()
    {
        IsLoading = true;
        _loadEvent.Raise();
    }

    private void RebuildBrowseCategories()
    {
        var lowerSearch = SearchText?.Trim().ToLowerInvariant() ?? "";

        BrowseCategories.Clear();

        foreach (var cat in AllCategories)
        {
            if (!cat.IsVisible) continue;

            var matchingTypes = cat.Families
                .SelectMany(f => f.Types)
                .Where(t => t.IsVisible)
                .Where(t => string.IsNullOrEmpty(lowerSearch)
                    || t.FamilyName.ToLowerInvariant().Contains(lowerSearch)
                    || t.TypeName.ToLowerInvariant().Contains(lowerSearch))
                .ToList();

            if (matchingTypes.Count == 0) continue;

            var browseItem = new BrowseCategoryItem
            {
                CategoryName = $"{cat.CategoryName}  ({matchingTypes.Count})",
            };
            foreach (var t in matchingTypes) browseItem.Types.Add(t);
            BrowseCategories.Add(browseItem);
        }
    }
}
