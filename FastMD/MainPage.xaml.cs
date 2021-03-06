﻿using Microsoft.Toolkit.Uwp.Helpers;
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
using Windows.Storage.Streams;
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
            UnformattedReb.FontSize = Settings.Default.FontSize;
            UnformattedText.FontSize = Settings.Default.FontSize;
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
            if (string.IsNullOrWhiteSpace(txt) == true)
            {
                UnformattedText.Text = String.Empty;
                Settings.Default.MDDocument = String.Empty;
            }
            else if (string.IsNullOrEmpty(txt) == true)
            {
                UnformattedText.Text = String.Empty;
                Settings.Default.MDDocument = String.Empty;
            }
            else
            {
                UnformattedText.Text = txt;
                Settings.Default.MDDocument = txt;
            }
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

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
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
                    switch (e.Key)
                    {
                        case VirtualKey.M: ShareFile_MD_Click(sender, e); break;
                        case VirtualKey.P: ShareFile_TXT_Click(sender, e); break;
                        case VirtualKey.R: ShareFile_RTF_Click(sender, e); break;
                        case VirtualKey.H: ShareFile_HTML_Click(sender, e); break;
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

        private async void ShareFile_MD_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("share", CreationCollisionOption.ReplaceExisting);
            StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".md", CreationCollisionOption.ReplaceExisting);
            UnformattedReb.Document.GetText(TextGetOptions.None, out string txtstring);
            await FileIO.WriteTextAsync(shareFile, txtstring);
            ShareFileEnd();
        }

        private async void ShareFile_TXT_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("share", CreationCollisionOption.ReplaceExisting);
            StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".txt", CreationCollisionOption.ReplaceExisting);
            UnformattedReb.Document.GetText(TextGetOptions.None, out string txtstring);
            await FileIO.WriteTextAsync(shareFile, txtstring);
            ShareFileEnd();
        }

        private async void ShareFile_RTF_Click(object sender, RoutedEventArgs e)
        {
            UnformattedReb.RequestedTheme = ElementTheme.Light;
            StorageFolder folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("share", CreationCollisionOption.ReplaceExisting);
            StorageFile file = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".rtf", CreationCollisionOption.ReplaceExisting);
            CachedFileManager.DeferUpdates(file);
            IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            UnformattedReb.Document.SaveToStream(Windows.UI.Text.TextGetOptions.FormatRtf, randAccStream);
            randAccStream.Dispose();
            ShareFileEnd();
        }

        private async void ShareFile_HTML_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("share", CreationCollisionOption.ReplaceExisting);
            StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".html", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(shareFile, ConvertToHtml(UnformattedReb));
            ShareFileEnd();
        }

        private IReadOnlyList<StorageFile> storageItems;

        private async void ShareFileEnd()
        {
            StorageFolder folder = await ApplicationData.Current.LocalCacheFolder.GetFolderAsync("share");
            IReadOnlyList<StorageFile> pickedFiles = await folder.GetFilesAsync();
            if (pickedFiles.Count > 0)
            {
                this.storageItems = pickedFiles;

                // Display the file names in the UI.
                string selectedFiles = String.Empty;
                for (int index = 0; index < pickedFiles.Count; index++)
                {
                    selectedFiles += pickedFiles[index].Name;

                    if (index != (pickedFiles.Count - 1))
                    {
                        selectedFiles += ", ";
                    }
                }
            }
            DataTransferManager.GetForCurrentView().DataRequested += ShareFile_DataRequested;
            DataTransferManager.ShowShareUI();
            if (Settings.Default.ThemeDefault == true) UnformattedReb.RequestedTheme = ElementTheme.Default;
            if (Settings.Default.ThemeLight == true) UnformattedReb.RequestedTheme = ElementTheme.Light;
            if (Settings.Default.ThemeDark == true) UnformattedReb.RequestedTheme = ElementTheme.Dark;
        }

        private void ShareFile_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.SetStorageItems(this.storageItems);
            args.Request.Data.Properties.Title = Package.Current.DisplayName;
            args.Request.Data.Properties.Description = "Share file";
        }
    }
}
