using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.Library.ProgressDialog;
using WoWonderDesktop.SQLiteDB;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.WindowsPages
{
    /// <summary>
    /// Interaction logic for SettingTwoFactorWindow.xaml
    /// </summary>
    public partial class SettingTwoFactorWindow : Window
    {
        private string TypeTwoFactor, TypeDialog = "Confirmation";

        public SettingTwoFactorWindow()
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_TwoFactorsAuthentication;

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }
                ModeDark_Window();

                var twoFactorUSer = ListUtils.MyProfileList?.FirstOrDefault()?.TwoFactor;
                switch (twoFactorUSer)
                {
                    case "0":
                        SelTwoFactor.Text = LocalResources.label5_Disable;
                        TypeTwoFactor = "off";
                        break;
                    default:
                        SelTwoFactor.Text = LocalResources.label5_Enable;
                        TypeTwoFactor = "on";
                        break;
                }

                TxtCode.Visibility = Visibility.Hidden; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Event
         
        private void Btn_cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SelTwoFactor_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string culture = ((ComboBoxItem)SelTwoFactor.SelectedValue).Tag as string;
                if (culture == null) return;

                if (culture == "0") //Enable
                {
                    SelTwoFactor.Text = LocalResources.label5_Enable;
                    TypeTwoFactor = "on";
                    TxtCode.Visibility = Visibility.Hidden;
                }
                else if (culture == "1") //Disable
                {
                    SelTwoFactor.Text = LocalResources.label5_Disable;
                    TypeTwoFactor = "off";
                    TxtCode.Visibility = Visibility.Hidden;
                }  
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_Save_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Methods.CheckForInternetConnection())
                {
                    MessageBox.Show(LocalResources.label_Please_check_your_internet);
                    return;
                }

                if (TypeDialog == "Confirmation")
                {
                    if (TypeTwoFactor == "on")
                    {
                        //Show a progress
                        ProgressDialogResult dialogResult = ProgressDialog.Execute(this, "Loading...",-1,async (worker, args) =>
                        {
                            try
                            {
                                int apiStatus;
                                dynamic respond;
                                if (ListUtils.SettingsSiteList?.TwoFactorType == "phone")
                                {
                                    var phoneNumber = ListUtils.MyProfileList.FirstOrDefault()?.PhoneNumber;
                                    if (!string.IsNullOrEmpty(phoneNumber) && Methods.FunString.IsPhoneNumber(phoneNumber))
                                    {
                                        (apiStatus, respond) = await RequestsAsync.Global.UpdateTwoFactorAsync(phoneNumber);
                                    }
                                    else
                                    {
                                        //Lbl_PhoneNumberIsWrong
                                        ProgressDialog.ExecuteDismiss();

                                        return;
                                    }
                                }
                                else
                                {
                                    (apiStatus, respond) = await RequestsAsync.Global.UpdateTwoFactorAsync();
                                }

                                if (apiStatus == 200)
                                {
                                    if (respond is not MessageObject result) return;
                                    if (result.Message.Contains("confirmation code sent"))
                                    {
                                        //Lbl_ConfirmationCodeSent

                                        ProgressDialog.ExecuteDismiss();

                                        TypeDialog = "ConfirmationCode";

                                        TxtCode.Visibility = Visibility.Visible;
                                        Btn_Save.Content = LocalResources.label5_Send;
                                    }
                                    else
                                    {
                                        //Show a Error image with a message 
                                        ProgressDialog.ExecuteDismiss();

                                        if (result.Message != null)
                                            MessageBox.Show(result.Message);
                                    }
                                }
                                else
                                {
                                    ProgressDialog.ExecuteDismiss();
                                    Methods.DisplayReportResult(respond);
                                }
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                                ProgressDialog.ExecuteDismiss();
                            }
                        });
                    }
                    else if (TypeTwoFactor == "off")
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => RequestsAsync.Global.UpdateTwoFactorAsync()});
                        var local = ListUtils.MyProfileList?.FirstOrDefault();
                        if (local != null)
                        {
                            local.TwoFactor = "0";

                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(local);
                        }

                        Close();
                    }
                }
                else if (TypeDialog == "ConfirmationCode")
                    SendButtonOnClick();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        // check code email if good or no than update data user 
        private async void SendButtonOnClick()
        {
            try
            {
                if (!Methods.CheckForInternetConnection())
                {
                    MessageBox.Show(LocalResources.label_Please_check_your_internet);
                    return;
                }

                if (string.IsNullOrEmpty(TxtCode.Text) || string.IsNullOrWhiteSpace(TxtCode.Text))
                {
                    MessageBox.Show(LocalResources.label_Please_check_your_internet); //Lbl_Please_enter_your_data
                    return;
                }

                //Show a progress
                //AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                var (apiStatus, respond) = await RequestsAsync.Global.UpdateTwoFactorAsync("verify", TxtCode.Text);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case MessageObject result:
                                    {
                                        Console.WriteLine(result.Message);

                                        var local = ListUtils.MyProfileList?.FirstOrDefault();
                                        if (local != null)
                                        {
                                            local.TwoFactor = "1";

                                            var sqLiteDatabase = new SqLiteDatabase();
                                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(local);

                                        }

                                        //ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_TwoFactorOn), ToastLength.Short);
                                        //AndHUD.Shared.Dismiss(this);

                                        Close();
                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        Methods.DisplayReportResult(respond);
                        break;
                }
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