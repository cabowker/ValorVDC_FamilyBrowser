using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using ValorVDC_FamilyBrowser.Commands.FamilyBrowser.Models;
using ValorVDC_FamilyBrowser.Services;

namespace ValorVDC_FamilyBrowser.Commands.FamilyBrowser.ExternalEvents;

public class LoadFamilyBrowserDataHandler : IExternalEventHandler
{
    private FamilyBrowserViewModel? _viewModel;
    private bool _viewActivatedSubscribed;

    public bool UseViewMemory { private get; set; }

    public void SetViewModel(FamilyBrowserViewModel vm) => _viewModel = vm;

    public void Execute(UIApplication app)
    {
        if (!_viewActivatedSubscribed)
        {
            app.ViewActivated += OnViewActivated;
            _viewActivatedSubscribed = true;
        }

        var doc = app.ActiveUIDocument?.Document;
        if (doc == null || _viewModel == null) return;

        UseViewMemory = false;

        try
        {
            var settings = _viewModel.CurrentScope == SettingsScope.User
                ? FamilyBrowserUserSettingsStorage.LoadUserSettings()
                : FamilyBrowserSettingsStorage.Load(doc);
            bool loadGenerated = settings.IsConfigured;

            var symbols = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(s => s.Category != null && s.Family != null)
                .ToList();

            var categories = symbols
                .GroupBy(s => s.Category.Name)
                .OrderBy(g => g.Key)
                .Select(categoryGroup =>
                {
                    var catItem = new FamilyBrowserCategoryItem
                    {
                        CategoryName = categoryGroup.Key,
                        IsVisible    = settings.IsCategoryVisible(categoryGroup.Key),
                    };

                    foreach (var familyGroup in categoryGroup
                        .GroupBy(s => s.Family.Name)
                        .OrderBy(g => g.Key))
                    {
                        var famItem = new FamilyBrowserFamilyItem { FamilyName = familyGroup.Key };

                        foreach (var sym in familyGroup.OrderBy(s => s.Name))
                        {
                            var isVisible = settings.IsSymbolVisible(sym.Id.Value, sym.Family.Name, sym.Name);

                            // Type Image is cheap (parameter read) — always load it.
                            // Generated preview is expensive (geometry render) — only for visible/configured types.
                            var preview = LoadTypeImage(doc, sym)
                                       ?? ((loadGenerated && isVisible) ? LoadGeneratedPreview(sym) : null);

                            famItem.Types.Add(new FamilyBrowserTypeItem
                            {
                                FamilyName = sym.Family.Name,
                                TypeName   = sym.Name,
                                SymbolId   = sym.Id,
                                IsVisible  = isVisible,
                                Preview    = preview,
                            });
                        }

                        catItem.Families.Add(famItem);
                    }

                    return catItem;
                })
                .ToList();

            _viewModel.UpdateData(categories, settings);
        }
        catch (Exception ex)
        {
            _viewModel?.SetStatus($"Error: {ex.Message}");
        }
    }

    private void OnViewActivated(object sender, ViewActivatedEventArgs e)
    {
        if (_viewModel == null) return;
        UseViewMemory = true;
        _viewModel.OnViewActivated();
    }

    // Reads the Type Image for a family symbol.
    // Checks the built-in ALL_MODEL_TYPE_IMAGE parameter first (standard families),
    // then falls back to a custom parameter named "Type Image" (common on tag families
    // which don't have the built-in parameter by default).
    private static BitmapSource? LoadTypeImage(Document doc, FamilySymbol symbol)
    {
        try
        {
            var param = symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_IMAGE)
                     ?? symbol.LookupParameter("Type Image");

            if (param == null || !param.HasValue) return null;

            // Standard image parameters store an ElementId pointing to an ImageType element.
            if (param.StorageType == StorageType.ElementId)
            {
                var imageId = param.AsElementId();
                if (imageId == ElementId.InvalidElementId) return null;

                if (doc.GetElement(imageId) is not ImageType imageType) return null;

                // Path-linked: load from disk if the original file still exists.
                if (!string.IsNullOrWhiteSpace(imageType.Path) && File.Exists(imageType.Path))
                {
                    var bi = new BitmapImage(new Uri(imageType.Path));
                    bi.Freeze();
                    return bi;
                }

                // Embedded: GetImage() returns System.Drawing.Image (Revit 2022+).
                using var sysImage = imageType.GetImage();
                if (sysImage == null) return null;
                using var bmp = new Bitmap(sysImage);
                return BitmapToBitmapSource(bmp);
            }

            // Some custom parameters store the image as a file path string.
            if (param.StorageType == StorageType.String)
            {
                var path = param.AsString();
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    var bi = new BitmapImage(new Uri(path));
                    bi.Freeze();
                    return bi;
                }
            }

            return null;
        }
        catch { return null; }
    }

    // Asks Revit to render a 64x64 geometry preview for the family symbol.
    private static BitmapSource? LoadGeneratedPreview(FamilySymbol symbol)
    {
        try
        {
            using var bmp = symbol.GetPreviewImage(new Size(64, 64));
            return bmp != null ? BitmapToBitmapSource(bmp) : null;
        }
        catch { return null; }
    }

    private static BitmapSource? BitmapToBitmapSource(Bitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        stream.Position = 0;
        var img = new BitmapImage();
        img.BeginInit();
        img.CacheOption  = BitmapCacheOption.OnLoad;
        img.StreamSource = stream;
        img.EndInit();
        img.Freeze();
        return img;
    }

    public string GetName() => "Load Family Browser Data";
}
