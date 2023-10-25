using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using static System.Windows.Media.ColorConverter;

namespace WoWonderDesktop.WindowsPages
{
    /// <summary>
    /// Interaction logic for ChooseWallpaperWindow.xaml
    /// </summary>
    public partial class ChooseWallpaperWindow : Window
    {
        public ChooseWallpaperWindow()
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_Wallpaper;

                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }

                ModeDark_Window();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Event 
        
        private string HexColor = "";
        private void HexColorText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                HexColor = HexColorText.Text;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Save hex color and close popup
        private void SaveColorButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine(HexColor);

                ListUtils.SettingsApp.BackgroundChatsImages = HexColor;

                //Update DataBase
                SqLiteDatabase database = new SqLiteDatabase();
                database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);

                ChatWindow.ChatWindowContext.GetSettings(ListUtils.SettingsApp);
                ColorPopupBox.IsPopupOpen = false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Close color popup without save 
        private void CloseColorButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ColorPopupBox.IsPopupOpen = false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        // Create OpenFileDialog Choose image Background Chat
        private void LnkChooseFile_OnClick(object sender, RoutedEventArgs e)
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
                    // Open document and image file path
                    string filename = open.FileName;

                    // display image in picture box  
                    BackgroundChat.ImageSource = new BitmapImage(new Uri(filename));
                    // ImageBrushChat.ImageSource = new BitmapImage(new Uri(filename));

                    ListUtils.SettingsApp.BackgroundChatsImages = filename;

                    //Update DataBase
                    SqLiteDatabase database = new SqLiteDatabase();
                    database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);

                    ChatWindow.ChatWindowContext.GetSettings(ListUtils.SettingsApp);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Create Reset image Background Chat 
        private void LnkChooseGallery_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        // Create Reset image Background Chat 
        private void LnkReset_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ListUtils.SettingsApp.BackgroundChatsImages = "#ffffff";

                //Update DataBase
                SqLiteDatabase database = new SqLiteDatabase();
                database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);

                ChatWindow.ChatWindowContext.GetSettings(ListUtils.SettingsApp);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_B2DFDB_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ListUtils.SettingsApp.BackgroundChatsImages = "#B2DFDB";

                //Update DataBase
                SqLiteDatabase database = new SqLiteDatabase();
                database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);

                ChatWindow.ChatWindowContext.GetSettings(ListUtils.SettingsApp);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }  
        }

        private void Btn_81D4FA_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ListUtils.SettingsApp.BackgroundChatsImages = "#81D4FA";

                //Update DataBase
                SqLiteDatabase database = new SqLiteDatabase();
                database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);

                ChatWindow.ChatWindowContext.GetSettings(ListUtils.SettingsApp);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_D1C4E9_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ListUtils.SettingsApp.BackgroundChatsImages = "#D1C4E9";

                //Update DataBase
                SqLiteDatabase database = new SqLiteDatabase();
                database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);

                ChatWindow.ChatWindowContext.GetSettings(ListUtils.SettingsApp);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_FFF9C4_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ListUtils.SettingsApp.BackgroundChatsImages = "#FFF9C4";

                //Update DataBase
                SqLiteDatabase database = new SqLiteDatabase();
                database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);

                ChatWindow.ChatWindowContext.GetSettings(ListUtils.SettingsApp);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_FFCDD2_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ListUtils.SettingsApp.BackgroundChatsImages = "#FFCDD2";

                //Update DataBase
                SqLiteDatabase database = new SqLiteDatabase();
                database.InsertOrUpdateSettingsApp(ListUtils.SettingsApp);

                ChatWindow.ChatWindowContext.GetSettings(ListUtils.SettingsApp);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void DoneButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
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