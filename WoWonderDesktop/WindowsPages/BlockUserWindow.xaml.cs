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
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.WindowsPages
{
    /// <summary>
    /// Interaction logic for BlockUserWindow.xaml
    /// </summary>
    public partial class BlockUserWindow : Window
    {
        public BlockUserWindow()
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label_Btn_Block_User;

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }
                ModeDark_Window();
                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Load Data 
        
        // Run background worker : Users Contact 
        private void StartApiService()
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadUser });
                else
                    MessageBox.Show(LocalResources.label_Please_check_your_internet);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Run background worker : Users Contact 
        private async Task LoadUser()
        {
            try
            {
                IProgress<int> progress = new Progress<int>(percentCompleted =>
                {
                    ProgressBarSearchUser.Value = percentCompleted;
                });

                if (Methods.CheckForInternetConnection())
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.GetBlockedUsersAsync();
                    if (apiStatus == 200)
                    {
                        if (respond is GetBlockedUsersObject result)
                        {
                            foreach (var item in from user in result.BlockedUsers let check = ListUtils.ListUsersBlock.FirstOrDefault(a => a.UserId == user.UserId) where check == null select WoWonderTools.UserFilter(user))
                            {
                                Dispatcher?.Invoke(delegate // <--- HERE
                                {
                                    try
                                    {
                                        item.TextColorFollowing = "#efefef";
                                        item.TextFollowing = LocalResources.label5_UnBlock;
                                        item.ColorFollow = Settings.MainColor;

                                        ListUtils.ListUsersBlock.Add(item);
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                });
                            }
                        }
                    }
                }

                Dispatcher?.Invoke(() =>
                {
                    try
                    {
                        if (ListUtils.ListUsersBlock.Count > 0)
                        {
                            UserList.ItemsSource = ListUtils.ListUsersBlock;
                            UserList.Visibility = Visibility.Visible;

                            EmptyPageContent.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            UserList.Visibility = Visibility.Collapsed;
                            EmptyPageContent.Visibility = Visibility.Visible;
                            EmptyPageContent.InflateLayout(EmptyPage.Type.NoBlockedUsers);
                        }

                        ProgressBarSearchUser.Visibility = Visibility.Collapsed;
                        ProgressBarSearchUser.IsIndeterminate = false;
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

        private void UserList_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    ListBox mi = (ListBox)sender;
                    Button originalSource = e.OriginalSource as Button;

                    var userId = originalSource?.CommandParameter.ToString();

                    var result = MessageBox.Show(LocalResources.label5_ConfirmationUnFriend, "", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            // User pressed Yes
                            try
                            {
                                var item = ListUtils.ListUsersBlock.FirstOrDefault(a => a.UserId == userId);
                                if (item != null)
                                {
                                    ListUtils.ListUsersBlock.Remove(item);
                                }

                                if (ListUtils.ListUsersBlock.Count > 0)
                                {
                                    UserList.ItemsSource = ListUtils.ListUsersBlock;
                                    UserList.Visibility = Visibility.Visible;

                                    EmptyPageContent.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    UserList.Visibility = Visibility.Collapsed;
                                    EmptyPageContent.Visibility = Visibility.Visible;
                                    EmptyPageContent.InflateLayout(EmptyPage.Type.NoBlockedUsers);
                                }
                                 
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.BlockUserAsync(userId, false) });//false >> "un-block"

                                var view = CollectionViewSource.GetDefaultView(ListUtils.ListUsersBlock);
                                view.Refresh(); 
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

    }
}