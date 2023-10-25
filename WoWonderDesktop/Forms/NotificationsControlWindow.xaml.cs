using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WoWonderClient.Requests;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for NotificationsControlWindow.xaml
    /// </summary>
    public partial class NotificationsControlWindow : Window
    {
        private readonly UserListFody DataUser;

        public NotificationsControlWindow(UserListFody data)
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_NotificationsControl ;

                DataUser = data;
                GetNotificationSettings();

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }
                ModeDark_Window();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        // Functions Get  Notification Settings User
        private void GetNotificationSettings()
        {
            try
            {
                if (DataUser != null)
                { 
                    var check = WoWonderTools.CheckMute(DataUser.ChatId, DataUser.ChatType, DataUser.Mute);
                    if (check)
                    {
                        Chk_Receive_notifications.IsChecked = false;
                    }
                    else
                    {
                        Chk_Receive_notifications.IsChecked = true;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click Button Close 
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
                        Background = new SolidColorBrush(darkBackgroundColor);

                        Border.Background = new SolidColorBrush(darkBackgroundColor);

                        Lbl_Conversation_Notifications.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        Lbl_Control_your_Notifications.Foreground = new SolidColorBrush(whiteBackgroundColor);

                        Lbl_Message_Notifications.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        Chk_Receive_notifications.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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

        private void Chk_Receive_notifications_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
               var isMute = Chk_Receive_notifications.IsChecked != null && Chk_Receive_notifications.IsChecked.Value;
               var idChat = DataUser.ChatId;
               var globalMute = DataUser.Mute;
                 
               var checkUser = ListUtils.ListUsers.FirstOrDefault(a => a.UserId == DataUser.UserId);
               var muteObject = new Classes.OptionLastChat
                {
                    ChatType = "user",
                    ChatId = DataUser.ChatId,
                    UserId = DataUser.UserId,
                    GroupId = "",
                    PageId = "",
                    Name = DataUser.Name
                };

                if (checkUser != null)
                {
                    checkUser.IsMute = isMute;
                    checkUser.Mute.Notify = isMute ? "no" : "yes";
                    globalMute = checkUser.Mute; 
                }
                 
                if (isMute)
                {
                    if (muteObject != null)
                    {
                        ListUtils.MuteList.Add(muteObject);

                        var sqLiteDatabase = new SqLiteDatabase();
                        sqLiteDatabase.InsertORDelete_Mute(muteObject);
                    }

                    var dictionary = new Dictionary<string, string>
                    {
                        {"notify", "no"},
                    };

                    if (globalMute != null)
                    {
                        dictionary.Add("call_chat", globalMute.CallChat);
                        dictionary.Add("archive", globalMute.Archive);
                        dictionary.Add("pin", globalMute.Pin);
                    }

                    if (Methods.CheckForInternetConnection())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.MuteChatsInfoAsync(idChat, "user", dictionary) });

                    //ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_AddedMute), ToastLength.Long);
                }
                else
                {
                    var checkMute = ListUtils.MuteList.FirstOrDefault(a => muteObject != null && a.ChatId == muteObject.ChatId && a.ChatType == muteObject.ChatType);
                    if (checkMute != null)
                    {
                        ListUtils.MuteList.Remove(checkMute);

                        var sqLiteDatabase = new SqLiteDatabase();
                        sqLiteDatabase.InsertORDelete_Mute(checkMute);
                    }

                    var dictionary = new Dictionary<string, string>
                    {
                        {"notify", "yes"},
                    };

                    if (globalMute != null)
                    {
                        dictionary.Add("call_chat", globalMute.CallChat);
                        dictionary.Add("archive", globalMute.Archive);
                        dictionary.Add("pin", globalMute.Pin);
                    }

                    if (Methods.CheckForInternetConnection())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.MuteChatsInfoAsync(idChat, "user", dictionary) });

                    //ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_RemovedMute), ToastLength.Long);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}