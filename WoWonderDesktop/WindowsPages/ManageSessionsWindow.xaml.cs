using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WoWonderClient.Classes.Global;
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
    /// Interaction logic for ManageSessionsWindow.xaml
    /// </summary>
    public partial class ManageSessionsWindow : Window
    {
        public ManageSessionsWindow()
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_ManageSessions;

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
                if (Methods.CheckForInternetConnection())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadSessions });
                else
                    MessageBox.Show(LocalResources.label_Please_check_your_internet);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task LoadSessions()
        {
            try
            {
                ProgressBarSearchSessions.Visibility = Visibility.Visible;
                ProgressBarSearchSessions.IsIndeterminate = true;

                if (ListUtils.SessionsList.Count > 0)
                    ListUtils.SessionsList.Clear();

                if (Methods.CheckForInternetConnection())
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.GetSessionsAsync();
                    if (apiStatus == 200)
                    {
                        if (respond is FetchSessionsObject result)
                        {
                            foreach (var item in from item in result.Data let check = ListUtils.SessionsList.FirstOrDefault(a => a.Id == item.Id) where check == null select WoWonderTools.SessionFilter(item))
                            {
                                Dispatcher?.Invoke(delegate // <--- HERE
                                {
                                    try
                                    {
                                        if (item.Browser != null)
                                            ListUtils.SessionsList.Add(item);
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

                Dispatcher?.Invoke(() =>
                {
                    try
                    {
                        if (ListUtils.SessionsList.Count > 0)
                        {
                            SessionsList.ItemsSource = ListUtils.SessionsList;
                            SessionsList.Visibility = Visibility.Visible;

                            EmptyPageContent.Visibility = Visibility.Collapsed; 
                        }
                        else
                        {
                            SessionsList.Visibility = Visibility.Collapsed;
                            EmptyPageContent.Visibility = Visibility.Visible;
                            EmptyPageContent.InflateLayout(EmptyPage.Type.NoSessions);
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
         
        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    Button originalSource = e.OriginalSource as Button;

                    var sessionsId = originalSource?.CommandParameter.ToString();

                    var result = MessageBox.Show(LocalResources.label5_AreYouSureLogoutFromThisDevice, "", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
                    switch (result)
                    {
                        case MessageBoxResult.Yes: 
                            // Sessions pressed Yes
                            try
                            {
                                var item = ListUtils.SessionsList.FirstOrDefault(a => a.Id == sessionsId);
                                if (item != null)
                                {
                                    ListUtils.SessionsList.Remove(item);
                                }

                                if (ListUtils.SessionsList.Count > 0)
                                {
                                    SessionsList.ItemsSource = ListUtils.SessionsList; 
                                }

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.DeleteSessionsAsync(sessionsId) });

                                var view = CollectionViewSource.GetDefaultView(ListUtils.SessionsList);
                                view.Refresh();
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                            break;
                        case MessageBoxResult.No:
                            // Sessions pressed No
                            break;
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

        private void SessionsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SessionsList.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SessionsList_OnMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                SessionsList.SelectedItem = null;
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
                        SearchTabDockPanel.Background = new SolidColorBrush(darkBackgroundColor);
                        SessionsList.Background = new SolidColorBrush(darkBackgroundColor);
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