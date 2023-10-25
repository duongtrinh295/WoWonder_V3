using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    /// Interaction logic for SelectMembersWindow.xaml
    /// </summary>
    public partial class SelectMembersWindow : Window, AnjoListBoxScrollListener.IListBoxOnScrollListener
    {
        private string SearchKey;
        private readonly string TypePage;
        private readonly AnjoListBoxScrollListener BoxScrollListener;
        private ObservableCollection<UserDataFody> SelectMembersList = new ObservableCollection<UserDataFody>();
        private readonly List<UserDataFody> SelectedList;
         
        public SelectMembersWindow(string type , List<UserDataFody> selectedList)
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_SelectMembers;
                TypePage = type;
                SelectedList = selectedList;

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
         
        private void LoadUser()
        {
            try
            {
                SelectMembersList = new ObservableCollection<UserDataFody>();
                if (ListUtils.ListUsers.Count > 0)
                {
                    foreach (var item in from item in ListUtils.ListUsers let check = SelectMembersList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select WoWonderTools.UserFilter(item))
                    {
                        item.Selected = IsSelected(item.UserId);
                        SelectMembersList.Add(item);
                    }
                }

                var sqlEntity = new SqLiteDatabase();
                var userList = sqlEntity.Get_MyContact();

                foreach (var item in from item in userList let check = SelectMembersList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select WoWonderTools.UserFilter(item))
                {
                    item.Selected = IsSelected(item.UserId);
                    SelectMembersList.Add(item);
                }

                Dispatcher?.Invoke(delegate // <--- HERE
                {
                    try
                    {
                        if (SelectMembersList.Count > 0)
                        {
                            UserList.ItemsSource = SelectMembersList;
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
                        TxtSearchBoxOnline.Foreground = new SolidColorBrush(whiteBackgroundColor);
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

        private void UserList_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (e.OriginalSource is CheckBox originalSource)
                {
                    var userId = originalSource.CommandParameter.ToString();

                    var item = SelectMembersList.FirstOrDefault(a => a.UserId == userId);
                    if (item != null)
                    {
                        if (originalSource.IsChecked != null)
                            item.Selected = originalSource.IsChecked.Value;

                        if (!item.Selected)
                            SelectedList?.Remove(item);

                        //var view = CollectionViewSource.GetDefaultView(SelectMembersList);
                        // view.Refresh();
                    }
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

        private void TxtSearchBoxOnline_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                SearchKey = TxtSearchBoxOnline.Text;

                if (e.Key == Key.Enter)
                {
                    if (Methods.CheckForInternetConnection())
                    {
                        ProgressBarSearchUser.Visibility = Visibility.Visible;
                        ProgressBarSearchUser.IsIndeterminate = true;

                        // your event handler here
                        e.Handled = true;

                        BoxScrollListener.IsLoading = false;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => Search_Async() });
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


        // Run background worker : Search
        private async Task Search_Async(string offset = "0")
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

                    if (SelectMembersList.Count > 0)
                    {
                        if (ListUtils.SelectMembersList.Count > 0)
                        {
                            var list = SelectMembersList.Where(a => a.Selected).ToList();
                            foreach (var item in from user in list let check = ListUtils.SelectMembersList.FirstOrDefault(a => a.UserId == user.UserId) where check == null select user)
                            {
                                item.Selected = IsSelected(item.UserId);
                                ListUtils.SelectMembersList.Add(item);
                            }
                        }
                        else
                        {
                            ListUtils.SelectMembersList = new ObservableCollection<UserDataFody>(SelectMembersList.Where(a => a.Selected).ToList());
                        }
                         
                        SelectMembersList.Clear();
                    }

                    var dictionary = new Dictionary<string, string>
                    {
                        {"user_id", UserDetails.UserId},
                        {"limit", "25"},
                        {"user_offset", offset},
                        {"gender", UserDetails.SearchGender},
                        {"search_key", SearchKey},
                        {"country", UserDetails.SearchCountry},
                        {"status", UserDetails.SearchStatus},
                        {"verified", UserDetails.SearchVerified},
                        {"filterbyage", UserDetails.SearchFilterByAge},
                        {"age_from", UserDetails.SearchAgeFrom},
                        {"age_to", UserDetails.SearchAgeTo},
                        {"image", UserDetails.SearchProfilePicture},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Global.SearchAsync(dictionary);
                    if (apiStatus == 200)
                    {
                        if (respond is GetSearchObject result)
                        {
                            if (result.Users?.Count > 0)
                            {
                                foreach (var item in from user in result.Users let check = SelectMembersList.FirstOrDefault(a => a.UserId == user.UserId) where check == null select WoWonderTools.UserFilter(user))
                                {
                                    Dispatcher?.Invoke(delegate // <--- HERE
                                    {
                                        try
                                        {
                                            item.Selected = IsSelected(item.UserId);
                                            SelectMembersList.Add(item);
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
                    else
                    {
                        BoxScrollListener.IsLoading = false;
                        Methods.DisplayReportResult(respond);
                    }

                    Dispatcher?.Invoke(delegate // <--- HERE
                    {
                        try
                        {
                            if (SelectMembersList.Count > 0)
                            {
                                UserList.ItemsSource = SelectMembersList;
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
                            BoxScrollListener.IsLoading = false;
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
                }
            }
            catch (Exception exception)
            {
                BoxScrollListener.IsLoading = false;
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

        private void TxtSearchBoxOnline_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchKey = TxtSearchBoxOnline.Text;
        }

        #region LoadMore

        public void OnLoadMore()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    //Code get last id where LoadMore >>
                    var item = SelectMembersList.LastOrDefault();
                    if (item != null && !string.IsNullOrEmpty(item.UserId) && !BoxScrollListener.IsLoading && !string.IsNullOrEmpty(SearchKey))
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => Search_Async(item.UserId) });
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

        private bool IsSelected(string userId)
        {
            try
            {
               var chk1 = SelectedList?.FirstOrDefault(a => a.UserId == userId);
               var chk2 = ListUtils.SelectMembersList?.FirstOrDefault(a => a.UserId == userId);
               return chk1 != null || chk2 != null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private void BtnSend_OnClick(object sender, RoutedEventArgs e)
        { 
            try
            {
                if (ListUtils.SelectMembersList.Count > 0)
                {
                    var list = SelectMembersList.Where(a => a.Selected).ToList();
                    foreach (var item in from user in list let check = ListUtils.SelectMembersList.FirstOrDefault(a => a.UserId == user.UserId) where check == null select user)
                    {
                        ListUtils.SelectMembersList.Add(item);
                    }
                }
                else
                {
                    ListUtils.SelectMembersList = new ObservableCollection<UserDataFody>(SelectMembersList.Where(a => a.Selected).ToList());
                }
               
                if (TypePage == "CreateGroup")
                {
                    CreateGroupChatWindow.Instance.GetResultSelectMembers(); 
                } 
                else if (TypePage == "EditGroup")
                {
                    EditGroupChatWindow.Instance.GetResultSelectMembers(); 
                }
                 
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}