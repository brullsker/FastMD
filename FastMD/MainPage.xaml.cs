using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace FastMD
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(Titlebar);
            UnformattedReb.Document.SetText(Windows.UI.Text.TextSetOptions.None, Settings.Default.MDDocument);
        }

        private void MarkdownText_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            try
            {
                var link = new Uri(e.Link);
                var linkOpen = Task.Run(() => Launcher.LaunchUriAsync(link));
            }
            catch
            {
            }
        }

        private void UnformattedReb_TextChanged(object sender, RoutedEventArgs e)
        {
            UnformattedReb.Document.GetText(Windows.UI.Text.TextGetOptions.None, out string txt);
            UnformattedText.Text = txt;
            Settings.Default.MDDocument = txt;
        }

        private void File_ClearAll_Click(object sender, RoutedEventArgs e)
        {
            UnformattedReb.Document.SetText(Windows.UI.Text.TextSetOptions.None, "");
        }
    }
}
