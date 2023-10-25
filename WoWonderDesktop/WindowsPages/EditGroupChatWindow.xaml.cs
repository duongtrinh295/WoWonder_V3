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
using Microsoft.Win32;
using WoWonderClient.Classes.GroupChat;
using WoWonderClient.Requests;
using WoWonderDesktop.Controls;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.Library.ProgressDialog;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.WindowsPages
{
    /// <summary>
    /// Interaction logic for EditGroupChatWindow.xaml
    /// </summary>
    public partial class EditGroupChatWindow : Window
    {
        public static EditGroupChatWindow Instance { get; private set; }
        private string GroupId, GroupPathImage, UsersIds;
        private readonly ObservableCollection<UserDataFody> PartsList = new ObservableCollection<UserDataFody>();
        private UserListFody SelectedChatData;

        public EditGroupChatWindow(UserListFody selectedChatData)
        {
            try
            {
                InitializeComponent();

                Title = LocalResources.label5_EditGroupChat;
                Instance = this;

                SelectedChatData = selectedChatData;
                 
                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }

                ModeDark_Window();

                ListUtils.SelectMembersList = new ObservableCollection<UserDataFody>();

                PartsList = new ObservableCollection<UserDataFody>();
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

                GroupId = SelectedChatData.GroupId;

                Image.Source = new BitmapImage(new Uri(SelectedChatData.Avatar));
                GroupPathImage = SelectedChatData.Avatar;

                TxtName.Text = Methods.FunString.DecodeString(SelectedChatData.GroupName);

                if (SelectedChatData?.Parts?.Count > 0)
                {
                    var sss = (SelectedChatData?.Parts).Where(dataPart => dataPart != null).ToList();

                    Console.WriteLine($"Participant ({sss.Count})");
                     
                    foreach (var item in from user in sss let check = PartsList.FirstOrDefault(a => a.UserId == user.UserId) where check == null select WoWonderTools.UserFilter(user))
                    {
                        PartsList.Add(item);
                    }
                }

                ShowEmptyPage();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void ShowEmptyPage()
        {
            try
            {
                if (PartsList?.Count > 0)
                {
                    UserList.ItemsSource = PartsList;
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

        //Select Image
        private void EditImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                OpenFileDialog open = new OpenFileDialog
                {
                    // Set filter for file extension and default file extension 
                    DefaultExt = ".png",
                    Filter = "Image Files(*.jpg; *.jpeg; *.png;)|*.jpg; *.png; *.jpeg;"
                };

                // Display OpenFileDialog by calling ShowDialog method 
                var result = open.ShowDialog();

                // Get the selected file name and display in a TextBox 
                if (result == true)
                {
                    if (Methods.CheckForInternetConnection())
                    {
                        // Open document and image file path
                        GroupPathImage = open.FileName;

                        Image.Source = new BitmapImage(new Uri(GroupPathImage));
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

        private void BtnCreate_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                    {
                        MessageBox.Show(LocalResources.label5_PleaseEnterName);
                        return;
                    }

                    if (TxtName.Text.Length < 4 && TxtName.Text.Length > 15)
                    {
                        MessageBox.Show(LocalResources.label5_ErrorLengthGroupName);
                        return;
                    }

                    if (GroupPathImage == SelectedChatData.Avatar)
                        GroupPathImage = "";

                    //Show a progress
                    ProgressDialogResult dialogResult = ProgressDialog.Execute(this, "Loading...", -1, (worker, args) =>
                    {
                        Dispatcher?.Invoke((Action)async delegate // <--- HERE
                        {
                            try
                            {
                                UsersIds = "";

                                var tester = PartsList.Where(s => s.Selected).ToList();
                                foreach (var user in tester)
                                {
                                    UsersIds += user.UserId + ",";
                                }

                                UsersIds = UsersIds.Remove(UsersIds.Length - 1, 1);

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.GroupChat.AddOrRemoveUserToGroupAsync(GroupId, UsersIds, "add_user") });
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }

                            try
                            { 
                                var (apiStatus, respond) = await RequestsAsync.GroupChat.EditGroupChatAsync(GroupId, TxtName.Text, GroupPathImage);
                                if (apiStatus == 200)
                                {
                                    if (respond is CreateGroupChatObject result)
                                    {
                                        var data = ListUtils.ListGroup.FirstOrDefault(a => a.GroupId == GroupId);
                                        if (data != null)
                                        {
                                            var index = ListUtils.ListGroup.IndexOf(data);
                                            if (index > -1)
                                            {
                                                SelectedChatData = WoWonderTools.UserChatFilter(result.Data.FirstOrDefault());
                                                ListUtils.ListGroup[index] = SelectedChatData;
                                            }
                                        }
                                        
                                        ProgressDialog.ExecuteDismiss();
                                        Close();
                                    }
                                }
                                else
                                {
                                    //Show a Error image with a message 
                                    ProgressDialog.ExecuteDismiss();

                                    if (respond.Message != null) MessageBox.Show(respond.Message);
                                }
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                                ProgressDialog.ExecuteDismiss();
                            }
                        });
                    });
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

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    if (e.OriginalSource is Button originalSource)
                    {
                        var userId = originalSource.CommandParameter.ToString();

                        var result = MessageBox.Show(LocalResources.label5_AreYouSureDeleteThisUser, "", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                // User pressed Yes
                                try
                                {
                                    var item = PartsList.FirstOrDefault(a => a.UserId == userId);
                                    if (item != null)
                                    {
                                        PartsList.Remove(item);
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.GroupChat.AddOrRemoveUserToGroupAsync(GroupId, userId, "remove_user") });

                                        if (userId == UserDetails.UserId)
                                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.GroupChat.ExitGroupChatAsync(GroupId) });
                                        
                                        var view = CollectionViewSource.GetDefaultView(PartsList);
                                        view.Refresh();
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

        private void BtnAddParticipant_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectMembersWindow membersWindow = new SelectMembersWindow("EditGroup", PartsList?.ToList());
                membersWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public void GetResultSelectMembers()
        {
            try
            {
                if (ListUtils.SelectMembersList.Count > 0)
                {
                    foreach (var item in from item in ListUtils.SelectMembersList let check = PartsList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                    {
                        Dispatcher?.Invoke((Action)delegate // <--- HERE
                        {
                            try
                            {
                                PartsList.Add(item);
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                    } 
                }

                ShowEmptyPage();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}