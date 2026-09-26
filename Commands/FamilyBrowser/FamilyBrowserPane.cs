using System;
using Autodesk.Revit.UI;

namespace ValorVDC_FamilyBrowser.Commands.FamilyBrowser;

public class FamilyBrowserPane : IDockablePaneProvider
{
    // Shared with ValorVDC_BIMTools so only one pane exists when both plugins are installed.
    public static readonly DockablePaneId PaneId =
        new DockablePaneId(new Guid("7B2C3D4E-F5A6-4B7C-8D9E-0A1B2C3D4E5F"));

    private readonly FamilyBrowserViewModel _viewModel;

    public FamilyBrowserPane(FamilyBrowserViewModel viewModel) => _viewModel = viewModel;

    public void SetupDockablePane(DockablePaneProviderData data)
    {
        var control = new FamilyBrowserControl { DataContext = _viewModel };
        data.FrameworkElement = control;
        data.InitialState = new DockablePaneState
        {
            DockPosition = DockPosition.Tabbed,
            TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser,
        };
    }
}
