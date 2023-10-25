using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WoWonderClient;
using WoWonderClient.Requests;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.WindowsPages
{
    /// <summary>
    /// Interaction logic for DeleteAccountWindow.xaml
    /// </summary>
    public partial class DeleteAccountWindow : Window
    {
        public DeleteAccountWindow()
        {
            try
            {
                InitializeComponent();
                 
               Title = LocalResources.label5_Delete_Account;
                
                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }

                ModeDark_Window();

                Lbl_DeleteAccount.Text = LocalResources.label5_Are_you_DeleteAccount + " " + Settings.ApplicationName +  "?";

                Lbl_ChkDelete.Text = LocalResources.label5_IWantToDelete1 + " " + UserDetails.Username + " " + 
                                     LocalResources.label5_IWantToDelete2 + " \n" + Settings.ApplicationName + " " +
                                     LocalResources.label5_IWantToDelete3;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Btn_check.IsChecked.Value)
                {
                    MessageBox.Show(LocalResources.label5_You_can_not_access_your_disapproval);
                    return;
                }

                var data = ListUtils.DataUserLoginList.FirstOrDefault(a => a.UserId == UserDetails.UserId);
                if (data != null)
                {
                    if (TxtPassword.Password == data.Password)
                    {
                        //close all window
                        if (Methods.CheckForInternetConnection())
                        {
                            SqLiteDatabase database = new SqLiteDatabase();
                            database.DropAll();

                            if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            {
                                UserDetails.Socket?.Emit_loggedoutEvent(UserDetails.AccessToken);
                                UserDetails.Socket?.DisconnectSocket();
                            }

                            ListUtils.ClearAllList();

                            Methods.Path.ClearFolder();
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Auth.DeleteUserAsync(data.Password) });
                        }

                        var assemblyName = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = assemblyName,
                                //RedirectStandardOutput = true,
                                //UseShellExecute = false,
                                CreateNoWindow = true,
                            }
                        };
                        process.Start();

                        Application.Current.Shutdown();
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(LocalResources.label5_Please_confirm_your_password);
                    }
                }

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

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
                        //TabDockPanel.Background = new SolidColorBrush(darkBackgroundColor);
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
