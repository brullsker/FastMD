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

// Die Elementvorlage "Inhaltsdialogfeld" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace FastMD
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        List<string> FontList = new List<string>();
        public SettingsDialog()
        {
            this.InitializeComponent();
            FontStuff();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void FontStuff()
        {
            string[] fonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
            Debug.WriteLine("Got fonts");
            FontList = fonts.ToList<string>(); Debug.WriteLine("FontList made");
            FontList.Sort(); Debug.WriteLine("List ordered");
            List<ComboBoxItem> FontItems = new List<ComboBoxItem>();
            foreach (string font in FontList) { FontItems.Add(new ComboBoxItem { Content = new TextBlock { Text = font, FontFamily = new FontFamily(font) } }); };
            FontFamBox.ItemsSource = FontItems;
            if (Settings.Default.FontFamSegFS == true)
            {
                int sui = FontList.IndexOf("Segoe UI");
                Debug.WriteLine(sui);
                Settings.Default.FontFamily = sui;
                Settings.Default.FontFamSegFS = false;
            }
            else Debug.WriteLine("Not first scan");
        }

        private void RestoreDefaultExpFN_Click(object sender, RoutedEventArgs e)
        {
            DefExpFN.Text = "FastMD Export";
        }

        private void FontFamBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string font = FontList[Settings.Default.FontFamily];
            Settings.Default.FontFamilyName = font;
        }

        private void FontSizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (FontSizeTextBox.Text != null)
                {
                    int fs = Convert.ToInt32(FontSizeTextBox.Text);
                    if (fs < 1)
                    {
                        fs = 1;
                        Settings.Default.FontSize = fs;
                    }
                    else if (fs > 1368)
                    {
                        fs = 1368;
                    }
                    else
                    {
                        Settings.Default.FontSize = fs;
                    }
                }
                else
                {
                    Settings.Default.FontSize = 1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Settings.Default.FontSize = 1;
                FontSizeTextBox.Text = "1";
            }
        }

        private void FontSizeTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FontSizeButtons.RequestedTheme = ElementTheme.Light;
        }

        private void FontSizeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.ThemeDefault == true) FontSizeButtons.RequestedTheme = ElementTheme.Default;
            if (Settings.Default.ThemeLight == true) FontSizeButtons.RequestedTheme = ElementTheme.Light;
            if (Settings.Default.ThemeDark == true) FontSizeButtons.RequestedTheme = ElementTheme.Dark;
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            int fs = Convert.ToInt32(FontSizeTextBox.Text);
            if (fs < 1638)
            {
                fs++;
                FontSizeTextBox.Text = fs.ToString();
            }
        }

        private void RepeatButton_Click_1(object sender, RoutedEventArgs e)
        {
            int fs = Convert.ToInt32(FontSizeTextBox.Text);
            if (fs > 0)
            {
                fs--;
                FontSizeTextBox.Text = fs.ToString();
            }
        }

        private void RestoreDefaultShrFN_Click(object sender, RoutedEventArgs e)
        {
            DefShrFN.Text = "FastNote Share";
        }
    }
}
