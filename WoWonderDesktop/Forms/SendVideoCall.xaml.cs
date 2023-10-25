using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using WoWonderClient;
using WoWonderClient.Classes.Call;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using WoWonderDesktop.Helpers.Controls;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for Send_Video_Call.xaml
    /// </summary>

    public partial class SendVideoCall : Window
    {
        #region Variables

        private string CallId = "";
        private string Url = "";
        private string ReciptId = "";
        private string UserName = "";
        private string MainAvatar = "";
        private readonly ChatWindow ChatWindow;

        #endregion

        #region Timer And MediaPlayer

        DispatcherTimer Timer = null;
        DispatcherTimer Timer2 = null;
        private int CounTCallTime = 0;
        readonly MediaPlayer MediaPlayer = new MediaPlayer();

        #endregion

        public SendVideoCall(string userName, string reciptId, string avatar, ChatWindow main)
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_SendVideoCall ;

                TextOfcall.Text = LocalResources.label_Calling + " " + userName;
                ProfileImage.Source = new BitmapImage(new Uri(avatar));
                MainAvatar = avatar;
                ReciptId = reciptId;
                UserName = userName;
                ChatWindow = main;
                
                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }

                if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                    UserDetails.Socket?.EmitAsync_Create_callEvent(reciptId);

                CreateCallAsync(reciptId);
                 
                var callsound = Methods.MainDestination + "calling.mp3";
                if (Directory.Exists(callsound) == false)
                {
                    File.WriteAllBytes(callsound, Properties.Resources.calling);
                }

                MediaPlayer.Open(new Uri(callsound));
                MediaPlayer.Volume = 1;
                MediaPlayer.Play();
                StartTimer();
                 
                ModeDark_Window();
            }
            catch (Exception exception)
            {
                Console.Write(exception.Message);
            }
        }

        private async void CreateCallAsync(string reciptId)
        {
            try
            {
                //Send_Video_Call
                var (apiStatus, respond) = await RequestsAsync.Call.CreateNewCallTwilioAsync(reciptId, TypeCall.Video);
                switch (apiStatus)
                {
                    case 200: 
                        if (respond is CallUserObject.DataCallUser result)
                        {
                            CallId = result.Id;
                            Url = result.Url;
                            StartApiWaitAnswer();
                        }
                        break;
                    default:
                        CloseWindow();
                        break;
                }
            }
            catch (Exception exception)
            {
                Console.Write(exception.Message);
            }
        }

        private void StartTimer()
        {
            try
            {
                Timer = new DispatcherTimer();
                Timer.Interval = TimeSpan.FromSeconds(18);
                Timer.Tick += new EventHandler(timer_Elapsed);
                Timer.Start();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StartApiWaitAnswer()
        {
            try
            {
                Timer2 = new DispatcherTimer();
                Timer2.Interval = TimeSpan.FromSeconds(5);
                Timer2.Tick += new EventHandler(Cheker_Elapsed);
                Timer2.Start();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void timer_Elapsed(object sender, EventArgs e)
        {
            try
            {
                switch (CounTCallTime <= 75)
                {
                    case true:
                    {
                        var callsound = Methods.MainDestination + "calling.mp3";
                        MediaPlayer.Open(new Uri(callsound));
                        MediaPlayer.Volume = 1;
                        MediaPlayer.Play();
                        CounTCallTime += 18;
                        break;
                    }
                    default:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Call.DeclineCallTwilioAsync(CallId, TypeCall.Video) });
                        CloseWindow();
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Cheker_Elapsed(object sender, EventArgs e)
        {
            try
            {
                Checke_Video_Call_Answer(CallId).ConfigureAwait(false); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task Checke_Video_Call_Answer(string callId)  
        {
            try
            {
                var response = await RequestsAsync.Call.CheckForAnswerTwilioAsync(callId, TypeCall.Video);
                switch (response.Item1)
                {
                    case 200:
                        Timer2?.Stop();
                        Timer?.Stop();

                        Dispatcher?.Invoke((Action)delegate // <--- HERE
                        {
                            try
                            {
                                MediaPlayer.Stop();

                                Classes.CallVideo cv = new Classes.CallVideo
                                {
                                    CallVideoUserId = ReciptId,
                                    CallVideoUserName = UserName,
                                    CallVideoAvatar = MainAvatar,
                                    CallVideoCallId = callId,
                                    CallVideoTupeIcon = "CallMade",
                                    CallVideoColorIcon = "green",
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
                             
                                VideoCallWindow videoCallFrm = new VideoCallWindow(Url, ChatWindow, cv);
                                videoCallFrm.Show();
                                CloseWindow();
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                        break; 
                    case 300:
                         
                        break; 
                    default:
                        Dispatcher?.Invoke((Action)delegate // <--- HERE
                        {
                            MediaPlayer.Stop();
                            TextOfcall.Text = UserName + " " + LocalResources.label_Declined_your_call;

                            Task.Run(() =>
                            {
                                Thread.Sleep(4500);
                                CloseWindow(); 
                            });
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
                CloseWindow();
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

                Classes.CallVideo cv = new Classes.CallVideo();

                cv.CallVideoUserId = ReciptId;
                cv.CallVideoUserName = UserName;
                cv.CallVideoCallId = CallId;
                cv.CallVideoAvatar = MainAvatar;
                cv.CallVideoTupeIcon = "CallMissed";
                cv.CallVideoColorIcon = "red";
                cv.CallVideoUserDataTime = Methods.Time.GetDataTime();
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

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Call.CloseCallTwilioAsync(CallId, TypeCall.Video) });

                CloseWindow();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void CloseWindow()
        {
            try
            {
                Dispatcher?.Invoke((Action)(() =>
                {
                    try
                    {
                        MediaPlayer?.Close();

                        Timer2?.Stop();
                        Timer?.Stop();

                        Close();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e); 
                    }
                }));
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

                        Lbl_callingUser.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        TextOfcall.Foreground = new SolidColorBrush(whiteBackgroundColor);
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
    }
}
