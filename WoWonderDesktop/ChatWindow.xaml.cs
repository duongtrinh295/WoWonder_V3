using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.WinUI.Notifications;
using MaterialDesignThemes.Wpf;
using WoWonderClient;
using WoWonderClient.Classes.Event;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.GroupChat;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using WoWonderDesktop.Controls;
using WoWonderDesktop.Forms;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.Library;
using WoWonderDesktop.SocketSystem;
using WoWonderDesktop.SQLiteDB;
using WoWonderDesktop.WindowsPages;
using WpfAnimatedGif;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window, AnjoListBoxScrollListener.IListBoxOnScrollListener
    {
        #region Variables

        public static ChatWindow ChatWindowContext;
        public static bool ApiRun, RunCall;
        public static string TypeChatPage, ChatId, UserId, GroupId, ReplyId, SizeFile;
        public string ChatColor, LastSeen, TaskWork = "Working";
        private string SearchGifsKey = "", SearchStickerKey = "", EmojiTabStop = "yes", SearchKey;
        public UserListFody SelectedChatData;

        private readonly AnjoListBoxScrollListener SearchUserBoxScrollListener;

        #endregion

        #region Timer And MediaPlayer

        private Timer DispatcherTimer;
        private Timer TimerMessages;

        #endregion

        public ChatWindow()
        {
            try
            {
                InitializeComponent();
                ChatWindowContext = this;

                Title = Settings.ApplicationName;

                if (Settings.FlowDirectionRightToLeft)
                    FlowDirection = FlowDirection.RightToLeft;

                SearchUserBoxScrollListener = new AnjoListBoxScrollListener(SearchUserList);
                SearchUserBoxScrollListener.SetScrollListener(this);

                BindData();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region General

        private void BindData()
        {
            try
            {
                if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                {
                    if (UserDetails.Socket == null)
                    {
                        UserDetails.Socket = new WoSocketHandler();
                        UserDetails.Socket?.InitStart();

                        //Connect to socket with access token
                        UserDetails.Socket?.Emit_Join(UserDetails.Username, UserDetails.AccessToken);
                    }
                }

                AllowDrop = true;
                DragEnter += new DragEventHandler(WinMin_DragEnter);
                Drop += new DragEventHandler(WinMin_Drop);

                Task.Factory.StartNew(GetGeneralAppData).ConfigureAwait(false);

                SqLiteDatabase database = new SqLiteDatabase();
                ListUtils.ListUsers = new ObservableCollection<UserListFody>(database.Get_LastUsersChat_List());

                var list = ListUtils.ListUsers.OrderByDescending(o => o.ChatTime).ToList();
                ListUtils.ListGroup = new ObservableCollection<UserListFody>(list.Where(o => o.ChatType == "group").ToList());
                //var listPin = list.Where(o => o.IsPin).ToList();
                list.RemoveAll(s => s.ChatType == "group");
                //list.RemoveAll(s => s.IsPin);
                //list.InsertRange(0, listPin);
                ListUtils.ListUsers = new ObservableCollection<UserListFody>(list);

                if (ListUtils.ListUsers.Count > 0)
                    ViewModel.ModelInstance?.LoadChats(ListUtils.ListUsers);

                if (ListUtils.ListGroup?.Count > 0)
                    GroupsList.ItemsSource = ListUtils.ListGroup;

                //ChatActivity
                if (Methods.CheckForInternetConnection())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadChatAsync, LoadSuggestionsAsync });

                //DoubleClick >> Message List Box
                //ListUtils.AddDoubleClickEventStyle(ChatMessgaeslistBox, new MouseButtonEventHandler(MouseButtonEventHandler)); 

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetGeneralAppData()
        {
            try
            {
                //Completed background worker : Users Contact
                SqLiteDatabase database = new SqLiteDatabase();

                ListUtils.ListUsersProfile = database.Get_MyContact();

                var dataUser = database.Get_MyProfile();
                var dataSettings = database.GetSettings();

                if (Methods.CheckForInternetConnection())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(UserDetails.UserId), ApiRequest.GetSettings_Api });

                var dataSettingsApp = database.GetSettingsApp();

                if (Settings.VideoCall)
                    ListUtils.ListCall = database.get_data_CallVideo();

                if (Settings.EnableChatMute)
                    ListUtils.MuteList = database.Get_MuteList();

                if (Settings.EnableChatPin)
                    ListUtils.PinList = database.Get_PinList();

                if (Settings.EnableChatArchive)
                    ListUtils.ArchiveList = database.Get_ArchiveList();

                if (Methods.CheckForInternetConnection())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetPinChats, ApiRequest.GetArchivedChats, TrendingGifAsync, GetEvent });

                ContactUserControl.Instance?.LoadUser();

                GetStickers();

                Dispatcher?.Invoke((Action)delegate // <--- HERE
                {
                    try
                    {
                        if (dataUser != null)
                        {
                            ImageProfileButton.ProfileImageSource = new BitmapImage(new Uri(dataUser.Avatar));
                        }

                        GetSettings(dataSettingsApp);

                        if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        {
                            if (UserDetails.Socket == null)
                            {
                                UserDetails.Socket = new WoSocketHandler();
                                UserDetails.Socket?.InitStart();
                            }

                            //Connect to socket with access token
                            if (!WoSocketHandler.IsJoined)
                                UserDetails.Socket?.Emit_Join(UserDetails.Username, UserDetails.AccessToken);
                        }
                        else
                        {
                            // Timer Control
                            DispatcherTimer = new Timer { Interval = Settings.RefreshChatActivitiesPer };
                            DispatcherTimer.Elapsed += ChatActitvity_Timer;
                            DispatcherTimer.Enabled = true;
                            DispatcherTimer.Start();
                        }

                        // timer messages
                        if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.RestApi)
                        {
                            TimerMessages = new Timer { Interval = Settings.UpdateMessageReceiverInt };
                            TimerMessages.Elapsed += TimerMessagesOnTick;
                            TimerMessages.Enabled = true;
                            TimerMessages.Start();
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

        public void GetSettings(DataTables.SettingsAppTb dataSettings)
        {
            try
            {
                dataSettings ??= new DataTables.SettingsAppTb();

                if (string.IsNullOrEmpty(dataSettings.DarkMode))
                    dataSettings.DarkMode = UserDetails.ModeDarkStlye.ToString();
                else
                    UserDetails.ModeDarkStlye = dataSettings.DarkMode == "true";

                if (string.IsNullOrEmpty(dataSettings.NotificationDesktop))
                    dataSettings.NotificationDesktop = "true";

                if (string.IsNullOrEmpty(dataSettings.NotificationPlaysound))
                    dataSettings.NotificationPlaysound = "true";

                ListUtils.SettingsApp = dataSettings;

                if (!string.IsNullOrEmpty(ListUtils.SettingsApp.BackgroundChatsImages))
                {
                    if (ListUtils.SettingsApp.BackgroundChatsImages.Contains("#"))
                    {
                        Contents_Grid.Background = new SolidColorBrush((Color)ConvertFromString(ListUtils.SettingsApp.BackgroundChatsImages));
                    }
                    else
                    {
                        ImageBrush myBrush = new ImageBrush();
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(ListUtils.SettingsApp.BackgroundChatsImages))
                        };
                        myBrush.ImageSource = image.Source;
                        Contents_Grid.Background = myBrush;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Last Chat

        public async Task LoadChatAsync()
        {
            try
            {
                if (ApiRun)
                    return;

                ApiRun = true;

                var fetch = "users";
                if (Settings.EnableChatGroup)
                    fetch += ",groups";

                if (Settings.EnableChatPage)
                    fetch += ",pages";

                var (apiStatus, respond) = await RequestsAsync.Message.GetChatAsync(fetch, "", "0", "35", "0", "35", "0", "35");
                if (apiStatus != 200 || respond is not LastChatObject result || result.Data == null)
                {
                    ApiRun = false;
                    Methods.DisplayReportResult(respond);
                }
                else
                {
                    LoadCall(result);
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        var countList = ListUtils.ListUsers?.Count;

                        if (countList > 0)
                        {
                            foreach (var itemChatObject in result.Data)
                            {
                                UserListFody item = WoWonderTools.UserChatFilter(itemChatObject);

                                var checkUser = ListUtils.ListUsers?.FirstOrDefault(a => a.ChatId == item.ChatId && a.ChatType == item.ChatType);

                                var archive = WoWonderTools.CheckArchive(item.ChatId, item.ChatType, item.Mute);
                                Classes.LastChatArchive archiveObject = archive.Item1;
                                item.IsArchive = archive.Item2;

                                if (item.IsPin || item.IsArchive)
                                    continue;

                                int index = -1;
                                if (checkUser != null)
                                    index = ListUtils.ListUsers.IndexOf(checkUser);

                                if (checkUser == null)
                                {
                                    Dispatcher?.Invoke((Action)delegate // <--- HERE
                                    {
                                        try
                                        {
                                            if (!item.IsArchive)
                                            {
                                                Console.WriteLine(item.Name);

                                                var checkPin = ListUtils.ListUsers?.LastOrDefault(o => o.IsPin);
                                                if (checkPin != null)
                                                {
                                                    var toIndex = ListUtils.ListUsers.IndexOf(checkPin) + 1;

                                                    ListUtils.ListUsers?.Insert(toIndex, item);
                                                }
                                                else
                                                {
                                                    ListUtils.ListUsers?.Insert(0, item);
                                                }
                                            }
                                            else
                                            {
                                                if (archiveObject != null)
                                                {
                                                    if (archiveObject.LastChat.LastMessage.LastMessageClass?.Id != item.LastMessage.LastMessageClass?.Id)
                                                    {
                                                        var checkPin = ListUtils.ListUsers?.LastOrDefault(o => o.IsPin);
                                                        if (checkPin != null)
                                                        {
                                                            var toIndex = ListUtils.ListUsers.IndexOf(checkPin) + 1;

                                                            ListUtils.ListUsers?.Insert(toIndex, item);
                                                        }
                                                        else
                                                        {
                                                            ListUtils.ListUsers?.Insert(0, item);
                                                        }

                                                        ListUtils.ArchiveList.Remove(archiveObject);

                                                        var sqLiteDatabase = new SqLiteDatabase();
                                                        sqLiteDatabase.InsertORDelete_Archive(archiveObject);
                                                    }
                                                }
                                            }

                                            if (item.LastMessage.LastMessageClass?.FromId != UserDetails.UserId && !item.IsMute)
                                            {
                                                if (WindowState == WindowState.Minimized)
                                                {
                                                    //switch (item.ChatType)
                                                    //{
                                                    //    case "user":
                                                    //        floating.Name = item.Name;
                                                    //        break;
                                                    //    case "page":
                                                    //        var userAdminPage = item.UserId;
                                                    //        if (userAdminPage == item.LastMessage.LastMessageClass?.ToData?.UserId)
                                                    //        {
                                                    //            floating.Name = item.LastMessage.LastMessageClass?.UserData.Name + "(" + item.PageName + ")";
                                                    //        }
                                                    //        else
                                                    //        {
                                                    //            floating.Name = item.LastMessage.LastMessageClass?.ToData?.Name + "(" + item.PageName + ")";
                                                    //        }
                                                    //        break;
                                                    //    case "group":
                                                    //        floating.Name = item.GroupName;
                                                    //        break;
                                                    //}

                                                    MsgPopupWindow popUp = new MsgPopupWindow(this, item.LastMessage.LastMessageClass.Text, item.Username, item.Avatar, item.UserId, item);
                                                    popUp.Activate();
                                                    popUp.Show();
                                                    popUp.Activate();

                                                    var workingAreaaa = SystemParameters.WorkArea;
                                                    var transform = PresentationSource.FromVisual(popUp).CompositionTarget.TransformFromDevice;
                                                    var corner = transform.Transform(new Point(workingAreaaa.Right, workingAreaaa.Bottom));

                                                    popUp.Left = corner.X - popUp.ActualWidth - 10;
                                                    popUp.Top = corner.Y - popUp.ActualHeight;

                                                    // create Notifications 
                                                    new ToastContentBuilder()
                                                        .AddArgument("action", "viewConversation")
                                                        .AddArgument("conversationId", !string.IsNullOrEmpty(item.ChatId) ? item.ChatId : item.UserId)
                                                        .AddText(item.Username)
                                                        .AddText(item.LastMessage.LastMessageClass.Text)
                                                        .AddAppLogoOverride(new Uri(item.Avatar, UriKind.Absolute), ToastGenericAppLogoCrop.Circle)
                                                        .Show();
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                                else
                                {
                                    Dispatcher?.Invoke((Action)delegate // <--- HERE
                                    {
                                        try
                                        {
                                            checkUser.LastseenUnixTime = item.LastseenUnixTime;
                                            checkUser.ChatTime = item.ChatTime;
                                            checkUser.Time = item.Time;

                                            if (item.LastMessage.LastMessageClass == null)
                                                return;

                                            if (checkUser.LastMessage.LastMessageClass.Text != item.LastMessage.LastMessageClass.Text || checkUser.LastMessage.LastMessageClass.Media != item.LastMessage.LastMessageClass.Media)
                                            {
                                                checkUser = item = WoWonderTools.UserChatFilter(itemChatObject);

                                                checkUser.LastMessage = item.LastMessage;
                                                checkUser.LastMessageText = checkUser.LastMessage.LastMessageClass.Text = item.LastMessage.LastMessageClass.Text;
                                                checkUser.MessageCount = item.MessageCount;

                                                if (index > 0 && checkUser.ChatType == item.ChatType)
                                                {
                                                    if (!item.IsPin)
                                                    {
                                                        var checkPin = ListUtils.ListUsers?.LastOrDefault(o => o.IsPin);
                                                        if (checkPin != null)
                                                        {
                                                            var toIndex = ListUtils.ListUsers.IndexOf(checkPin) + 1;

                                                            if (index != toIndex)
                                                            {
                                                                ListUtils.ListUsers?.Move(index, toIndex);
                                                            }

                                                            //ICollectionView view = CollectionViewSource.GetDefaultView(ListUtils.ListUsers);
                                                            //view?.Refresh();
                                                            ViewModel.ModelInstance?.UpdateLastChatAllList(checkUser);
                                                        }
                                                        else
                                                        {
                                                            if (index != 0)
                                                            {
                                                                ListUtils.ListUsers?.Move(index, 0);
                                                            }

                                                            //ICollectionView view = CollectionViewSource.GetDefaultView(ListUtils.ListUsers);
                                                            //view?.Refresh();
                                                            ViewModel.ModelInstance?.UpdateLastChatAllList(checkUser);
                                                        }
                                                    }

                                                    if (item.LastMessage.LastMessageClass.FromId != UserDetails.UserId && !item.IsMute)
                                                    {
                                                        if (WindowState == WindowState.Minimized)
                                                        {
                                                            //switch (item.ChatType)
                                                            //{
                                                            //    case "user":
                                                            //        floating.Name = item.Name;
                                                            //        break;
                                                            //    case "page":
                                                            //        var userAdminPage = item.UserId;
                                                            //        if (userAdminPage == item.LastMessage.LastMessageClass?.ToData?.UserId)
                                                            //        {
                                                            //            floating.Name = item.LastMessage.LastMessageClass?.UserData.Name + "(" + item.PageName + ")";
                                                            //        }
                                                            //        else
                                                            //        {
                                                            //            floating.Name = item.LastMessage.LastMessageClass?.ToData?.Name + "(" + item.PageName + ")";
                                                            //        }
                                                            //        break;
                                                            //    case "group":
                                                            //        floating.Name = item.GroupName;
                                                            //        break;
                                                            //}

                                                            MsgPopupWindow popUp = new MsgPopupWindow(this, item.LastMessage.LastMessageClass.Text, item.Username, item.Avatar, item.UserId, item);
                                                            popUp.Activate();
                                                            popUp.Show();
                                                            popUp.Activate();

                                                            var workingAreaaa = SystemParameters.WorkArea;
                                                            var transform = PresentationSource.FromVisual(popUp).CompositionTarget.TransformFromDevice;
                                                            var corner = transform.Transform(new Point(workingAreaaa.Right, workingAreaaa.Bottom));

                                                            popUp.Left = corner.X - popUp.ActualWidth - 10;
                                                            popUp.Top = corner.Y - popUp.ActualHeight;

                                                            // create Notifications 
                                                            new ToastContentBuilder()
                                                                .AddArgument("action", "viewConversation")
                                                                .AddArgument("conversationId", !string.IsNullOrEmpty(item.ChatId) ? item.ChatId : item.UserId)
                                                                .AddText(item.Username)
                                                                .AddText(item.LastMessage.LastMessageClass.Text)
                                                                .AddAppLogoOverride(new Uri(item.Avatar, UriKind.Absolute), ToastGenericAppLogoCrop.Circle)
                                                                .Show();
                                                        }
                                                    }
                                                }
                                                else if (index == 0 && checkUser.ChatType == item.ChatType)
                                                {
                                                    //ICollectionView view = CollectionViewSource.GetDefaultView(ListUtils.ListUsers);
                                                    //view?.Refresh();
                                                    ViewModel.ModelInstance?.UpdateLastChatAllList(checkUser);
                                                }
                                            }

                                            if (checkUser.LastseenStatus?.ToLower() != item.LastseenStatus?.ToLower())
                                            {
                                                checkUser.LastseenStatus = item.LastseenStatus;
                                                checkUser.Showlastseen = item.Showlastseen;
                                                checkUser.LastSeenTimeText = item.LastSeenTimeText;

                                                if (index > -1 && checkUser.ChatType == item.ChatType)
                                                {
                                                    //ICollectionView view = CollectionViewSource.GetDefaultView(ListUtils.ListUsers);
                                                    //view?.Refresh();
                                                    ViewModel.ModelInstance?.UpdateLastChatAllList(checkUser);
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                            }
                        }
                        else
                        {
                            foreach (var itemChatObject in result.Data.Where(chatObject => !chatObject.IsArchive && chatObject.Mute?.Archive == "no"))
                            {
                                UserListFody item = WoWonderTools.UserChatFilter(itemChatObject);

                                Console.WriteLine(item.Name);

                                var archive = WoWonderTools.CheckArchive(item.ChatId, item.ChatType, item.Mute);
                                Classes.LastChatArchive archiveObject = archive.Item1;
                                item.IsArchive = archive.Item2;

                                if (item.IsPin || item.IsArchive)
                                    continue;

                                Dispatcher?.Invoke((Action)delegate // <--- HERE
                                {
                                    try
                                    {
                                        if (item.IsPin)
                                        {
                                            ListUtils.ListUsers?.Insert(0, item);
                                        }
                                        else
                                        {
                                            ListUtils.ListUsers?.Add(item);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }

                        Dispatcher?.Invoke((Action)delegate // <--- HERE
                        {
                            try
                            {
                                var list = ListUtils.ListUsers.OrderByDescending(o => o.ChatTime).ToList();
                                ListUtils.ListGroup = new ObservableCollection<UserListFody>(list.Where(o => o.ChatType == "group").ToList());
                                //var listPin = list.Where(o => o.IsPin).ToList();
                                list.RemoveAll(s => s.ChatType == "group");
                                //list.RemoveAll(s => s.IsPin);
                                //list.RemoveAll(s => s.IsArchive);
                                //list.InsertRange(0, listPin);
                                ListUtils.ListUsers = new ObservableCollection<UserListFody>(list);

                                if (ListUtils.ListUsers.Count > 0)
                                    ViewModel.ModelInstance?.LoadChats(ListUtils.ListUsers);

                                //ICollectionView view = CollectionViewSource.GetDefaultView(ListUtils.ListUsers);
                                //view?.Refresh(); 

                                // Insert data user in database
                                SqLiteDatabase database = new SqLiteDatabase();
                                database.Insert_Or_Update_LastUsersChat(ListUtils.ListUsers);

                                if (ListUtils.ListGroup?.Count > 0)
                                    GroupsList.ItemsSource = ListUtils.ListGroup;

                                ProgressBarLastChat.Visibility = Visibility.Collapsed;
                                ProgressBarLastChat.IsIndeterminate = false;
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });

                        ApiRun = false;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                ApiRun = false;
            }
        }

        private void ChatActitvity_Timer(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadChatAsync });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LoadCall(dynamic respond)
        {
            try
            {
                if (respond == null || !Settings.VideoCall || RunCall /*|| VideoAudioComingCallActivity.IsActive*/)
                    return;

                //string typeCalling = "";
                CallUserObject callUser = null!;

                switch (respond)
                {
                    case LastChatObject chatObject:
                        var twilioVideoCall = chatObject.VideoCall ?? false;
                        var twilioAudioCall = chatObject.AudioCall ?? false;

                        if (twilioVideoCall)
                        {
                            //typeCalling = "Twilio_video_call";
                            callUser = chatObject.VideoCallUser?.CallUserClass;
                        }
                        else if (twilioAudioCall)
                        {
                            //typeCalling = "Twilio_audio_call";
                            callUser = chatObject.AudioCallUser?.CallUserClass;
                        }

                        break;
                }

                Dispatcher?.Invoke((Action)delegate // <--- HERE
                {
                    if (callUser != null)
                    {
                        RunCall = true;

                        string userId = callUser.UserId;
                        string avatar = callUser.Avatar;
                        string name = callUser.Name;
                        string callId = callUser.Data?.Id;

                        RecieveCallWindow openReceiver = new RecieveCallWindow(userId, avatar, name, callId, this) { Left = -550 };
                        openReceiver.ShowDialog();
                    }
                    else
                    {
                        //if (VideoAudioComingCallActivity.IsActive)
                        //    VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

                RunCall = false;

                //if (VideoAudioComingCallActivity.IsActive)
                //    VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio(); 
            }
        }

        public async Task OpenChat(UserListFody selectedGroup, string type)
        {
            try
            {
                if (selectedGroup == null)
                    return;

                UserId = selectedGroup.UserId;
                ChatId = selectedGroup.ChatId;
                TypeChatPage = type;

                SelectedChatData = selectedGroup;

                ListUtils.ListMessages.Clear();
                ListUtils.LastSharedFiles.Clear();
                ListUtils.ListSharedFiles.Clear();

                LoadingMessagesPanel.Visibility = Visibility.Visible;

                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ////Hide NoMessagePanel when it is clicking
                        //NoMessagePanel.Visibility = Visibility.Hidden;

                        if (type == "user")
                        {
                            if (Methods.CheckForInternetConnection())
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetFavoriteMessageApi, GetPinMessageApi });

                            var time = selectedGroup.ChatTime ?? selectedGroup.Time;
                            bool success = int.TryParse(time, out var number);
                            LastSeen = success ? LocalResources.label_last_seen + " " + Methods.Time.TimeAgo(number, false) : LocalResources.label_last_seen + " " + time;

                            //get profile user */ When the conversation is selected, the data moves to the profile \*
                            ContactInfoControl.ControlInstance.LoadDataUserProfile(selectedGroup);
                        }
                        else
                        {
                            if (selectedGroup.Parts != null)
                                LastSeen = Methods.FunString.FormatPriceValue(selectedGroup.Parts.Count) + " " + "people join this";
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }).ConfigureAwait(false);

                //MessageProgressBar.Visibility = Visibility.Visible;
                //MessageProgressBar.IsIndeterminate = true;

                if (!WoSocketHandler.IsJoined && Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                    UserDetails.Socket?.Emit_Join(UserDetails.Username, UserDetails.AccessToken);

                Methods.Path.Chack_MyFolder(type == "user" ? selectedGroup.UserId : selectedGroup.GroupId);

                Dispatcher?.Invoke((Action)delegate // <--- HERE
                {
                    try
                    {
                        if (Settings.VideoCall && type == "user")
                        {
                            var dataSettings = ListUtils.SettingsSiteList;
                            if (dataSettings?.WhoCall == "pro") //just pro user can chat 
                            {
                                var dataUser = ListUtils.MyProfileList?.FirstOrDefault()?.IsPro;
                                if (dataUser == "0") // Not Pro remove call
                                {
                                    VideoCallButton.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    VideoCallButton.Visibility = Visibility.Visible;
                                }
                            }
                            else //all users can chat
                            {
                                if (dataSettings?.VideoChat == "0" || !Settings.VideoCall)
                                {
                                    VideoCallButton.Visibility = Visibility.Collapsed;
                                    Settings.VideoCall = false;
                                }
                                else
                                {
                                    VideoCallButton.Visibility = Visibility.Visible;
                                }
                            }
                        }
                        else
                        {
                            VideoCallButton.Visibility = Visibility.Collapsed;
                        }

                        if (ConversationControl.ControlInstance.MediaPlayer.CanPause)
                        {
                            ConversationControl.ControlInstance.MediaPlayer.Pause();
                            ConversationControl.ControlInstance.Timer.Stop();
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                selectedGroup.MessageCountVisibility = Visibility.Collapsed;
                selectedGroup.MessageCount = "0";
                //BtnLoadmoreMessages.Content = LocalResources.label_Load_More_Messages;

                //View data and Colors in Chat Messgaes listBox
                ChatColor = selectedGroup.ChatColor ?? Settings.MainColor;

                //========== View data in Chat Messgaes listBox ===========
                //Hide NoMessagePanel when it is clicking
                //NoMessagePanel.Visibility = Visibility.Collapsed;

                if (type == "user")
                {
                    SqLiteDatabase database = new SqLiteDatabase();
                    var messagesList = database.GetMessages_List(UserDetails.UserId, UserId, 0);
                    if (messagesList?.Count > 0) //database.. Get Messages
                    {
                        ListUtils.ListMessages = messagesList;

                        if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        {
                            TaskWork = "Working";
                            MessageUpdater();
                            UserDetails.Socket?.EmitAsync_SendSeenMessages(UserId, UserDetails.AccessToken, UserDetails.UserId);
                        }
                        else
                        {
                            if (TimerMessages != null)
                            {
                                TimerMessages.Start();
                            }
                        }

                        Dispatcher?.Invoke((Action)delegate { LoadingMessagesPanel.Visibility = Visibility.Collapsed; });
                    }
                    else // or server.. Get Messages
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetMessages_Api });
                    }

                    //===================== 

                    await Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (selectedGroup.LastMessage.LastMessageClass.Seen == "0" || selectedGroup.ChatColorCircleVisibility == Visibility.Visible)
                            {
                                selectedGroup.LastMessageColor = "#7E7E7E";
                                selectedGroup.SMessageFontWeight = "Light";

                                var data = ListUtils.ListUsers.FirstOrDefault(a => a.UserId == selectedGroup.UserId);
                                if (data != null)
                                {
                                    data.LastMessageColor = "#7E7E7E";
                                    data.SMessageFontWeight = "Light";

                                    //var view = CollectionViewSource.GetDefaultView(ListUtils.ListUsers);
                                    //view.Refresh();
                                    Dispatcher?.Invoke(() => ViewModel.ModelInstance?.LoadChats(ListUtils.ListUsers));
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    }).ConfigureAwait(false);
                }
                else
                {
                    UserId = "";
                    GroupId = selectedGroup.GroupId;

                    // or server.. Get Messages Group
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetGroupMessages_Api });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //########################## User Messages ##########################

        #region User Messages

        private async Task GetMessages_Api()
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    var (apiStatus, respond) = await RequestsAsync.Message.FetchUserMessagesAsync(UserId);
                    if (apiStatus == 200)
                    {
                        if (respond is UserMessagesObject result)
                        {
                            var respondList = result.Messages.Count;
                            if (respondList > 0)
                            {
                                result.Messages.Reverse();

                                bool add = false;
                                foreach (var item in from item in result.Messages let check = ListUtils.ListMessages.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    var type = WoWonderTools.GetTypeModel(item);
                                    if (type == MessageModelType.None)
                                        continue;

                                    add = true;
                                    Dispatcher?.Invoke((Action)delegate // <--- HERE
                                    {
                                        try
                                        {
                                            ListUtils.ListMessages.Add(WoWonderTools.MessageFilter(UserId, SelectedChatData.Name, item, ChatColor));
                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                    });
                                }

                                if (add)
                                {
                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                    // Insert data user in database
                                    dbDatabase.Insert_Or_Replace_MessagesTable(ListUtils.ListMessages);
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(respond);

                    Dispatcher?.Invoke((Action)delegate { LoadingMessagesPanel.Visibility = Visibility.Collapsed; });
                }
                else MessageBox.Show(LocalResources.label_Please_check_your_internet);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async Task GetMessagesById(string id)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    var (apiStatus, respond) = await RequestsAsync.Message.FetchUserMessagesAsync(UserId, "0", "0", "1", id);
                    if (apiStatus == 200)
                    {
                        if (respond is UserMessagesObject result)
                        {
                            var countList = ListUtils.ListMessages.Count;
                            var respondList = result.Messages.Count;
                            if (respondList > 0)
                            {
                                foreach (var item in result.Messages)
                                {
                                    var type = WoWonderTools.GetTypeModel(item);
                                    if (type == MessageModelType.None)
                                        continue;

                                    var check = ListUtils.ListMessages.FirstOrDefault(a => a.Id == item.Id);
                                    if (check == null)
                                    {
                                        Dispatcher?.Invoke((Action)delegate // <--- HERE
                                        {
                                            try
                                            {
                                                ListUtils.ListMessages.Add(WoWonderTools.MessageFilter(UserId, SelectedChatData.Name, item, ChatColor));

                                                ViewModel.ModelInstance.UpdateMessage(true);
                                            }
                                            catch (Exception exception)
                                            {
                                                Methods.DisplayReportResultTrack(exception);
                                            }
                                        });
                                    }
                                    else
                                    {
                                        check = WoWonderTools.MessageFilter(UserId, SelectedChatData.Name, item, ChatColor);
                                        check.Id = item.Id;
                                        check.Seen = item.Seen;

                                        ViewModel.ModelInstance.UpdateMessage(false);
                                    }
                                }

                                if (ListUtils.ListMessages.Count > countList)
                                {
                                    SqLiteDatabase liteDatabase = new SqLiteDatabase();
                                    // Insert data user in database
                                    liteDatabase.Insert_Or_Replace_MessagesTable(ListUtils.ListMessages);
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(respond);

                    //SwipeRefreshLayout.Refreshing = false;
                    //SwipeRefreshLayout.Enabled = false;
                }
                else MessageBox.Show(LocalResources.label_Please_check_your_internet);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void MessageUpdater()
        {
            try
            {
                if (TaskWork == "Working")
                {
                    TaskWork = "Stop";

                    if (Methods.CheckForInternetConnection())
                    {
                        //var data = ListUtils.ListMessages.LastOrDefault();
                        //var lastMessageId = data?.MesData?.Id ?? "0";
                        var (apiStatus, respond) = await RequestsAsync.Message.FetchUserMessagesAsync(UserId, "0", "0", "35");
                        if (apiStatus == 200)
                        {
                            if (respond is UserMessagesObject result)
                            {
                                try
                                {
                                    var typing = result.Typing.ToString();
                                    ViewModel.ModelInstance.LastSeen = typing == "1" ? LocalResources.label_Typping : LastSeen ?? LastSeen;

                                    var isRecording = result.IsRecording.ToString();
                                    ViewModel.ModelInstance.LastSeen = isRecording == "1" ? LocalResources.label5_Recording : LastSeen ?? LastSeen;

                                    ViewModel.ModelInstance.UpdateLastSeen();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }

                                var countList = ListUtils.ListMessages.Count;
                                var respondList = result.Messages.Count;
                                if (respondList > 0)
                                {
                                    result.Messages.Reverse();

                                    foreach (var item in result.Messages)
                                    {
                                        var type = WoWonderTools.GetTypeModel(item);
                                        if (type == MessageModelType.None)
                                            continue;

                                        var check = ListUtils.ListMessages.FirstOrDefault(a => a.Id == item.Id);
                                        if (check == null)
                                        {
                                            Dispatcher?.Invoke((Action)delegate // <--- HERE
                                            {
                                                try
                                                {
                                                    var dataFody = WoWonderTools.MessageFilter(UserId, SelectedChatData.Name, item, ChatColor);

                                                    ListUtils.ListMessages.Add(dataFody);

                                                    ViewModel.ModelInstance.UpdateMessage(false);

                                                    #region Last chat

                                                    var dataUser = ListUtils.ListUsers?.FirstOrDefault(a => a.UserId == UserId);
                                                    if (dataUser != null)
                                                    {
                                                        if (dataFody.UserData != null)
                                                        {
                                                            dataUser.UserId = dataFody.UserData.UserId;
                                                            dataUser.Avatar = dataFody.UserData.Avatar;
                                                        }

                                                        dataUser.LastMessage = new LastMessageUnion
                                                        {
                                                            LastMessageClass = dataFody,
                                                        };
                                                        dataUser.LastMessage.LastMessageClass.ChatColor = ChatColor;

                                                        var index = ListUtils.ListUsers?.IndexOf(dataUser);
                                                        if (index > -1 && index != 0)
                                                        {
                                                            ListUtils.ListUsers.Move(Convert.ToInt32(index), 0);
                                                            ViewModel.ModelInstance.LoadChats(ListUtils.ListUsers);

                                                            ViewModel.ModelInstance.UpdateChatAllList(dataFody);
                                                        }
                                                    }

                                                    #endregion
                                                }
                                                catch (Exception exception)
                                                {
                                                    Methods.DisplayReportResultTrack(exception);
                                                }
                                            });

                                            //if (UserDetails.SoundControl)
                                            //    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_GetMesseges.mp3");
                                        }
                                        else if (check.FromId == UserDetails.UserId && check.Seen != item.Seen) // right
                                        {
                                            check = WoWonderTools.MessageFilter(UserId, SelectedChatData.Name, item, ChatColor);
                                            check.Id = item.Id;
                                            check.Seen = item.Seen;

                                            #region Last chat

                                            var dataUser = ListUtils.ListUsers?.FirstOrDefault(a => a.UserId == UserId);
                                            if (dataUser != null)
                                            {
                                                if (item.UserData != null)
                                                {
                                                    dataUser.UserId = item.UserData.UserId;
                                                    dataUser.Avatar = item.UserData.Avatar;
                                                }

                                                dataUser.LastMessage = new LastMessageUnion
                                                {
                                                    LastMessageClass = check,
                                                };

                                                var index = ListUtils.ListUsers?.IndexOf(dataUser);
                                                if (index > -1 && index != 0)
                                                {
                                                    ListUtils.ListUsers.Move(Convert.ToInt32(index), 0);
                                                    ViewModel.ModelInstance?.LoadChats(ListUtils.ListUsers);

                                                    ViewModel.ModelInstance?.UpdateChatAllList(check);
                                                }
                                            }

                                            #endregion

                                            Console.WriteLine(check);
                                        }
                                    }

                                    if (ListUtils.ListMessages.Count > countList)
                                    {
                                        SqLiteDatabase liteDatabase = new SqLiteDatabase();
                                        // Insert data user in database
                                        liteDatabase.Insert_Or_Replace_MessagesTable(ListUtils.ListMessages);
                                    }

                                    //if (ListUtils.ListMessages.Count > 0)
                                    //{
                                    //    SayHiLayout.Visibility = ViewStates.Gone;
                                    //    SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
                                    //}
                                    //else if (ListUtils.ListMessages.Count == 0 && ShowEmpty != "no")
                                    //{
                                    //    SayHiLayout.Visibility = ViewStates.Visible;
                                    //    SayHiSuggestionsRecycler.Visibility = ViewStates.Visible;
                                    //}
                                }
                            }
                        }
                        else Methods.DisplayReportResult(respond);
                    }

                    TaskWork = "Working";
                }
            }
            catch (Exception e)
            {
                TaskWork = "Working";
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void TimerMessagesOnTick(object sender, ElapsedEventArgs e)
        {
            try
            {
                switch (TypeChatPage)
                {
                    case "user":
                        MessageUpdater();
                        break;
                    case "group":
                        GroupMessageUpdater();
                        break;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        //########################## Load More User Messages ##########################

        #region Load More User Messages

        public async void LoadMoreUserMessages()
        {
            try
            {
                if (RunLoadMore)
                    return;

                if (TypeChatPage == "user")
                {
                    //Code get first Message id where LoadMore >>
                    var local = await LoadMore_Messages_Database();
                    if (local != "1")
                        await LoadMoreMessages_API();
                }
                else if (TypeChatPage == "group")
                {
                    await LoadMoreGroupMessages_API();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task<string> LoadMore_Messages_Database()
        {
            try
            {
                if (RunLoadMore)
                    return "1";

                RunLoadMore = true;

                var firstMessageId = ListUtils.ListMessages.FirstOrDefault()?.Id ?? 0;
                SqLiteDatabase dbDatabase = new SqLiteDatabase();

                var localList = dbDatabase.GetMessageList(UserDetails.UserId, UserId, firstMessageId);
                if (localList?.Count > 0) //Database.. Get Messages Local
                {
                    foreach (var item in from item in localList let check = ListUtils.ListMessages.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                    {
                        var type = WoWonderTools.GetTypeModel(item);
                        if (type == MessageModelType.None)
                            continue;

                        Dispatcher?.Invoke((Action)delegate // <--- HERE
                        {
                            try
                            {
                                ListUtils.ListMessages.Insert(0, WoWonderTools.MessageFilter(UserId, SelectedChatData.Name, item, ChatColor));
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                    }

                    RunLoadMore = false;
                    return "1";
                }

                RunLoadMore = false;
                return "0";
            }
            catch (Exception e)
            {
                RunLoadMore = false;
                Methods.DisplayReportResultTrack(e);
                await Task.Delay(0);
                return "0";
            }
        }

        private bool RunLoadMore;
        private async Task LoadMoreMessages_API()
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    if (RunLoadMore)
                        return;

                    RunLoadMore = true;

                    var firstMessageId = ListUtils.ListMessages.FirstOrDefault()?.Id.ToString();

                    var (apiStatus, respond) = await RequestsAsync.Message.FetchUserMessagesAsync(UserId, firstMessageId);
                    if (apiStatus == 200)
                    {
                        if (respond is UserMessagesObject result)
                        {
                            var respondList = result.Messages.Count;
                            if (respondList > 0)
                            {
                                bool add = false;
                                foreach (var item in from item in result.Messages let check = ListUtils.ListMessages.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    var type = WoWonderTools.GetTypeModel(item);
                                    if (type == MessageModelType.None)
                                        continue;

                                    Dispatcher?.Invoke((Action)delegate // <--- HERE
                                    {
                                        try
                                        {
                                            add = true;
                                            ListUtils.ListMessages.Insert(0, WoWonderTools.MessageFilter(UserId, SelectedChatData.Name, item, ChatColor));
                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                    });
                                }

                                if (add)
                                {
                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                    // Insert data user in database
                                    dbDatabase.Insert_Or_Replace_MessagesTable(ListUtils.ListMessages);
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(respond);

                    RunLoadMore = false;
                }
                else MessageBox.Show(LocalResources.label_Please_check_your_internet);
            }
            catch (Exception e)
            {
                RunLoadMore = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //########################## Load More Group Messages ##########################

        #region Load More Group Messages

        private async Task LoadMoreGroupMessages_API()
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    if (RunLoadMore)
                        return;

                    RunLoadMore = true;

                    var firstMessageId = ListUtils.ListMessages.FirstOrDefault()?.Id;

                    var (apiStatus, respond) = await RequestsAsync.GroupChat.FetchGroupChatMessagesAsync(GroupId, firstMessageId?.ToString());
                    if (apiStatus == 200)
                    {
                        if (respond is GroupMessagesObject result)
                        {
                            var respondList = result.Data.Messages.Count;
                            if (respondList > 0)
                            {
                                foreach (var item in from item in result.Data.Messages let check = ListUtils.ListMessages.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    var type = WoWonderTools.GetTypeModel(item);
                                    if (type == MessageModelType.None)
                                        continue;

                                    Dispatcher?.Invoke((Action)delegate // <--- HERE
                                    {
                                        try
                                        {
                                            ListUtils.ListMessages.Insert(0, WoWonderTools.MessageFilter(GroupId, WoWonderTools.GetNameFinal(item.UserData), item, ChatColor, "group"));
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
                    else Methods.DisplayReportResult(respond);

                    RunLoadMore = false;
                }
                else MessageBox.Show(LocalResources.label_Please_check_your_internet);
            }
            catch (Exception e)
            {
                RunLoadMore = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //########################## Pin Messages ##########################

        #region Pin Messages

        private async Task GetPinMessageApi()
        {
            if (!Settings.EnablePinMessageSystem || string.IsNullOrEmpty(ChatId))
                return;

            if (ListUtils.PinnedMessageList.Count > 0)
                ListUtils.PinnedMessageList.Clear();

            var (apiStatus, respond) = await RequestsAsync.Message.GetPinMessageAsync(ChatId);
            if (apiStatus != 200 || respond is not DetailsMessagesObject result || result.Data == null)
            {
                Methods.DisplayReportResult(respond);
            }
            else
            {
                var respondList = result.Data.Count;
                if (respondList > 0)
                {
                    foreach (var item in from item in result.Data let check = ListUtils.PinnedMessageList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                    {
                        var type = WoWonderTools.GetTypeModel(item);
                        if (type == MessageModelType.None)
                            continue;

                        ListUtils.PinnedMessageList.Add(WoWonderTools.MessageFilter(UserId, SelectedChatData.Name, item, ChatColor));
                    }
                }

                Dispatcher?.Invoke((Action)LoadPinnedMessage);
            }
        }

        private void LoadPinnedMessage()
        {
            try
            {
                if (ListUtils.PinnedMessageList?.Count > 0)
                {
                    PinMessageView.Visibility = Visibility.Visible;
                    var lastChat = ListUtils.PinnedMessageList.LastOrDefault();
                    if (lastChat != null)
                    {
                        switch (lastChat?.ModelType)
                        {
                            case MessageModelType.LeftText:
                            case MessageModelType.RightText:
                                ShortPinMessage.Text = LocalResources.label5_LastPinnedMessage + " : " + lastChat.Text;
                                break;
                            case MessageModelType.LeftVideo:
                            case MessageModelType.RightVideo:
                                ShortPinMessage.Text = LocalResources.label5_LastPinnedMessage + " : " + LocalResources.label_cuont_video;
                                break;
                            case MessageModelType.LeftGif:
                            case MessageModelType.RightGif:
                                ShortPinMessage.Text = LocalResources.label5_LastPinnedMessage + " : " + LocalResources.label_gift;
                                break;
                            case MessageModelType.LeftSticker:
                            case MessageModelType.RightSticker:
                                ShortPinMessage.Text = LocalResources.label5_LastPinnedMessage + " : " + LocalResources.label5_Sticker;
                                break;
                            case MessageModelType.LeftImage:
                            case MessageModelType.RightImage:
                                ShortPinMessage.Text = LocalResources.label5_LastPinnedMessage + " : " + LocalResources.label_cuont_images;
                                break;
                            case MessageModelType.LeftAudio:
                            case MessageModelType.RightAudio:
                                ShortPinMessage.Text = LocalResources.label5_LastPinnedMessage + " : " + LocalResources.label5_VoiceMessage;
                                break;
                            case MessageModelType.LeftFile:
                            case MessageModelType.RightFile:
                                ShortPinMessage.Text = LocalResources.label5_LastPinnedMessage + " : " + LocalResources.label_cuont_file;
                                break;
                            case MessageModelType.LeftMap:
                            case MessageModelType.RightMap:
                                ShortPinMessage.Text = LocalResources.label5_LastPinnedMessage + " : " + LocalResources.label_location;
                                break;
                            case MessageModelType.LeftContact:
                            case MessageModelType.RightContact:
                                ShortPinMessage.Text = LocalResources.label5_LastPinnedMessage + " : " + LocalResources.label5_Contact;
                                break;
                            case MessageModelType.LeftProduct:
                            case MessageModelType.RightProduct:
                                ShortPinMessage.Text = LocalResources.label5_LastPinnedMessage + " : " + LocalResources.label5_Product;
                                break;
                            case MessageModelType.None:
                                break;
                        }
                    }
                }
                else
                {
                    PinMessageView.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        private void PinMessageView_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                PinnedMessageWindow pinnedMessageWindow = new PinnedMessageWindow();
                pinnedMessageWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //########################## Favorite Messages ##########################

        #region Favorite Messages

        private async Task GetFavoriteMessageApi()
        {
            if (!Settings.EnableFavoriteMessageSystem || string.IsNullOrEmpty(ChatId))
                return;

            if (ListUtils.StartedMessageList.Count > 0)
                ListUtils.StartedMessageList.Clear();

            var (apiStatus, respond) = await RequestsAsync.Message.GetFavoriteMessageAsync(ChatId);
            if (apiStatus != 200 || respond is not DetailsMessagesObject result || result.Data == null)
            {
                Methods.DisplayReportResult(respond);
            }
            else
            {
                var respondList = result.Data.Count;
                if (respondList > 0)
                {
                    foreach (var item in from item in result.Data let check = ListUtils.StartedMessageList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                    {
                        var type = WoWonderTools.GetTypeModel(item);
                        if (type == MessageModelType.None)
                            continue;

                        ListUtils.StartedMessageList.Add(WoWonderTools.MessageFilter(UserId, SelectedChatData.Name, item, ChatColor));
                    }
                }
            }
        }

        #endregion

        //########################## Send message  ##########################

        #region Send Button >> MessageBoxText

        private void MessageBoxText_OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {

                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MessageBoxText_OnGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TextRange textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd);
                if (textRange.Text == LocalResources.label_MessageBoxText + "\r\n")
                {
                    textRange.Text = "";
                    // MessageBoxText.Foreground = BtnSwitchDarkMode.IsChecked != null && BtnSwitchDarkMode.IsChecked.Value ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MessageBoxText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextRange textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd);
                if (textRange.Text.Length > 0 & !string.IsNullOrWhiteSpace(textRange.Text))
                {
                    BackgroundWorker bw = new BackgroundWorker();
                    bw.DoWork += (o, args) => TypingEvent("Typping");
                    bw.RunWorkerAsync();
                }
                else
                {
                    BackgroundWorker bw = new BackgroundWorker();
                    bw.DoWork += (o, args) => TypingEvent("removing");
                    bw.RunWorkerAsync();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TypingEvent(string status)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    switch (status)
                    {
                        case "Typping":
                            if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                                UserDetails.Socket?.EmitAsync_TypingEvent(UserId, UserDetails.AccessToken);
                            else
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.SetChatTypingStatusAsync(UserId, "Typping") });
                            break;
                        case "removing":
                            if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                                UserDetails.Socket?.EmitAsync_StoppedEvent(UserId, UserDetails.AccessToken);
                            else
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.SetChatTypingStatusAsync(UserId, "removing") });
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MessageBoxText_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
                {
                    string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd).Text;
                    string textMsg = textRange;
                    int spaces = Regex.Matches(textRange, "\r\n").Count;

                    if (spaces < 3)
                    {
                        textMsg = textRange.Replace("\r\n", "");
                    }

                    if (string.IsNullOrEmpty(textMsg) || string.IsNullOrWhiteSpace(textMsg) || textMsg == "Write your Message\r\n")
                    {
                        return;
                    }
                    else
                    {
                        Dispatcher?.Invoke((Action)delegate // <--- HERE
                        {
                            try
                            {
                                ViewModel.ModelInstance.SendMessage();
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                    }

                    e.Handled = true;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Drop

        private void WinMin_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effects = DragDropEffects.Copy;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private readonly List<string> ChekFile = new List<string>();

        private void WinMin_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(SelectedChatData.Name))
                    return;

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files != null && files.Length != 0)
                {
                    if (files.Length == 1)
                    {
                        // do what you want
                        foreach (string fils in files)
                        {
                            string fileNameMedia = fils.Split('\\').Last();

                            long size = new FileInfo(fils).Length;
                            double totalSize = size / 1024.0F / 1024.0F;
                            SizeFile = totalSize.ToString("0.### KB");

                            string filecopy = Methods.FilesDestination + UserId;
                            string checkExtension = Methods.Check_FileExtension(fils);
                            string copyedfile = "";
                            if (checkExtension == "File")
                            {
                                copyedfile = filecopy + "\\" + "file\\" + fileNameMedia;
                                if (!File.Exists(copyedfile))
                                {
                                    File.Copy(fils, copyedfile, true);
                                }
                            }

                            if (checkExtension == "Image")
                            {
                                copyedfile = filecopy + "\\" + "images\\" + fileNameMedia;
                                if (!File.Exists(copyedfile))
                                {
                                    File.Copy(fils, copyedfile, true);
                                }
                            }

                            if (checkExtension == "Video")
                            {
                                copyedfile = filecopy + "\\" + "video\\" + fileNameMedia;
                                if (!File.Exists(copyedfile))
                                {
                                    File.Copy(fils, copyedfile, true);
                                }
                            }

                            if (checkExtension == "Audio")
                            {
                                copyedfile = filecopy + "\\" + "sound\\" + fileNameMedia;
                                if (!File.Exists(copyedfile))
                                {
                                    File.Copy(fils, copyedfile, true);
                                }
                            }

                            string[] f = copyedfile.Split(new char[] { '.' });
                            string[] allowedExtenstion = ListUtils.SettingsSiteList?.AllowedExtenstion.Split(new char[] { ',' }) ?? "jpg,png,jpeg,gif,mkv,docx,zip,rar,pdf,doc,mp3,mp4,flv,wav,txt,mov,avi,webm,wav,mpeg".Split(new char[] { ',' });
                            if (allowedExtenstion.Contains(f.Last()))
                            {
                                try
                                {
                                    if (Methods.CheckForInternetConnection())
                                    {
                                        Dispatcher?.Invoke(async delegate // <--- HERE
                                        {
                                            try
                                            {
                                                int index = ListUtils.ListUsers.IndexOf(ListUtils.ListUsers.FirstOrDefault(a => a.UserId == UserId));
                                                if (index > -1 && index != 0)
                                                {
                                                    ListUtils.ListUsers.Move(index, 0);
                                                }

                                                //Uplouding.Visibility = Visibility.Visible;
                                                //Uplouding.IsIndeterminate = true;
                                                //NoMessagePanel.Visibility = Visibility.Hidden;
                                                if (!ChekFile.Contains(fils))
                                                {
                                                    var conversation = MethodToSendAttachmentMessages(copyedfile);

                                                    var (apiStatus, respond) = await RequestsAsync.Message.SendMessageAsync(UserId, conversation.Id.ToString(), "", "", copyedfile, "", "", "", "", "", "", "", ReplyId);
                                                    if (apiStatus == 200)
                                                    {
                                                        if (respond is SendMessageObject res)
                                                        {
                                                            //MethodToSendAttachmentMessages(copyedfile);

                                                            var lastMessage = ListUtils.ListMessages.LastOrDefault(a => a.Id == conversation.Id);
                                                            if (lastMessage != null)
                                                            {
                                                                lastMessage = WoWonderTools.MessageFilter(UserId, SelectedChatData.Name, res.MessageData.FirstOrDefault(), ChatColor);
                                                                lastMessage.Id = res.MessageData[0].Id;

                                                                ViewModel.ModelInstance.UpdateChangedMessage();
                                                                ViewModel.ModelInstance.UpdateChatAllList(conversation);

                                                                //Update
                                                                ViewModel.ModelInstance.UpdateMessage(true);
                                                            }
                                                        }
                                                    }
                                                    ViewModel.ModelInstance.CancelReply();
                                                }
                                            }
                                            catch (Exception exception)
                                            {
                                                Methods.DisplayReportResultTrack(exception);
                                            }
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Methods.DisplayReportResultTrack(ex);
                                }
                            }
                            else
                            {
                                MessageBox.Show(LocalResources.label5_ErrorSelectedFileExtenstion, LocalResources.label5_Error, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }

                            ChekFile.Add(fils);
                        }

                        //if (Uplouding.Visibility == Visibility.Visible)
                        //{
                        //    Uplouding.Visibility = Visibility.Collapsed;
                        //    Uplouding.IsIndeterminate = false;
                        //    NoMessagePanel.Visibility = Visibility.Hidden;
                        //}
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        // sent Messages file
        public MessagesDataFody MethodToSendAttachmentMessages(string fileNameAttachment)
        {
            try
            {
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);
                string time = DateTime.Now.ToString("hh:mm");

                string checkExtension = Methods.Check_FileExtension(fileNameAttachment);

                string fileNameMedia = fileNameAttachment.Split('\\').Last();

                var m = new MessagesDataFody
                {
                    Id = unixTimestamp,
                    FromId = UserDetails.UserId,
                    GroupId = GroupId,
                    Img_user_message = UserDetails.Avatar,
                    ToId = UserId,

                    Media = fileNameAttachment,
                    MediaFileName = fileNameMedia,
                    FileSize = SizeFile,

                    Time = time,
                    TimeText = time,
                    Position = "right",
                    ModelType = MessageModelType.RightFile,
                    ChatColor = ChatColor,
                    EndChatColor = Settings.ColorMessageThemeGradient ? WoWonderTools.GetColorEnd(ChatColor) : ChatColor,

                    Seen = "-1",
                    SendFile = true,
                    ErrorSendMessage = false,
                    ReplyId = ReplyId,
                };

                Visibility playVisibility = Visibility.Collapsed;
                Visibility pauseVisibility = Visibility.Collapsed;
                Visibility progresSVisibility = Visibility.Collapsed;
                Visibility downloadVisibility = Visibility.Visible;
                Visibility iconFileVisibility = Visibility.Collapsed;
                Visibility hlinkDownloadVisibility = Visibility.Visible;
                Visibility hlinkOpenVisibility = Visibility.Collapsed;

                string typeIconFile = "File";
                if (fileNameAttachment.EndsWith("rar") || fileNameAttachment.EndsWith("RAR") || fileNameAttachment.EndsWith("zip") || fileNameAttachment.EndsWith("ZIP"))
                {
                    m.ModelType = MessageModelType.RightFile;
                    typeIconFile = "ZipBox";
                }
                else if (fileNameAttachment.EndsWith("txt") || fileNameAttachment.EndsWith("TXT"))
                {
                    m.ModelType = MessageModelType.RightFile;
                    typeIconFile = "NoteText";
                }
                else if (fileNameAttachment.EndsWith("docx") || fileNameAttachment.EndsWith("DOCX"))
                {
                    m.ModelType = MessageModelType.RightFile;
                    typeIconFile = "FileWord";
                }
                else if (fileNameAttachment.EndsWith("doc") || fileNameAttachment.EndsWith("DOC"))
                {
                    m.ModelType = MessageModelType.RightFile;
                    typeIconFile = "FileWord";
                }
                else if (fileNameAttachment.EndsWith("pdf") || fileNameAttachment.EndsWith("PDF"))
                {
                    m.ModelType = MessageModelType.RightFile;
                    typeIconFile = "FilePdf";
                }

                if (fileNameAttachment.Length > 25)
                {
                    fileNameAttachment = Methods.FunString.SubStringCutOf(fileNameAttachment, 25) + "." + fileNameAttachment.Split('.').Last();
                }

                if (checkExtension == "Image")
                {
                    m.ModelType = MessageModelType.RightImage;
                    m.ChatColor = ChatColor;
                }

                if (checkExtension == "Audio")
                {
                    m.Position = "right";
                    m.ModelType = MessageModelType.RightAudio;

                    m.Play_Visibility = Visibility.Visible;
                    m.Pause_Visibility = Visibility.Collapsed;
                    m.ProgresSVisibility = Visibility.Collapsed;
                    m.Download_Visibility = Visibility.Collapsed;
                }

                else if (checkExtension == "Video")
                {
                    m.Position = "right";
                    m.ModelType = MessageModelType.RightVideo;

                    m.Play_Visibility = Visibility.Visible;
                    m.ProgresSVisibility = Visibility.Collapsed;
                    m.Download_Visibility = Visibility.Collapsed;
                }
                else if (checkExtension == "File")
                {
                    m.ModelType = MessageModelType.RightFile;

                    m.Icon_File_Visibility = Visibility.Visible;
                    m.ProgresSVisibility = Visibility.Collapsed;
                    m.Download_Visibility = Visibility.Collapsed;
                    m.Hlink_Download_Visibility = Visibility.Collapsed;
                    m.Hlink_Open_Visibility = Visibility.Visible;
                }

                //style 
                m.Img_user_message = ViewModel.ModelInstance.AvatarUser;
                m.ProgresSVisibility = progresSVisibility;
                m.Download_Visibility = downloadVisibility;
                m.Play_Visibility = playVisibility;
                m.Pause_Visibility = pauseVisibility;
                m.Icon_File_Visibility = iconFileVisibility;
                m.Hlink_Download_Visibility = hlinkDownloadVisibility;
                m.Hlink_Open_Visibility = hlinkOpenVisibility;
                m.Type_Icon_File = typeIconFile;
                m.sound_slider_value = 0;
                m.Progress_Value = 0;
                m.sound_time = "";

                ListUtils.ListMessages.Add(WoWonderTools.MessageFilter(UserId, SelectedChatData.Name, m, ChatColor, TypeChatPage));
                return m;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        // sent Messages text
        public void MethodToSendMessages(string textMsg)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);

                    string time = DateTime.Now.ToString("hh:mm");

                    Dispatcher?.Invoke((Action)async delegate // <--- HERE
                    {
                        try
                        {
                            var conversation = new MessagesDataFody
                            {
                                Id = unixTimestamp,
                                FromId = UserDetails.UserId,
                                Img_user_message = UserDetails.Avatar,
                                GroupId = GroupId,
                                ToId = UserId,

                                Text = textMsg,

                                Time = time,
                                TimeText = time,
                                Position = "right",
                                ModelType = MessageModelType.RightText,
                                ChatColor = ChatColor,
                                EndChatColor = Settings.ColorMessageThemeGradient ? WoWonderTools.GetColorEnd(ChatColor) : ChatColor,

                                Seen = "-1",
                                SendFile = true,
                                ErrorSendMessage = false,
                                ReplyId = ReplyId,
                            };

                            //Add message to converstion list
                            ListUtils.ListMessages.Add(WoWonderTools.MessageFilter(TypeChatPage == "user" ? UserId : GroupId, UserDetails.FullName, conversation, ChatColor, TypeChatPage));

                            if (TypeChatPage == "user")
                            {
                                var (apiStatus, respond) = await RequestsAsync.Message.SendMessageAsync(UserId, time2, textMsg, "", "", "", "", "", "", "", "", "", ReplyId);
                                if (apiStatus == 200)
                                {
                                    if (respond is SendMessageObject result)
                                    {
                                        var lastMessage = ListUtils.ListMessages.LastOrDefault(a => a.Id == unixTimestamp);
                                        if (lastMessage != null)
                                        {
                                            lastMessage.Id = result.MessageData[0].Id;
                                            lastMessage = WoWonderTools.MessageFilter(UserId, SelectedChatData.Name, result.MessageData.FirstOrDefault(), ChatColor);
                                            Console.WriteLine(lastMessage.Id);

                                            SqLiteDatabase database = new SqLiteDatabase();
                                            database.Insert_Or_Update_To_one_MessagesTable(lastMessage);

                                            ViewModel.ModelInstance.UpdateChatAllList(lastMessage);

                                            //Update
                                            ViewModel.ModelInstance.UpdateMessage(true);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var (apiStatus, respond) = await RequestsAsync.GroupChat.Send_MessageToGroupChatAsync(GroupId, time2, textMsg, "", "", "", "", "", "", "", ReplyId);
                                if (apiStatus == 200)
                                {
                                    if (respond is GroupSendMessageObject res)
                                    {
                                        var lastMessage = ListUtils.ListMessages.LastOrDefault(a => a.Id == unixTimestamp);
                                        if (lastMessage != null)
                                        {
                                            var item = res.Data.FirstOrDefault();
                                            if (item == null)
                                                return;

                                            lastMessage.Id = item.Id;
                                            lastMessage = WoWonderTools.MessageFilter(GroupId, WoWonderTools.GetNameFinal(item.UserData), item, ChatColor, "group");
                                            Console.WriteLine(lastMessage.Id);

                                            ViewModel.ModelInstance.UpdateChatAllList(lastMessage);

                                            //Update
                                            ViewModel.ModelInstance.UpdateMessage(true);
                                        }
                                    }
                                }
                            }

                            ViewModel.ModelInstance.CancelReply();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //########################## Delete Messages ##########################

        #region Delete Messages

        // Run background worker :  delete messages
        public async void DeleteMessages_Async(string userId)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    await Task.Factory.StartNew(async () =>
                    {
                        var (apiStatus, respond) = await RequestsAsync.Message.DeleteConversationAsync(userId);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageObject result)
                            {
                                Console.WriteLine(result.Message);
                                SqLiteDatabase database = new SqLiteDatabase();
                                database.RemoveUser_All_Table(UserDetails.UserId, userId);

                                Methods.Path.Delete_dataFile_user(UserId);

                                var delete = ListUtils.ListUsers.FirstOrDefault(a => a.UserId == userId);
                                if (delete != null)
                                {
                                    App.Current.Dispatcher?.Invoke((Action)delegate // <--- HERE
                                    {
                                        try
                                        {
                                            ListUtils.ListUsers.Remove(delete);
                                            ListUtils.ListMessages.Clear();
                                            ListUtils.ListSharedFiles.Clear();

                                            //Scroll Top >> 
                                            //wael

                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                    });
                                }

                            }
                        }
                    });

                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //########################## Reply Messages ##########################

        #region Reply Messages

        //Reply Messages
        public void ReplyItems(MessagesDataFody selectedItemPositions)
        {
            try
            {
                if (selectedItemPositions != null)
                {
                    ViewModel.ModelInstance.OpenReply();

                    ReplyId = selectedItemPositions.Id.ToString();

                    TxtOwnerName.Text = selectedItemPositions.MessageUser?.UserDataClass?.UserId == UserDetails.UserId ? "You" : SelectedChatData.Name;

                    if (selectedItemPositions.ModelType == MessageModelType.LeftText || selectedItemPositions.ModelType == MessageModelType.RightText)
                    {
                        MessageFileThumbnail.Visibility = Visibility.Collapsed;
                        TxtMessageType.Visibility = Visibility.Collapsed;
                        TxtShortMessage.Text = selectedItemPositions.Text;
                    }
                    else
                    {
                        MessageFileThumbnail.Visibility = Visibility.Visible;
                        var fileName = selectedItemPositions.Media.Split('/').Last();

                        switch (selectedItemPositions.ModelType)
                        {
                            case MessageModelType.LeftVideo:
                            case MessageModelType.RightVideo:
                                {
                                    TxtMessageType.Visibility = Visibility.Collapsed;
                                    TxtShortMessage.Text = LocalResources.label_cuont_video;

                                    if (string.IsNullOrEmpty(selectedItemPositions.ImageVideo))
                                    {
                                        selectedItemPositions.ImageVideo = Methods.MultiMedia.Get_ThumbnailVideo_messages(selectedItemPositions.Media, UserId, selectedItemPositions);
                                    }

                                    MessageFileThumbnail.Source = new BitmapImage(new Uri(selectedItemPositions.ImageVideo));

                                    break;
                                }
                            case MessageModelType.LeftGif:
                            case MessageModelType.RightGif:
                                {
                                    TxtMessageType.Visibility = Visibility.Collapsed;
                                    TxtShortMessage.Text = LocalResources.label_gift;
                                    selectedItemPositions.Media = Methods.MultiMedia.GetFile(UserId, fileName, "images", selectedItemPositions.Media);

                                    MessageFileThumbnail.Source = new BitmapImage(new Uri(selectedItemPositions.Media));

                                    break;
                                }
                            case MessageModelType.LeftSticker:
                            case MessageModelType.RightSticker:
                                {
                                    TxtMessageType.Visibility = Visibility.Collapsed;
                                    TxtShortMessage.Text = LocalResources.label5_Sticker;

                                    selectedItemPositions.Media = Methods.MultiMedia.Get_Sticker_messages(fileName, selectedItemPositions.Media, selectedItemPositions);

                                    MessageFileThumbnail.Source = new BitmapImage(new Uri(selectedItemPositions.Media));
                                    break;
                                }
                            case MessageModelType.LeftImage:
                            case MessageModelType.RightImage:
                                {
                                    TxtMessageType.Visibility = Visibility.Collapsed;
                                    TxtShortMessage.Text = LocalResources.label_cuont_images;

                                    selectedItemPositions.Media = Methods.MultiMedia.GetFile(UserId, fileName, "images", selectedItemPositions.Media);

                                    MessageFileThumbnail.Source = new BitmapImage(new Uri(selectedItemPositions.Media));
                                    break;
                                }
                            case MessageModelType.LeftAudio:
                            case MessageModelType.RightAudio:
                                {
                                    TxtMessageType.Visibility = Visibility.Collapsed;
                                    TxtShortMessage.Text = LocalResources.label_cuont_sounds;
                                    MessageFileThumbnail.Source = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Audio_File.png"));
                                    break;
                                }
                            case MessageModelType.LeftFile:
                            case MessageModelType.RightFile:
                                {
                                    TxtMessageType.Text = LocalResources.label_cuont_file;

                                    var fileNameWithoutExtension = fileName.Split('.').First();
                                    var fileNameExtension = fileName.Split('.').Last();

                                    TxtShortMessage.Text = Methods.FunString.SubStringCutOf(fileNameWithoutExtension, 10) + fileNameExtension;
                                    MessageFileThumbnail.Source = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Image_File.png"));
                                    break;
                                }
                            case MessageModelType.LeftMap:
                            case MessageModelType.RightMap:
                                {
                                    TxtMessageType.Visibility = Visibility.Collapsed;
                                    TxtShortMessage.Text = LocalResources.label_location;
                                    MessageFileThumbnail.Source = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Image_Map.png"));
                                    break;
                                }
                            case MessageModelType.LeftContact:
                            case MessageModelType.RightContact:
                                {
                                    TxtMessageType.Text = LocalResources.label5_Contact;
                                    TxtShortMessage.Text = selectedItemPositions.ContactName;
                                    MessageFileThumbnail.Source = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/no_profile_image_circle.png"));
                                    break;
                                }
                            case MessageModelType.LeftProduct:
                            case MessageModelType.RightProduct:
                                {
                                    TxtMessageType.Visibility = Visibility.Collapsed;
                                    TxtShortMessage.Text = LocalResources.label5_Product;
                                    string imageUrl = !string.IsNullOrEmpty(selectedItemPositions.Media) ? selectedItemPositions.Media : selectedItemPositions.Product?.ProductClass?.Images[0]?.Image;
                                    MessageFileThumbnail.Source = new BitmapImage(new Uri(imageUrl));
                                    break;
                                }
                            case MessageModelType.LeftText:
                            case MessageModelType.RightText:
                            case MessageModelType.None:
                                break;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CloseReplyUi()
        {
            try
            {
                ViewModel.ModelInstance.CancelReply();
                ReplyId = "";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //########################## Group Messages ##########################

        #region Group Messages

        private async Task GetGroupMessages_Api()
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    var (apiStatus, respond) = await RequestsAsync.GroupChat.FetchGroupChatMessagesAsync(GroupId);
                    if (apiStatus == 200)
                    {
                        if (respond is GroupMessagesObject result)
                        {
                            var respondList = result.Data.Messages.Count;
                            if (respondList > 0)
                            {
                                result.Data.Messages.Reverse();

                                foreach (var item in from item in result.Data.Messages let check = ListUtils.ListMessages.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    var type = WoWonderTools.GetTypeModel(item);
                                    if (type == MessageModelType.None)
                                        continue;

                                    Dispatcher?.Invoke((Action)delegate // <--- HERE
                                    {
                                        try
                                        {
                                            ListUtils.ListMessages.Add(WoWonderTools.MessageFilter(GroupId, WoWonderTools.GetNameFinal(item.UserData), item, ChatColor, "group"));
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
                    else Methods.DisplayReportResult(respond);

                    Dispatcher?.Invoke((Action)delegate { LoadingMessagesPanel.Visibility = Visibility.Collapsed; });
                }
                else MessageBox.Show(LocalResources.label_Please_check_your_internet);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void GroupMessageUpdater()
        {
            try
            {
                if (TaskWork == "Working")
                {
                    TaskWork = "Stop";

                    if (Methods.CheckForInternetConnection())
                    {
                        //var data = ListUtils.ListMessages.LastOrDefault();
                        //var lastMessageId = data?.MesData?.Id ?? "0";
                        var (apiStatus, respond) = await RequestsAsync.GroupChat.FetchGroupChatMessagesAsync(GroupId, "0", "0", "35");
                        if (apiStatus == 200)
                        {
                            if (respond is GroupMessagesObject result)
                            {
                                var countList = ListUtils.ListMessages.Count;
                                var respondList = result.Data.Messages.Count;
                                if (respondList > 0)
                                {
                                    foreach (var item in result.Data.Messages)
                                    {
                                        var type = WoWonderTools.GetTypeModel(item);
                                        if (type == MessageModelType.None)
                                            continue;

                                        var check = ListUtils.ListMessages.FirstOrDefault(a => a.Id == item.Id);
                                        if (check == null)
                                        {
                                            Dispatcher?.Invoke((Action)delegate // <--- HERE
                                            {
                                                try
                                                {
                                                    var dataFody = WoWonderTools.MessageFilter(GroupId, WoWonderTools.GetNameFinal(item.UserData), item, ChatColor, "group");

                                                    ListUtils.ListMessages.Add(dataFody);

                                                    ViewModel.ModelInstance.UpdateMessage(false);
                                                }
                                                catch (Exception exception)
                                                {
                                                    Methods.DisplayReportResultTrack(exception);
                                                }
                                            });

                                            //if (UserDetails.SoundControl)
                                            //    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_GetMesseges.mp3");
                                        }
                                        else if (check.Seen == "0" && check.Seen != item.Seen) // right
                                        {
                                            check = WoWonderTools.MessageFilter(GroupId, WoWonderTools.GetNameFinal(item.UserData), item, ChatColor, "group");
                                            check.Id = item.Id;
                                            check.Seen = item.Seen;

                                            Console.WriteLine(check);
                                        }
                                    }

                                    //if (ListUtils.ListMessages.Count > 0)
                                    //{
                                    //    SayHiLayout.Visibility = ViewStates.Gone;
                                    //    SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
                                    //}
                                    //else if (ListUtils.ListMessages.Count == 0 && ShowEmpty != "no")
                                    //{
                                    //    SayHiLayout.Visibility = ViewStates.Visible;
                                    //    SayHiSuggestionsRecycler.Visibility = ViewStates.Visible;
                                    //}
                                }
                            }
                        }
                        else Methods.DisplayReportResult(respond);
                    }

                    TaskWork = "Working";
                }
            }
            catch (Exception e)
            {
                TaskWork = "Working";
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async Task GetGroupMessagesById(string id)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    var (apiStatus, respond) = await RequestsAsync.GroupChat.FetchMessagesByIdAsync(GroupId, id);
                    if (apiStatus == 200)
                    {
                        if (respond is GroupMessagesByIdObject result)
                        {
                            var countList = ListUtils.ListMessages.Count;
                            var respondList = result.Data.Count;
                            if (respondList > 0)
                            {
                                foreach (var item in result.Data)
                                {
                                    var type = WoWonderTools.GetTypeModel(item);
                                    if (type == MessageModelType.None)
                                        continue;

                                    var check = ListUtils.ListMessages.FirstOrDefault(a => a.Id == item.Id);
                                    if (check == null)
                                    {
                                        Dispatcher?.Invoke((Action)delegate // <--- HERE
                                        {
                                            try
                                            {
                                                var dataFody = WoWonderTools.MessageFilter(GroupId, WoWonderTools.GetNameFinal(item.UserData), item, ChatColor, "group");

                                                ListUtils.ListMessages.Add(dataFody);

                                                ViewModel.ModelInstance.UpdateMessage(false);
                                            }
                                            catch (Exception exception)
                                            {
                                                Methods.DisplayReportResultTrack(exception);
                                            }
                                        });
                                    }
                                    else
                                    {
                                        check.Id = item.Id;
                                        check = WoWonderTools.MessageFilter(GroupId, WoWonderTools.GetNameFinal(item.UserData), item, ChatColor, "group");

                                        ViewModel.ModelInstance.UpdateMessage(true);
                                    }
                                }

                                //if (MAdapter.DifferList.Count > 0)
                                //{
                                //    SayHiLayout.Visibility = ViewStates.Gone;
                                //    SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
                                //}
                                //else if (MAdapter.DifferList.Count == 0 && ShowEmpty != "no")
                                //{
                                //    SayHiLayout.Visibility = ViewStates.Visible;
                                //    SayHiSuggestionsRecycler.Visibility = ViewStates.Visible;
                                //}
                            }
                        }
                    }
                    else Methods.DisplayReportResult(respond);
                }
                else MessageBox.Show(LocalResources.label_Please_check_your_internet);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //########################## Search User & Suggestions##########################

        #region Search User

        // Run background worker : Search
        private async Task Search_Async(string offset = "0")
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    IProgress<int> progress = new Progress<int>(percentCompleted =>
                    {
                        ProgressBarSearchUser.Value = percentCompleted;
                    });

                    if (ListUtils.ListSearch.Count > 0)
                        ListUtils.ListSearch.Clear();

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
                        if (respond is GetSearchObject { Users: { Count: > 0 } } result)
                        {
                            foreach (var item in from user in result.Users let check = ListUtils.ListSearch.FirstOrDefault(a => a.UserId == user.UserId) where check == null select WoWonderTools.UserFilter(user))
                            {
                                Dispatcher?.Invoke((Action)delegate // <--- HERE
                                {
                                    try
                                    {
                                        ListUtils.ListSearch.Add(item);
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
                            if (ListUtils.ListSearch.Count > 0)
                            {
                                SearchUserList.ItemsSource = ListUtils.ListSearch;
                                SearchUserList.Visibility = Visibility.Visible;

                                EmptyPageContentSearchUser.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                SearchUserList.Visibility = Visibility.Collapsed;
                                EmptyPageContentSearchUser.Visibility = Visibility.Visible;
                                EmptyPageContentSearchUser.InflateLayout(EmptyPage.Type.NoSearchResult);
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
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // click Button Search User
        private void Btn_SearchUser_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    ProgressBarSearchUser.Visibility = Visibility.Visible;
                    ProgressBarSearchUser.IsIndeterminate = true;

                    EmptyPageContentSearchUser.Visibility = Visibility.Collapsed;

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => Search_Async() });
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

        private void TxtSearchBoxOnline_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchKey = TxtSearchBoxOnline.Text;
        }

        //Event Enter Search User
        private void Txt_SearchBoxOnline_KeyUp(object sender, KeyEventArgs e)
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

                        EmptyPageContentSearchUser.Visibility = Visibility.Collapsed;

                        // your event handler here
                        e.Handled = true;

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

        private void SearchUser_list_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UserDataFody selectedGroup = (UserDataFody)SearchUserList.SelectedItem;
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

        private void SearchUser_list_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                SearchUserList.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // click Button Follow
        private void btn_Follow_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    ListBox mi = (ListBox)sender;
                    Button originalSource = e.OriginalSource as Button;

                    var followId = originalSource?.CommandParameter.ToString();

                    var item = ListUtils.ListSearch.FirstOrDefault(a => a.UserId == followId);
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

                        var view = CollectionViewSource.GetDefaultView(ListUtils.ListSearch);
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

        #endregion

        #region Load Suggestions 

        private async Task LoadSuggestionsAsync()
        {
            if (Methods.CheckForInternetConnection())
            {
                var (apiStatus, respond) = await RequestsAsync.Global.GetRecommendedUsersAsync("25", "0").ConfigureAwait(false);
                if (apiStatus == 200)
                {
                    if (respond is ListUsersObject result)
                    {
                        if (result.Data.Count > 0)
                        {
                            foreach (var item in from item in result.Data let check = ListUtils.ListSearch.FirstOrDefault(a => a.UserId == item.UserId) where check == null select WoWonderTools.UserFilter(item))
                            {
                                Dispatcher?.Invoke((Action)delegate // <--- HERE
                                {
                                    try
                                    {
                                        ListUtils.ListSearch.Add(item);
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                });
                            }

                            Dispatcher?.Invoke((Action)delegate // <--- HERE
                            {
                                try
                                {
                                    if (ListUtils.ListSearch.Count > 0)
                                    {
                                        SearchUserList.ItemsSource = ListUtils.ListSearch;
                                        SearchUserList.Visibility = Visibility.Visible;

                                        EmptyPageContentSearchUser.Visibility = Visibility.Collapsed;
                                    }
                                    else
                                    {
                                        SearchUserList.Visibility = Visibility.Collapsed;
                                        EmptyPageContentSearchUser.Visibility = Visibility.Visible;
                                        EmptyPageContentSearchUser.InflateLayout(EmptyPage.Type.NoSearchResult);
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
                    }
                }
                else
                    Methods.DisplayReportResult(respond);
            }
            else
            {
                MessageBox.Show(LocalResources.label_Please_check_your_internet);
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
                    var item = ListUtils.ListSearch.LastOrDefault();
                    if (item != null && !string.IsNullOrEmpty(item.UserId) && !SearchUserBoxScrollListener.IsLoading && !string.IsNullOrEmpty(SearchKey))
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

        //########################## Search Gifs ##########################

        #region Search Gifs

        public static ObservableCollection<GifGiphyClass.Datum> ListTrendingGif = new ObservableCollection<GifGiphyClass.Datum>();

        // Run background worker : Gifs
        private async Task TrendingGifAsync()
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    var response = await ApiRequest.TrendingGif("0");
                    if (response?.Count > 0)
                    {
                        ListTrendingGif = new ObservableCollection<GifGiphyClass.Datum>(response);
                    }

                    Dispatcher?.Invoke((Action)delegate // <--- HERE
                    {
                        try
                        {
                            GifsListView.ItemsSource = ListTrendingGif;
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Run background worker : Gifs
        private async Task Gifs_Async()
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    IProgress<int> progress = new Progress<int>(percentCompleted =>
                    {
                        ProgressBarSearchGifs.Value = percentCompleted;
                    });

                    var response = await ApiRequest.SearchGif(SearchGifsKey, "0");
                    if (response?.Count > 0)
                    {

                        if (ListUtils.ListGifs.Count > 0)
                        {
                            foreach (var g in response)
                            {
                                Dispatcher?.Invoke((Action)delegate // <--- HERE
                                {
                                    try
                                    {
                                        ListUtils.ListGifs.Add(g);
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }
                        else
                        {
                            ListUtils.ListGifs = new ObservableCollection<GifGiphyClass.Datum>(response);
                        }

                        Dispatcher?.Invoke((Action)delegate // <--- HERE
                        {
                            try
                            {
                                GifsListView.ItemsSource = ListUtils.ListGifs;

                                ProgressBarSearchGifs.Visibility = Visibility.Collapsed;
                                ProgressBarSearchGifs.IsIndeterminate = false;
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event show or hide Progress Bar load gifs
        private void Gifs_MediaElement_OnAnimationLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Image b)
                {
                    if (b.DataContext is GifGiphyClass.Datum data)
                    {
                        ImageAnimationController controller = ImageBehavior.GetAnimationController(b);
                        controller?.Pause();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event On Mouse Enter Play gifs 
        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender is Image b)
                {
                    ImageAnimationController controller = ImageBehavior.GetAnimationController(b);
                    controller?.Play();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event  On Mouse Leave Pause gifs
        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender is Image b)
                {
                    ImageAnimationController controller = ImageBehavior.GetAnimationController(b);
                    controller?.Pause();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Enter Search Gifs
        private void Txt_searchGifs_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    SearchGifsKey = TxtSearchGifs.Text;

                    if (e.Key == Key.Enter)
                    {
                        ListUtils.ListGifs.Clear();
                        ProgressBarSearchGifs.Visibility = Visibility.Visible;
                        ProgressBarSearchGifs.IsIndeterminate = true;

                        // your event handler here
                        e.Handled = true;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { Gifs_Async });
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

        // click Button Search Gifs
        private void Btn_searchGifs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    ListUtils.ListGifs.Clear();

                    ProgressBarSearchGifs.Visibility = Visibility.Visible;
                    ProgressBarSearchGifs.IsIndeterminate = true;

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { Gifs_Async });
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

        //Event Button Sent Gifs and Insert data user in database
        private void GifsListview_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    GifGiphyClass.Datum selectedGroup = (GifGiphyClass.Datum)GifsListView.SelectedItem;

                    if (selectedGroup != null)
                    {
                        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);
                        string time = DateTime.Now.ToString("hh:mm");

                        var conversation = new MessagesDataFody
                        {
                            Id = unixTimestamp,
                            FromId = UserDetails.UserId,
                            Img_user_message = UserDetails.Avatar,
                            GroupId = GroupId,
                            ToId = UserId,

                            Media = selectedGroup.Images.FixedHeightSmall.Mp4,
                            MediaFileName = selectedGroup.Images.FixedHeightDownsampled.Url,

                            Time = time,
                            TimeText = time,
                            Position = "right",
                            ModelType = MessageModelType.RightGif,
                            ChatColor = ChatColor,
                            EndChatColor = Settings.ColorMessageThemeGradient ? WoWonderTools.GetColorEnd(ChatColor) : ChatColor,

                            Seen = "-1",
                            SendFile = true,
                            ErrorSendMessage = false,
                            ReplyId = ReplyId,
                        };

                        ListUtils.ListMessages.Add(WoWonderTools.MessageFilter(TypeChatPage == "user" ? UserId : GroupId, UserDetails.FullName, conversation, ChatColor, TypeChatPage));

                        Task.Factory.StartNew(() =>
                        {
                            SendMessageTask(unixTimestamp, "", "", "", "", selectedGroup.Images.FixedHeightDownsampled.Url).ConfigureAwait(false);
                        });

                        //Update
                        ViewModel.ModelInstance.UpdateMessage(true);
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

        //Get data image gif from Gifs Table
        private void Txt_searchGifs_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SearchGifsKey = TxtSearchBoxOnline.Text;
                if (string.IsNullOrEmpty(SearchGifsKey))
                {
                    ListUtils.ListGifs.Clear();

                    GifsListView.ItemsSource = ListTrendingGif;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //########################## Stickers ##########################

        #region Stickers

        public static ObservableCollection<StickersModel> FullStickersList = new ObservableCollection<StickersModel>();
        public static ObservableCollection<StickersModel> ListTabStickers = new ObservableCollection<StickersModel>();
        public static ObservableCollection<StickersModel> SearchStickersList = new ObservableCollection<StickersModel>();

        private void StickerSmall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (e.OriginalSource is Button originalSource)
                {
                    var packageId = originalSource.CommandParameter.ToString();

                    var item = FullStickersList.FirstOrDefault(a => a.PackageId == packageId);
                    if (item != null)
                    {
                        StickersListBox.ScrollIntoView(item);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click Button Open Window : Setting_Stickers
        private void Button_S_Setting_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SettingStickersWindow settingStickers = new SettingStickersWindow();
                settingStickers.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Functions Get Stickers
        public void GetStickers()
        {
            try
            {
                SqLiteDatabase database = new SqLiteDatabase();

                var dataStickers = database.Get_From_StickersTable();
                if (dataStickers != null)
                {
                    Dispatcher?.Invoke((Action)delegate // <--- HERE
                    {
                        try
                        {
                            FullStickersList = new ObservableCollection<StickersModel>();
                            ListTabStickers = new ObservableCollection<StickersModel>();

                            foreach (var sticker in dataStickers)
                            {
                                if (sticker.SVisibility)
                                {
                                    FullStickersList.Add(new StickersModel()
                                    {
                                        PackageId = sticker.PackageId,
                                        Name = sticker.SName,
                                        Visibility = sticker.SVisibility ? Visibility.Visible : Visibility.Collapsed,
                                        Count = sticker.SCount,
                                        ListSticker = StickersModel.GetStickers(sticker.PackageId)
                                    });

                                    ListTabStickers.Add(new StickersModel()
                                    {
                                        PackageId = sticker.PackageId,
                                        Name = sticker.SName,
                                        Image = StickersModel.GetStickersIcon(sticker.PackageId)
                                    });
                                }
                            }

                            StickersListBox.ItemsSource = FullStickersList;
                            TabStickersBox.ItemsSource = ListTabStickers;
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // click Button moving to Left
        private void Btn_S_left_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HorizontalScrollViewer.ScrollToLeftEnd();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // click Button moving to Righ
        private void Btn_S_Right_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HorizontalScrollViewer.ScrollToRightEnd();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // click Button Open search for Stickers
        private void Btn_searchStickers_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IconSearchStickers.Tag.ToString() == "Search")
                {
                    TxtSearchStickers.Visibility = Visibility.Visible;

                    HorizontalScrollViewer.Visibility = Visibility.Hidden;
                    BtnSLeft.Visibility = Visibility.Hidden;
                    BtnSRight.Visibility = Visibility.Hidden;

                    IconSearchStickers.Tag = "Close";
                    IconSearchStickers.Kind = PackIconKind.Close;
                }
                else
                {
                    TxtSearchStickers.Visibility = Visibility.Hidden;

                    HorizontalScrollViewer.Visibility = Visibility.Visible;
                    BtnSLeft.Visibility = Visibility.Visible;
                    BtnSRight.Visibility = Visibility.Visible;

                    IconSearchStickers.Tag = "Search";
                    IconSearchStickers.Kind = PackIconKind.Magnify;

                    SearchStickerKey = "";
                    TxtSearchStickers.Text = "";

                    StickersListBox.ItemsSource = FullStickersList;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Txt_searchStickers_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    SearchStickerKey = TxtSearchStickers.Text;

                    if (e.Key == Key.Enter)
                    {
                        if (string.IsNullOrEmpty(SearchStickerKey))
                            return;

                        ProgressBarSearchStickers.Visibility = Visibility.Visible;
                        ProgressBarSearchStickers.IsIndeterminate = true;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => StartSearchStickerRequest() });
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

        private void Txt_searchStickers_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SearchStickerKey = TxtSearchStickers.Text;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async Task StartSearchStickerRequest(string offset = "1")
        {
            if (string.IsNullOrEmpty(SearchStickerKey))
                return;

            SearchStickersList.Clear();

            var result = await StickersModel.ApiStipop.ApiGetSearch(SearchStickerKey, offset);
            if (result != null)
            {
                var respondUserList = result.Body.StickerList.Count;
                if (respondUserList > 0)
                {
                    var sticker = new StickersModel
                    {
                        ListSticker = new List<StickersModel.MyStickerIcon>(),
                        Name = ""
                    };

                    foreach (var item in from item in result.Body.StickerList let check = SearchStickersList.FirstOrDefault(a => a.PackageId == item.StickerId) where check == null select item)
                    {
                        sticker.PackageId = item.StickerId;
                        sticker.ListSticker.Add(new StickersModel.MyStickerIcon() { Icon = item.StickerImg });
                    }

                    Dispatcher?.Invoke((Action)delegate // <--- HERE
                    {
                        try
                        {
                            SearchStickersList.Add(sticker);

                            StickersListBox.ItemsSource = SearchStickersList;

                            ProgressBarSearchStickers.Visibility = Visibility.Collapsed;
                            ProgressBarSearchStickers.IsIndeterminate = false;
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });

                }
            }
        }

        // click Button Send Messages type Sticker
        private void StickerBig_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    if (e.OriginalSource is Button originalSource)
                    {
                        string url = originalSource.Tag?.ToString();

                        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);
                        string time = DateTime.Now.ToString("hh:mm");

                        MessagesDataFody conversation = new MessagesDataFody()
                        {
                            Id = unixTimestamp,
                            FromId = UserDetails.UserId,
                            Img_user_message = UserDetails.Avatar,
                            GroupId = GroupId,
                            ToId = UserId,

                            Media = url,

                            TimeText = time,
                            Position = "right",
                            Seen = "-1",
                            Time = unixTimestamp.ToString(),
                            ModelType = MessageModelType.RightSticker,
                            SendFile = true,
                            ChatColor = ChatColor,
                            EndChatColor = Settings.ColorMessageThemeGradient ? WoWonderTools.GetColorEnd(ChatColor) : ChatColor,
                            ErrorSendMessage = false,
                            ReplyId = ReplyId,
                        };

                        ListUtils.ListMessages.Add(WoWonderTools.MessageFilter(TypeChatPage == "user" ? UserId : GroupId, UserDetails.FullName, conversation, ChatColor, TypeChatPage));

                        Task.Factory.StartNew(() =>
                        {
                            SendMessageTask(unixTimestamp, "", "", url, "1", "").ConfigureAwait(false);
                        });

                        //Update
                        ViewModel.ModelInstance.UpdateMessage(true);
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

        // Run Api :  Send Messages type Sticker / Gifs / Contact
        private async Task SendMessageTask(long hashId, string text, string contact, string sticker, string stickerId, string gifUrl)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    if (TypeChatPage == "user")
                    {
                        (int, dynamic) respond = await RequestsAsync.Message.SendMessageAsync(UserId, hashId.ToString(), text, contact, "", sticker, stickerId, gifUrl, "", "", "", "", ReplyId);
                        if (respond.Item1 == 200)
                        {
                            if (respond.Item2 is SendMessageObject result)
                            {
                                var lastMessage = ListUtils.ListMessages.LastOrDefault(a => a.Id == hashId);
                                if (lastMessage != null)
                                {
                                    lastMessage = WoWonderTools.MessageFilter(UserId, SelectedChatData.Name, result.MessageData.FirstOrDefault(), ChatColor);
                                    lastMessage.Id = result.MessageData[0].Id;

                                    ViewModel.ModelInstance.UpdateChatAllList(lastMessage);

                                    //Update
                                    ViewModel.ModelInstance.UpdateMessage(true);
                                }
                            }
                        }
                    }
                    else
                    {
                        (int, dynamic) respond = await RequestsAsync.GroupChat.Send_MessageToGroupChatAsync(GroupId, hashId.ToString(), text, contact, "", sticker, stickerId, gifUrl, "", "", ReplyId);
                        if (respond.Item1 == 200)
                        {
                            if (respond.Item2 is GroupSendMessageObject res)
                            {
                                var lastMessage = ListUtils.ListMessages.LastOrDefault(a => a.Id == hashId);
                                if (lastMessage != null)
                                {
                                    var item = res.Data.FirstOrDefault();
                                    if (item == null)
                                        return;

                                    lastMessage = WoWonderTools.MessageFilter(GroupId, WoWonderTools.GetNameFinal(item.UserData), item, ChatColor, "group");
                                    lastMessage.Id = item.Id;

                                    ViewModel.ModelInstance.UpdateChatAllList(lastMessage);

                                    //Update
                                    ViewModel.ModelInstance.UpdateMessage(true);
                                }
                            }
                        }
                    }

                    CloseReplyUi();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //########################## Emoji Icon ##########################

        #region Emoji

        #region Emoji Icon Click

        private void NormalSmile_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" :)");
            MessageBoxText.Focus();

        }

        private void LaughingEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" (<");
            MessageBoxText.Focus();

        }

        private void HappyFaceEmoji_Click(object sender, RoutedEventArgs e)
        {

            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" **)");
            MessageBoxText.Focus();
        }

        private void CrazyEmoji_Click(object sender, RoutedEventArgs e)
        {

            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" :p");
            MessageBoxText.Focus();


        }

        private void ChesekyEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" :_p");
            MessageBoxText.Focus();


        }

        private void CoolEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" B)");
            MessageBoxText.Focus();

        }

        private void CringeEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" :D");
            MessageBoxText.Focus();
        }

        private void CossolEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" /_)");
            MessageBoxText.Focus();


        }

        private void AngelEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" 0)");
            MessageBoxText.Focus();

        }

        private void CryingEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" :_(");
            MessageBoxText.Focus();

        }

        private void SadfaceEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun("  :(");
            MessageBoxText.Focus();


        }

        private void KissingEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun("  :*");
            MessageBoxText.Focus();


        }

        private void HeartEmoji_Click(object sender, RoutedEventArgs e)
        {

            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun("  <3");
            MessageBoxText.Focus();


        }

        private void BreakingHeartEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" </3");
            MessageBoxText.Focus();


        }

        private void HeartEyesEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" *_*");
            MessageBoxText.Focus();


        }

        private void StarEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition =
                MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" <5");
            MessageBoxText.Focus();


        }

        private void SurprisedEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition = MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" :o");
            MessageBoxText.Focus();


        }

        private void ScreamEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition =
                MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" :0");
            MessageBoxText.Focus();
        }

        private void PainedFaceEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition =
                MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" o(");
            MessageBoxText.Focus();
        }

        private void DissatisfiedEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition =
                MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" -_(");
            MessageBoxText.Focus();


        }

        private void AngryEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition =
                MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" x(");
            MessageBoxText.Focus();


        }

        private void AngryFace_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition =
                MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun("  X(");
            MessageBoxText.Focus();


        }

        private void FaceWithStraightMouthEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition =
                MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" -_-");
            MessageBoxText.Focus();


        }

        private void PuzzledEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition =
                MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun(" :-/");
            MessageBoxText.Focus();


        }

        private void StraightFacedEmoji_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition =
                MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun("  :|");
            MessageBoxText.Focus();

        }

        private void HeavyExclamation_Click(object sender, RoutedEventArgs e)
        {
            string textRange = new TextRange(MessageBoxText.Document.ContentStart, MessageBoxText.Document.ContentEnd)
                .Text;
            if (textRange.Contains("Write your Message"))
            {
                MessageBoxText.Document.Blocks.Clear();
            }

            MessageBoxText.CaretPosition =
                MessageBoxText.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            MessageBoxText.CaretPosition?.InsertTextInRun("  !_");
            MessageBoxText.Focus();


        }

        #endregion

        #region Emoji Icon Events Click/Mouse

        private void SmileButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Color color = (Color)ConvertFromString(Settings.MainColor);
            EmojiSmileofSmileButton.Foreground = new SolidColorBrush(color);
            ImogiPanel.Visibility = Visibility.Visible;
        }

        private void EmojisPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (UserDetails.ModeDarkStlye)
            {
                EmojiSmileofSmileButton.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                Color color = (Color)ConvertFromString(Settings.SeconderyColor);
                EmojiSmileofSmileButton.Foreground = new SolidColorBrush(color);
            }

            ImogiPanel.Visibility = Visibility.Hidden;
        }

        private void EmojisPanel_MouseMove(object sender, MouseEventArgs e)
        {
            Color color = (Color)ConvertFromString(Settings.MainColor);
            EmojiSmileofSmileButton.Foreground = new SolidColorBrush(color);
            ImogiPanel.Visibility = Visibility.Visible;
        }

        private void EmojisPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            Color color = (Color)ConvertFromString(Settings.MainColor);
            EmojiSmileofSmileButton.Foreground = new SolidColorBrush(color);
        }

        private void SmileButton_Click(object sender, RoutedEventArgs e)
        {


            //>> Next Update

            //EmojisPanel.Visibility = System.Windows.Visibility.Hidden;

            //if (EmojisPanel.Children.Contains(EmojiTabcontrol))
            //{
            //    GifsListview.ItemsSource = null;

            //    RightPanel.Visibility = System.Windows.Visibility.Visible;
            //    UserAboutSection.Visibility = System.Windows.Visibility.Collapsed;
            //    UserFriendsSection.Visibility = System.Windows.Visibility.Collapsed;
            //    UserMediaSection.Visibility = System.Windows.Visibility.Collapsed;
            //    UserDeleteChatSection.Visibility = System.Windows.Visibility.Collapsed;
            //    EmojisPanel.Children.Remove(EmojiTabcontrol);
            //    EmojiTabcontrol.Margin = new Thickness(0, 0, 0, 0);

            //    RightPanel.Children.Add(EmojiTabcontrol);
            //    EmojiTabStop = "no";
            //    RightPanelScrollViewer.ScrollToEnd();

            //}
            //else
            //{
            //    RightPanel.Visibility = System.Windows.Visibility.Collapsed;
            //    UserAboutSection.Visibility = System.Windows.Visibility.Visible;
            //    UserFriendsSection.Visibility = System.Windows.Visibility.Visible;
            //    UserMediaSection.Visibility = System.Windows.Visibility.Visible;
            //    UserDeleteChatSection.Visibility = System.Windows.Visibility.Visible;
            //    RightPanel.Children.Remove(EmojiTabcontrol);
            //    EmojiTabcontrol.Margin = new Thickness(0, 0, 0, 0);
            //    EmojisPanel.Children.Add(EmojiTabcontrol);

            //}

        }


        #endregion

        private void SmileButton_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                Color color = (Color)ConvertFromString(Settings.MainColor);
                //wael
                //if (RightPanel.Children.Contains(EmojiTabcontrol))
                //{
                //    EmojiSmileofSmileButton.Foreground = new SolidColorBrush(color);
                //    ImogiPanel.Visibility = Visibility.Hidden;

                //}
                //else
                if (EmojiTabStop == "yes")
                {
                    EmojiSmileofSmileButton.Foreground = new SolidColorBrush(color);
                    ImogiPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    Color color2 = (Color)ConvertFromString(Settings.SeconderyColor);
                    EmojiSmileofSmileButton.Foreground = new SolidColorBrush(color2);
                    EmojiTabStop = "yes";
                    ImogiPanel.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //########################## Windows ##########################

        #region windows

        private void Btn_Minimize_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                WindowState = WindowState.Minimized;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_Maximize_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Width != 1400)
                {
                    //WindowState = WindowState.Normal;
                    Hide();
                    Width = 1400;
                    Height = 900;
                    Left = 0;
                    Top = 0;
                    WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    Show();
                }
                else
                {
                    Hide();
                    //WindowState = WindowState.Maximized;

                    Width = SystemParameters.PrimaryScreenWidth + 30;
                    Height = SystemParameters.PrimaryScreenHeight + 20;
                    Left = -5;
                    Top = -10;
                    WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_Close_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
                Application.Current.Shutdown();
                Environment.Exit(0);
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
                //Grid gridWindow = sender as Grid;
                //Color darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                //Color whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                //GridWindow = gridWindow;
                //if (ModeDarkStlye)
                //{
                //    if (gridWindow != null) gridWindow.Background = new SolidColorBrush(darkBackgroundColor);
                //}
                //else
                //{
                //    if (gridWindow != null) gridWindow.Background = new SolidColorBrush(whiteBackgroundColor);
                //}
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TitleApp_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //TextBlock titleApp = sender as TextBlock;
                //Color darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                //Color whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                //TitleApp = titleApp;
                //if (ModeDarkStlye)
                //{
                //    titleApp.Foreground = new SolidColorBrush(whiteBackgroundColor);
                //}
                //else
                //{
                //    titleApp.Foreground = new SolidColorBrush(darkBackgroundColor);
                //}
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_Minimize_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Button buttonWindowMinimize = sender as Button;
                //Color darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                //Color whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                //ButtonWindowMin = buttonWindowMinimize;
                //if (ModeDarkStlye)
                //{
                //    buttonWindowMinimize.Foreground = new SolidColorBrush(whiteBackgroundColor);
                //}
                //else
                //{
                //    buttonWindowMinimize.Foreground = new SolidColorBrush(darkBackgroundColor);
                //}
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void btn_Maximize_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Button buttonWindowMaximize = sender as Button;
                //Color darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                //Color whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                //ButtonWindowMax = buttonWindowMaximize;
                //if (ModeDarkStlye)
                //{
                //    buttonWindowMaximize.Foreground = new SolidColorBrush(whiteBackgroundColor);
                //}
                //else
                //{
                //    buttonWindowMaximize.Foreground = new SolidColorBrush(darkBackgroundColor);
                //}
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void btn_Close_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Button buttonWindowClose = sender as Button;
                //Color darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                //Color whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                //ButtonWindowX = buttonWindowClose;
                //if (ModeDarkStlye)
                //{
                //    buttonWindowClose.Foreground = new SolidColorBrush(whiteBackgroundColor);
                //}
                //else
                //{
                //    buttonWindowClose.Foreground = new SolidColorBrush(darkBackgroundColor);
                //}
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Groups

        private void GroupsExpander_OnCollapsed(object sender, RoutedEventArgs e)
        {
            try
            {
                GroupsExpander.Height = 50;
                RowDefinitionGroups.Height = new GridLength(50);
                GroupsList.Height = 0;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void GroupsExpander_OnExpanded(object sender, RoutedEventArgs e)
        {
            try
            {
                GroupsExpander.Height = 260;
                RowDefinitionGroups.Height = new GridLength(260);
                GroupsList.Height = 240;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Chat Group
        private void GroupsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UserListFody selectedGroup = (UserListFody)GroupsList.SelectedItem;
                if (selectedGroup != null)
                {
                    if (selectedGroup.GroupName == Name)
                        return;

                    //getting Name from selected chat
                    ViewModel.ModelInstance.Name = selectedGroup.GroupName;

                    if (selectedGroup.Parts != null)
                        ViewModel.ModelInstance.LastSeen = Methods.FunString.FormatPriceValue(selectedGroup.Parts.Count) + " " + "people join this";

                    ViewModel.ModelInstance.OnPropertyChanged("Name");

                    //getting Avatar from selected chat
                    ViewModel.ModelInstance.AvatarUser = selectedGroup.Avatar;
                    ViewModel.ModelInstance.OnPropertyChanged("AvatarUser");

                    ViewModel.ModelInstance.CloseContactInfo();

                    //Get message 
                    ViewModel.ModelInstance.LoadMessagesDataFody(selectedGroup, "group");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void GroupsList_OnMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                GroupsList.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Create New Group
        private void BtnCreateGroup_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateGroupChatWindow addGroupChat = new CreateGroupChatWindow();
                addGroupChat.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }

        }

        #endregion

        private void VideoCallButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SendVideoCall sendCall = new SendVideoCall(SelectedChatData.Name, UserId, SelectedChatData.Avatar, this);
                sendCall.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ImageProfileButton_OnTap(object sender, RoutedEventArgs e)
        {
            try
            {
                MyProfileWindow profileWindow = new MyProfileWindow();
                profileWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Event

        private async Task GetEvent()
        {
            if (Methods.CheckForInternetConnection() && !string.IsNullOrEmpty(UserDetails.AccessToken) && Settings.EnableEvent)
            {
                var dictionary = new Dictionary<string, string>
                {
                    {"offset", "0"},
                    {"fetch", "events"}
                };

                var (apiStatus, respond) = await RequestsAsync.Event.GetEventsAsync(dictionary, "15").ConfigureAwait(false);
                if (apiStatus != 200 || respond is not GetEventsObject result || result.Events == null)
                {
                    //Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Events.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Events let check = ListUtils.EventsList.FirstOrDefault(a => a.Id == item.Id) where check == null select WoWonderTools.EventFilter(item))
                        {
                            Dispatcher?.Invoke((Action)delegate // <--- HERE
                            {
                                try
                                {
                                    ListUtils.EventsList?.Add(item);
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                        }
                    }

                    Dispatcher?.Invoke((Action)delegate // <--- HERE
                    {
                        try
                        {
                            if (ListUtils.EventsList.Count > 0)
                            {
                                EventList.ItemsSource = ListUtils.EventsList;
                                EventList.Visibility = Visibility.Visible;

                                EmptyPageContentEvent.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                EventList.Visibility = Visibility.Collapsed;
                                EmptyPageContentEvent.Visibility = Visibility.Visible;
                                EmptyPageContentEvent.InflateLayout(EmptyPage.Type.NoEvent);
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

        private void EventList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                EventList.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EventList_OnMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                EventList.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EventStackPanel_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Grid mi = (Grid)sender;
                var eventId = mi.Tag?.ToString();

                var selectedGroup = ListUtils.EventsList.FirstOrDefault(a => a.Id == eventId);
                if (selectedGroup != null)
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EventProfilePanel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                StackPanel mi = (StackPanel)sender;
                var eventId = mi.Tag?.ToString();

                var selectedGroup = ListUtils.EventsList.FirstOrDefault(a => a.Id == eventId);
                if (selectedGroup != null)
                {
                    if (Methods.CheckForInternetConnection())
                    {
                        var p = new Process
                        {
                            StartInfo = new ProcessStartInfo(selectedGroup.UserData.Url) { UseShellExecute = true }
                        };
                        p.Start();
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

    }
}