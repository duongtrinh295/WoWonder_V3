using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for Setting_Stickers_Window.xaml
    /// </summary>
    public partial class SettingStickersWindow : Window
    {
        #region Lists Items Declaration

        private readonly ObservableCollection<StickersModel> ListStickers = new ObservableCollection<StickersModel>();
        private readonly ObservableCollection<StickersModel> ListTrending = new ObservableCollection<StickersModel>();

        #endregion

        #region Variables

        private string StickerSName = "";
        private readonly ChatWindow ChatWindow;

        #endregion

        public SettingStickersWindow()
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_SettingStickers;

                ChatWindow = ChatWindow.ChatWindowContext;
                GetStickers();
                
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

        //########################## My Stickers ##########################

        #region My Stickers

        // Functions Get Stickers */ Visible /*
        private void GetStickers()
        {
            try
            {
                ListStickers.Clear();

                SqLiteDatabase database = new SqLiteDatabase();
                var dataStickers = database.Get_From_StickersTable();
                if (dataStickers != null)
                { 
                    foreach (var sticker in dataStickers)
                    {
                        if (sticker.SVisibility)
                        {
                            ListStickers.Add(new StickersModel()
                            {
                                PackageId = sticker.PackageId,
                                Name = sticker.SName,
                                Visibility = sticker.SVisibility ? Visibility.Visible : Visibility.Collapsed,
                                Count = sticker.SCount + " " + LocalResources.label_Item_My_Stickers,
                                ListSticker = StickersModel.GetStickers(sticker.PackageId),
                                Image = StickersModel.GetStickersIcon(sticker.PackageId),
                                SColorBackground = UserDetails.ModeDarkStlye ? "#232323" : "#ffffff",
                                SColorForeground = UserDetails.ModeDarkStlye ? "#efefef" : "#444444",
                            }); 
                        }
                        else
                        {
                            ListTrending.Add(new StickersModel()
                            {
                                PackageId = sticker.PackageId,
                                Name = sticker.SName,
                                Visibility = sticker.SVisibility ? Visibility.Visible : Visibility.Collapsed,
                                Count = sticker.SCount,
                                ListSticker = StickersModel.GetStickers(sticker.PackageId),
                                Image = StickersModel.GetStickersIcon(sticker.PackageId),
                                SColorBackground = UserDetails.ModeDarkStlye ? "#232323" : "#ffffff",
                                SColorForeground = UserDetails.ModeDarkStlye ? "#efefef" : "#444444",
                            });
                        } 
                    }
                      
                    StickersListview.ItemsSource = ListStickers;
                    TrendingListview.ItemsSource = ListTrending;
                }
                
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Selection Changed null is SelectedItem StickersListview
        private void StickersListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                StickersListview.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Mouse Move null is SelectedItem StickersListview
        private void StickersListview_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                StickersListview.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // click Button delete Stickers
        private void Button_delete_Stickers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mi = (Button)sender;
                StickerSName = mi.CommandParameter.ToString();

                var selectedGroup = ListStickers.FirstOrDefault(a => a.Name == StickerSName);

                if (selectedGroup != null)
                {
                    selectedGroup.Visibility = Visibility.Collapsed;

                    SqLiteDatabase database = new SqLiteDatabase();
                    database.Update_To_StickersTable(selectedGroup.Name, false);
                    

                    var deleteStickers = ListStickers.FirstOrDefault(a => a.Name == selectedGroup.Name);
                    ListStickers.Remove(deleteStickers);
                    StickersListview.ItemsSource = ListStickers;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //########################## Trending ##########################

        #region Trending
         
        // click Button Add Stickers
        private void Button_Add_Stickers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mi = (Button)sender;
                StickerSName = mi.CommandParameter.ToString();

                var selectedGroup = ListTrending.FirstOrDefault(a => a.Name == StickerSName);

                if (selectedGroup != null)
                {
                    selectedGroup.Visibility = Visibility.Visible;

                    SqLiteDatabase database = new SqLiteDatabase();
                    database.Update_To_StickersTable(selectedGroup.Name, true);
                    

                    var deleteStickers = ListTrending.FirstOrDefault(a => a.Name == selectedGroup.Name);
                    ListTrending.Remove(deleteStickers);
                    TrendingListview.ItemsSource = ListTrending;

                    ListStickers.Add(selectedGroup);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Selection Changed null is SelectedItem TrendingListview
        private void TrendingListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TrendingListview.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Mouse Move null is SelectedItem TrendingListview
        private void TrendingListview_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                TrendingListview.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        // click Button dune windows : Setting Stickers
        private void CloseSetting_StickersButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //ChatWindow.GetStickers();
                Close();
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
                    {
                        Background = new SolidColorBrush(darkBackgroundColor);

                        Grid.Background = new SolidColorBrush(darkBackgroundColor);
                        StickersTabcontrol.BorderBrush = new SolidColorBrush(darkBackgroundColor);
                        StickersTabcontrol.Background = new SolidColorBrush(darkBackgroundColor);

                        Item_My_Stickers.Background = new SolidColorBrush(darkBackgroundColor);
                        StickersListview.Background = new SolidColorBrush(darkBackgroundColor);

                        Item_Trending.Background = new SolidColorBrush(darkBackgroundColor);
                        TrendingListview.Background = new SolidColorBrush(darkBackgroundColor);

                        switch (ListStickers.Count > 0)
                        {
                            // ListBoxItem >> ListStickers
                            case true:
                            {
                                foreach (var items in ListStickers)
                                {
                                    items.SColorBackground = "#232323";
                                    items.SColorForeground = "#efefef";
                                }

                                break;
                            }
                        }

                        switch (ListTrending.Count > 0)
                        {
                            // ListBoxItem >> ListTrending
                            case true:
                            {
                                foreach (var items in ListTrending)
                                {
                                    items.SColorBackground = "#232323";
                                    items.SColorForeground = "#efefef";
                                }

                                break;
                            }
                        }

                        break;
                    }
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

        private void Btn_Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleApp_OnLoaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
