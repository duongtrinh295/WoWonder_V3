using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using WoWonderClient;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using WoWonderDesktop.Forms;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.Library;
using WoWonderDesktop.SQLiteDB;
using WpfAnimatedGif;

namespace WoWonderDesktop.Controls
{
    /// <summary>
    /// Interaction logic for ConversationControl.xaml
    /// </summary>
    public partial class ConversationControl : UserControl, AnjoListBoxScrollListener.IListBoxOnScrollListener
    {
        public static ConversationControl ControlInstance { get; private set; }
        private MessagesDataFody SelectedChat;

        public MediaPlayer MediaPlayer = new MediaPlayer();
        public DispatcherTimer Timer = new DispatcherTimer();
        public readonly AnjoListBoxScrollListener BoxScrollListener;

        public ConversationControl()
        {
            InitializeComponent();
            ControlInstance = this;
            BoxScrollListener = new AnjoListBoxScrollListener(ChatMessgaeslistBox);
            BoxScrollListener.SetScrollListener(this);
        }
          
        #region Menu Message

        private void DropDownMenuMessage_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems?.Count > 0 && e.AddedItems[0] is string item)
                {
                    var source = (ListBox)e.Source;
                    MessagesDataFody selectedGroup = (MessagesDataFody)source.DataContext;
                    if (selectedGroup != null)
                    {
                        if (item == LocalResources.label5_MessageInfo)
                        {
                            MessageInfoWindow infoWindow = new MessageInfoWindow(selectedGroup);
                            infoWindow.ShowDialog();
                        }
                        else if (item == LocalResources.label5_DeleteMessage)
                        {
                            if (Methods.CheckForInternetConnection())
                            {
                                var index = ListUtils.ListMessages.IndexOf(selectedGroup);
                                if (index != -1)
                                {
                                    ListUtils.ListMessages.Remove(selectedGroup);
                                }

                                if (ChatWindow.TypeChatPage == "user")
                                {
                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                    dbDatabase.Delete_OneMessageUser(selectedGroup.Id);

                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.DeleteMessageAsync(selectedGroup.Id.ToString()) });
                                }
                                else
                                {
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.GroupChat.DeleteGroupChatAsync(selectedGroup.Id.ToString()) });
                                }
                            }
                        }
                        else if (item == LocalResources.label5_Reply)
                        {
                            ChatWindow.ChatWindowContext.ReplyItems(selectedGroup);
                        }
                        else if (item == LocalResources.label5_Forward)
                        {
                            ForwardMessageWindow forwardMessageWindow = new ForwardMessageWindow(selectedGroup);
                            forwardMessageWindow.ShowDialog();
                        }
                        else if (item == LocalResources.label5_Pin || item == LocalResources.label5_UnPin)
                        {
                            var data = ListUtils.ListMessages.FirstOrDefault(a => a.Id == selectedGroup.Id);
                            if (data != null)
                            {
                                var isPinned = !selectedGroup.IsPinned;

                                selectedGroup.IsPinned = isPinned;
                                data.IsPinned = isPinned;

                                var indexItem = data.MenuMessageItems.IndexOf(item);
                                if (indexItem > -1)
                                {
                                    data.MenuMessageItems[indexItem] = isPinned ? LocalResources.label5_UnPin : LocalResources.label5_Pin;
                                    selectedGroup.MenuMessageItems[indexItem] = isPinned ? LocalResources.label5_UnPin : LocalResources.label5_Pin;
                                }

                                var pinnedMessage = ListUtils.PinnedMessageList.FirstOrDefault(a => a.Id == selectedGroup.Id);
                                if (pinnedMessage != null)
                                {
                                    ListUtils.PinnedMessageList.Remove(pinnedMessage);
                                }

                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                dbDatabase.Insert_Or_Update_To_one_MessagesTable(selectedGroup);
                                 
                                if (Methods.CheckForInternetConnection())
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.PinMessageAsync(selectedGroup.Id.ToString(), ChatWindow.ChatId, isPinned ? "yes" : "no", "user") });
                            }
                        }
                        else if (item == LocalResources.label5_Favorite || item == LocalResources.label5_UnFavorite)
                        {
                            var data = ListUtils.ListMessages.FirstOrDefault(a => a.Id == selectedGroup.Id);
                            if (data != null)
                            {
                                selectedGroup.Fav = selectedGroup.Fav == "yes" ? "no" : "yes";
                                 
                                selectedGroup.FavoriteVisibility = selectedGroup.Fav == "yes" ? Visibility.Visible : Visibility.Collapsed;
                                data.Fav = selectedGroup.Fav;

                                var indexItem = data.MenuMessageItems.IndexOf(item);
                                if (indexItem > -1)
                                {
                                    data.MenuMessageItems[indexItem] = selectedGroup.Fav == "yes" ? LocalResources.label5_UnFavorite : LocalResources.label5_Favorite;
                                    selectedGroup.MenuMessageItems[indexItem] = selectedGroup.Fav == "yes" ? LocalResources.label5_UnFavorite : LocalResources.label5_Favorite;
                                }

                                var startedMessage = ListUtils.StartedMessageList.FirstOrDefault(a => a.Id == selectedGroup.Id);
                                if (startedMessage != null)
                                {
                                    ListUtils.StartedMessageList.Remove(startedMessage);
                                }

                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                dbDatabase.Insert_Or_Update_To_one_MessagesTable(selectedGroup);

                                if (Methods.CheckForInternetConnection())
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.FavoriteMessageAsync(selectedGroup.Id.ToString(), ChatWindow.ChatId, selectedGroup.Fav == "yes" ? "yes" : "no", "user") });
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
         
        private void DropDownMenuMessage_OnMouseMove(object sender, MouseEventArgs e)
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

        #region Reaction

        private void ReactionMenuMessage_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems?.Count > 0 && e.AddedItems[0] is ReactFody item)
                {
                    var source = (ListBox)e.Source;
                    MessagesDataFody dataMessageObject = (MessagesDataFody)source.DataContext;

                    string resReact = item.React_Image;
                    dataMessageObject.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();

                    if (item.React_Type == "like")
                    {
                        dataMessageObject.Reaction.Type = "1";
                        string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Like").Value?.Id ?? "1";

                        if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            UserDetails.Socket?.EmitAsync_message_reaction(dataMessageObject.Id.ToString(), react, UserDetails.AccessToken);
                        else
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(dataMessageObject.Id.ToString(), react) });
                    }
                    else if (item.React_Type == "love")
                    {
                        dataMessageObject.Reaction.Type = "2";
                        string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Love").Value?.Id ?? "2";

                        if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            UserDetails.Socket?.EmitAsync_message_reaction(dataMessageObject.Id.ToString(), react, UserDetails.AccessToken);
                        else
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(dataMessageObject.Id.ToString(), react) });
                    }
                    else if (item.React_Type == "haha")
                    {
                        dataMessageObject.Reaction.Type = "3";
                        string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "HaHa").Value?.Id ?? "3";

                        if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            UserDetails.Socket?.EmitAsync_message_reaction(dataMessageObject.Id.ToString(), react, UserDetails.AccessToken);
                        else
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(dataMessageObject.Id.ToString(), react) });
                    }
                    else if (item.React_Type == "wow")
                    {
                        dataMessageObject.Reaction.Type = "4";
                        string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Wow").Value?.Id ?? "4";
                       
                        if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            UserDetails.Socket?.EmitAsync_message_reaction(dataMessageObject.Id.ToString(), react, UserDetails.AccessToken);
                        else
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(dataMessageObject.Id.ToString(), react) });
                    }
                    else if (item.React_Type == "sad")
                    {
                        dataMessageObject.Reaction.Type = "5";
                        string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Sad").Value?.Id ?? "5";

                        if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            UserDetails.Socket?.EmitAsync_message_reaction(dataMessageObject.Id.ToString(), react, UserDetails.AccessToken);
                        else
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(dataMessageObject.Id.ToString(), react) });
                    }
                    else if (item.React_Type == "angry")
                    {
                        dataMessageObject.Reaction.Type = "6";
                        string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Angry").Value?.Id ?? "6";

                        if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            UserDetails.Socket?.EmitAsync_message_reaction(dataMessageObject.Id.ToString(), react, UserDetails.AccessToken);
                        else
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(dataMessageObject.Id.ToString(), react) });
                    }

                    dataMessageObject.Reaction.IsReacted = true;
                    dataMessageObject.Reaction.Count++;

                    BitmapImage logo = new BitmapImage();
                    logo.BeginInit();
                    logo.UriSource = new Uri(resReact);
                    logo.EndInit();

                    dataMessageObject.ReactionImage = logo;
                    dataMessageObject.ReactionVisibility = Visibility.Visible;

                    var dataClass = ListUtils.ListMessages?.FirstOrDefault(a => a.Id == dataMessageObject.Id);
                    if (dataClass != null)
                    {
                        dataClass = dataMessageObject;
                        dataClass.Reaction = dataMessageObject.Reaction;
                        dataClass.ReactionImage = dataMessageObject.ReactionImage;
                        dataClass.ReactionVisibility = dataMessageObject.ReactionVisibility;

                        if (ChatWindow.TypeChatPage == "user")
                        {
                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.Insert_Or_Update_To_one_MessagesTable(dataClass);
                        }
                         
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        ////////////////////// 
        /// message type Sound
        ////////////////////// 

        #region Sound

        //Click Button Download Sound in cash
        private void Btn_Sound_Download_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {

                    Button mi = (Button)sender;
                    var messagesId = long.Parse(mi.CommandParameter?.ToString() ?? "0");
                     
                    var selectedGroup = ListUtils.ListMessages.FirstOrDefault(a => a.Id == messagesId);

                    if (selectedGroup != null)
                    {
                        if (selectedGroup.Download_Visibility == Visibility.Visible)
                        {
                            selectedGroup.ProgresSVisibility = Visibility.Visible;
                            selectedGroup.Download_Visibility = Visibility.Collapsed;
                            selectedGroup.Play_Visibility = Visibility.Collapsed;
                            selectedGroup.Pause_Visibility = Visibility.Collapsed;
                           
                            Task.Factory.StartNew(() =>
                            {
                                Methods.MultiMedia.GetFile(ChatWindow.TypeChatPage == "user" ? ChatWindow.UserId : ChatWindow.GroupId, selectedGroup.MediaFileName, "sound", selectedGroup.Media);
                            });
                        }
                    }
                }
                else
                {
                    MessageBox.Show(LocalResources.label5_Error);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click Button Play Sound
        private void Btn_Play_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button mi = (Button)sender;
                var messagesId = long.Parse(mi.CommandParameter?.ToString() ?? "0");

                if (MediaPlayer.CanPause)
                {
                    MediaPlayer.Pause();
                    Timer.Stop();
                }

                var removePause = ListUtils.ListMessages.Where(a => a.Pause_Visibility == Visibility.Visible).ToList();
                if (removePause?.Count > 0)
                {
                    foreach (var pauseIcon in removePause)
                    {
                        pauseIcon.Pause_Visibility = Visibility.Collapsed;
                        pauseIcon.Play_Visibility = Visibility.Visible;
                    }
                }

                var selectedGroup = ListUtils.ListMessages.FirstOrDefault(a => a.Id == messagesId);

                if (selectedGroup?.Play_Visibility == Visibility.Visible)
                {
                    selectedGroup.ProgresSVisibility = Visibility.Collapsed;
                    selectedGroup.Play_Visibility = Visibility.Collapsed;
                    selectedGroup.Pause_Visibility = Visibility.Visible;
                    ViewModel.ModelInstance.UpdateMessage(false);

                    string soundPath = Methods.MultiMedia.GetFile(ChatWindow.TypeChatPage == "user" ? ChatWindow.UserId : ChatWindow.GroupId, selectedGroup.MediaFileName, "sound", selectedGroup.Media);

                    SelectedChat = selectedGroup;
                    if (selectedGroup.sound_slider_value > 0)
                    {

                        MediaPlayer = new MediaPlayer();
                        MediaPlayer.Open(new Uri(soundPath));
                        double dd = Convert.ToDouble(selectedGroup.sound_slider_value);
                        MediaPlayer.Position = TimeSpan.FromSeconds(dd);
                        MediaPlayer.Play();
                        Timer.Start();
                    }
                    else
                    {
                        MediaPlayer = new MediaPlayer();
                        MediaPlayer.Open(new Uri(soundPath));
                        MediaPlayer.Play();

                        Timer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(1)
                        };
                        Timer.Tick += timer_Tick;
                        Timer.Start();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click Button Pause Sound
        private void Btn_Pause_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button mi = (Button)sender;
                var messagesId = long.Parse(mi.CommandParameter?.ToString() ?? "0");

                var selectedGroup = ListUtils.ListMessages.FirstOrDefault(a => a.Id == messagesId);
                if (selectedGroup?.Pause_Visibility == Visibility.Visible)
                {
                    if (MediaPlayer.CanPause)
                    {
                        MediaPlayer.Pause();
                        Timer.Stop();
                    }

                    selectedGroup.Pause_Visibility = Visibility.Collapsed;
                    selectedGroup.Play_Visibility = Visibility.Visible;
                    ViewModel.ModelInstance.UpdateMessage(false);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if ((MediaPlayer.Source != null) && (MediaPlayer.NaturalDuration.HasTimeSpan))
                {
                    if (Convert.ToInt32(MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds) != SelectedChat.sound_slider_value)
                    {
                        SelectedChat.sound_slider_value = Convert.ToInt32(MediaPlayer.Position.TotalSeconds);

                        SelectedChat.sound_time = TimeSpan.FromSeconds(SelectedChat.sound_slider_value).ToString(TimeSpan.FromSeconds(MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds).ToString(@"hh") == "00" ? @"mm\:ss" : @"hh\:mm\:ss");
                        SelectedChat.Progress_Value = (int)MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    }
                    else
                    {
                        SelectedChat.Progress_Value = 0;
                        SelectedChat.sound_slider_value = 0;
                        SelectedChat.Pause_Visibility = Visibility.Collapsed;
                        SelectedChat.Play_Visibility = Visibility.Visible;
                        MediaPlayer.Stop();
                        Timer.Stop();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RangeBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                Slider mi = (Slider)sender;
                if (MediaPlayer != null && MediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    mi.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Thumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            try
            {
                Slider mi = (Slider)sender;
                TimeSpan ts = new TimeSpan(0, 0, 0, (int)mi.Value);
                MediaPlayer.Position = ts;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UIElement_OnDragEnter(object sender, DragEventArgs e)
        {
            try
            {
                Slider mi = (Slider)sender;
                if (mi.Template.FindName("Thumb", mi) is Thumb t)
                {
                    Mouse.Capture(t);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        ////////////////////// 
        /// message type Image
        ////////////////////// 

        #region Image

        private void Btn_Open_img_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button mi = (Button)sender;
                var messagesId = long.Parse(mi.CommandParameter?.ToString() ?? "0");

                var selectedGroup = ListUtils.ListMessages.FirstOrDefault(a => a.Id == messagesId);
                if (selectedGroup != null)
                {
                    if (selectedGroup.ModelType == MessageModelType.LeftMap || selectedGroup.ModelType == MessageModelType.RightMap)
                    {
                        var url = "https://maps.google.com/?q=" + selectedGroup.Lat + "," + selectedGroup.Lng;
                        var p = new Process
                        {
                            StartInfo = new ProcessStartInfo(url) { UseShellExecute = true }
                        };
                        p.Start();
                    }
                    else
                    {
                        Methods.MultiMedia.open_img(ChatWindow.TypeChatPage == "user" ? ChatWindow.UserId : ChatWindow.GroupId, selectedGroup.MediaFileName, selectedGroup.Media);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click Button Download Image in PC
        private void Btn_Save_img_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    Button mi = (Button)sender;
                    var messagesId = long.Parse(mi.CommandParameter?.ToString() ?? "0");

                    var selectedGroup = ListUtils.ListMessages.FirstOrDefault(a => a.Id == messagesId);
                    if (selectedGroup != null)
                    {
                        string pathDeck = Methods.MultiMedia.GetFile(ChatWindow.TypeChatPage == "user" ? ChatWindow.UserId : ChatWindow.GroupId, selectedGroup.MediaFileName, "images", selectedGroup.Media);
                        if (pathDeck.Contains("http:"))
                        {
                            MessageBox.Show(LocalResources.label5_ThisFileDownload);
                        }

                        SaveFileDialog sfd = new SaveFileDialog
                        {
                            Filter = "Images|*.png;*.bmp;*.png",
                            FileName = selectedGroup.MediaFileName
                        };
                        ImageFormat format = ImageFormat.Png;
                        bool? sdf = sfd.ShowDialog();
                        if (sdf == true)
                        {
                            string ext = Path.GetExtension(sfd.FileName);
                            switch (ext)
                            {
                                case ".png":
                                    format = ImageFormat.Jpeg;
                                    break;
                                case ".bmp":
                                    format = ImageFormat.Bmp;
                                    break;
                                case ".emf":
                                    format = ImageFormat.Emf;
                                    break;
                                case ".exif":
                                    format = ImageFormat.Exif;
                                    break;
                                case ".icon":
                                    format = ImageFormat.Icon;
                                    break;
                                case ".memoryBmp":
                                    format = ImageFormat.MemoryBmp;
                                    break;
                                case ".tiff":
                                    format = ImageFormat.Tiff;
                                    break;
                                case ".wmf":
                                    format = ImageFormat.Wmf;
                                    break;
                            }

                            Console.WriteLine(format);
                            File.Copy(pathDeck, sfd.FileName, true);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(LocalResources.label5_Error);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        ////////////////////// 
        /// message type video
        ////////////////////// 

        #region video

        //Click Button Play video
        private void Btn_Play_video_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button mi = (Button)sender;
                var messagesId = long.Parse(mi.CommandParameter?.ToString() ?? "0");

                var selectedGroup = ListUtils.ListMessages.FirstOrDefault(a => a.Id == messagesId);
                if (selectedGroup != null)
                {
                    string pathDeck = Methods.MultiMedia.GetFile(ChatWindow.TypeChatPage == "user" ? ChatWindow.UserId : ChatWindow.GroupId, selectedGroup.MediaFileName, "video", selectedGroup.Media);
                    VideoMediaPlayerWindow vmp = new VideoMediaPlayerWindow(pathDeck);
                    vmp.ShowDialog();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click Button Download video  in cash
        private void Btn_Download_video_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    Button mi = (Button)sender;
                    var messagesId = long.Parse(mi.CommandParameter?.ToString() ?? "0");

                    var selectedGroup = ListUtils.ListMessages.FirstOrDefault(a => a.Id == messagesId);

                    if (selectedGroup != null)
                    {
                        if (selectedGroup.Download_Visibility == Visibility.Visible)
                        {
                            selectedGroup.ProgresSVisibility = Visibility.Visible;
                            selectedGroup.Download_Visibility = Visibility.Collapsed;
                            selectedGroup.Play_Visibility = Visibility.Collapsed;

                            Task.Factory.StartNew(() =>
                            {
                                Methods.MultiMedia.SaveFile(ChatWindow.TypeChatPage == "user" ? ChatWindow.UserId : ChatWindow.GroupId, selectedGroup.MediaFileName, "video", selectedGroup.Media);
                            });
                        }
                    }
                }
                else
                {
                    MessageBox.Show(LocalResources.label5_Error);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region file

        /// message type file
        ////////////////////// 
        //Click Hyper link Download file
        private void HyperlinkDownload_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    Button mi = (Button)sender;
                    var messagesId = long.Parse(mi.CommandParameter?.ToString() ?? "0");

                    var selectedGroup = ListUtils.ListMessages.FirstOrDefault(a => a.Id == messagesId);

                    if (selectedGroup?.Download_Visibility == Visibility.Visible)
                    {
                        selectedGroup.ProgresSVisibility = Visibility.Visible;
                        selectedGroup.Download_Visibility = Visibility.Collapsed;
                        selectedGroup.Icon_File_Visibility = Visibility.Collapsed;
                        selectedGroup.Hlink_Download_Visibility = Visibility.Collapsed;
                        selectedGroup.Hlink_Open_Visibility = Visibility.Visible;

                        Task.Factory.StartNew(() =>
                        {
                            Methods.MultiMedia.SaveFile(ChatWindow.TypeChatPage == "user" ? ChatWindow.UserId : ChatWindow.GroupId, selectedGroup.MediaFileName, "file", selectedGroup.Media);
                        });
                    }
                }
                else
                {
                    MessageBox.Show(LocalResources.label5_Error);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click  Hyper link Open file from deck PC
        private void HyperlinkOpen_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button mi = (Button)sender;
                var messagesId = long.Parse(mi.CommandParameter?.ToString() ?? "0");

                var selectedGroup = ListUtils.ListMessages.FirstOrDefault(a => a.Id == messagesId);
                if (selectedGroup != null)
                {
                    string pathdeck = Methods.MultiMedia.GetFile(ChatWindow.TypeChatPage == "user" ? ChatWindow.UserId : ChatWindow.GroupId, selectedGroup.MediaFileName, "file", selectedGroup.Media);

                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo(pathdeck) { UseShellExecute = true }
                    };
                    p.Start();

                    e.Handled = true;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        ////////////////////// 
        /// message type gifs
        ////////////////////// 

        #region Gifs

        private void Gifs_Media_OnAnimationLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Image b)
                {
                    DependencyObject item = b;
                    while (item is ListBoxItem == false)
                    {
                        item = VisualTreeHelper.GetParent(b);
                    }

                    ListBoxItem lbi = (ListBoxItem)item;
                    if ((sender as FrameworkElement).DataContext is MessagesDataFody data)
                    {
                        //data.G_Bar_load_gifSVisibility = Visibility.Collapsed;
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

        #endregion

        #region gifs



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

        #endregion

        private void ImageUser_OnTap(object sender, RoutedEventArgs e)
        { 
            try
            {
                RoundProfileButton mi = (RoundProfileButton)sender; 
                var messagesId = long.Parse(mi.Tag?.ToString() ?? "0");

                var selectedGroup = ListUtils.ListMessages.FirstOrDefault(a => a.Id == messagesId);
                if (selectedGroup != null)
                {
                    if (selectedGroup.MessageUser?.UserDataClass?.UserId == UserDetails.UserId)
                    {
                        
                    }
                    else
                    {
                        if (!ViewModel.ModelInstance.IsContactInfoOpen)
                            ViewModel.ModelInstance.OpenContactinfoCommand.Execute("Open");

                    }
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }
         
        public void OnLoadMore()
        {
             
        }

        public void OnLoadUp()
        {
            ChatWindow.ChatWindowContext?.LoadMoreUserMessages();
        }
         
    }
} 