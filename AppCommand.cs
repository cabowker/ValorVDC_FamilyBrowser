using System;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using ValorVDC_FamilyBrowser.Commands.FamilyBrowser;
using ValorVDC_FamilyBrowser.Commands.FamilyBrowser.ExternalEvents;

namespace ValorVDC_FamilyBrowser;

[Transaction(TransactionMode.Manual)]
public class AppCommand : IExternalApplication
{
    private static FamilyBrowserViewModel _viewModel;

    public Result OnStartup(UIControlledApplication application)
    {
        var panel = application.GetRibbonPanels(Tab.AddIns)
                        .FirstOrDefault(p => p.Name == "Family Tools")
                    ?? application.CreateRibbonPanel(Tab.AddIns, "Family Tools");

        var loadHandler       = new LoadFamilyBrowserDataHandler();
        var placeHandler      = new PlaceFamilyTypeHandler();
        var saveHandler       = new SaveFamilyBrowserSettingsHandler();
        var loadEvent         = ExternalEvent.Create(loadHandler);
        var placeEvent        = ExternalEvent.Create(placeHandler);
        var saveSettingsEvent = ExternalEvent.Create(saveHandler);

        _viewModel = new FamilyBrowserViewModel(
            loadHandler, loadEvent,
            placeHandler, placeEvent,
            saveHandler, saveSettingsEvent);

        // RegisterDockablePane is safe to call even if BimTools already registered this pane ID.
        // The second call throws; we catch and continue — the Show/Hide button still works.
        try
        {
            application.RegisterDockablePane(FamilyBrowserPane.PaneId, "Family Browser",
                new FamilyBrowserPane(_viewModel));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ValorVDC Family Browser] Pane already registered: {ex.Message}");
        }

        application.ControlledApplication.DocumentOpened  += (_, _) => _viewModel.RequestDataLoad();
        application.ControlledApplication.DocumentCreated += (_, _) => _viewModel.RequestDataLoad();

        ShowFamilyBrowserCommand.CreateButton(panel);

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
}
