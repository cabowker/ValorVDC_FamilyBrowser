using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ValorVDC_FamilyBrowser.Commands.FamilyBrowser;

public partial class FamilyBrowserControl : UserControl
{
    public FamilyBrowserControl()
    {
        InitializeComponent();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
