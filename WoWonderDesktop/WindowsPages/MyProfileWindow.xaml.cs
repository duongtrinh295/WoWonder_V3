using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
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
    /// Interaction logic for MyProfileWindow.xaml
    /// </summary>
    public partial class MyProfileWindow : Window
    {
        private UserDataObject SelectedChatData;
        private string GenderStatus, IdRelationShip;
        private List<string> RelationshipList;

        public MyProfileWindow()
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_MyProfile;

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }

                ModeDark_Window(); 
                LoadUser(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #region Load Data 

        private void LoadUser()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();
                sqlEntity.Get_MyProfile();
                sqlEntity.GetSettings();

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

                RelationshipList = new List<string>() { "None", "Single", "In a relationship", "Married", "Engaged" };
                foreach (var item in RelationshipList)
                {
                    SelRelationship.Items.Add(item);
                }
                  
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    LoadDataUserProfile(dataUser); 
                }

                StartApiService(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Run background worker : Users Contact 
        private void StartApiService()
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadDataUser });
                else
                    MessageBox.Show(LocalResources.label_Please_check_your_internet);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Run background worker : My profile 
        private async Task LoadDataUser()
        {
            try
            { 
                if (Methods.CheckForInternetConnection())
                {
                    (int apiStatus, var respond) = await RequestsAsync.Global.GetUserDataAsync(UserDetails.UserId, "user_data");
                    if (apiStatus == 200 && respond is GetUserDataObject result)
                    {
                        var dbt = ClassMapper.Mapper?.Map<UserDataFody>(result.UserData);
                        UserDetails.Avatar = result.UserData.Avatar;
                        UserDetails.Cover = result.UserData.Cover;
                        UserDetails.Username = result.UserData.Username;
                        UserDetails.FullName = result.UserData.Name;
                        UserDetails.Email = result.UserData.Email;

                        ListUtils.MyProfileList = new ObservableCollection<UserDataFody>() { dbt };

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.Insert_Or_Update_To_MyProfileTable(dbt);

                        LoadDataUserProfile(dbt);

                    }
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        //Load User Profile 
        public void LoadDataUserProfile(UserDataObject userData)
        {
            App.Current.Dispatcher?.Invoke((Action)delegate
            {
                try
                {
                    if (userData == null)
                        return;

                    SelectedChatData = userData;

                    ImageUser.ProfileImageSource = new BitmapImage(new Uri(userData.Avatar));

                    TxtFullName.Text = WoWonderTools.GetNameFinal(userData);

                    IconVerified.Visibility = userData.Verified == "1" ? Visibility.Visible : Visibility.Collapsed;
                    IconPro.Visibility = userData.IsPro == "1" ? Visibility.Visible : Visibility.Collapsed;

                    TxtUserName.Text = Methods.FunString.DecodeString(userData.Username);
                    TxtFirstName.Text = Methods.FunString.DecodeString(userData.FirstName);
                    TxtLastName.Text = Methods.FunString.DecodeString(userData.LastName);

                    TxtEmail.Text = userData.Email;

                    switch (string.IsNullOrEmpty(userData.Birthday))
                    {
                        case false when userData.Birthday != "00-00-0000" && userData.Birthday != "0000-00-00":
                            try
                            {
                                DateTime date = DateTime.Parse(userData.Birthday);
                                string newFormat = date.Day + "-" + date.Month + "-" + date.Year;
                                TxtBirthday.Text = newFormat;
                            }
                            catch
                            {
                                //Methods.DisplayReportResultTrack(e);
                                TxtBirthday.Text = userData.Birthday;
                            }
                            break;
                    }

                    if (!string.IsNullOrEmpty(userData.Gender))
                    {
                        switch (ListUtils.SettingsSiteList?.Genders?.Count)
                        {
                            case > 0:
                                {
                                    var value = ListUtils.SettingsSiteList?.Genders?.FirstOrDefault(a => a.Key?.ToLower() == userData.Gender?.ToLower()).Value;
                                    if (value != null)
                                    {
                                        SelGender.Text = value;
                                        GenderStatus = userData.Gender;
                                    }
                                    else
                                    {
                                        if (userData.Gender.ToLower() == LocalResources.label5_Male)
                                        {
                                            SelGender.Text = LocalResources.label5_Male;
                                            GenderStatus = "male";
                                        }
                                        else if (userData.Gender.ToLower() == LocalResources.label_BoxItem_female)
                                        {
                                            SelGender.Text = LocalResources.label_BoxItem_female;
                                            GenderStatus = "female";
                                        }
                                        else
                                        {
                                            SelGender.Text = LocalResources.label5_Male;
                                            GenderStatus = "male";
                                        }
                                    }

                                    break;
                                }
                            default:
                                {
                                    if (userData.Gender.ToLower() == LocalResources.label5_Male)
                                    {
                                        SelGender.Text = LocalResources.label5_Male;
                                        GenderStatus = "male";
                                    }
                                    else if (userData.Gender.ToLower() == LocalResources.label_BoxItem_female)
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

                    TxtAbout.Text = WoWonderTools.GetAboutFinal(userData);
                    TxtPhone.Text = userData.PhoneNumber;

                    TxtWork.Text = userData.Working;
                    TxtSchool.Text = userData.School;
                    TxtWebsite.Text = userData.Website;

                    TxtLocation.Text = userData.Address;

                    TxtFacebook.Text = userData.Facebook;
                    TxtTwitter.Text = userData.Twitter;
                    TxtLinkedin.Text = userData.Linkedin;
                    TxtInstagram.Text = userData.Instagram;
                    TxtYouTube.Text = userData.Youtube;

                    IdRelationShip = userData.RelationshipId;
                    string relationship = WoWonderTools.GetRelationship(Convert.ToInt32(userData.RelationshipId));
                    SelRelationship.Text = Methods.FunString.StringNullRemover(relationship) != "Empty" ? relationship : "";
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            });
        }

        #endregion

        #region Event 

        //Edit Avatar
        private async void EditImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                OpenFileDialog open = new OpenFileDialog
                {
                    // Set filter for file extension and default file extension 
                    DefaultExt = ".png",
                    Filter = "Image Files(*.jpg; *.jpeg; *.png;)|*.jpg; *.png; *.jpeg;"
                };

                // Display OpenFileDialog by calling ShowDialog method 
                var result = open.ShowDialog();

                // Get the selected file name and display in a TextBox 
                if (result == true)
                {
                    if (Methods.CheckForInternetConnection())
                    {
                        // Open document and image file path
                        string filename = open.FileName;

                        // display image in picture box  
                        ImageUser.ProfileImageSource = new BitmapImage(new Uri(filename));
                        if (ChatWindow.ChatWindowContext?.ImageProfileButton != null)
                        {
                            ChatWindow.ChatWindowContext.ImageProfileButton.ProfileImageSource = new BitmapImage(new Uri(filename));
                        }

                        var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserAvatarAsync(filename);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageObject update)
                            {
                                var dataRow = ListUtils.MyProfileList.FirstOrDefault();
                                
                                string avatarSplit = open.FileName.Split('\\').Last();
                                if (dataRow != null)
                                {
                                    dataRow.Avatar = filename;
                                    UserDetails.Avatar = filename;

                                    SqLiteDatabase database = new SqLiteDatabase();
                                    database.Insert_Or_Update_To_MyProfileTable(dataRow);
                                     
                                    Methods.MultiMedia.Get_image(UserDetails.UserId, avatarSplit, dataRow.Avatar);
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(LocalResources.label_Please_check_your_internet);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private async void Btn_save_EditProfile_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    if (!string.IsNullOrEmpty(TxtPhone.Text) && !Methods.FunString.IsPhoneNumber(TxtPhone.Text))
                    {
                        MessageBox.Show(LocalResources.label5_PhoneNumberIsWrong);
                        return;
                    }

                    if (!string.IsNullOrEmpty(TxtWebsite.Text) && Methods.FunString.Check_Regex(TxtWebsite.Text) != "Website")
                    {
                        MessageBox.Show(LocalResources.label5_PleaseEnterWebsite);
                        return;
                    }
                      
                    var dictionary = new Dictionary<string, string>
                    {
                        {"username", TxtUserName.Text.Replace(" ","")},
                        {"first_name", TxtFirstName.Text},
                        {"last_name", TxtLastName.Text},
                        {"email", TxtEmail.Text},
                        {"birthday", TxtBirthday.DisplayDate.ToString("dd-MM-yyyy")},
                        {"gender", GenderStatus},
                        {"about", TxtAbout.Text}, 
                        {"phone_number", TxtPhone.Text},
                        
                        {"website", TxtWebsite.Text},
                        {"working", TxtWork.Text},
                        {"school", TxtSchool.Text},
                        {"relationship", IdRelationShip},
                        
                        {"facebook", TxtFacebook.Text},
                        {"twitter", TxtTwitter.Text},
                        {"youtube", TxtYouTube.Text},
                        {"linkedin", TxtLinkedin.Text},
                        {"instagram", TxtInstagram.Text},

                        {"website", TxtWebsite.Text},
                        {"school", TxtSchool.Text},
                        {"address", TxtLocation.Text},
                    };
                    
                    var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserDataAsync(dictionary);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case MessageObject result:
                                        {
                                            var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                            if (dataUser != null)
                                            {
                                                dataUser.Username = TxtUserName.Text;
                                                dataUser.LastName = TxtLastName.Text;
                                                dataUser.LastName = TxtLastName.Text;
                                                dataUser.Email = TxtEmail.Text;
                                                dataUser.Birthday = TxtBirthday.DisplayDate.ToString("dd-MM-yyyy");
                                                dataUser.Gender = GenderStatus;
                                                dataUser.GenderText = SelGender.Text;
                                                dataUser.About = TxtAbout.Text;

                                                dataUser.Address = TxtLocation.Text;
                                                dataUser.PhoneNumber = TxtPhone.Text;
                                                dataUser.Website = TxtWebsite.Text;
                                                dataUser.Working = TxtWork.Text;
                                                dataUser.School = TxtSchool.Text;
                                                dataUser.RelationshipId = IdRelationShip;

                                                var sqLiteDatabase = new SqLiteDatabase();
                                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                            }
                                            
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

        //select Gender
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

        //select Relationship 
        private void SelRelationship_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string itemString = e.AddedItems[0]?.ToString();

                SelRelationship.Text = itemString;

                var key = RelationshipList.FirstOrDefault(a => a == itemString);
                IdRelationShip = RelationshipList.IndexOf(key).ToString();
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