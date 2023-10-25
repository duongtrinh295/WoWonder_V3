using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using WoWonderDesktop.Controls;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.Library;
using WoWonderDesktop.SQLiteDB;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.WindowsPages
{
    /// <summary>
    /// Interaction logic for NearByWindows.xaml
    /// </summary>
    public partial class NearByWindows : Window, AnjoListBoxScrollListener.IListBoxOnScrollListener
    {
        private readonly AnjoListBoxScrollListener BoxScrollListener;

        public NearByWindows()
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_NearBy;

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }

                ModeDark_Window();

                BoxScrollListener = new AnjoListBoxScrollListener(UserList);
                BoxScrollListener.SetScrollListener(this);

                LoadUser();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Load Data 

        public void LoadUser()
        {
            try
            { 
                Dispatcher?.Invoke(() =>
                {
                    try
                    {
                        ProgressBarSearchUser.Visibility = Visibility.Visible;
                        ProgressBarSearchUser.IsIndeterminate = true;

                        if (ListUtils.ListNearbyUsers.Count > 0)
                        {
                            UserList.ItemsSource = ListUtils.ListNearbyUsers;
                            UserList.Visibility = Visibility.Visible;

                            EmptyPageContent.Visibility = Visibility.Collapsed;
                        } 
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                 
                var lastIdUser = ListUtils.ListNearbyUsers.LastOrDefault()?.UserId ?? "0";
                StartApiService(lastIdUser);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Run background worker : Users Contact 
        private void StartApiService(string offset = "0")
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => LoadUser(offset) });
                else
                    MessageBox.Show(LocalResources.label_Please_check_your_internet);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Run background worker : Users Contact 
        private async Task LoadUser(string offset = "0")
        {
            try
            {
                if (BoxScrollListener.IsLoading)
                    return;

                IProgress<int> progress = new Progress<int>(percentCompleted =>
                {
                    ProgressBarSearchUser.Value = percentCompleted;
                });

                if (Methods.CheckForInternetConnection())
                {
                    BoxScrollListener.IsLoading = true;

                    var dictionary = new Dictionary<string, string>
                    {
                        {"limit", "20"},
                        {"offset", offset},
                        //{"gender", UserDetails.NearByGender},
                        //{"keyword", ""},
                        //{"status", UserDetails.NearByStatus},
                        //{"distance", UserDetails.NearByDistanceCount},
                        {"lat", UserDetails.Lat},
                        {"lng", UserDetails.Lng},
                        //{"relship", UserDetails.NearByRelationship},
                    };
                     
                    var (apiStatus, respond) = await RequestsAsync.Nearby.GetNearbyUsersAsync(dictionary);
                    if (apiStatus == 200)
                    {
                        if (respond is GetNearbyUsersObject result)
                        {
                            foreach (var item in from user in result.NearbyUsers let check = ListUtils.ListNearbyUsers.FirstOrDefault(a => a.UserId == user.UserId) where check == null select WoWonderTools.UserFilter(user))
                            {
                                Dispatcher?.Invoke(delegate // <--- HERE
                                {
                                    try
                                    { 
                                        ListUtils.ListNearbyUsers.Add(item);
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                });
                            }
                        }
                    }
                    else
                    {
                        BoxScrollListener.IsLoading = false;
                        Methods.DisplayReportResult(respond);
                    }
                }

                Dispatcher?.Invoke(() =>
                {
                    try
                    {
                        if (ListUtils.ListNearbyUsers.Count > 0)
                        {
                            UserList.ItemsSource = ListUtils.ListNearbyUsers;
                            UserList.Visibility = Visibility.Visible;

                            EmptyPageContent.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            UserList.Visibility = Visibility.Collapsed;
                            EmptyPageContent.Visibility = Visibility.Visible;
                            EmptyPageContent.InflateLayout(EmptyPage.Type.NoNearBy);
                        }

                        ProgressBarSearchUser.Visibility = Visibility.Collapsed;
                        ProgressBarSearchUser.IsIndeterminate = false;

                        BoxScrollListener.IsLoading = false;
                    }
                    catch (Exception exception)
                    {
                        BoxScrollListener.IsLoading = false;

                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception e)
            {
                BoxScrollListener.IsLoading = false;

                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void UserList_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    ListBox mi = (ListBox)sender;
                    Button originalSource = e.OriginalSource as Button;

                    var followId = originalSource?.CommandParameter.ToString();

                    var item = ListUtils.ListNearbyUsers.FirstOrDefault(a => a.UserId == followId);
                    if (item != null)
                    {
                        item.IsFollowing ??= "0";

                        var dbDatabase = new SqLiteDatabase();
                        switch (item.IsFollowing)
                        {
                            case "0": // Add Or request friends
                            case "no":
                            case "No":
                                if (item.ConfirmFollowers == "1" || Settings.ConnectivitySystem == 0)
                                {
                                    item.IsFollowing = "2";
                                    item.TextColorFollowing = Settings.MainColor;
                                    item.TextFollowing = LocalResources.label5_Request;
                                    item.ColorFollow = "#efefef";

                                    dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(item, "Update");
                                }
                                else
                                {
                                    item.IsFollowing = "1";

                                    item.TextColorFollowing = "#efefef";
                                    item.TextFollowing = Settings.ConnectivitySystem == 1 ? LocalResources.label5_Following : LocalResources.label5_Friends;
                                    item.ColorFollow = (Settings.MainColor);

                                    dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(item, "Insert");
                                }

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowUserAsync(item.UserId) });

                                break;
                            case "1": // Remove friends
                            case "yes":
                            case "Yes":
                                item.IsFollowing = "0";

                                item.TextColorFollowing = Settings.MainColor;
                                item.TextFollowing = Settings.ConnectivitySystem == 1 ? LocalResources.label5_Follow : LocalResources.label5_AddFriends;
                                item.ColorFollow = "#efefef";

                                dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(item, "Delete");

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowUserAsync(item.UserId) });

                                break;
                            case "2": // Remove request friends

                                var result = MessageBox.Show(LocalResources.label5_ConfirmationUnFriend, "", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);

                                switch (result)
                                {
                                    case MessageBoxResult.Yes:
                                        // User pressed Yes
                                        try
                                        {
                                            item.IsFollowing = "0";

                                            item.TextColorFollowing = Settings.MainColor;
                                            item.TextFollowing = Settings.ConnectivitySystem == 1 ? LocalResources.label5_Follow : LocalResources.label5_AddFriends;
                                            item.ColorFollow = "#efefef";

                                            dbDatabase = new SqLiteDatabase();
                                            dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(item, "Delete");

                                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowUserAsync(item.UserId) });
                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                        break;
                                    case MessageBoxResult.No:
                                        // User pressed No
                                        break;
                                }

                                break;
                        }

                        var view = CollectionViewSource.GetDefaultView(ListUtils.ListNearbyUsers);
                        view.Refresh();
                    }
                }
                else
                {
                    MessageBox.Show(LocalResources.label_Please_check_your_internet);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UserList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UserList.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UserList_OnMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                UserList.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Window

        private void ModeDark_Window()
        {
            try
            {
                var darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                var whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);
                Color selverBackgroundColor = (Color)ConvertFromString(Settings.LigthBackground_Color);

                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        Background = new SolidColorBrush(darkBackgroundColor);
                        TabDockPanel.Background = new SolidColorBrush(darkBackgroundColor);
                        UserList.Background = new SolidColorBrush(darkBackgroundColor);
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


        #region LoadMore

        public void OnLoadMore()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    //Code get last id where LoadMore >>
                    var item = ListUtils.ListNearbyUsers.LastOrDefault();
                    if (item != null && !string.IsNullOrEmpty(item.UserId) && !BoxScrollListener.IsLoading)
                    { 
                        StartApiService(item.UserId);
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            });
        }

        public void OnLoadUp()
        {

        }

        #endregion

    }
}