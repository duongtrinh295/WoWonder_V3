using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for MsgPopupWindow.xaml
    /// </summary>
    public partial class MsgPopupWindow : Window
    {
        private readonly string UserId;
        private ChatWindow ChatWindow;
        private readonly UserListFody DataUser;

        public MsgPopupWindow(ChatWindow main , string messeges, string username, string image, string userId , UserListFody userListFody)
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_MsgPopup;
                ChatWindow = main;


                P_username.Text = username;
                P_msgContent.Text = messeges;
                UserId = userId;
                DataUser = userListFody;

                profileimage.Source = new BitmapImage(new Uri(image, UriKind.Absolute));
                Stylechanger();
                
                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }

                if (Settings.NotificationPlaysound == "true" && !DataUser.IsMute)
                {
                    var popupsound = Methods.MainDestination + "Popupsound.mp3";
                    if (Directory.Exists(popupsound) == false)
                    {
                        File.WriteAllBytes(popupsound, Properties.Resources.Popupsound);
                    }

                    MediaPlayer mediaPlayer = new MediaPlayer();
                    mediaPlayer.Open(new Uri(popupsound));
                    mediaPlayer.Play();
                    StartTimer();
                }

                ModeDark_Window();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        DispatcherTimer Timer = null;
        private void StartTimer()
        {
            try
            {
                Timer = new DispatcherTimer();
                Timer.Interval = TimeSpan.FromSeconds(5);
                Timer.Tick += timer_Elapsed;
                Timer.Start();
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
                Timer.Stop();
                PopUpform.Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        private void Stylechanger()
        {
            try
            {
                Color bgColor = (Color)ConvertFromString(Settings.PopUpBackroundColor);
                Color frColor = (Color)ConvertFromString(Settings.PopUpTextFromcolor);
                Color frmsgColor = (Color)ConvertFromString(Settings.PopUpMsgTextcolor);

                Backround.Background = new SolidColorBrush(bgColor);
                P_username.Foreground = new SolidColorBrush(frColor);
                P_msgContent.Foreground = new SolidColorBrush(frmsgColor);
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
                var ligthBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        Background = new SolidColorBrush(darkBackgroundColor);

                        Backround.Background = new SolidColorBrush(darkBackgroundColor);
                        mask.Background = new SolidColorBrush(darkBackgroundColor);

                        P_username.Foreground = new SolidColorBrush(ligthBackgroundColor);
                        P_msgContent.Foreground = new SolidColorBrush(ligthBackgroundColor);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Backround_OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {  
                var dataUsers = ListUtils.ListUsers.FirstOrDefault(a => a.UserId == UserId);
                if (dataUsers != null)
                {
                    var index = ListUtils.ListUsers.IndexOf(ListUtils.ListUsers.FirstOrDefault(a => a.UserId == UserId));
                    switch (index > -1)
                    {
                        case true:
                            //ChatWindow.ChatActivityList.SelectedIndex = index;

                            ChatWindow.WindowState = WindowState.Normal;

                            Timer.Stop();
                            PopUpform.Close();
                            break;
                    }
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        } 
    }
}
