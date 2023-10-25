using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WoWonderClient.Requests;
using WoWonderDesktop.Forms;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using WoWonderDesktop.WindowsPages;

namespace WoWonderDesktop.Controls
{
    /// <summary>
    /// Interaction logic for MoreOptionsPopup.xaml
    /// </summary>
    public partial class MoreOptionsPopup : UserControl
    {
        public MoreOptionsPopup()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button b)
                {
                    var text = b.Content?.ToString();
                    if (text == LocalResources.label_Refresh)
                    {
                        ChatWindow.ChatWindowContext.ProgressBarLastChat.Visibility = Visibility.Visible;
                        ChatWindow.ChatWindowContext.ProgressBarLastChat.IsIndeterminate = true;

                        ListUtils.ListUsers.Clear();
                        ViewModel.ModelInstance?.ClearChats();

                        SqLiteDatabase database = new SqLiteDatabase();
                        database.ClearAll_LastUsersChat();
                        database.ClearAll_Messages();

                        if (Methods.CheckForInternetConnection())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetPinChats, ApiRequest.GetArchivedChats, ChatWindow.ChatWindowContext.LoadChatAsync });

                    }  
                    else if (text == LocalResources.label_Settings)
                    {
                        SettingsWindows settingsWindows = new SettingsWindows();
                        settingsWindows.ShowDialog();
                    }
                    
                    //Chat window
                    else if (text == LocalResources.label5_InfoGroup)
                    {
                        if (ChatWindow.ChatWindowContext?.SelectedChatData?.Owner != null && ChatWindow.ChatWindowContext.SelectedChatData.Owner.Value)
                        {
                            EditGroupChatWindow editGroupChatWindow = new EditGroupChatWindow(ChatWindow.ChatWindowContext.SelectedChatData);
                            editGroupChatWindow.ShowDialog();
                        }
                        else
                        {
                            GroupChatProfileWindow chatProfileWindow = new GroupChatProfileWindow(ChatWindow.ChatWindowContext?.SelectedChatData);
                            chatProfileWindow.ShowDialog();
                        }
                    }
                    else if (text == LocalResources.label5_AllMedia)
                    {
                        SharedFilesWindow sharedFileForm = new SharedFilesWindow(ChatWindow.ChatWindowContext.SelectedChatData);
                        sharedFileForm.ShowDialog();
                    }
                    else if (text == LocalResources.label5_ChangeWallpaper)
                    {
                        ChooseWallpaperWindow wallpaperWindow = new ChooseWallpaperWindow();
                        wallpaperWindow.ShowDialog(); 
                    }
                    else if (text == LocalResources.label5_StartedMessages)
                    {
                        StartedMessagesWindow startedMessagesWindow = new StartedMessagesWindow();
                        startedMessagesWindow.ShowDialog();
                    }
                    else if (text == LocalResources.label5_Block)
                    {
                        OnMenuBlock_Click();
                    }
                    else if (text == LocalResources.label5_ClearChat)
                    {
                        OnMenuClearChat_Click();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void OnMenuBlock_Click()
        {
            try
            {
                var result = MessageBox.Show(LocalResources.label5_AreYouSureBlockUser, LocalResources.label5_Block, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // User pressed Yes 

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(ChatWindow.ChatWindowContext.SelectedChatData, "Delete");

                        if (Methods.CheckForInternetConnection())
                        {
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.BlockUserAsync(ChatWindow.UserId, true) });
                        }

                        MessageBox.Show(LocalResources.label_blocked, LocalResources.label5_Block, MessageBoxButton.OK, MessageBoxImage.Exclamation);

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

        private void OnMenuClearChat_Click()
        {
            try
            {
                var result = MessageBox.Show(LocalResources.label5_AreYouSureDeleteMessages, LocalResources.label5_ClearChat, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // User pressed Yes
                        ListUtils.ListMessages.Clear();

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.DeleteAllMessagesUser(UserDetails.UserId, ChatWindow.UserId);

                        ViewModel.ModelInstance.UpdateMessage(false);

                        if (Methods.CheckForInternetConnection())
                        {
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.DeleteConversationAsync(ChatWindow.UserId) });
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
    }
}
