using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ValorVDC_FamilyBrowser.Commands.FamilyBrowser.Models;

public partial class FamilyBrowserTypeItem : ObservableObject
{
    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private BitmapSource _preview;

    public string FamilyName { get; init; }
    public string TypeName { get; init; }
    public string DisplayName => $"{FamilyName} : {TypeName}";
    public ElementId SymbolId { get; init; }
}
