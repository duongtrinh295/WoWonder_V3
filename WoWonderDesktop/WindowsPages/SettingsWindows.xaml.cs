using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WoWonderClient;
using WoWonderClient.Requests;
using WoWonderDesktop.Forms;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.WindowsPages
{
    /// <summary>
    /// Interaction logic for SettingsWindows.xaml
    /// </summary>
    public partial class SettingsWindows : Window
    {
        private string SWhoCanFollowMe, SWhoCanMessageMe, SWhoCanSeeMyBirthday, SConfirmRequestFollows, SOnlineUsersPref;
        private bool RunLoadData;

        public SettingsWindows()
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label_Settings;

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }

                ModeDark_Window();
                LoadData();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Load Data 

        private void LoadData()
        {
            try
            {
               RunLoadData = true;

                var sqlEntity = new SqLiteDatabase();
                sqlEntity.Get_MyProfile();
                sqlEntity.GetSettings();
                sqlEntity.GetSettingsApp();

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetSettings_Api });


                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    //WhoCanFollowMe
                    //===========================================
                    if (dataUser.FollowPrivacy == "0") //Everyone
                    {
                        SWhoCanFollowMe = "0";
                        SelWhoCanFollow.SelectedIndex = 0;
                    }
                    else if (dataUser.FollowPrivacy == "1") //People_i_Follow
                    {
                        SWhoCanFollowMe = "1";
                        SelWhoCanFollow.SelectedIndex = 1;
                    }
                    else
                    {
                        SWhoCanFollowMe = "0";
                        SelWhoCanFollow.SelectedIndex = 0;
                    }

                    //WhoCanMessageMe
                    //===========================================
                    if (dataUser.MessagePrivacy == "0") //Everyone
                    {
                        SWhoCanMessageMe = "0";
                        SelWhoCanMessage.SelectedIndex = 1;
                    }
                    else if (dataUser.MessagePrivacy == "1") //People_i_Follow
                    {
                        SWhoCanMessageMe = "1";
                        SelWhoCanMessage.SelectedIndex = 1;
                    }
                    else if (dataUser.MessagePrivacy == "2") //No_body
                    {
                        SWhoCanMessageMe = "2";
                        SelWhoCanMessage.SelectedIndex = 1;
                    }
                    else
                    {
                        SWhoCanMessageMe = "0";
                        SelWhoCanMessage.SelectedIndex = 1;
                    }

                    //WhoCanSeeMyBirthday
                    //===========================================
                    if (dataUser.BirthPrivacy == "0") //Everyone
                    {
                        SWhoCanSeeMyBirthday = "0";
                        SelWhoCanSeeBirthday.SelectedIndex = 0;
                    }
                    else if (dataUser.BirthPrivacy == "1") //People_i_Follow
                    {
                        SWhoCanSeeMyBirthday = "1";
                        SelWhoCanSeeBirthday.SelectedIndex = 1;
                    }
                    else if (dataUser.BirthPrivacy == "2") //No_body
                    {
                        SWhoCanSeeMyBirthday = "2";
                        SelWhoCanSeeBirthday.SelectedIndex = 2;
                    }
                    else
                    {
                        SWhoCanSeeMyBirthday = "0";
                        SelWhoCanSeeBirthday.SelectedIndex = 0;
                    }

                    //RequestFollows
                    //===========================================
                    if (dataUser.ConfirmFollowers == "0")
                    {
                        SConfirmRequestFollows = "0";
                        BtnSwitchConfirmRequest.IsChecked = false;
                    }
                    else
                    {
                        SConfirmRequestFollows = "1";
                        BtnSwitchConfirmRequest.IsChecked = true;
                    }

                    //Status
                    //===========================================
                    if (dataUser.Status == "0")
                    {
                        SOnlineUsersPref = "0";
                        BtnSwitchStatus.IsChecked = true; 
                    }
                    else
                    {
                        SOnlineUsersPref = "1";
                        BtnSwitchStatus.IsChecked = false;  
                    }
                }

                //if (!string.IsNullOrEmpty(ListUtils.SettingsSiteList.BackgroundChatsImages))
                //{
                //    BackgroundChat.Source = new BitmapImage(new Uri(ListUtils.SettingsSiteList.BackgroundChatsImages));
                //}
                //else
                //{
                //    var BackgroundChatsImages = @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Backgrounds/emoji-background.png";
                //    ListUtils.SettingsSiteList.BackgroundChatsImages = BackgroundChatsImages;

                //    BitmapImage logo = new BitmapImage();
                //    logo.BeginInit();
                //    logo.UriSource = new Uri(BackgroundChatsImages);
                //    logo.EndInit();

                //    BackgroundChat.Source = logo;
                //}


                if (ListUtils.SettingsApp != null)
                {
                    //DarkMode
                    //===========================================
                    BtnSwitchDarkMode.IsChecked = !string.IsNullOrEmpty(ListUtils.SettingsApp.DarkMode) && Convert.ToBoolean(ListUtils.SettingsApp.DarkMode);

                    //Notification
                    //===========================================
                    Settings.NotificationDesktop = ListUtils.SettingsApp.NotificationDesktop == "true" ? "true" : "false";
                    Settings.NotificationPlaysound = ListUtils.SettingsApp.NotificationPlaysound == "true" ? "true" : "false";

                    BtnCheckBoxNotificationPopup.IsChecked = ListUtils.SettingsApp.NotificationDesktop != null && Convert.ToBoolean(ListUtils.SettingsApp.NotificationDesktop);
                    BtnCheckBoxPlaySound.IsChecked = ListUtils.SettingsApp.NotificationPlaysound != null && Convert.ToBoolean(ListUtils.SettingsApp.NotificationPlaysound);

                    //Language
                    //===========================================
                    Settings.LangResources = ListUtils.SettingsApp.LangResources;

                    switch (ListUtils.SettingsApp.LangResources)
                    {
                        case "":
                        case "en-US":
                            SelLanguage.SelectedIndex = 0;
                            break;
                        case "ar":
                            SelLanguage.SelectedIndex = 1;
                            break;
                        default:
                            SelLanguage.SelectedIndex = 0;
                            break;
                    }
                }

                if (ListUtils.SettingsSiteList != null)
                {

                    //Affiliates
                    //===========================================
                    //https://demo.wowonder.com/register?ref=waelanjo
                    TxtLinkAffiliates.Text = InitializeWoWonder.WebsiteUrl + "/register?ref=" + UserDetails.Username;

                    switch (Convert.ToInt32(ListUtils.SettingsSiteList?.AmountPercentRef ?? "0"))
                    {
                        case > 0:
                            TxtMyAffiliates.Text = LocalResources.label5_EarnUpTo + " %" + ListUtils.SettingsSiteList?.AmountPercentRef + " " + LocalResources.label5_ForEachUserYourReferToUs + " !";
                            break;
                        default:
                            var (currency, currencyIcon) = WoWonderTools.GetCurrency(ListUtils.SettingsSiteList?.AdsCurrency);
                            Console.WriteLine(currency);

                            TxtMyAffiliates.Text = LocalResources.label5_EarnUpTo + " " + currencyIcon + ListUtils.SettingsSiteList?.AmountRef + " " + LocalResources.label5_ForEachUserYourReferToUs + " !";
                            break;
                    }
                }

                RunLoadData = false;
            }
            catch (Exception e)
            {
                RunLoadData = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event 

        //Open Profile
        private void LnkEditProfile_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MyProfileWindow profileWindow = new MyProfileWindow();
                profileWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Unchecked Dark Mode >> Off
        private void BtnSwitchDarkMode_OnUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RunLoadData) return;
                UserDetails.ModeDarkStlye = false;

                ModeLigth_Window();

                if (ListUtils.SettingsApp != null)
                {
                    ListUtils.SettingsApp.DarkMode = "false";

                    //Update DataBase
                    SqLiteDatabase database = new SqLiteDatabase();
                    database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Checked Dark Mode >> On
        private void BtnSwitchDarkMode_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RunLoadData) return;
                UserDetails.ModeDarkStlye = true;

                ModeDark_Window();

                if (ListUtils.SettingsApp != null)
                {
                    ListUtils.SettingsApp.DarkMode = "true";

                    //Update DataBase
                    SqLiteDatabase database = new SqLiteDatabase();
                    database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click Link Open Window : Users Blocked
        private void LnkBlockUser_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                BlockUserWindow blockUserWindow = new BlockUserWindow();
                blockUserWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click Link Open Window : Setting Stickers
        private void LnkSticker_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SettingStickersWindow settingStickers = new SettingStickersWindow();
                settingStickers.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Change Wallpaper and theme app 
        private void LnkWallpaper_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChooseWallpaperWindow wallpaperWindow = new ChooseWallpaperWindow();
                wallpaperWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Click Link Open Window : Manage local strong
        private void LnkManageLocal_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ManageLocalStrongWindow mls = new ManageLocalStrongWindow();
                mls.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click Link Open browser : url page About Us
        private void LnkAbout_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    string url = InitializeWoWonder.WebsiteUrl + "/terms/about-us"; 
                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo(url) { UseShellExecute = true }
                    };
                    p.Start();
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

        //Click Link Open browser : url page Contact Us
        private void LnkContactUs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    string url = InitializeWoWonder.WebsiteUrl + "/contact-us";
                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo(url) { UseShellExecute = true }
                    };
                    p.Start();
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

        //Click Link Open browser : url page Privacy Policy
        private void LnkPrivacyPolicy_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    string url = InitializeWoWonder.WebsiteUrl + "/terms/privacy-policy";
                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo(url) { UseShellExecute = true }
                    };
                    p.Start();
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

        //Delete Account
        private void LnkDeleteAccount_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteAccountWindow accountWindow = new DeleteAccountWindow();
                accountWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LnkLogout_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(LocalResources.label5_AreYouLogout, LocalResources.label_Logout_Button, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                    {
                        ApiRequest.Logout(); 
                        Close();
                    }
                        break;
                    case MessageBoxResult.No:
                        // User pressed No
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click Link Open Window : change password
        private void LnkChangePassword_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangePasswordWindow passwordWindow = new ChangePasswordWindow();
                passwordWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        private void LnkTwoFactors_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SettingTwoFactorWindow twoFactorWindow = new SettingTwoFactorWindow();
                twoFactorWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        private void LnkManageSessions_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ManageSessionsWindow sessionsWindow = new ManageSessionsWindow();
                sessionsWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Update privacy for : >> Who Can Follow
        private void SelWhoCanFollow_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string culture = ((ComboBoxItem)SelWhoCanFollow.SelectedValue).Tag as string;
                if (culture == null || RunLoadData) return;

                if (culture == "0") //Everyone
                {
                    SWhoCanFollowMe = "0";
                }
                else if (culture == "1") //People_i_Follow
                {
                    SWhoCanFollowMe= "1";
                }
                 
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.FollowPrivacy = SWhoCanFollowMe;
                }

                if (Methods.CheckForInternetConnection())
                {
                    var dataPrivacy = new Dictionary<string, string>
                    {
                        {"follow_privacy", SWhoCanFollowMe},
                    };
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
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

        //Update privacy for : >> Who Can Message
        private void SelWhoCanMessage_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string culture = ((ComboBoxItem)SelWhoCanMessage.SelectedValue).Tag as string;
                if (culture == null || RunLoadData) return;

                if (culture == "0") //Everyone
                {
                    SWhoCanMessageMe = "0";
                }
                else if (culture == "1") //People_i_Follow
                {
                    SWhoCanMessageMe = "1";
                } 
                else if (culture == "2") //No_body
                {
                    SWhoCanMessageMe = "2";
                }
                 
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.MessagePrivacy = SWhoCanMessageMe;
                }
                 
                if (Methods.CheckForInternetConnection())
                {
                    var dataPrivacy = new Dictionary<string, string>
                    {
                        {"message_privacy", SWhoCanMessageMe},
                    };
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
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

        //Update privacy for : >> Who Can See Birthday
        private void SelWhoCanSeeBirthday_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string culture = ((ComboBoxItem)SelWhoCanSeeBirthday.SelectedValue).Tag as string;
                if (culture == null || RunLoadData) return;
                
                if (culture == "0") //Everyone
                {
                    SWhoCanSeeMyBirthday = "0";
                }
                else if (culture == "1") //People_i_Follow
                {
                    SWhoCanSeeMyBirthday = "1";
                }
                else if (culture == "2") //No_body
                {
                    SWhoCanSeeMyBirthday = "2";
                }

                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.BirthPrivacy = SWhoCanSeeMyBirthday;
                }

                if (Methods.CheckForInternetConnection())
                {
                    var dataPrivacy = new Dictionary<string, string>
                    {
                        {"birth_privacy", SWhoCanSeeMyBirthday},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
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

        //Update privacy for : UnChecked Confirm Request >> Off
        private void BtnSwitchConfirmRequest_OnUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RunLoadData) return;
                SConfirmRequestFollows = "0";

                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.ConfirmFollowers = "0";
                    var sqLiteDatabase = new SqLiteDatabase();
                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                }

                if (Methods.CheckForInternetConnection())
                {
                    var dataPrivacy = new Dictionary<string, string>
                    {
                        {"confirm_followers", SConfirmRequestFollows},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
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

        //Update privacy for : Checked Confirm Request >> On
        private void BtnSwitchConfirmRequest_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RunLoadData) return;
                SConfirmRequestFollows = "1";

                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.ConfirmFollowers = "1";
                    var sqLiteDatabase = new SqLiteDatabase();
                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                }

                if (Methods.CheckForInternetConnection())
                {
                    var dataPrivacy = new Dictionary<string, string>
                    {
                        {"confirm_followers", SConfirmRequestFollows},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
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

        //Status >> Checked OnlineUser >> On
        private void BtnSwitchStatus_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RunLoadData) return;
                SOnlineUsersPref = "0";

                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.Status = "0";
                    var sqLiteDatabase = new SqLiteDatabase();
                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                }

                if (Methods.CheckForInternetConnection())
                {
                    var dataPrivacy = new Dictionary<string, string>
                    {
                        {"status", SOnlineUsersPref},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
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

        //Status >> UnChecked OnlineUser >> Off
        private void BtnSwitchStatus_OnUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RunLoadData) return;
                SOnlineUsersPref = "1";

                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.Status = "1";
                    var sqLiteDatabase = new SqLiteDatabase();
                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                }

                if (Methods.CheckForInternetConnection())
                {
                    var dataPrivacy = new Dictionary<string, string>
                    {
                        {"status", SOnlineUsersPref},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
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

        // Selection Connection language
        private void SelLanguage_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string culture = ((ComboBoxItem)SelLanguage.SelectedValue).Tag as string;
                if (culture == null || RunLoadData) return;

                SetCulture(culture);

                Settings.LangResources = culture;
                
                if (ListUtils.SettingsApp != null)
                {
                    ListUtils.SettingsApp.LangResources = Settings.LangResources;
                   
                    SqLiteDatabase database = new SqLiteDatabase();
                    database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private static void SetCulture(string language)
        {
            try
            {
                CultureInfo myCulture = new CultureInfo(language);
                LocalResources.Culture = myCulture;
                CultureInfo.DefaultThreadCurrentCulture = myCulture;
                Thread.CurrentThread.CurrentCulture = myCulture;
                Thread.CurrentThread.CurrentUICulture = myCulture;

                //new ChineseLunisolarCalendar();
                //new HebrewCalendar();
                //new HijriCalendar();
                //new JapaneseCalendar();
                //new JapaneseLunisolarCalendar();
                //new KoreanCalendar();
                //new KoreanLunisolarCalendar();
                //new PersianCalendar();
                //new TaiwanCalendar();
                //new TaiwanLunisolarCalendar();
                //new ThaiBuddhistCalendar();
                //new UmAlQuraCalendar(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //UnChecked Desktop Notifications >> false
        private void BtnCheckBoxNotificationPopup_OnUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RunLoadData) return;
                if (ListUtils.SettingsApp != null)
                {
                    ListUtils.SettingsApp.NotificationDesktop = "false";

                    //Update DataBase
                    SqLiteDatabase database = new SqLiteDatabase();
                    database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);
                }

                Settings.NotificationDesktop = "false";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Checked Desktop Notifications >> true
        private void BtnCheckBoxNotificationPopup_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RunLoadData) return;
                if (ListUtils.SettingsApp != null)
                {
                    ListUtils.SettingsApp.NotificationDesktop = "true";

                    //Update DataBase
                    SqLiteDatabase database = new SqLiteDatabase();
                    database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);
                }

                Settings.NotificationDesktop = "true";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //UnChecked Play sound >> false
        private void BtnCheckBoxPlaySound_OnUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RunLoadData) return;
                if (ListUtils.SettingsApp != null)
                {
                    ListUtils.SettingsApp.NotificationPlaysound = "false";

                    //Update DataBase
                    SqLiteDatabase database = new SqLiteDatabase();
                    database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);
                }

                Settings.NotificationPlaysound = "false";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Checked Play sound >> true
        private void BtnCheckBoxPlaySound_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RunLoadData) return;
                if (ListUtils.SettingsApp != null)
                {
                    ListUtils.SettingsApp.NotificationPlaysound = "true";

                    //Update DataBase
                    SqLiteDatabase database = new SqLiteDatabase();
                    database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);
                }

                Settings.NotificationPlaysound = "true";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        //Copy Link Affiliates
        private void BtnCopy_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(TxtLinkAffiliates.Text);
                MessageBox.Show("Text copied to clipboard");
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
                        TabDockPanel.Background = new SolidColorBrush(darkBackgroundColor);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ModeLigth_Window()
        {
            try
            {
                //wael chage to light mode >> color
                var darkBackgroundColor = (Color)ConvertFromString(Settings.DarkBackground_Color);
                var whiteBackgroundColor = (Color)ConvertFromString(Settings.WhiteBackground_Color);
                Color selverBackgroundColor = (Color)ConvertFromString(Settings.LigthBackground_Color);

                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        Background = new SolidColorBrush(darkBackgroundColor);
                        TabDockPanel.Background = new SolidColorBrush(darkBackgroundColor);
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