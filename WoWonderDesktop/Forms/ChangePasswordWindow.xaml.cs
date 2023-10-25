using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WoWonderClient.Classes.Global;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderClient.Requests;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for ChangePassword_Window.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow()
        {
            try
            {
                InitializeComponent();

                Title = LocalResources.label3_ChangePassword;
                ModeDark_Window();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public void ModeDark_Window()
        {
            try
            {
                var darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                var whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        Background = new SolidColorBrush(darkBackgroundColor);
                        MainGrid.Background = new SolidColorBrush(darkBackgroundColor);

                        IconCurrentPass.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        IconNewPass.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        IconConfirmPass.Foreground = new SolidColorBrush(whiteBackgroundColor);

                        CurrentPasswordTextBox.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        NewPasswordTextBox.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        ConfirmPasswordTextBox.Foreground = new SolidColorBrush(whiteBackgroundColor);
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

        private void TitleApp_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var titleApp = sender as TextBlock;
                var darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                var whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        titleApp.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        break;
                    default:
                        titleApp.Foreground = new SolidColorBrush(darkBackgroundColor);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_Minimize_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var buttonWindow = sender as Button;
                var darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                var whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);

                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        buttonWindow.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        break;
                    default:
                        buttonWindow.Foreground = new SolidColorBrush(darkBackgroundColor);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Methods.CheckForInternetConnection())
                {
                    MessageBox.Show(LocalResources.label_Please_check_your_internet);
                    return;
                }

                if (string.IsNullOrEmpty(CurrentPasswordTextBox.Password) || string.IsNullOrEmpty(NewPasswordTextBox.Password) || string.IsNullOrEmpty(ConfirmPasswordTextBox.Password))
                {
                    MessageBox.Show(LocalResources.label3_Please_check_your_details);
                    return;
                }

                if (NewPasswordTextBox.Password != ConfirmPasswordTextBox.Password)
                {
                    MessageBox.Show(LocalResources.label3_Your_password_dont_match);
                    return;
                }

                switch (string.IsNullOrEmpty(CurrentPasswordTextBox.Password))
                {
                    case false when !string.IsNullOrEmpty(NewPasswordTextBox.Password) && !string.IsNullOrEmpty(ConfirmPasswordTextBox.Password):
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"new_password", NewPasswordTextBox.Password},
                            {"current_password", CurrentPasswordTextBox.Password}, 
                        };
                     
                        var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy); 
                        switch (apiStatus)
                        {
                            case 200:
                            {
                                switch (respond)
                                {
                                    case MessageObject result:
                                    {
                                        MessageBox.Show(result.Message);

                                        if (result.Message.Contains("updated"))
                                        {
                                            UserDetails.Password = NewPasswordTextBox.Password;
                                        }

                                        break;
                                    }
                                }

                                break;
                            }
                            default:
                                Methods.DisplayReportResult(respond);
                                break;
                        }

                        break;
                    }
                    default:
                        MessageBox.Show(LocalResources.label3_Please_check_your_details);
                        break;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void Btn_Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
