using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Auth;
using WoWonderClient.Classes.Global;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using WoWonderClient.Requests;
 
namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for Login_Window.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        #region Variables Declaration

        public static bool DisplayWorkThrothPages;
        public bool IsCanceled;
        public string TimeZone = Settings.CodeTimeZone, Cheeked = "";
        private string GenderStatus = "male";
        private bool IsActiveUser = true;
       

        #endregion

        public LoginWindow()
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label_Btn_Login ;

                StyleChanger();

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }

                InitializeWoWonder.Initialize(Settings.TripleDesAppServiceProvider, Assembly.GetExecutingAssembly().GetName(true).Name, Settings.WebExceptionSecurity, Settings.SetApisReportMode);

                GetTimezone();
                 
                var smsOrEmail = ListUtils.SettingsSiteList?.SmsOrEmail;
                switch (smsOrEmail)
                {
                    case "sms":
                        PhoneNumberPanel.Visibility = Visibility.Visible;
                        break;
                }

                switch (ListUtils.SettingsSiteList?.Genders?.Count > 0)
                {
                    case true:
                    {
                        foreach (var item in ListUtils.SettingsSiteList?.Genders)
                        {
                            SelGender.Items.Add(item.Value);
                        }

                        break;
                    }
                    default:
                        SelGender.Items.Add(LocalResources.label5_Male);
                        SelGender.Items.Add(LocalResources.label_BoxItem_female);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void GetTimezone()
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                    TimeZone = await ApiRequest.GetTimeZoneAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //########################## login ##########################

        #region login


        // On Click Button forgot password
        private void Btn_Forgot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo(InitializeWoWonder.WebsiteUrl + "/forgot-password") {UseShellExecute = true}
                };
                p.Start(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Enter Button login
        private void Btn_Login_OnKeyDown(object sender, KeyEventArgs e)
        {
            Btn_Login_Async();
        }

        // On Click Button login 
        private void Btn_Login_OnClick(object sender, RoutedEventArgs e)
        {
            Btn_Login_Async();
        }

        private async void Btn_Login_Async()
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    switch (Btn_check.IsChecked)
                    {
                        case true:
                            Cheeked = "1";
                            break;
                    }

                    switch (IsCanceled)
                    {
                        case true:
                            IsCanceled = false;
                            break;
                    }

                    TextBlockMessege.Text = LocalResources.label_TextBlockMessege;
                    ProgressBarMessege.Visibility = Visibility.Visible;
                    IconError.Visibility = Visibility.Collapsed;

                    if (string.IsNullOrEmpty(NameTextBox.Text.Replace(" ", "")) || string.IsNullOrWhiteSpace(NameTextBox.Text.Replace(" ", "")))
                    {
                        TextBlockMessege.Focus();
                        TextBlockMessege.Text = LocalResources.label_Please_write_your_username;
                        ProgressBarMessege.Visibility = Visibility.Collapsed;
                        IconError.Visibility = Visibility.Visible;
                        return;

                    }

                    if (string.IsNullOrEmpty(PasswordBox.Password) || string.IsNullOrWhiteSpace(PasswordBox.Password))
                    {
                        PasswordBox.Focus();
                        TextBlockMessege.Text = LocalResources.label_Please_write_your_password;
                        ProgressBarMessege.Visibility = Visibility.Collapsed;
                        IconError.Visibility = Visibility.Visible;
                        return;
                    }

                    var (apiStatus, respond) = await RequestsAsync.Auth.AuthAsync(NameTextBox.Text, PasswordBox.Password, TimeZone);
                    switch (apiStatus)
                    {
                        case 200 when respond is AuthObject result:
                            {
                                var emailValidation = ListUtils.SettingsSiteList?.EmailValidation ?? "0";
                                if (emailValidation == "1")
                                {
                                    IsActiveUser = await CheckIsActiveUser(result.UserId);
                                }
                                 
                                if (IsActiveUser)
                                {
                                    UserDetails.UserId = result.UserId;
                                    UserDetails.Username = NameTextBox.Text;
                                    UserDetails.FullName = NameTextBox.Text;
                                    UserDetails.Password = PasswordBox.Password;
                                    UserDetails.Status = "Active";
                                    UserDetails.Email = NameTextBox.Text;
                                    UserDetails.Time = result.Timezone;

                                    Current.AccessToken = UserDetails.AccessToken = result.AccessToken;

                                    switch (Cheeked)
                                    {
                                        case "1":
                                            {
                                                DataTables.LoginTb login = new DataTables.LoginTb
                                                {
                                                    UserId = result.UserId,
                                                    Username = NameTextBox.Text,
                                                    Password = PasswordBox.Password,
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

                                    if (IsCanceled != true)
                                    {
                                        //Show Main Window
                                        DialogHost.IsOpen = false;
                                        DisplayWorkThrothPages = true;

                                        SqLiteDatabase database = new SqLiteDatabase();
                                        database.Insert_To_StickersTable();


                                        Hide();
                                        ChatWindow wm = new ChatWindow();
                                        wm.Show();
                                    }
                                }
                                break;
                            }
                        case 200:
                            {
                                switch (respond)
                                {
                                    case AuthMessageObject messageObject:
                                        {
                                            UserDetails.UserId = messageObject.UserId;
                                            UserDetails.Username = NameTextBox.Text;
                                            UserDetails.FullName = NameTextBox.Text;
                                            UserDetails.Password = PasswordBox.Password;
                                            UserDetails.Status = "Pending";
                                            UserDetails.Email = NameTextBox.Text;

                                            //Insert user data to database
                                            var login = new DataTables.LoginTb
                                            {
                                                UserId = messageObject.UserId,
                                                Username = NameTextBox.Text,
                                                Password = PasswordBox.Password,
                                                AccessToken = "",
                                                Status = "Pending"
                                            };

                                            ListUtils.DataUserLoginList.Clear();
                                            ListUtils.DataUserLoginList.Add(login);

                                            SqLiteDatabase database = new SqLiteDatabase();
                                            database.InsertOrUpdateLogin(login);


                                            //VerificationCode : "TypeCode" >>  "TwoFactor"
                                            TwoFactorWindow twoFactor = new TwoFactorWindow(this, "TwoFactor");
                                            twoFactor.Show();
                                            break;
                                        }
                                }

                                break;
                            }
                        case 400:
                            {
                                switch (respond)
                                {
                                    case ErrorObject error:
                                        TextBlockMessege.Text = error.Error.ErrorText;
                                        ProgressBarMessege.Visibility = Visibility.Collapsed;
                                        IconError.Visibility = Visibility.Visible;
                                        break;
                                }

                                break;
                            }
                        default:
                            TextBlockMessege.Text = respond.ToString();
                            ProgressBarMessege.Visibility = Visibility.Collapsed;
                            IconError.Visibility = Visibility.Visible;
                            break;
                    }
                }
                else
                {
                    TextBlockMessege.Text = LocalResources.label_Please_check_your_internet;
                    ProgressBarMessege.Visibility = Visibility.Collapsed;
                    IconError.Visibility = Visibility.Visible;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                TextBlockMessege.Text = exception.Message;
                ProgressBarMessege.Visibility = Visibility.Collapsed;
                IconError.Visibility = Visibility.Visible;
            }
        }

        #endregion

        //########################## Register ##########################

        #region Register

        // Run background worker : Register

        private async void Btn_Register_Async()
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    try
                    {
                        switch (IsCanceled)
                        {
                            case true:
                                IsCanceled = false;
                                break;
                        }
                         
                        TextBlockMessege.Text = LocalResources.label_TextBlockMessege;
                        ProgressBarMessege.Visibility = Visibility.Visible;
                        IconError.Visibility = Visibility.Collapsed;

                        var smsOrEmail = ListUtils.SettingsSiteList?.SmsOrEmail;
                        switch (smsOrEmail)
                        {
                            case "sms" when string.IsNullOrEmpty(RegisterPhoneNumberTextBox.Text):
                                TextBlockMessege.Focus();
                                TextBlockMessege.Text = LocalResources.label3_Please_write_your_Phone;
                                ProgressBarMessege.Visibility = Visibility.Collapsed;
                                IconError.Visibility = Visibility.Visible;
                                return;
                        }
                          
                        if (string.IsNullOrEmpty(RegisterUsernameTextBox.Text.Replace(" ", "")) || string.IsNullOrWhiteSpace(RegisterUsernameTextBox.Text.Replace(" ", "")))
                        {
                            TextBlockMessege.Focus();
                            TextBlockMessege.Text = LocalResources.label_Please_write_your_username;
                            ProgressBarMessege.Visibility = Visibility.Collapsed;
                            IconError.Visibility = Visibility.Visible;
                            return;
                        }

                        if (string.IsNullOrEmpty(RegisterEmailTextBox.Text.Replace(" ", "")) || string.IsNullOrWhiteSpace(RegisterEmailTextBox.Text.Replace(" ", "")))
                        {
                            PasswordBox.Focus();
                            TextBlockMessege.Text = LocalResources.label_Please_write_your_Email;
                            ProgressBarMessege.Visibility = Visibility.Collapsed;
                            IconError.Visibility = Visibility.Visible;
                            return;
                        }

                        var check = Methods.FunString.IsEmailValid(RegisterEmailTextBox.Text.Replace(" ", ""));
                        switch (check)
                        {
                            case false:
                                PasswordBox.Focus();
                                TextBlockMessege.Text = "IsEmailValid";
                                ProgressBarMessege.Visibility = Visibility.Collapsed;
                                IconError.Visibility = Visibility.Visible;
                                return;
                        }

                        if (string.IsNullOrEmpty(RegisterPasswordTextBox.Password) || string.IsNullOrWhiteSpace(RegisterPasswordTextBox.Password))
                        {
                            PasswordBox.Focus();
                            TextBlockMessege.Text = LocalResources.label_Please_write_your_password;
                            ProgressBarMessege.Visibility = Visibility.Collapsed;
                            IconError.Visibility = Visibility.Visible;
                            return;
                        }

                        if (string.IsNullOrEmpty(RegisterConfirmPasswordTextBox.Password) || string.IsNullOrWhiteSpace(RegisterConfirmPasswordTextBox.Password))
                        {
                            PasswordBox.Focus();
                            TextBlockMessege.Text = LocalResources.label_Please_write_confirm_password;
                            ProgressBarMessege.Visibility = Visibility.Collapsed;
                            IconError.Visibility = Visibility.Visible;
                            return;
                        }
                        
                        if (string.IsNullOrEmpty(SelGender.Text) || string.IsNullOrWhiteSpace(SelGender.Text))
                        {
                            SelGender.Focus();
                            TextBlockMessege.Text = LocalResources.label5_PleaseEnterYourData;
                            ProgressBarMessege.Visibility = Visibility.Collapsed;
                            IconError.Visibility = Visibility.Visible;
                            return;
                        }

                        var (apiStatus, respond) = await RequestsAsync.Auth.CreateAccountAsync(RegisterUsernameTextBox.Text.Replace(" ", ""), RegisterPasswordTextBox.Password , RegisterConfirmPasswordTextBox.Password,
                            RegisterEmailTextBox.Text, GenderStatus, RegisterPhoneNumberTextBox.Text , "");
                        switch (apiStatus)
                        {
                            case 200 when respond is CreatAccountObject result:
                            {
                                UserDetails.UserId = result.UserId;
                                UserDetails.Username = RegisterUsernameTextBox.Text;
                                UserDetails.FullName = RegisterUsernameTextBox.Text;
                                UserDetails.Password = RegisterPasswordTextBox.Password;
                                UserDetails.Status = "Active";
                                UserDetails.Email = RegisterEmailTextBox.Text;
                                UserDetails.Time = TimeZone;
                                Current.AccessToken = UserDetails.AccessToken = result.AccessToken;

                                switch (Cheeked)
                                {
                                    case "1":
                                    {
                                        DataTables.LoginTb login = new DataTables.LoginTb
                                        {
                                            UserId = result.UserId,
                                            Username = RegisterUsernameTextBox.Text,
                                            Password = RegisterPasswordTextBox.Password,
                                            AccessToken = Current.AccessToken,
                                            Status = "Active",
                                            Email = RegisterEmailTextBox.Text,
                                        };
                                        SqLiteDatabase database = new SqLiteDatabase();
                                        database.InsertOrUpdateLogin(login);
                                        
                                        break;
                                    }
                                }

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(UserDetails.UserId) });

                                if (IsCanceled != true)
                                {
                                    //Show Main Window
                                    DialogHost.IsOpen = false;
                                    DisplayWorkThrothPages = true;

                                    SqLiteDatabase database = new SqLiteDatabase();
                                    database.Insert_To_StickersTable();
                                    

                                    Hide();
                                    ChatWindow wm = new ChatWindow();
                                    wm.Show();
                                }

                                break;
                            }
                            case 200:
                            {
                                switch (respond)
                                {
                                    case AuthMessageObject messageObject when smsOrEmail == "sms":
                                    {
                                        UserDetails.UserId = messageObject.UserId;
                                        UserDetails.Username = RegisterUsernameTextBox.Text;
                                        UserDetails.Password = RegisterPasswordTextBox.Password;
                                        UserDetails.Status = "Pending";
                                        UserDetails.Email = RegisterEmailTextBox.Text;

                                        //Insert user data to database
                                        var login = new DataTables.LoginTb
                                        {
                                            UserId = messageObject.UserId,
                                            Username = NameTextBox.Text,
                                            Password = PasswordBox.Password,
                                            AccessToken = "",
                                            Status = "Pending",
                                            Email = RegisterEmailTextBox.Text
                                        };

                                        ListUtils.DataUserLoginList.Clear();
                                        ListUtils.DataUserLoginList.Add(login);

                                        SqLiteDatabase database = new SqLiteDatabase();
                                        database.InsertOrUpdateLogin(login);
                                        

                                        //VerificationCode : "TypeCode" >>  "AccountSms"
                                        TwoFactorWindow twoFactor = new TwoFactorWindow(this, "AccountSms");
                                        twoFactor.Show();
                                        break;
                                    }
                                    case AuthMessageObject messageObject when smsOrEmail == "mail":
                                        TextBlockMessege.Text = messageObject.Message;
                                        ProgressBarMessege.Visibility = Visibility.Collapsed;
                                        IconError.Visibility = Visibility.Visible;
                                        IconError.Kind = PackIconKind.AlertCircle;
                                        IconError.Foreground = Brushes.Orange;
                                        break;
                                    case AuthMessageObject messageObject:
                                        TextBlockMessege.Text = respond.ToString();
                                        ProgressBarMessege.Visibility = Visibility.Collapsed;
                                        IconError.Visibility = Visibility.Visible;
                                        break;
                                }

                                break;
                            }
                            case 400:
                            {
                                switch (respond)
                                {
                                    case ErrorObject error:
                                        TextBlockMessege.Focus();
                                        TextBlockMessege.Text = error.Error.ErrorText;
                                        ProgressBarMessege.Visibility = Visibility.Collapsed;
                                        IconError.Visibility = Visibility.Visible;
                                        break;
                                }

                                break;
                            }
                            default:
                                TextBlockMessege.Focus();
                                TextBlockMessege.Text = respond.ToString();
                                ProgressBarMessege.Visibility = Visibility.Collapsed;
                                IconError.Visibility = Visibility.Visible;
                                IconError.Kind = PackIconKind.AlertCircle;
                                IconError.Foreground = Brushes.Orange;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        TextBlockMessege.Focus();
                        TextBlockMessege.Text = ex.Message;
                        ProgressBarMessege.Visibility = Visibility.Collapsed;
                        IconError.Visibility = Visibility.Visible;
                        Methods.DisplayReportResultTrack(ex);
                    }
                }
                else
                {
                    TextBlockMessege.Focus();
                    TextBlockMessege.Text = LocalResources.label_Please_check_your_internet;
                    ProgressBarMessege.Visibility = Visibility.Collapsed;
                    IconError.Visibility = Visibility.Visible;
                }
            }
            catch (Exception exception)
            {
                TextBlockMessege.Focus();
                TextBlockMessege.Text = exception.Message;
                ProgressBarMessege.Visibility = Visibility.Collapsed;
                IconError.Visibility = Visibility.Visible;

                Methods.DisplayReportResultTrack(exception);
            }

        }

        // On Click Button Register 
        private void Registerbutton_OnClick(object sender, RoutedEventArgs e)
        {
            Btn_Register_Async();
        }

        // On Click Button New Acount
        private void newAcountButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RightBigText.Content = LocalResources.label_RightBigText_Signin;
                RightSmallText.Text = LocalResources.label_RightSmallText_Signin;
                 
                RightImage.ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Backgrounds/" + Settings.LoginImg, UriKind.Absolute));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void SelGender_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string itemString = e.AddedItems[0]?.ToString();

                switch (ListUtils.SettingsSiteList?.Genders?.Count)
                {
                    case > 0:
                    {
                        SelGender.Text = itemString;

                        var key = ListUtils.SettingsSiteList?.Genders?.FirstOrDefault(a => a.Value == itemString).Key;
                        GenderStatus = key ?? "male";
                        break;
                    }
                    default:
                    {  
                        if (itemString == LocalResources.label5_Male)
                        {
                            SelGender.Text = LocalResources.label5_Male;
                            GenderStatus = "male";
                        }
                        else if (itemString == LocalResources.label_BoxItem_female)
                        {
                            SelGender.Text = LocalResources.label_BoxItem_female;
                            GenderStatus = "female";
                        }
                        else
                        {
                            SelGender.Text = LocalResources.label5_Male;
                            GenderStatus = "male";
                        }

                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //########################## login using Social Meida ##########################

        #region login Social Meida


        // On Click Button login using Facebook 
        private void FacebookButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SocialLogin sl = new SocialLogin(this,"Facebook");
                sl.ShowDialog(); 

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // On Click Button login using Twitter 
        private void TwitterButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SocialLogin sl = new SocialLogin(this,"Twitter");
                sl.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // On Click Button login using Vk 
        private void VkButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SocialLogin sl = new SocialLogin(this,"Vk");
                sl.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // On Click Button login using Instagram 
        private void InstagramButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SocialLogin sl = new SocialLogin(this,"Instagram");
                sl.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // On Click Button login using Google 
        private void GoogleButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SocialLogin sl = new SocialLogin(this,"Google");
                sl.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //########################## Settings ##########################

        #region Settings

        private async Task<bool> CheckIsActiveUser(string userId)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Auth.IsActiveUserAsync(userId);
                switch (apiStatus)
                {
                    case 200 when respond is MessageObject auth:
                        Console.WriteLine(auth);
                        return true;
                    case 400:
                        {
                            if (respond is ErrorObject error)
                            {
                                var errorText = error.Error.ErrorText;
                                var errorId = error.Error.ErrorId;
                                 
                                switch (errorId)
                                {
                                    case "5":
                                        TextBlockMessege.Text = LocalResources.label5_ThisUserNotActive;
                                        break;
                                    case "4":
                                        TextBlockMessege.Text = LocalResources.label5_UserNotFound;
                                        break;
                                    default:
                                        TextBlockMessege.Text = errorText;
                                        break;
                                }

                                ProgressBarMessege.Visibility = Visibility.Collapsed;
                                IconError.Visibility = Visibility.Visible;
                            }

                            break;
                        }
                    case 404:
                        TextBlockMessege.Text = respond.ToString();
                        ProgressBarMessege.Visibility = Visibility.Collapsed;
                        IconError.Visibility = Visibility.Visible;
                        break;
                }

                return false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }


        //Functions Get path and data from class Settings
        private void StyleChanger()
        {
            try
            {
                switch (Settings.FacebookIcon)
                {
                    case true:
                        FacebookButton.Visibility = Visibility.Visible;
                        break;
                }
                switch (Settings.TwitterIcon)
                {
                    case true:
                        TwitterButton.Visibility = Visibility.Visible;
                        break;
                }
                switch (Settings.InstagramIcon)
                {
                    case true:
                        InstagramButton.Visibility = Visibility.Visible;
                        break;
                }
                //switch (Settings.VkIcon)
                //{
                //    case true:
                //        VkButton.Visibility = Visibility.Visible;
                //        break;
                //}
                switch (Settings.GoogleIcon)
                {
                    case true:
                        GoogleButton.Visibility = Visibility.Visible;
                        Lbl_Orvia.Visibility = Visibility.Visible;
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //########################## Windows ##########################

        #region Windows

        // On Click Button Close
        private void Btn_Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
            Application.Current.Shutdown();
            Environment.Exit(0);

        }

        // On Click Button Back
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RightBigText.Content = LocalResources.label_Btn_Login;
                RightSmallText.Text = LocalResources.label_RightSmallText_Login;
                RightImage.ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Backgrounds/" + Settings.RegisterImg, UriKind.Absolute)); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LoadingCancelbutton_OnClick(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
        }

        #endregion

    }
}