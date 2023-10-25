using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for Managelocalstrong_Window.xaml
    /// </summary>
    public partial class ManageLocalStrongWindow : Window
    {
        private double Size = 0;

        public ManageLocalStrongWindow()
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label_Lnk_Manage_local;

                count_all_file();

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }
                ModeDark_Window();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Functions Get count all file
        private void count_all_file()
        {
            try
            {
                //count all directry
                //var directoryInfo = new DirectoryInfo(Methods.FilesDestination);
                //int directoryCount = directoryInfo.GetDirectories().Length;

                // Will Retrieve count of all type #Images in directry and sub directries
                int imagesCount = Directory.GetFiles(Methods.FilesDestination, "*.png",
                                           SearchOption.AllDirectories)
                                       .Length + Directory
                                       .GetFiles(Methods.FilesDestination, "*.jpeg", SearchOption.AllDirectories)
                                       .Length + Directory
                                       .GetFiles(Methods.FilesDestination, "*.gif", SearchOption.AllDirectories)
                                       .Length + Directory
                                       .GetFiles(Methods.FilesDestination, "*.png", SearchOption.AllDirectories)
                                       .Length;

                // Will Retrieve count of all type  #Video in directry and sub directries
                int videoCount = Directory.GetFiles(Methods.FilesDestination, "*.mp4",
                                          SearchOption.AllDirectories)
                                      .Length + Directory
                                      .GetFiles(Methods.FilesDestination, "*.mpeg", SearchOption.AllDirectories)
                                      .Length + Directory
                                      .GetFiles(Methods.FilesDestination, "*.avi", SearchOption.AllDirectories)
                                      .Length + Directory
                                      .GetFiles(Methods.FilesDestination, "*.flv", SearchOption.AllDirectories)
                                      .Length + Directory
                                      .GetFiles(Methods.FilesDestination, "*.wmv", SearchOption.AllDirectories)
                                      .Length + Directory
                                      .GetFiles(Methods.FilesDestination, "*.mpg-4", SearchOption.AllDirectories)
                                      .Length + Directory
                                      .GetFiles(Methods.FilesDestination, "*.mpg", SearchOption.AllDirectories)
                                      .Length;

                // Will Retrieve count of all type #Audio in directry and sub directries
                int soundsCount = Directory.GetFiles(Methods.FilesDestination, "*.mp3",
                                           SearchOption.AllDirectories)
                                       .Length + Directory
                                       .GetFiles(Methods.FilesDestination, "*.aac", SearchOption.AllDirectories)
                                       .Length + Directory
                                       .GetFiles(Methods.FilesDestination, "*.aiff", SearchOption.AllDirectories)
                                       .Length + Directory
                                       .GetFiles(Methods.FilesDestination, "*.amr", SearchOption.AllDirectories)
                                       .Length + Directory
                                       .GetFiles(Methods.FilesDestination, "*.arf", SearchOption.AllDirectories)
                                       .Length + Directory
                                       .GetFiles(Methods.FilesDestination, "*.m4a", SearchOption.AllDirectories)
                                       .Length + Directory
                                       .GetFiles(Methods.FilesDestination, "*.wav", SearchOption.AllDirectories)
                                       .Length;

                // Will Retrieve count of all type #Files in directry and sub directries
                int fileCount = Directory.GetFiles(Methods.FilesDestination, "*.txt",
                                         SearchOption.AllDirectories)
                                     .Length + Directory
                                     .GetFiles(Methods.FilesDestination, "*.pdf", SearchOption.AllDirectories)
                                     .Length +
                                 Directory.GetFiles(Methods.FilesDestination, "*.rar",
                                         SearchOption.AllDirectories)
                                     .Length + Directory
                                     .GetFiles(Methods.FilesDestination, "*.zip", SearchOption.AllDirectories)
                                     .Length + Directory
                                     .GetFiles(Methods.FilesDestination, "*.docx", SearchOption.AllDirectories)
                                     .Length + Directory
                                     .GetFiles(Methods.FilesDestination, "*.doc", SearchOption.AllDirectories)
                                     .Length;

                // Calculate total size of all pngs.
                double x = GetDirectorySize(Methods.FilesDestination);
                double totalSize = x / 1024.0F / 1024.0F;

                LblCuontImages.Content = LocalResources.label_cuont_images + " = " + imagesCount;
                LblCuontVideo.Content = LocalResources.label_cuont_video + " = " + videoCount;
                LblCuontSounds.Content = LocalResources.label_cuont_sounds + " = " + soundsCount;
                LblCuontFile.Content = LocalResources.label_cuont_file + " = " + fileCount;
                LblTotalSize.Content = LocalResources.label_total_size + " = " + totalSize.ToString("0.### MB");
            } 
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Functions Get Directory Size
        private double GetDirectorySize(string directory)
        {
            try
            {
                foreach (string dir in Directory.GetDirectories(directory))
                {
                    GetDirectorySize(dir);
                }

                foreach (FileInfo file in new DirectoryInfo(directory).GetFiles())
                {
                    Size += file.Length;
                }
                return Size;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        //Click Link Clear all data file
        private void Lnk_Clear_all_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Methods.Path.ClearFolder();
                LblCuontImages.Content = LocalResources.label_cuont_images + " = " + 0;
                LblCuontVideo.Content = LocalResources.label_cuont_video + " = " + 0;
                LblCuontSounds.Content = LocalResources.label_cuont_sounds + " = " + 0;
                LblCuontFile.Content = LocalResources.label_cuont_file + " = " + 0;
                LblTotalSize.Content = LocalResources.label_total_size + " = " + 0.ToString("0.00 MB");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void ModeDark_Window()
        {
            try
            {
                var darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                var whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        Background = new SolidColorBrush(darkBackgroundColor);

                        BorderMain.Background = new SolidColorBrush(darkBackgroundColor);
                        TxtLocalStrong.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        LblCuontImages.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        LblCuontVideo.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        LblCuontSounds.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        LblCuontFile.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        LblTotalSize.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         

        #region Window
         
        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var gridWindow = sender as Grid;
                var darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                var whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        gridWindow.Background = new SolidColorBrush(darkBackgroundColor);
                        break;
                    default:
                        gridWindow.Background = new SolidColorBrush(whiteBackgroundColor);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_Close_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Button buttonWindowClose = sender as Button;
                Color darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                Color whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                if (UserDetails.ModeDarkStlye)
                {
                    buttonWindowClose.Foreground = new SolidColorBrush(whiteBackgroundColor);
                }
                else
                {
                    buttonWindowClose.Foreground = new SolidColorBrush(darkBackgroundColor);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleApp_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBlock titleApp = sender as TextBlock;
                Color darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                Color whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                if (UserDetails.ModeDarkStlye)
                {
                    titleApp.Foreground = new SolidColorBrush(whiteBackgroundColor);
                }
                else
                {
                    titleApp.Foreground = new SolidColorBrush(darkBackgroundColor);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

    }
}
