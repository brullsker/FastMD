using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        FontFamily DefaultFontFamily = new FontFamily("Consolas");
        FontFamily DefaultAppFontFamily = new FontFamily("Segoe UI");
        public MainPage()
        {
            this.InitializeComponent();
            if (Settings.Default.ThemeDefault == true) this.RequestedTheme = ElementTheme.Default;
            if (Settings.Default.ThemeLight == true) this.RequestedTheme = ElementTheme.Light;
            if (Settings.Default.ThemeDark == true) this.RequestedTheme = ElementTheme.Dark;
        }

        private void ChangeTheme(object sender, RoutedEventArgs e)
        {
            //if (tdef.IsChecked == true) this.RequestedTheme = ElementTheme.Default;
            //if (tlit.IsChecked == true) this.RequestedTheme = ElementTheme.Light;
            //if (tdrk.IsChecked == true) this.RequestedTheme = ElementTheme.Dark;
        }

        private void MDInputArea_TextChanged(object sender, RoutedEventArgs e)
        {
            //string text = TextToolbar.Formatter?.Text;
            //Preview.Text = string.IsNullOrWhiteSpace(text) ? "Nothing to Preview" : text;
            MDInputArea.FontFamily = DefaultFontFamily;
            MDInputArea.Document.GetText(Windows.UI.Text.TextGetOptions.None, out string txt);
            TestItem.Document.SetText(Windows.UI.Text.TextSetOptions.None, txt);
            TestItem.FontFamily = DefaultAppFontFamily;
        }

        int count = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (count == 0)
            {
                Debug.WriteLine("Count: " + count.ToString());
                vswg.Orientation = Orientation.Horizontal;
                Debug.WriteLine("Orientation: " + vswg.Orientation.ToString());
                count = 1;
                Debug.WriteLine("Count: " + count.ToString());
            }
            else if (count == 1)
            {
                Debug.WriteLine("Count: " + count.ToString());
                vswg.Orientation = Orientation.Vertical;
                Debug.WriteLine("Orientation: " + vswg.Orientation.ToString());
                count = 0;
                Debug.WriteLine("Count: " + count.ToString());
            }
            ChangeSize();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeSize();
        }

        private void ChangeSize()
        {
            double winw = Window.Current.Bounds.Width;
            double winh = Window.Current.Bounds.Height;
            double divide = Convert.ToDouble(2);
            double pane;
            if (MainSplitView.IsPaneOpen == true) pane = MainSplitView.OpenPaneLength;
            else pane = Convert.ToDouble(0);
            vswg.Height = winh - TopRow.ActualHeight - BtmRow.ActualHeight;
            vswg.Width = winw - pane;
            Debug.WriteLine("H: " + vswg.Height.ToString() + " | W: " + vswg.Width.ToString());
            if (count == 0)
            {
                MDInputArea.Height = vswg.Height / divide;
                MDInputArea.Width = vswg.Width;
                TestItem.Height = vswg.Height / divide;
                TestItem.Width = vswg.Width;
            }
            if (count == 1)
            {
                MDInputArea.Height = vswg.Height;
                MDInputArea.Width = vswg.Width / divide;
                TestItem.Height = vswg.Height;
                TestItem.Width = vswg.Width / divide;
            }
        }

        private void PaneButton_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
            ChangeSize();
        }
    }
}
