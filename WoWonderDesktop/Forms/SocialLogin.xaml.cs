using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WoWonderDesktop.Helpers.Controls;
using WoWonderClient;
using WoWonderClient.Classes.Auth;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.OauthLogin.Service;
using WoWonderDesktop.SQLiteDB;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for SocialLogin.xaml
    /// </summary>
    public partial class SocialLogin : Window
    {
        public string Type;
        private LoginWindow LoginWindow;
        public SocialLogin(LoginWindow loginWindow, string type)
        {
            try
            {
                InitializeComponent();

                LoginWindow = loginWindow;
                Type = type;

                Title = LocalResources.label5_SocialLogin;
                
                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }


                switch (type)
                {
                    //else if (type == "Twitter")
                    //{
                    //    SocialLoginbrowser. = Client.WebsiteUrl +"/app_api.php?type=user_login_with&provider=Twitter&hash=" + HashSet;
                    //}
                    //else if (type == "Vk")
                    //{
                    //    SocialLoginbrowser.Address = Client.WebsiteUrl + "/app_api.php?type=user_login_with&provider=Vkontakte&hash=" + HashSet;
                    //}
                    //else if (type == "Instagram")
                    //{
                    //    SocialLoginbrowser.Address = Client.WebsiteUrl + "/app_api.php?type=user_login_with&provider=Instagram&hash=" + HashSet;
                    //}
                    case "Facebook":
                        SocialLoginbrowser.Navigate(new Uri(FacebookService.BeginAuthentication()));
                        break;
                    case "Google":
                        SocialLoginbrowser.Navigate(new Uri(GoogleService.BeginAuthentication()));
                        break;
                }
                ModeDark_Window();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ModeDark_Window()
        {
            try
            {
                var darkBackgroundColor = (Color)ColorConverter.ConvertFromString(Settings.DarkBackground_Color);
                var whiteBackgroundColor = (Color)ColorConverter.ConvertFromString(Settings.WhiteBackground_Color);

                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        Background = new SolidColorBrush(darkBackgroundColor);

                        Panel.Background = new SolidColorBrush(darkBackgroundColor);
                        LoadingPanel.Background = new SolidColorBrush(darkBackgroundColor);

                        Lbl_Loading.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        Lbl_connection.Foreground = new SolidColorBrush(whiteBackgroundColor);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void SocialLoginbrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                //https://www.facebook.com/connect/login_success.html?code=AQBFqQQqIlDNImnZd9V8A9yZHbnlDhM7Wps6MmOmDkjQ-4dQb3vWXi2ixjp2xjNAO708Z6_VPEwja3FASNSizfKf7BNpfmeIdExAW7q-40WHIHcljjCs9_HX_Tpfhp86uY3hrEf7KUHcwgZXsw7Xoojyxy46rbOpXWMFcXLN21HyopuXR25a58-WSMvo8pV6EkWUVZWA8ggqowGZc4OcOGflxy-EBqbKDj8rFX2qK5_GnLnp4wXy1GiAowHSjjs7Vx2s3k1uQAjFQ6FuRN6XhHvQtEQXeM48IAlbtPlospnhPKvoCeQWP0Dw6yf43tGugoziMxHlbpMd7UqH0fWqBX2D#_=_

                switch (string.IsNullOrEmpty(e.Uri?.AbsoluteUri))
                {
                    case false when e.Uri.AbsoluteUri.Contains("code="):
                        {
                            var code = e.Uri.AbsoluteUri.Split("code=")?.Last();
                            switch (Type)
                            {
                                case "Facebook":
                                    await FacebookService.RequestToken(code);

                                    // SocialLoginApi()
                                    break;
                                case "Google":
                                    await GoogleService.RequestToken(code);

                                    // SocialLoginApi()
                                    break;
                            }

                            break;
                        }
                }

                Dispatcher?.Invoke((Action)delegate // <--- HERE
                {
                    try
                    {
                        if (LoadingPanel.Visibility != Visibility.Collapsed)
                            LoadingPanel.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void SocialLoginApi(string accessToken, string provider)
        {
            try
            {
                //send api
                (int apiStatus, var respond) = await RequestsAsync.Auth.SocialLoginAsync(accessToken, provider);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case AuthObject result:
                                    {
                                        UserDetails.UserId = result.UserId;
                                        //UserDetails.Username = LoginWindow.NameTextBox.Text;
                                        //UserDetails.FullName = LoginWindow.NameTextBox.Text;
                                        //UserDetails.Password = LoginWindow.PasswordBox.Password;
                                        UserDetails.Status = "Active";
                                        //UserDetails.Email = LoginWindow.NameTextBox.Text;
                                        UserDetails.Time = result.Timezone;

                                        Current.AccessToken = UserDetails.AccessToken = result.AccessToken;

                                        DataTables.LoginTb login = new DataTables.LoginTb
                                        {
                                            UserId = result.UserId,
                                            //Username = LoginWindow.NameTextBox.Text,
                                            //Password = LoginWindow.PasswordBox.Password,
                                            AccessToken = Current.AccessToken,
                                            Status = "Active"
                                        };

                                        ListUtils.DataUserLoginList.Clear();
                                        ListUtils.DataUserLoginList.Add(login);

                                        SqLiteDatabase database = new SqLiteDatabase();
                                        database.InsertOrUpdateLogin(login);
                                        database.Insert_To_StickersTable();


                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(UserDetails.UserId) });

                                        LoginWindow.DisplayWorkThrothPages = true;

                                        //Show Main Window
                                        LoginWindow.DialogHost.IsOpen = false;

                                        Hide();
                                        LoginWindow.Hide();

                                        ChatWindow wm = new ChatWindow();
                                        wm.Show();
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
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SocialLoginbrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                var url = e.Uri.AbsoluteUri;


            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}