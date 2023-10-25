using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.Library;
using WoWonderDesktop.SQLiteDB;
using WoWonderDesktop.WindowsPages;

namespace WoWonderDesktop.Controls
{
    /// <summary>
    /// Interaction logic for ContactUserControl.xaml
    /// </summary>
    public partial class ContactUserControl : UserControl, AnjoListBoxScrollListener.IListBoxOnScrollListener
    {
        private string SearchKey;
        private readonly AnjoListBoxScrollListener BoxScrollListener;

        public static ContactUserControl Instance { get; private set; }
         
        public ContactUserControl()
        {
            try
            {
                InitializeComponent();

                Instance = this;

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }
                BoxScrollListener = new AnjoListBoxScrollListener(UserList);
                BoxScrollListener.SetScrollListener(this);
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
                if (ListUtils.ListUsersProfile.Count == 0)
                {
                    var sqlEntity = new SqLiteDatabase();
                    var userList = sqlEntity.Get_MyContact();

                    ListUtils.ListUsersProfile = new ObservableCollection<UserDataFody>(userList);
                }

                Dispatcher?.Invoke(() =>
                {
                    try
                    { 
                        if (ListUtils.ListUsersProfile.Count > 0)
                        {
                            UserList.ItemsSource = ListUtils.ListUsersProfile;
                            UserList.Visibility = Visibility.Visible;

                            EmptyPageContent.Visibility = Visibility.Collapsed;

                            ProgressBarSearchUser.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            UserList.Visibility = Visibility.Collapsed;
                             
                            ProgressBarSearchUser.Visibility = Visibility.Visible;
                            ProgressBarSearchUser.IsIndeterminate = true;

                            StartApiService();
                        }
                        
                        if (Methods.CheckForInternetConnection())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadGeneralData });
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

        // Run background worker : Users Contact 
        private void StartApiService(string offset = "0")
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadUser(offset) });
                else
                    MessageBox.Show(LocalResources.label_Please_check_your_internet);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Run background worker : Users Contact 
        private async Task LoadUser(string offset)
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

                    var (apiStatus, respond) = await RequestsAsync.Global.GetFriendsAsync(UserDetails.UserId, "following", "20", offset);
                    if (apiStatus == 200)
                    {
                        if (respond is GetFriendsObject result)
                        {
                            foreach (var item in from user in result.DataFriends.Following let check = ListUtils.ListUsersProfile.FirstOrDefault(a => a.UserId == user.UserId) where check == null select WoWonderTools.UserFilter(user))
                            {
                                Dispatcher?.Invoke(delegate // <--- HERE
                                {
                                    try
                                    {
                                        ListUtils.ListUsersProfile.Add(item);
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                });
                            }

                            // Insert data user in database
                            SqLiteDatabase database = new SqLiteDatabase();
                            database.Insert_Or_Replace_MyContactTable(ListUtils.ListUsersProfile);
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
                        if (ListUtils.ListUsersProfile.Count > 0)
                        {
                            UserList.ItemsSource = ListUtils.ListUsersProfile;
                            UserList.Visibility = Visibility.Visible;

                            EmptyPageContent.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            UserList.Visibility = Visibility.Collapsed;
                            EmptyPageContent.Visibility = Visibility.Visible;
                            EmptyPageContent.InflateLayout(EmptyPage.Type.NoUsers);
                        }

                        ProgressBarSearchUser.Visibility = Visibility.Collapsed;
                        ProgressBarSearchUser.IsIndeterminate = false;

                        BoxScrollListener.IsLoading = false;
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

        #region General

        //Get General Data Using Api >> Friend Requests and Group Chat Requests
        private async Task LoadGeneralData()
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    var fetch = "friend_requests";

                    if (Settings.EnableChatGroup)
                        fetch += ",group_chat_requests";

                    var (apiStatus, respond) = await RequestsAsync.Global.GetGeneralDataAsync(false, UserDetails.OnlineUsers, "", fetch).ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        if (respond is GetGeneralDataObject result)
                        {
                            // FriendRequests
                            var respondListFriendRequests = result.FriendRequests?.Count;
                            if (respondListFriendRequests > 0)
                            {
                                foreach (var item in from item in result.FriendRequests let check = ListUtils.FriendRequestsList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select WoWonderTools.UserFilter(item))
                                {
                                    Dispatcher?.Invoke((Action)delegate // <--- HERE
                                    {
                                        try
                                        {
                                            item.Username = "@" + item.Username;

                                            ListUtils.FriendRequestsList.Add(item);
                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                    });
                                } 
                            }

                            // Group Requests
                            if (Settings.EnableChatGroup)
                            {
                                var respondListGroupRequests = result?.GroupChatRequests?.Count;
                                if (respondListGroupRequests > 0)
                                {
                                    foreach (var item in from item in result.GroupChatRequests let check = ListUtils.GroupRequestsList.FirstOrDefault(a => a.GroupId == item.GroupId) where check == null select item)
                                    {
                                        Dispatcher?.Invoke((Action)delegate // <--- HERE
                                        {
                                            try
                                            {
                                                if (item.GroupTab?.GroupId != null)
                                                {
                                                    item.GroupTab = WoWonderTools.UserChatFilter(item.GroupTab);
                                                    ListUtils.GroupRequestsList.Add(item);
                                                }
                                            }
                                            catch (Exception exception)
                                            {
                                                Methods.DisplayReportResultTrack(exception);
                                            }
                                        });
                                    }
                                }
                            }

                            Dispatcher?.Invoke((Action)delegate // <--- HERE
                            {
                                try
                                {
                                    if (ListUtils.FriendRequestsList.Count > 0)
                                    {
                                        RequestButton.Content = LocalResources.label5_Request + " (" + ListUtils.FriendRequestsList.Count + ")";
                                    }
                                    else if (ListUtils.GroupRequestsList.Count > 0)
                                    {
                                        RequestButton.Content = LocalResources.label5_Request + " (" + ListUtils.GroupRequestsList.Count + ")";
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            }); 
                        }
                    }
                    else Methods.DisplayReportResult(respond);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        #region Event

        private void RequestButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                RequestWindow requestWindow = new RequestWindow();
                requestWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UserList_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    ListBox mi = (ListBox)sender;
                    Button originalSource = e.OriginalSource as Button;

                    var followId = originalSource?.CommandParameter.ToString();

                    var item = ListUtils.ListUsersProfile.FirstOrDefault(a => a.UserId == followId);
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

                        var view = CollectionViewSource.GetDefaultView(ListUtils.ListUsersProfile);
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
                UserDataFody selectedGroup = (UserDataFody)UserList.SelectedItem;
                if (selectedGroup != null)
                {
                    if (selectedGroup.Name == Name || !WoWonderTools.ChatIsAllowed(selectedGroup))
                        return;

                    //getting Name from selected chat
                    ViewModel.ModelInstance.Name = selectedGroup.Name;

                    var time = selectedGroup.LastseenUnixTime;
                    bool success = int.TryParse(time, out var number);
                    ViewModel.ModelInstance.LastSeen = success ? LocalResources.label_last_seen + " " + Methods.Time.TimeAgo(number, false) : LocalResources.label_last_seen + " " + time;

                    ViewModel.ModelInstance.OnPropertyChanged("Name");

                    //getting Avatar from selected chat
                    ViewModel.ModelInstance.AvatarUser = selectedGroup.Avatar;
                    ViewModel.ModelInstance.OnPropertyChanged("AvatarUser");

                    //Get message 
                    ViewModel.ModelInstance.LoadMessagesDataFody(WoWonderTools.ConvertToUserList(selectedGroup), "user");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void TxtSearchBoxOnline_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                SearchKey = TxtSearchBoxOnline.Text;

                if (e.Key == Key.Enter)
                {
                    if (Methods.CheckForInternetConnection())
                    {
                        // your event handler here
                        e.Handled = true;
                        
                        //if that user is in Normal Unpinned User list find there...
                        var filteredUsers = new ObservableCollection<UserDataFody>(ListUtils.ListUsersProfile.Where(user => user.Name.ToLower().Contains(SearchKey) //if ContactName Contains SearchText then add it in filtered User list
                                    || user.Username != null && user.Username.ToLower().Contains(SearchKey) //if Message Contains SearchText then add it in filtered User list
                        ));

                       if (filteredUsers.Count > 0)
                       {
                           UserList.ItemsSource = filteredUsers;
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
                    else
                    {
                        MessageBox.Show(LocalResources.label_Please_check_your_internet);
                    }
                }
                else if (e.Key == Key.Escape)
                {
                    TxtSearchBoxOnline.Text = "";
                    if (ListUtils.ListUsersProfile.Count > 0)
                    {
                        UserList.ItemsSource = ListUtils.ListUsersProfile;
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

        #region LoadMore

        public void OnLoadMore()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    //Code get last id where LoadMore >>
                    var item = ListUtils.ListUsersProfile.LastOrDefault();
                    if (item != null && !string.IsNullOrEmpty(item.UserId) && !BoxScrollListener.IsLoading)
                    {
                        SqLiteDatabase database = new SqLiteDatabase();
                        var list = database.Get_MyContact(int.Parse(item.UserId));
                        if (list?.Count > 0)
                        {
                            var listNew = list.Where(c => !ListUtils.ListUsersProfile.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                            if (listNew.Count > 0)
                                Dispatcher?.Invoke(() => ListUtils.AddRange(ListUtils.ListUsersProfile, listNew));
                            else
                                StartApiService(item.UserId);
                        }
                        else
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