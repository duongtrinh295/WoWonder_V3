using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderClient.Classes.Call;
using WoWonderClient.Requests;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.SQLiteDB;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for UsersBlocked_Window.xaml
    /// </summary>
    public partial class RecieveCallWindow : Window
    {
        #region Variables

        private readonly string MainIDuser = "", MainName = "", MainCallId = "", MainAvatar = "";
        private readonly ChatWindow ChatWindow;
        private DispatcherTimer Timer;
        private int CountCallTime;
        private readonly MediaPlayer MediaPlayer = new MediaPlayer();

        #endregion
         
        public RecieveCallWindow(string duser, string avatar, string name, string callId, ChatWindow main)
        { 
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_RecieveCall;
                
                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }

                MainIDuser = duser;
                MainName = name;
                MainCallId = callId;
                MainAvatar = avatar;
                string avatarSplit = MainAvatar.Split('/').Last();
                ProfileImage.Source = new BitmapImage(new Uri(Methods.MultiMedia.Get_image(MainIDuser, avatarSplit, MainAvatar)));
                ChatWindow = main;

                TextOfcall.Text = name + " " + LocalResources.label_is_calling_you;

                var callsound = Methods.MainDestination + "videocall.mp3";
                if (Directory.Exists(callsound) == false)
                {
                    File.WriteAllBytes(callsound, Properties.Resources.video_call);
                }

                MediaPlayer.Open(new Uri(callsound));
                MediaPlayer.Volume = 1;
                MediaPlayer.Play();
                StartTimer();
                 
                ModeDark_Window();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        void StartTimer()
        {
            try
            {
                Timer = new DispatcherTimer();
                Timer.Interval = TimeSpan.FromSeconds(3);
                Timer.Tick += new EventHandler(timer_Elapsed);
                Timer?.Start();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        void timer_Elapsed(object sender, EventArgs e)
        {
            try
            {
                switch (CountCallTime <= 40)
                {
                    case true when ChatWindow.RunCall:
                        {
                            var callsound = Methods.MainDestination + "videocall.mp3";
                            MediaPlayer.Open(new Uri(callsound));
                            MediaPlayer.Volume = 1;
                            MediaPlayer.Play();
                            CountCallTime += 3;
                            break;
                        }
                    default:
                        ChatWindow.RunCall = false;
                        Close();
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MediaPlayer.Stop();
                var callsound = Methods.MainDestination + "Decline_Call.mp3";
                if (Directory.Exists(callsound) == false)
                {
                    File.WriteAllBytes(callsound, Properties.Resources.Decline_Call);
                }

                MediaPlayer.Open(new Uri(callsound));
                MediaPlayer.Volume = 1;
                MediaPlayer.Play();

                string avatarSplit = MainAvatar.Split('/').Last();

                Classes.CallVideo cv = new Classes.CallVideo
                {
                    CallVideoUserId = MainIDuser,
                    CallVideoAvatar = Methods.MultiMedia.Get_image(MainIDuser, avatarSplit, MainAvatar),
                    CallVideoUserName = MainName,
                    CallVideoCallId = MainCallId,
                    CallVideoTupeIcon = "CallMissed",
                    CallVideoColorIcon = "red",
                    CallVideoUserDataTime = Methods.Time.GetDataTime()
                };
                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        cv.SColorBackground = "#232323";
                        cv.SColorForeground = "#efefef";
                        break;
                    default:
                        cv.SColorBackground = "#ffffff";
                        cv.SColorForeground = "#444444";
                        break;
                }

                SqLiteDatabase database = new SqLiteDatabase();
                database.Insert_To_CallVideoTable(cv);

                //switch (ChatWindow.NoCall.Visibility)
                //{
                //    case Visibility.Visible:
                //        ChatWindow.NoCall.Visibility = Visibility.Collapsed;
                //        break;
                //}

                Timer?.Stop();

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Call.DeclineCallTwilioAsync(MainCallId, TypeCall.Video) });

                ChatWindow.RunCall = false;
                Close();
            }
            catch (Exception exception)
            {
                ChatWindow.RunCall = false;
                Close();
                Methods.DisplayReportResultTrack(exception);
            } 
        }

        private async void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                Timer?.Stop();

                var (apiStatus, respond) = await RequestsAsync.Call.AnswerCallTwilioAsync(MainCallId, TypeCall.Video);
                switch (apiStatus)
                {
                    case 200:
                    {
                        switch (respond)
                        {
                            case AnswerCallObject callObject:
                            {
                                string avatarSplit = MainAvatar.Split('/').Last();
                                Classes.CallVideo cv = new Classes.CallVideo
                                {
                                    CallVideoUserId = MainIDuser,
                                    CallVideoAvatar = Methods.MultiMedia.Get_image(MainIDuser, avatarSplit, MainAvatar),
                                    CallVideoUserName = MainName,
                                    CallVideoCallId = MainCallId,
                                    CallVideoTupeIcon = "CallReceived",
                                    CallVideoColorIcon = "green",
                                    CallVideoUserDataTime = Methods.Time.GetDataTime(),
                                    SColorBackground = UserDetails.ModeDarkStlye ? "#232323" : "#ffffff",
                                    SColorForeground = UserDetails.ModeDarkStlye ? "#efefef" : "#444444",
                                };
                                         
                                SqLiteDatabase database = new SqLiteDatabase();
                                database.Insert_To_CallVideoTable(cv);


                                //switch (ChatWindow.NoCall.Visibility)
                                //{
                                //    case Visibility.Visible:
                                //        ChatWindow.NoCall.Visibility = Visibility.Collapsed;
                                //        break;
                                //}
                                 
                                VideoCallWindow videoCallFrm = new VideoCallWindow(callObject.Url, ChatWindow, cv); //maybe bug check
                                videoCallFrm.Show();

                                Close();
                                break;
                            }
                        }

                        break;
                    }
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            } 
        }

        private void ModeDark_Window()
        {
            var darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
            var whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

            switch (UserDetails.ModeDarkStlye)
            {
                case true:
                    Background = new SolidColorBrush(darkBackgroundColor);

                    Border.Background = new SolidColorBrush(darkBackgroundColor);

                    Lbl_callingUser.Foreground = new SolidColorBrush(whiteBackgroundColor);
                    TextOfcall.Foreground = new SolidColorBrush(whiteBackgroundColor);
                    break;
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
    }
}
