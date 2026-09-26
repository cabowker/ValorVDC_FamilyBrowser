using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ValorVDC_FamilyBrowser.Commands.FamilyBrowser.Models;

public partial class FamilyBrowserCategoryItem : ObservableObject
{
    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private bool _isExpanded = false;

    public string CategoryName { get; init; }
    public ObservableCollection<FamilyBrowserFamilyItem> Families { get; } = new();

    public int TotalTypeCount => Families.Sum(f => f.Types.Count);
}
