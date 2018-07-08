using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        FontFamily DefaultFontFamily = new FontFamily("Consolas");
        FontFamily DefaultAppFontFamily = new FontFamily("Segoe UI");
        public MainPage()
        {
            this.InitializeComponent();
            TextToolBar.Format = Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarFormats.Format.MarkDown;
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
            string text = TextToolBar.Formatter?.Text;
            Preview.Text = string.IsNullOrWhiteSpace(text) ? "Nothing to Preview" : text;
            MDInputArea.FontFamily = DefaultFontFamily;
            MDInputArea.Document.GetText(Windows.UI.Text.TextGetOptions.None, out string txt);
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
                if (EditorVisibiltyToggle.IsChecked == true && PreviewVisibiltyToggle.IsChecked == true)
                {
                    MDInputArea.Height = vswg.Height / divide;
                    MDInputArea.Width = vswg.Width;
                    Preview.Height = vswg.Height / divide;
                    Preview.Width = vswg.Width;
                }
                else if (EditorVisibiltyToggle.IsChecked == true && PreviewVisibiltyToggle.IsChecked == false)
                {
                    MDInputArea.Height = vswg.Height;
                    MDInputArea.Width = vswg.Width;
                }
                else if (EditorVisibiltyToggle.IsChecked == false && PreviewVisibiltyToggle.IsChecked == true)
                {
                    Preview.Height = vswg.Height;
                    Preview.Width = vswg.Width;
                }
            }
            if (count == 1)
            {
                if (EditorVisibiltyToggle.IsChecked == true && PreviewVisibiltyToggle.IsChecked == true)
                {
                    MDInputArea.Height = vswg.Height;
                    MDInputArea.Width = vswg.Width / divide;
                    Preview.Height = vswg.Height;
                    Preview.Width = vswg.Width / divide;
                }
                else if (EditorVisibiltyToggle.IsChecked == true && PreviewVisibiltyToggle.IsChecked == false)
                {
                    MDInputArea.Height = vswg.Height;
                    MDInputArea.Width = vswg.Width;
                }
                else if (EditorVisibiltyToggle.IsChecked == false && PreviewVisibiltyToggle.IsChecked == true)
                {
                    Preview.Height = vswg.Height;
                    Preview.Width = vswg.Width;
                }
            }
        }

        private void PaneButton_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = true;
            MainSplitView_PaneOpening(sender, e);
            ChangeSize();
        }
        private void MainSplitView_PaneOpening(object sender, RoutedEventArgs e)
        {
            PaneButton.Scale(scaleX: 0f, scaleY: 0f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
            PaneButton_Close.Scale(scaleX: 1f, scaleY: 1f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
        }

        private void PaneButton_Close_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = false;
        }

        private void MainSplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            PaneButton_Close.Scale(scaleX: 0f, scaleY: 0f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
            PaneButton.Scale(scaleX: 1f, scaleY: 1f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
        }

        private void VisbilityToggle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (EditorVisibiltyToggle.IsChecked == true)
            {
                MDInputArea.Visibility = Visibility.Visible;
            }
            else
            {
                MDInputArea.Visibility = Visibility.Collapsed;
            }
            if (PreviewVisibiltyToggle.IsChecked == true)
            {
                Preview.Visibility = Visibility.Visible;
            }
            else
            {
                Preview.Visibility = Visibility.Collapsed;
            }
            if (EditorVisibiltyToggle.IsChecked == false && PreviewVisibiltyToggle.IsChecked == false)
            {
                EditorVisibiltyToggle.IsChecked = true;
                VisbilityToggle_Tapped(sender, e);
            }
            ChangeSize();
        }
    }
}
