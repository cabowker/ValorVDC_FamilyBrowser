using System;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ValorVDC_FamilyBrowser.Commands.FamilyBrowser.ExternalEvents;

public class PlaceFamilyTypeHandler : IExternalEventHandler
{
    public ElementId? SymbolId { get; set; }

    // Set by the ViewModel before raising the event.
    // Called on the WPF dispatcher thread.
    public Action? OnStarted  { get; set; }
    public Action? OnFinished { get; set; }

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

            // Notify the panel that placement is starting (updates UI on WPF thread).
            Application.Current?.Dispatcher.BeginInvoke(OnStarted ?? (() => { }));

            // Blocking call — stays active until user presses ESC.
            uiDoc.PromptForFamilyInstancePlacement(symbol);
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            // User pressed ESC — normal exit, nothing to report.
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Family Browser place failed: {ex.Message}");
        }
        finally
        {
            // Always clear the active-placement indicator.
            Application.Current?.Dispatcher.BeginInvoke(OnFinished ?? (() => { }));
        }
    }

    public string GetName() => "Place Family Type";
}
