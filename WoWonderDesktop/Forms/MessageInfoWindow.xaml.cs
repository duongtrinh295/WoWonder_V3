using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for MessageInfo.xaml
    /// </summary>
    public partial class MessageInfoWindow : Window
    { 
        public MessageInfoWindow(MessagesDataFody item)
        {
            try
            {
                InitializeComponent(); 
                Title = LocalResources.label_Lnk_Manage_local ;

                LoadMessageInfo(item);

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

        private void LoadMessageInfo(MessagesDataFody item)
        {
            try
            {
                DateTime dateTime = Methods.Time.UnixTimeStampToDateTime(int.Parse(item.Time));
                LblDelivered.Content = LocalResources.label5_Delivered + " : " + dateTime.ToLongDateString() + ", " + dateTime.ToShortTimeString();

                if (item.Seen != "0")
                {
                    DateTime dateTimeSeen = Methods.Time.UnixTimeStampToDateTime(int.Parse(item.Seen));
                    LblRead.Content = LocalResources.label5_Read + " : " + dateTimeSeen.ToLongDateString() + ", " + dateTimeSeen.ToShortTimeString();
                }
                else
                {
                    LblRead.Content = LocalResources.label5_Read + " : " + "---";
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Click Button Cancel 
        private void Btn_cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
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

                        BorderMain.Background = new SolidColorBrush(darkBackgroundColor);
                        TxtMessageInfo.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        LblRead.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        LblDelivered.Foreground = new SolidColorBrush(whiteBackgroundColor);
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
