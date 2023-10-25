using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WoWonderDesktop.Controls;
using WoWonderDesktop.Forms;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.WindowsPages
{
    /// <summary>
    /// Interaction logic for CallWindows.xaml
    /// </summary>
    public partial class CallWindows : Window
    {
        public CallWindows()
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_Call;

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }
                ModeDark_Window();
                LoadCall(); 
                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Load Data 

        private void LoadCall()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();
                var CallList = sqlEntity.get_data_CallVideo();
                ListUtils.ListCall = new ObservableCollection<Classes.CallVideo>(CallList);

               Dispatcher?.Invoke(() =>
                {
                    try
                    {
                        if (ListUtils.ListCall.Count > 0)
                        {
                            CallsList.ItemsSource = ListUtils.ListCall;
                            CallsList.Visibility = Visibility.Visible;

                            EmptyPageContent.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            CallsList.Visibility = Visibility.Collapsed;
                            EmptyPageContent.Visibility = Visibility.Visible;
                            EmptyPageContent.InflateLayout(EmptyPage.Type.NoCall);
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

        #endregion
         
        #region Event

        private void SendVideoCallButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button mi = (Button)sender;
                string callId = mi.CommandParameter.ToString();

                var selectedGroup = ListUtils.ListCall.FirstOrDefault(a => a.CallVideoCallId == callId);
                if (selectedGroup != null)
                {
                    SendVideoCall sendCall = new SendVideoCall(selectedGroup.CallVideoUserName, ChatWindow.UserId, selectedGroup.CallVideoAvatar, ChatWindow.ChatWindowContext);
                    sendCall.ShowDialog();
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_delete_call_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button mi = (Button)sender;
                string callId = mi.CommandParameter.ToString();

                SqLiteDatabase database = new SqLiteDatabase();
                database.remove_data_CallVideo(callId);
                 
                Classes.CallVideo deleteCalls = ListUtils.ListCall.FirstOrDefault(a => a.CallVideoCallId == callId);
                if (deleteCalls != null)
                {
                    ListUtils.ListCall.Remove(deleteCalls);
                    CallsList.ItemsSource = ListUtils.ListCall;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Selection Changed null is SelectedItem Calls_list
        private void CallsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                CallsList.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Mouse Move null is SelectedItem Calls_list
        private void CallsList_OnMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                CallsList.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

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