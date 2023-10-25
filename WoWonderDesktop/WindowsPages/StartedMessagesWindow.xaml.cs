using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using WoWonderDesktop.Controls;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.WindowsPages
{
    /// <summary>
    /// Interaction logic for StartedMessagesWindow.xaml
    /// </summary>
    public partial class StartedMessagesWindow : Window
    {
        public StartedMessagesWindow()
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_StartedMessages;

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }

                ModeDark_Window();
                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Load Data 

        private void StartApiService()
        {
            try
            {
                if (ListUtils.StartedMessageList.Count > 0)
                {
                    ConversationControl.ControlInstance.ChatMessgaeslistBox.ItemsSource = ListUtils.StartedMessageList;
                    ConversationControl.ControlInstance.ChatMessgaeslistBox.Visibility = Visibility.Visible;

                    EmptyPageContent.Visibility = Visibility.Collapsed;
                }
                 
                if (Methods.CheckForInternetConnection())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadFavorite });
                else
                    MessageBox.Show(LocalResources.label_Please_check_your_internet);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task LoadFavorite()
        {
            try
            {
                ProgressBarSearchSessions.Visibility = Visibility.Visible;
                ProgressBarSearchSessions.IsIndeterminate = true;
                 
                if (Methods.CheckForInternetConnection())
                {
                    if (!Settings.EnableFavoriteMessageSystem || string.IsNullOrEmpty(ChatWindow.ChatId))
                        return;
                     
                    var (apiStatus, respond) = await RequestsAsync.Message.GetFavoriteMessageAsync(ChatWindow.ChatId);
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

                                ListUtils.StartedMessageList.Add(WoWonderTools.MessageFilter(ChatWindow.UserId, ChatWindow.ChatWindowContext.SelectedChatData.Name, item, ChatWindow.ChatWindowContext.ChatColor));
                            }
                        }
                    }
                }

                Dispatcher?.Invoke(() =>
                {
                    try
                    {
                        if (ListUtils.StartedMessageList.Count > 0)
                        {
                            ConversationControl.ControlInstance.ChatMessgaeslistBox.ItemsSource = ListUtils.StartedMessageList;
                            ConversationControl.ControlInstance.ChatMessgaeslistBox.Visibility = Visibility.Visible;

                            EmptyPageContent.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            ConversationControl.ControlInstance.ChatMessgaeslistBox.Visibility = Visibility.Collapsed;
                            EmptyPageContent.Visibility = Visibility.Visible;
                            EmptyPageContent.InflateLayout(EmptyPage.Type.NoStartedMessages);
                        }

                        ProgressBarSearchSessions.Visibility = Visibility.Collapsed;
                        ProgressBarSearchSessions.IsIndeterminate = false;
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

        #endregion

        #region Event
         
        #endregion

        #region Window

        private void ModeDark_Window()
        {
            try
            {
                var darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                var whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);
                Color selverBackgroundColor = (Color)ConvertFromString(Settings.LigthBackground_Color);

                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        Background = new SolidColorBrush(darkBackgroundColor);
                        SearchTabDockPanel.Background = new SolidColorBrush(darkBackgroundColor);
                        //StartedMessageList.Background = new SolidColorBrush(darkBackgroundColor);
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

        private void Btn_Close_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Button buttonWindowClose = sender as Button;
                Color darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                Color whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                if (UserDetails.ModeDarkStlye)
                {
                    buttonWindowClose.Foreground = new SolidColorBrush(whiteBackgroundColor);
                }
                else
                {
                    buttonWindowClose.Foreground = new SolidColorBrush(darkBackgroundColor);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleApp_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBlock titleApp = sender as TextBlock;
                Color darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                Color whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                if (UserDetails.ModeDarkStlye)
                {
                    titleApp.Foreground = new SolidColorBrush(whiteBackgroundColor);
                }
                else
                {
                    titleApp.Foreground = new SolidColorBrush(darkBackgroundColor);
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