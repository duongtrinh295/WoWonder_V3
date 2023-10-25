using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WoWonderClient.Requests;
using WoWonderDesktop.Controls;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.WindowsPages
{
    /// <summary>
    /// Interaction logic for RequestWindow.xaml
    /// </summary>
    public partial class RequestWindow : Window
    {
        public RequestWindow()
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_Request;

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }

                TXtFriendRequests.Text = Settings.ConnectivitySystem == 1 ? LocalResources.label5_FollowRequests : LocalResources.label5_FriendRequests;

                ModeDark_Window();
                ShowEmptyPage();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Follow/Friend Request


        private void ShowEmptyPage()
        {
            try
            {
                Dispatcher?.Invoke((Action)delegate // <--- HERE
                {
                    try
                    {
                        if (ListUtils.FriendRequestsList.Count > 0)
                        {
                            FriendRequestsList.ItemsSource = ListUtils.FriendRequestsList;
                            FriendRequestsList.Visibility = Visibility.Visible;

                            EmptyPageFriendRequests.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            FriendRequestsList.Visibility = Visibility.Collapsed;

                            EmptyPageFriendRequests.Visibility = Visibility.Visible;
                            EmptyPageFriendRequests.InflateLayout(EmptyPage.Type.NoFriendRequests);
                        }
                        
                        if (ListUtils.GroupRequestsList.Count > 0)
                        {
                            GroupRequestsList.ItemsSource = ListUtils.GroupRequestsList;
                            GroupRequestsList.Visibility = Visibility.Visible;

                            EmptyPageGroupRequests.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            GroupRequestsList.Visibility = Visibility.Collapsed;

                            EmptyPageGroupRequests.Visibility = Visibility.Visible;
                            EmptyPageGroupRequests.InflateLayout(EmptyPage.Type.NoGroupRequest);
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Event

        //Reject Friend Requests
        private void ButtonDelete_FriendRequests_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button originalSource = e.OriginalSource as Button;

                var userId = originalSource?.CommandParameter?.ToString();

                var item = ListUtils.FriendRequestsList.FirstOrDefault(w => w.UserId == userId);
                if (item != null)
                {
                    if (Methods.CheckForInternetConnection())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowRequestActionAsync(item.UserId, false) });// false >> Decline

                        ListUtils.FriendRequestsList.Remove(item);

                        ShowEmptyPage();
                    }
                    else
                    {
                        MessageBox.Show(LocalResources.label_Please_check_your_internet);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Accept Friend Requests
        private void ButtonAdd_FriendRequests_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button originalSource = e.OriginalSource as Button;

                var userId = originalSource?.CommandParameter?.ToString();

                var item = ListUtils.FriendRequestsList.FirstOrDefault(w => w.UserId == userId);
                if (item != null)
                {
                    if (Methods.CheckForInternetConnection())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowRequestActionAsync(item.UserId, true) }); // true >> Accept 

                        ListUtils.FriendRequestsList.Remove(item);

                        ShowEmptyPage();
                    }
                    else
                    {
                        MessageBox.Show(LocalResources.label_Please_check_your_internet);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Reject Group Requests
        private void ButtonDelete_GroupRequests_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button originalSource = e.OriginalSource as Button;

                var groupId = originalSource?.CommandParameter?.ToString();

                var item = ListUtils.GroupRequestsList.FirstOrDefault(w => w.GroupId == groupId);
                if (item != null)
                {
                    if (Methods.CheckForInternetConnection())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.GroupChat.RejectGroupChatRequestAsync(item.GroupId) });

                        ListUtils.GroupRequestsList.Remove(item);

                        ShowEmptyPage();
                    }
                    else
                    {
                        MessageBox.Show(LocalResources.label_Please_check_your_internet);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Accept Group Requests
        private void ButtonAdd_GroupRequests_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button originalSource = e.OriginalSource as Button;

                var groupId = originalSource?.CommandParameter?.ToString();

                var item = ListUtils.GroupRequestsList.FirstOrDefault(w => w.GroupId == groupId);
                if (item != null)
                {
                    if (Methods.CheckForInternetConnection())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.GroupChat.AcceptGroupChatRequestAsync(item.GroupId) });

                        ListUtils.GroupRequestsList.Remove(item);

                        ShowEmptyPage();
                    }
                    else
                    {
                        MessageBox.Show(LocalResources.label_Please_check_your_internet);
                    }
                }
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

    }
}