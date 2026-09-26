using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ValorVDC_FamilyBrowser.Commands.FamilyBrowser.Models;

public partial class FamilyBrowserTypeItem : ObservableObject
{
    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private BitmapSource? _preview;

    public required string FamilyName { get; init; }
    public required string TypeName { get; init; }
    public string DisplayName => $"{FamilyName} : {TypeName}";
    public required ElementId SymbolId { get; init; }
}
