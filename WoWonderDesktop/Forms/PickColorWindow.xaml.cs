using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WoWonderClient.Requests;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for Edit_MyProfile_Window.xaml
    /// </summary>
    public partial class PickColorWindow : Window
    {
        private readonly string Duser;
        private readonly ChatWindow Main;
        public PickColorWindow(string idUser, ChatWindow window)
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label_Pick_color;
                
                Duser = idUser;
                Main = window;

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }
                ModeDark_Window();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void ChangeColor(string hexColor)
        {
            try
            {
                var updaterColor = ListUtils.ListMessages.Where(a => a.ToId == Duser).ToList();
                switch (updaterColor.Count > 0)
                {
                    case true:
                    {
                        foreach (var items in updaterColor)
                        {
                            items.ChatColor = hexColor;
                            items.EndChatColor = WoWonderTools.GetColorEnd(hexColor);  
                        }
                     
                        var chatPanelColor = (Color)ConvertFromString(hexColor);
                        var chatForegroundColor = (Color)ConvertFromString("#ffffff");

                        //switch (Settings.ChangeChatPanelColor)
                        //{
                        //    case true:
                        //        Main.ChatInfoPanel.Background = new SolidColorBrush(chatPanelColor);
                        //        break;
                        //}
                     
                        //Main.ProfileToggle.Background = new SolidColorBrush(chatPanelColor);
                        //Main.ProfileToggle.Foreground = new SolidColorBrush(chatForegroundColor);
                        //ChatWindow.ChatColor = hexColor;
                        //Main.DropDownMenueOnMessageBox.Foreground = new SolidColorBrush(chatForegroundColor);
                        //Main.ChatTitleChange.Foreground = new SolidColorBrush(chatForegroundColor);
                        //Main.ChatSeen.Foreground = new SolidColorBrush(chatForegroundColor);
                     
                        var view = CollectionViewSource.GetDefaultView(ListUtils.ListMessages);
                        view.Refresh();

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => RequestsAsync.Message.ChangeChatColorAsync(Duser, hexColor)});

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void Btn_b582af_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#b582af");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_a52729_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#a52729");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_fc9cde_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#fc9cde");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_f9c270_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#f9c270");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_70a0e0_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#70a0e0");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_56c4c5_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#56c4c5");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_f33d4c_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#f33d4c");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_a1ce79_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#a1ce79");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_a085e2_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#a085e2");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_ed9e6a_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#ed9e6a");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_2b87ce_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#2b87ce");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_f2812b_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#f2812b");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_0ba05d_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#0ba05d");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_0e71ea_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#0e71ea");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_aa2294_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeColor("#aa2294");
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

                    Lbl_Pick_color.Foreground = new SolidColorBrush(whiteBackgroundColor);
                    Lbl_Everyone_in_this_conversation.Foreground = new SolidColorBrush(whiteBackgroundColor);
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
