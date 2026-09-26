using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ValorVDC_FamilyBrowser;

internal static class ImageUtilities
{
    internal static BitmapSource LoadImage(Assembly assembly, string resourceName)
    {
        try
        {
            var fullName = $"{assembly.GetName().Name}.Resources.{resourceName}";
            using var stream = assembly.GetManifestResourceStream(fullName);
            if (stream == null) return null;

            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = stream;
            img.EndInit();
            img.Freeze();
            return img;
        }
        catch { return null; }
    }
}
