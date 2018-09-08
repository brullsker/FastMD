using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
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
        DispatcherTimer GridSplitTimer = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(Titlebar);
            UnformattedReb.Document.SetText(Windows.UI.Text.TextSetOptions.None, Settings.Default.MDDocument);
            GridSplitTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            GridSplitTimer.Tick += GridSplitTimer_Tick;
            UpdateFont();
        }

        void UpdateFont()
        {
            FontFamily fontfam = new FontFamily(Settings.Default.FontFamilyName);
            UnformattedText.FontFamily = fontfam;
            UnformattedReb.FontFamily = fontfam;
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

        private async void File_Export_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileSavePicker picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("Markdown", new List<string>() { ".md", ".mdown", ".markdown" });
            picker.FileTypeChoices.Add("Plain text", new List<string>() { ".txt" });
            picker.FileTypeChoices.Add("HTML page", new List<string>() { ".html" });
            picker.SuggestedFileName = Settings.Default.DefaultExportName;
            StorageFile saveFile = await picker.PickSaveFileAsync();
            if (saveFile != null)
            {
                if (saveFile.FileType == ".html") await FileIO.WriteTextAsync(saveFile, ConvertToHtml(UnformattedReb));
                else await FileIO.WriteTextAsync(saveFile, UnformattedText.Text);
            }
        }
        private async void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog cd = new SettingsDialog();
            cd.Closed += SettingsDialogClosed;
            await cd.ShowAsync();
        }
        private async void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog cd = new AboutDialog();
            await cd.ShowAsync();
        }

        void SettingsDialogClosed(object sender, ContentDialogClosedEventArgs e)
        {
            UpdateFont();
        }

        public static string ConvertToHtml(RichEditBox richEditBox)
        {
            string strColour, strFntName, strHTML;
            richEditBox.Document.GetText(TextGetOptions.None, out string text);
            ITextRange txtRange = richEditBox.Document.GetRange(0, text.Length);
            strHTML = "<html>";
            int lngOriginalStart = txtRange.StartPosition;
            int lngOriginalLength = txtRange.EndPosition;
            float shtSize = 11;
            // txtRange.SetRange(txtRange.StartPosition, txtRange.EndPosition);
            bool bOpened = false, liOpened = false, numbLiOpened = false, iOpened = false, uOpened = false, bulletOpened = false, numberingOpened = false;
            for (int i = 0; i < text.Length; i++)
            {
                txtRange.SetRange(i, i + 1);
                if (i == 0)
                {
                    strColour = Windows.UI.Colors.Black.ToHex().ToString();
                    shtSize = txtRange.CharacterFormat.Size;
                    strFntName = txtRange.CharacterFormat.Name;
                    strHTML += "<span style=\"font-family:" + strFntName + "; font-size: " + shtSize + "pt; color: #" + strColour.Substring(3) + "\">";
                }
                if (txtRange.CharacterFormat.Size != shtSize)
                {
                    shtSize = txtRange.CharacterFormat.Size;
                    strHTML += "</span><span style=\"font-family: " + txtRange.CharacterFormat.Name + "; font-size: " + txtRange.CharacterFormat.Size + "pt; color: #" + txtRange.CharacterFormat.ForegroundColor.ToString().Substring(3) + "\">";
                }
                if (txtRange.Character == Convert.ToChar(13))
                {
                    strHTML += "<br/>";
                }
                #region bullet
                if (txtRange.ParagraphFormat.ListType == MarkerType.Bullet)
                {
                    if (!bulletOpened)
                    {
                        strHTML += "<ul>";
                        bulletOpened = true;
                    }

                    if (!liOpened)
                    {
                        strHTML += "<li>";
                        liOpened = true;
                    }

                    if (txtRange.Character == Convert.ToChar(13))
                    {
                        strHTML += "</li>";
                        liOpened = false;
                    }
                }
                else
                {
                    if (bulletOpened)
                    {
                        strHTML += "</ul>";
                        bulletOpened = false;
                    }
                }
                #endregion
                #region numbering
                if (txtRange.ParagraphFormat.ListType == MarkerType.LowercaseRoman)
                {
                    if (!numberingOpened)
                    {
                        strHTML += "<ol type=\"i\">";
                        numberingOpened = true;
                    }

                    if (!numbLiOpened)
                    {
                        strHTML += "<li>";
                        numbLiOpened = true;
                    }

                    if (txtRange.Character == Convert.ToChar(13))
                    {
                        strHTML += "</li>";
                        numbLiOpened = false;
                    }
                }
                else
                {
                    if (numberingOpened)
                    {
                        strHTML += "</ol>";
                        numberingOpened = false;
                    }
                }
                #endregion
                #region bold
                if (txtRange.CharacterFormat.Bold == FormatEffect.On)
                {
                    if (!bOpened)
                    {
                        strHTML += "<b>";
                        bOpened = true;
                    }
                }
                else
                {
                    if (bOpened)
                    {
                        strHTML += "</b>";
                        bOpened = false;
                    }
                }
                #endregion
                #region italic
                if (txtRange.CharacterFormat.Italic == FormatEffect.On)
                {
                    if (!iOpened)
                    {
                        strHTML += "<i>";
                        iOpened = true;
                    }
                }
                else
                {
                    if (iOpened)
                    {
                        strHTML += "</i>";
                        iOpened = false;
                    }
                }
                #endregion
                #region underline
                if (txtRange.CharacterFormat.Underline == UnderlineType.Single)
                {
                    if (!uOpened)
                    {
                        strHTML += "<u>";
                        uOpened = true;
                    }
                }
                else
                {
                    if (uOpened)
                    {
                        strHTML += "</u>";
                        uOpened = false;
                    }
                }
                #endregion
                strHTML += txtRange.Character;
            }
            strHTML += "</span></html>";
            return strHTML;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void FixedAreaSizesButton_Click(object sender, RoutedEventArgs e)
        {
            if (FixedAreaSizesButton.IsChecked == false)
            {
                GridSplitter.IsEnabled = true;
                DisabledGridSplitterElement.Visibility = Visibility.Collapsed;
                InActiveGridSplitterElement.Visibility = Visibility.Visible;
                ActiveGridSplitterElement.Visibility = Visibility.Collapsed;
            }
            else
            {
                GridSplitter.IsEnabled = false;
                DisabledGridSplitterElement.Visibility = Visibility.Visible;
                InActiveGridSplitterElement.Visibility = Visibility.Collapsed;
                ActiveGridSplitterElement.Visibility = Visibility.Collapsed;
            }
        }

        private void GridSplitter_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            GridSplitTimer.Start();
            DisabledGridSplitterElement.Visibility = Visibility.Collapsed;
            InActiveGridSplitterElement.Visibility = Visibility.Collapsed;
            ActiveGridSplitterElement.Visibility = Visibility.Visible;
        }

        async void GridSplitTimer_Tick(object sender, object e)
        {
            if (Convert.ToInt32(Row1.ActualHeight) < 20)
            {
                await Row1Content.Fade(value: 0f, duration: 125, delay: 0, easingType: EasingType.Linear).StartAsync(); Row1Content.Visibility = Visibility.Collapsed;
            }
            else
            {
                Row1Content.Visibility = Visibility.Visible; await Row1Content.Fade(value: 1f, duration: 125, delay: 0, easingType: EasingType.Linear).StartAsync();
            }

            if (Convert.ToInt32(Row3.ActualHeight) < 20)
            {
                await Row3Content.Fade(value: 0f, duration: 125, delay: 0, easingType: EasingType.Linear).StartAsync(); Row3Content.Visibility = Visibility.Collapsed;
            }
            else
            {
                Row3Content.Visibility = Visibility.Visible; await Row3Content.Fade(value: 1f, duration: 125, delay: 0, easingType: EasingType.Linear).StartAsync();
            }
        }

        private void GridSplitter_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            GridSplitTimer.Stop();
            DisabledGridSplitterElement.Visibility = Visibility.Collapsed;
            InActiveGridSplitterElement.Visibility = Visibility.Visible;
            ActiveGridSplitterElement.Visibility = Visibility.Collapsed;
        }

        private async void ShareSelection_Click(object sender, RoutedEventArgs e)
        {
            if (UnformattedReb.Document.Selection.Length > 0)
            {
                DataTransferManager.GetForCurrentView().DataRequested += ShareText_DataRequested;
                DataTransferManager.ShowShareUI();
            }
            else
            {
                MessageDialog md = new MessageDialog("You didn't select any text in the unformatted source code.", "No selection found");
                await md.ShowAsync();
            }
        }
        private void ShareText_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.SetText(UnformattedReb.Document.Selection.Text);
            args.Request.Data.Properties.Title = Package.Current.DisplayName;
            args.Request.Data.Properties.Description = "Share selected text";
        }

        private void ShareWholeText_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested += ShareTextAll_DataRequested;
            DataTransferManager.ShowShareUI();
        }
        private void ShareTextAll_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            UnformattedReb.Document.GetText(Windows.UI.Text.TextGetOptions.None, out string data);
            args.Request.Data.SetText(data);
            args.Request.Data.Properties.Title = Package.Current.DisplayName;
            args.Request.Data.Properties.Description = "Share whole text";
        }

        private static bool IsCtrlKeyPressed()
        {
            var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
            return (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }
        private static bool IsShiftKeyPressed()
        {
            var shiftState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);
            return (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }
        private static bool IsFKeyPressed()
        {
            var fState = CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.F);
            return (fState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        private async void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (IsCtrlKeyPressed() == true && IsShiftKeyPressed() == false)
            {
                switch (e.Key)
                {
                    case VirtualKey.S: File_Export_Click(sender, e); break;
                    case VirtualKey.D: File_ClearAll_Click(sender, e); break;
                    case VirtualKey.T: ShareWholeText_Click(sender, e); break;
                    case VirtualKey.O: SettingsMenuItem_Click(sender, e); break;
                    case VirtualKey.U: AboutMenuItem_Click(sender, e); break;
                }
                if (IsFKeyPressed() == true)
                {
                    MessageDialog medi = new MessageDialog("Not yet implemented");
                    switch (e.Key)
                    {
                        case VirtualKey.M: await medi.ShowAsync(); break;
                        case VirtualKey.P: await medi.ShowAsync(); break;
                        case VirtualKey.R: await medi.ShowAsync(); break;
                        case VirtualKey.H: await medi.ShowAsync(); break;
                    }
                }
            }
            if (IsCtrlKeyPressed() == true && IsShiftKeyPressed() == true)
            {
                switch (e.Key)
                {
                    case VirtualKey.T: ShareSelection_Click(sender, e); break;
                }
            }
        }
    }
}
