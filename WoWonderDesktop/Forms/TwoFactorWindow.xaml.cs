using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using WoWonderClient;
using WoWonderClient.Classes.Auth;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for TwoFactorWindow.xaml
    /// </summary>
    public partial class TwoFactorWindow : Window
    {
        private readonly string TypeCode;
        private readonly LoginWindow LoginWindow;
        public TwoFactorWindow(LoginWindow login , string typeCode)
        {
            try
            {
                InitializeComponent();
                TypeCode = typeCode;
                LoginWindow = login;

                Lbl_VerificationText.Text = LocalResources.label3_VerificationText1 + "\n" + LocalResources.label3_VerificationText2;

                Title = TypeCode == "TwoFactor" ? LocalResources.label5_TwoFactorsAuthentication : LocalResources.label3_ActiveAccount;
                ModeDark_Window();

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        private async void BtnVerify_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (string.IsNullOrEmpty(TxtCode.Text))
                {
                    case false when Methods.CheckForInternetConnection():
                    {
                        switch (TypeCode)
                        {
                            case "TwoFactor":
                            {
                                (int apiStatus, var respond) = await RequestsAsync.Auth.TwoFactorAsync(UserDetails.UserId, TxtCode.Text);
                                switch (apiStatus)
                                {
                                    case 200:
                                    {
                                        switch (respond)
                                        {
                                            case AuthObject result:
                                            {
                                                UserDetails.UserId = result.UserId;
                                                UserDetails.Username = LoginWindow.NameTextBox.Text;
                                                UserDetails.FullName = LoginWindow.NameTextBox.Text;
                                                UserDetails.Password = LoginWindow.PasswordBox.Password;
                                                UserDetails.Status = "Active";
                                                UserDetails.Email = LoginWindow.NameTextBox.Text;
                                                UserDetails.Time = result.Timezone;

                                                Current.AccessToken = UserDetails.AccessToken = result.AccessToken;

                                                switch (LoginWindow.Cheeked)
                                                {
                                                    case "1":
                                                    {
                                                        DataTables.LoginTb login = new DataTables.LoginTb
                                                        {
                                                            UserId = result.UserId,
                                                            Username = LoginWindow.NameTextBox.Text,
                                                            Password = LoginWindow.PasswordBox.Password,
                                                            AccessToken = Current.AccessToken,
                                                            Status = "Active"
                                                        };

                                                        ListUtils.DataUserLoginList.Clear();
                                                        ListUtils.DataUserLoginList.Add(login);

                                                        SqLiteDatabase database = new SqLiteDatabase();
                                                        database.InsertOrUpdateLogin(login);
                                                        
                                                        break;
                                                    }
                                                }

                                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(UserDetails.UserId) });

                                                if (LoginWindow.IsCanceled != true)
                                                {
                                                    //Show Main Window
                                                    LoginWindow.DialogHost.IsOpen = false;
                                                    LoginWindow.DisplayWorkThrothPages = true;

                                                    SqLiteDatabase database = new SqLiteDatabase();
                                                    database.Insert_To_StickersTable();
                                                    

                                                    Hide();
                                                    LoginWindow.Hide();

                                                    ChatWindow wm = new ChatWindow();
                                                    wm.Show();
                                                }

                                                break;
                                            }
                                        }

                                        break;
                                    }
                                    default:
                                    {
                                        switch (respond)
                                        {
                                            case ErrorObject errorMessage:
                                            {
                                                var errorId = errorMessage.Error.ErrorId;
                                                switch (errorId)
                                                {
                                                    case "3":
                                                        MessageBox.Show(LocalResources.label3_CodeNotCorrect);
                                                        break;
                                                }
                                                break;
                                            }
                                        }
                                        Methods.DisplayReportResult(respond);
                                        break;
                                    }
                                }

                                break;
                            }
                            case "AccountSms":
                            {
                                (int apiStatus, var respond) = await RequestsAsync.Auth.ActiveAccountSmsAsync(UserDetails.UserId, TxtCode.Text);
                                switch (apiStatus)
                                {
                                    case 200:
                                    {
                                        switch (respond)
                                        {
                                            case AuthObject result:
                                            {
                                                UserDetails.UserId = result.UserId;
                                                UserDetails.Username = LoginWindow.RegisterUsernameTextBox.Text;
                                                UserDetails.FullName = LoginWindow.RegisterUsernameTextBox.Text;
                                                UserDetails.Password = LoginWindow.RegisterPasswordTextBox.Password;
                                                UserDetails.Status = "Active";
                                                UserDetails.Email = LoginWindow.RegisterEmailTextBox.Text;
                                                UserDetails.Time = result.Timezone;

                                                switch (LoginWindow.Cheeked)
                                                {
                                                    case "1":
                                                    {
                                                        DataTables.LoginTb login = new DataTables.LoginTb
                                                        {
                                                            UserId = result.UserId,
                                                            Username = LoginWindow.RegisterUsernameTextBox.Text,
                                                            Password = LoginWindow.RegisterPasswordTextBox.Password,
                                                            AccessToken = Current.AccessToken,
                                                            Status = "Active",
                                                            Email = LoginWindow.RegisterEmailTextBox.Text,
                                                        };
                                                        SqLiteDatabase database = new SqLiteDatabase();
                                                        database.InsertOrUpdateLogin(login);
                                                        
                                                        break;
                                                    }
                                                }

                                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(UserDetails.UserId) });

                                                if (LoginWindow.IsCanceled != true)
                                                {
                                                    //Show Main Window
                                                    LoginWindow.DialogHost.IsOpen = false;
                                                    LoginWindow.DisplayWorkThrothPages = true;

                                                    SqLiteDatabase database = new SqLiteDatabase();
                                                    database.Insert_To_StickersTable();
                                                    

                                                    Hide();
                                                    LoginWindow.Hide();

                                                    ChatWindow wm = new ChatWindow();
                                                    wm.Show();
                                                }

                                                break;
                                            }
                                        }

                                        break;
                                    }
                                    default:
                                    {
                                        switch (respond)
                                        {
                                            case ErrorObject errorMessage:
                                            {
                                                var errorId = errorMessage.Error.ErrorId;
                                                switch (errorId)
                                                {
                                                    case "3":
                                                        MessageBox.Show(LocalResources.label3_CodeNotCorrect);
                                                        break;
                                                }
                                                break;
                                            }
                                        }
                                        Methods.DisplayReportResult(respond);
                                        break;
                                    }
                                }

                                break;
                            }
                        }

                        break;
                    }
                    case false:
                        MessageBox.Show(LocalResources.label_Please_check_your_internet);
                        break;
                    default:
                        MessageBox.Show(LocalResources.label3_CodeNotCorrect);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

                        Lbl_VerificationText.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        CodeIcon.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        TxtCode.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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

        private void Btn_Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
