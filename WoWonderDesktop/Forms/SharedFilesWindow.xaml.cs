using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WoWonderClient.Classes.Global;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for SharedFilesWindow.xaml
    /// </summary>
    public partial class SharedFilesWindow : Window
    {
        public SharedFilesWindow(UserDataObject user)
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_SharedFiles;

                PolluteSharedFilesOn_list();
                HeaderText.Content = WoWonderTools.GetNameFinal(user);
               
                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }
                ModeDark_Window();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Function Get data My Profile
        private void PolluteSharedFilesOn_list()
        {
            try
            {
                var sharedList = ListUtils.ListSharedFiles.OrderBy(T => T.FileDate).ToList();

                var imagesListItem = new ObservableCollection<Classes.SharedFile>();
                var mediaListItem = new ObservableCollection<Classes.SharedFile>();
                var fileListItem = new ObservableCollection<Classes.SharedFile>();

                switch (sharedList.Count > 1)
                {
                    case true:
                    {
                        var imageList = sharedList.Where(a => a.FileType == "Image").ToList();
                        switch (imageList.Count > 0)
                        {
                            case true:
                            {
                                foreach (var imageItem in imageList)
                                {
                                    switch (UserDetails.ModeDarkStlye)
                                    {
                                        case true:
                                            imageItem.SColorBackground = "#232323";
                                            imageItem.SColorForeground = "#efefef";
                                            break;
                                        default:
                                            imageItem.SColorBackground = "#ffffff";
                                            imageItem.SColorForeground = "#444444";
                                            break;
                                    }

                                    imagesListItem.Add(imageItem);
                                }

                                break;
                            }
                        }

                        var mediaList = sharedList.Where(a => a.FileType == "Media").ToList();
                        switch (mediaList.Count > 0)
                        {
                            case true:
                            {
                                foreach (var imageItem in mediaList)
                                {
                                    switch (UserDetails.ModeDarkStlye)
                                    {
                                        case true:
                                            imageItem.SColorBackground = "#232323";
                                            imageItem.SColorForeground = "#efefef";
                                            break;
                                        default:
                                            imageItem.SColorBackground = "#ffffff";
                                            imageItem.SColorForeground = "#444444";
                                            break;
                                    }

                                    mediaListItem.Add(imageItem);
                                }

                                break;
                            }
                        }

                        var fileList = sharedList.Where(a => a.FileType == "File").ToList();
                        switch (fileList.Count > 0)
                        {
                            case true:
                            {
                                foreach (var imageItem in fileList)
                                {
                                    switch (UserDetails.ModeDarkStlye)
                                    {
                                        case true:
                                            imageItem.SColorBackground = "#232323";
                                            imageItem.SColorForeground = "#efefef";
                                            break;
                                        default:
                                            imageItem.SColorBackground = "#ffffff";
                                            imageItem.SColorForeground = "#444444";
                                            break;
                                    }

                                    fileListItem.Add(imageItem);
                                }

                                break;
                            }
                        }

                        ImagesListview.ItemsSource = imagesListItem;
                        MediaListview.ItemsSource = mediaListItem;
                        FilesListview.ItemsSource = fileListItem;
                        break;
                    }
                    default:
                        ImagesListview.ItemsSource = sharedList;
                        MediaListview.ItemsSource = sharedList;
                        FilesListview.ItemsSource = sharedList;
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ImagesListview_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Classes.SharedFile selectedGroup = (Classes.SharedFile)ImagesListview.SelectedItem;

                if (selectedGroup != null)
                {
                    switch (selectedGroup.FileType)
                    {
                        case "Image":
                        {
                            var p = new Process
                            {
                                StartInfo = new ProcessStartInfo(selectedGroup.FilePath) { UseShellExecute = true }
                            };
                            p.Start();
                            break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MediaListview_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Classes.SharedFile selectedGroup = (Classes.SharedFile)MediaListview.SelectedItem;

                if (selectedGroup != null)
                {
                    switch (selectedGroup.FileType)
                    {
                        case "Media":
                        {
                            var p = new Process
                            {
                                StartInfo = new ProcessStartInfo(selectedGroup.FilePath) {UseShellExecute = true}
                            };
                            p.Start();
                            break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FilesListview_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Classes.SharedFile selectedGroup = (Classes.SharedFile)FilesListview.SelectedItem;

                if (selectedGroup != null)
                {
                    switch (selectedGroup.FileType)
                    {
                        case "File":
                        {
                            var p = new Process
                            {
                                StartInfo = new ProcessStartInfo(selectedGroup.FilePath) { UseShellExecute = true }
                            };
                            p.Start();
                            break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

                        Border.Background = new SolidColorBrush(darkBackgroundColor);
                        Lbl_Share_files.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        HeaderText.Foreground = new SolidColorBrush(whiteBackgroundColor);

                        SharedFilesTabcontrol.Background = new SolidColorBrush(darkBackgroundColor);
                        SharedFilesTabcontrol.BorderBrush = new SolidColorBrush(darkBackgroundColor);

                        Item_Images.Background = new SolidColorBrush(darkBackgroundColor);
                        ImagesListview.Background = new SolidColorBrush(darkBackgroundColor);

                        MediaListview.Background = new SolidColorBrush(darkBackgroundColor);

                        Item_Files.Background = new SolidColorBrush(darkBackgroundColor);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

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
    }
}
