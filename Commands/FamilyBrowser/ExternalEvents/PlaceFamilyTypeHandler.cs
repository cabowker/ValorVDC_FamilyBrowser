using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ValorVDC_FamilyBrowser.Commands.FamilyBrowser.ExternalEvents;

public class PlaceFamilyTypeHandler : IExternalEventHandler
{
    public ElementId SymbolId { get; set; }

    public void Execute(UIApplication app)
    {
        var uiDoc = app.ActiveUIDocument;
        if (uiDoc == null || SymbolId == null || SymbolId == ElementId.InvalidElementId) return;

        try
        {
            var symbol = uiDoc.Document.GetElement(SymbolId) as FamilySymbol;
            if (symbol == null) return;

            if (!symbol.IsActive)
            {
                using var tx = new Transaction(uiDoc.Document, "Activate Family Symbol");
                tx.Start();
                symbol.Activate();
                tx.Commit();
            }

            uiDoc.PostRequestForElementTypePlacement(symbol);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Family Browser place failed: {ex.Message}");
        }
    }

    public string GetName() => "Place Family Type";
}
