using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.GroupChat;
using WoWonderClient.Requests;
using WoWonderDesktop.Controls;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.WindowsPages
{
    /// <summary>
    /// Interaction logic for GroupChatProfileWindow.xaml
    /// </summary>
    public partial class GroupChatProfileWindow : Window
    { 
        private readonly UserListFody SelectedChatData;

        public GroupChatProfileWindow(UserListFody selectedChatData)
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_MyProfile;
                SelectedChatData = selectedChatData;

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }

                ModeDark_Window();
                LoadData();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #region Load Data

        private void LoadData()
        {
            try
            {
                if (SelectedChatData == null)
                    return;

                Image.Source = new BitmapImage(new Uri(SelectedChatData.Avatar));

                TxtName.Text = Methods.FunString.DecodeString(SelectedChatData.GroupName);  
                 
                if (SelectedChatData?.Parts?.Count > 0)
                {
                    var sss = (SelectedChatData?.Parts).Where(dataPart => dataPart != null).ToList();

                    TxtParticipant.Text = "Participant (" + sss.Count + ")";

                    var list  = new ObservableCollection<UserDataObject>(); 
                    foreach (var item in from user in sss let check = list.FirstOrDefault(a => a.UserId == user.UserId) where check == null select WoWonderTools.UserFilter(user))
                    {
                        list.Add(item);
                    }
                    
                    UserList.ItemsSource = list;
                    UserList.Visibility = Visibility.Visible;

                    EmptyPageContent.Visibility = Visibility.Collapsed;
                }
                else
                {
                    UserList.Visibility = Visibility.Collapsed;

                    EmptyPageContent.Visibility = Visibility.Visible;
                    EmptyPageContent.InflateLayout(EmptyPage.Type.NoUsers);
                }
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
                    if (e.OriginalSource is CheckBox originalSource)
                    {
                        var userId = originalSource.CommandParameter.ToString();

                        var item = ListUtils.ListUsersProfile.FirstOrDefault(a => a.UserId == userId);
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
         
        //Delete group chat
        private async void DeleteGroupButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(LocalResources.label5_AreYouSureDeleteGroup, "", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // User pressed Yes
                        try
                        {
                            var (apiStatus, respond) = await RequestsAsync.GroupChat.DeleteGroupChatAsync(SelectedChatData.GroupId);
                            if (apiStatus == 200)
                            {
                                if (respond is AddOrRemoveUserToGroupObject groupObject)
                                {
                                    Console.WriteLine(groupObject.MessageData);

                                    MessageBox.Show(LocalResources.label5_GroupSuccessfullyDeleted);

                                    //remove new item to my Group list  
                                    var data = ListUtils.ListGroup?.FirstOrDefault(a => a.GroupId == SelectedChatData.GroupId);
                                    if (data != null)
                                    {
                                        ListUtils.ListGroup.Remove(data);
                                    }

                                    Close();
                                }
                            }
                            else
                            {
                                Methods.DisplayReportResult(respond);
                            }
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Leave group chat
        private async void ExitGroupButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(LocalResources.label5_AreYouSureExitGroup, "", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // User pressed Yes
                        try
                        {
                            var (apiStatus, respond) = await RequestsAsync.GroupChat.ExitGroupChatAsync(SelectedChatData.GroupId);
                            if (apiStatus == 200)
                            {
                                if (respond is AddOrRemoveUserToGroupObject groupObject)
                                {
                                    Console.WriteLine(groupObject.MessageData);

                                    MessageBox.Show(LocalResources.label5_GroupSuccessfullyLeaved);

                                    //remove new item to my Group list  
                                    var data = ListUtils.ListGroup?.FirstOrDefault(a => a.GroupId == SelectedChatData.GroupId);
                                    if (data != null)
                                    {
                                        ListUtils.ListGroup.Remove(data);
                                    }

                                    Close();
                                }
                            }
                            else
                            {
                                Methods.DisplayReportResult(respond);
                            }
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