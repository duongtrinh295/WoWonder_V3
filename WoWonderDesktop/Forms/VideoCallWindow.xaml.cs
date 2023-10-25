using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Timers;
using CefSharp;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for Video_Call_Window.xaml
    /// </summary>
    public partial class VideoCallWindow : Window
    {
        private readonly Timer T;
        private int H, M, S;
        private readonly ChatWindow ChatWindow;
        private readonly Classes.CallVideo Cv; 
        public VideoCallWindow(string result, ChatWindow main, Classes.CallVideo cv)
        {
            try
            {  
                InitializeComponent();
                Title = LocalResources.label_Video_call;

                VideoWEBRTC.Address = result;
                ChatWindow = main;
                Cv = cv;

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }
                
                // Timer Control
                T = new Timer {Interval = 1000};
                // 1 Sec
                T.Elapsed += OnTimerEvent;
                T.Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        // Window State Minimized 
        private void Hide_OnClick(object sender, RoutedEventArgs e)
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

        // Window State Maximized 
        private void BtnFullScreenExpand_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                WindowState = WindowState.Maximized;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        // Event Timer
        private void OnTimerEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                S++;
                switch (S)
                {
                    case 60:
                        S = 0;
                        M++;
                        break;
                }
                switch (M)
                {
                    case 60:
                        M = 0;
                        H++;
                        break;
                }
                Dispatcher?.Invoke((Action)delegate // <--- HERE
                {
                    lbl_Status_time.Content = string.Format("{0}:{1}:{2}", H.ToString().PadLeft(2, '0'),
                        M.ToString().PadLeft(2, '0'), S.ToString().PadLeft(2, '0'));
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Close Timer
        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChatWindow.RunCall = false;
                T?.Close();
                Close();
            }
            catch (Exception exception)
            {
                ChatWindow.RunCall = false;
                Close();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnMessage_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataUsers = ListUtils.ListUsers.FirstOrDefault(a => a.UserId == Cv.CallVideoUserId);
                if (dataUsers != null)
                {
                    var index = ListUtils.ListUsers.IndexOf(dataUsers);
                    switch (index > -1)
                    {
                        case true:
                           // ChatWindow.ChatActivityList.SelectedIndex = index;
                            WindowState = WindowState.Minimized;
                            break;
                    }
                }
                else
                {
                    var dataUsers2 = ListUtils.ListUsersProfile.FirstOrDefault(a => a.UserId == Cv.CallVideoUserId);
                    if (dataUsers2 != null)
                    {
                        var index = ListUtils.ListUsersProfile.IndexOf(dataUsers2);
                        switch (index > -1)
                        {
                            case true:
                               // ChatWindow.UserContactsList.SelectedIndex = index;
                                WindowState = WindowState.Minimized;
                                break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void VideoWEBRTC_OnMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                BorderVideoControls.Visibility = Visibility.Visible;

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);

            }
        }

        private void VideoWEBRTC_OnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                BorderVideoControls.Visibility = Visibility.Collapsed;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        
        private void VideoControls_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (BorderVideoControls.IsVisible)
            {
                case true:
                    BorderVideoControls.Visibility = Visibility.Collapsed;
                    break;
                default:
                    BorderVideoControls.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void BtnFullScreenCompress_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void VideoWEBRTC_OnMouseMove(object sender, MouseEventArgs e)
        {
            switch (BorderVideoControls.IsVisible)
            {
                case true:
                    BorderVideoControls.Visibility = Visibility.Collapsed;
                    break;
                default:
                    BorderVideoControls.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void VideoWEBRTC_OnLoadingStateChanged(object  sender, LoadingStateChangedEventArgs e)
        {
             
        }
    }
}
