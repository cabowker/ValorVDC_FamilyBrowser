using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ValorVDC_FamilyBrowser.Commands.FamilyBrowser;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class ShowFamilyBrowserCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var pane = commandData.Application.GetDockablePane(FamilyBrowserPane.PaneId);
        if (pane.IsShown())
            pane.Hide();
        else
            pane.Show();
        return Result.Succeeded;
    }

    public static void CreateButton(RibbonPanel panel)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var icon     = ImageUtilities.LoadImage(assembly, "FamilyPaletteButton_32x32.png");

        var buttonData = new PushButtonData(
            "FamilyBrowserButton",
            "Family\nBrowser",
            assembly.Location,
            typeof(ShowFamilyBrowserCommand).FullName)
        {
            ToolTip = "Show or hide the Family Browser palette.",
        };

        if (icon != null)
            buttonData.LargeImage = icon;

        panel.AddItem(buttonData);
    }
}
