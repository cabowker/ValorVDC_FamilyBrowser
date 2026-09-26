using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ValorVDC_FamilyBrowser.Commands.FamilyBrowser.Models;

public partial class FamilyBrowserFamilyItem : ObservableObject
{
    public string FamilyName { get; init; }
    public ObservableCollection<FamilyBrowserTypeItem> Types { get; } = new();
}
