using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WoWonderDesktop.Forms;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;

namespace WoWonderDesktop.Controls
{
    /// <summary>
    /// Interaction logic for ContactInfoControl.xaml
    /// </summary>
    public partial class ContactInfoControl : UserControl
    {
        public static ContactInfoControl ControlInstance { get; private set; }
        private UserListFody SelectedChatData;

        public ContactInfoControl()
        {
            InitializeComponent();
            ControlInstance = this; 
        }
         
        //Load User Profile 
        public void LoadDataUserProfile(UserListFody userData)
        {
            App.Current.Dispatcher?.Invoke((Action)delegate
            {
                try
                {
                    if (userData == null)
                        return;

                    SelectedChatData = userData;

                    ImageUser.ProfileImageSource = new BitmapImage(new Uri(userData.Avatar));

                    var online = WoWonderTools.GetStatusOnline(Convert.ToInt32(userData.LastseenUnixTime), userData.LastseenStatus);
                    ImageUser.IsOnline = online;

                    TxtFullName.Text = WoWonderTools.GetNameFinal(userData);

                    IconVerified.Visibility = userData.Verified == "1" ? Visibility.Visible : Visibility.Collapsed;
                    IconPro.Visibility = userData.IsPro == "1" ? Visibility.Visible : Visibility.Collapsed;

                    TxtLocation.Text = string.IsNullOrEmpty(userData.Address) ? LocalResources.label_location_not_available : userData.Address;
                    TxtAbout.Text = WoWonderTools.GetAboutFinal(userData); 

                    FacebookButton.Visibility = !string.IsNullOrEmpty(userData.Facebook) ? Visibility.Visible : Visibility.Collapsed;
                    TwitterButton.Visibility = !string.IsNullOrEmpty(userData.Twitter) ? Visibility.Visible : Visibility.Collapsed;
                    InstagramButton.Visibility = !string.IsNullOrEmpty(userData.Instagram) ? Visibility.Visible : Visibility.Collapsed;
                    youtubeButton.Visibility = !string.IsNullOrEmpty(userData.Youtube) ? Visibility.Visible : Visibility.Collapsed;

                    if (!string.IsNullOrEmpty(userData.Working))
                        TxtWork.Text = userData.Working;
                    else
                        WorkGrid.Visibility = Visibility.Collapsed;

                    if (!string.IsNullOrEmpty(userData.School))
                        TxStudy.Text = userData.School;
                    else
                        StudyGrid.Visibility = Visibility.Collapsed;


                    if (!string.IsNullOrEmpty(userData.Gender))
                    {
                        switch (ListUtils.SettingsSiteList?.Genders?.Count)
                        {
                            case > 0:
                                {
                                    var value = ListUtils.SettingsSiteList?.Genders?.FirstOrDefault(a => a.Key == userData.Gender).Value;
                                    if (value != null)
                                    {
                                        TxtGender.Text = value;
                                    }
                                    else
                                    {
                                        TxtGender.Text = userData.Gender.ToLower() switch
                                        {
                                            "male" => LocalResources.label5_Male,
                                            "female" => LocalResources.label_BoxItem_female,
                                            _ => userData.Gender
                                        };
                                    }

                                    break;
                                }
                            default:
                                {
                                    TxtGender.Text = userData.Gender.ToLower() switch
                                    {
                                        "male" => LocalResources.label5_Male,
                                        "female" => LocalResources.label_BoxItem_female,
                                        _ => userData.Gender
                                    };
                                    break;
                                }
                        }
                    }
                    else
                    {
                        GenderGrid.Visibility = Visibility.Collapsed;
                    }

                    switch (string.IsNullOrEmpty(userData.Birthday))
                    {
                        case false when userData.BirthPrivacy != "2" && userData.Birthday != "00-00-0000" && userData.Birthday != "0000-00-00":
                            try
                            {
                                DateTime date = DateTime.Parse(userData.Birthday);
                                string newFormat = date.Day + "/" + date.Month + "/" + date.Year;
                                TxtBirthday.Text = newFormat;
                            }
                            catch
                            {
                                //Methods.DisplayReportResultTrack(e);
                                TxtBirthday.Text = userData.Birthday;
                            }
                            break;
                        default:
                            BirthdayGrid.Visibility = Visibility.Collapsed;
                            break;
                    }
                     
                    string relationship = WoWonderTools.GetRelationship(Convert.ToInt32(userData.RelationshipId));
                    if (!string.IsNullOrEmpty(relationship))
                        TxtRelationship.Text = relationship;
                    else
                        RelationshipGrid.Visibility = Visibility.Collapsed;
                     
                    Get_SharedFiles(userData.UserId);
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            });
        }
         
        //Get Shared Files Profile user in Right Side "images, Media, file"
        private async void Get_SharedFiles(string userId)
        {
            try
            {
                await WoWonderTools.GetSharedFiles(userId);

                if (ListUtils.ListSharedFiles.Count == 0)
                {
                    MediaPanel.Visibility = Visibility.Collapsed;
                    MediaList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MediaPanel.Visibility = Visibility.Visible;
                    MediaList.Visibility = Visibility.Visible;
                    MediaList.ItemsSource = ListUtils.LastSharedFiles;
                }
                 
                // TxtCountMedia.Text = "Media (" + ListUtils.ListSharedFiles.Count + ")";

                //var view = CollectionViewSource.GetDefaultView(ListUtils.LastSharedFiles);
                //view.Refresh();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Event 
         
        //Open Shared Files from deck PC
        private void MediaList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Classes.SharedFile selectedGroup = (Classes.SharedFile)MediaList.SelectedItem;

                if (selectedGroup != null)
                {
                    if (selectedGroup.FileType == "Image" || selectedGroup.FileType == "File" || selectedGroup.FileType == "Media")
                    {
                        var p = new Process
                        {
                            StartInfo = new ProcessStartInfo(selectedGroup.FilePath) { UseShellExecute = true }
                        };
                        p.Start();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        } 

        private void MediaList_OnMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                MediaList.SelectedItem = null;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Facebook
        private void FacebookButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo("https://www.facebook.com/" + SelectedChatData.Facebook) { UseShellExecute = true }
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

        //Open Twitter
        private void TwitterButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo("https://twitter.com/#!/" + SelectedChatData.Twitter) { UseShellExecute = true }
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

        //Open Instagram
        private void InstagramButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo("https://www.instagram.com/" + SelectedChatData.Instagram) { UseShellExecute = true }
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

        //Open Youtube
        private void YoutubeButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo("https://www.youtube.com/" + SelectedChatData.Youtube) { UseShellExecute = true }
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

        //Click Button Open Window : Shared Files 
        private void BtnSeeAll_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SharedFilesWindow sharedFileForm = new SharedFilesWindow(SelectedChatData);
                sharedFileForm.ShowDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion
        
        private void HyperlinkOpen_OnClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}