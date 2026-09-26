using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ValorVDC_FamilyBrowser.Commands.FamilyBrowser.Models;

public partial class BrowseCategoryItem : ObservableObject
{
    [ObservableProperty]
    private bool _isExpanded = true;

    public string CategoryName { get; init; }
    public ObservableCollection<FamilyBrowserTypeItem> Types { get; } = new();
}
