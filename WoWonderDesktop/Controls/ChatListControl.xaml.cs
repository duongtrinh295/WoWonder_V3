using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WoWonderClient.Requests;
using WoWonderDesktop.Forms;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;

namespace WoWonderDesktop.Controls
{
    /// <summary>
    /// Interaction logic for ChatListControl.xaml
    /// </summary>
    public partial class ChatListControl : UserControl
    {
        public ChatListControl()
        {
            InitializeComponent();
        }

        private void ImageProfileButton_OnTap(object sender, RoutedEventArgs e)
        {
            try
            {
                RoundProfileButton mi = (RoundProfileButton)sender;
                var userId = mi.Tag?.ToString();

                var selectedGroup = ListUtils.ListUsers.FirstOrDefault(a => a.UserId == userId);
                if (selectedGroup != null)
                {
                    if (selectedGroup.UserId == UserDetails.UserId)
                    {

                    }
                    else
                    {
                        ViewModel.ModelInstance.GetSelectedChatCommand.Execute(selectedGroup);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Menu Chat

        private void DropDownMenuChat_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems?.Count > 0 && e.AddedItems[0] is string item)
                {
                    var Source = (ListBox)e.Source;
                    UserListFody selectedGroup = (UserListFody)Source.DataContext;
                    if (selectedGroup != null)
                    {
                        if (item == LocalResources.label5_Archive || item == LocalResources.label5_UnArchive)
                        {
                            var isArchive = !selectedGroup.IsArchive;
                            selectedGroup.IsArchive = isArchive;
                           
                            if (isArchive)
                            {
                                ViewModel.ModelInstance.ArchiveChatCommand.Execute(selectedGroup);
                            }
                            else
                            {
                                ViewModel.ModelInstance.UnArchiveChatCommand.Execute(selectedGroup);
                            }
                        }
                        else if (item == LocalResources.label_Btn_delete_chat)
                        {
                            if (Methods.CheckForInternetConnection())
                            {
                               ChatWindow.ChatWindowContext.DeleteMessages_Async(selectedGroup.UserId);
                            }
                            else
                            {
                                MessageBox.Show(LocalResources.label_Please_check_your_internet);
                            }
                        }
                        else if (item == LocalResources.label5_Pin || item == LocalResources.label5_UnPin)
                        {
                            var isPin = !selectedGroup.IsPin;
                            if (isPin)
                            {
                                ViewModel.ModelInstance.PinChatCommand.Execute(selectedGroup);
                            }
                            else
                            {
                                ViewModel.ModelInstance.UnPinChatCommand.Execute(selectedGroup);
                            }
                        }
                        else if (item == LocalResources.label5_MuteNotification || item == LocalResources.label5_UnMuteNotification)
                        {

                            NotificationsControlWindow notificationsControlWindow = new NotificationsControlWindow(selectedGroup);
                            notificationsControlWindow.Show();
                        }
                        else if (item == LocalResources.label_Btn_Block_User)
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
                        else if (item == LocalResources.label_Video_call)
                        {
                            SendVideoCall sendCall = new SendVideoCall(selectedGroup.Name, ChatWindow.UserId, selectedGroup.Avatar, ChatWindow.ChatWindowContext);
                            sendCall.ShowDialog();
                        }
                        else if (item == LocalResources.label_ProfileViewLinkButton)
                        {
                            if (Methods.CheckForInternetConnection())
                            {
                                var p = new Process
                                {
                                    StartInfo = new ProcessStartInfo(selectedGroup.Url) { UseShellExecute = true }
                                };
                                p.Start();
                            }
                            else
                            {
                                MessageBox.Show(LocalResources.label_Please_check_your_internet);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DropDownMenuChat_OnMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var Source = (ListBox)e.Source;
                Source.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        #endregion

    }
}
