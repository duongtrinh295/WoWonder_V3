using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.WindowsPages;

namespace WoWonderDesktop.Controls
{
    /// <summary>
    /// Interaction logic for MenuListControl.xaml
    /// </summary>
    public partial class MenuListControl : UserControl
    {

        public MenuListControl()
        {
            try
            {
                InitializeComponent();
                var itemList = new List<Classes.MenuItems>
                {
                    //I will provide path data for icons in the description so you may copy and paste them because it is very long to type all of them
                    //so copy and paste like i am doing here below
                    //now if you remember "IsItemSelected" it is for to select the 2nd icon of our menu
                    new Classes.MenuItems()
                    {
                        Page= "Chat",
                        PathData = "M187.466,0c-0.1,0-0.3,0-0.6,0c-101.2,0-183.5,82.3-183.5,183.5c0,41.3,14.1,81.4,39.9,113.7l-26.7,62.1 c-2.2,5.1,0.2,11,5.2,13.1c1.8,0.8,3.8,1,5.7,0.7l97.9-17.2c19.6,7.1,40.2,10.7,61,10.6c101.2,0,183.5-82.3,183.5-183.5 C370.066,82.1,288.366,0.1,187.466,0z M124.666,146.6h54c5.5,0,10,4.5,10,10s-4.5,10-10,10h-54c-5.5,0-10-4.5-10-10 S119.166,146.6,124.666,146.6z M248.666,216.6h-124c-5.5,0-10-4.5-10-10s4.5-10,10-10h124c5.5,0,10,4.5,10,10 S254.166,216.6,248.666,216.6z",
                        IsItemSelected = true
                    },
                    new Classes.MenuItems()
                    {
                        Page= "Call",
                        PathData = "M499.66,376.96l-71.68-71.68c-25.6-25.6-69.12-15.359-79.36,17.92c-7.68,23.041-33.28,35.841-56.32,30.72 c-51.2-12.8-120.32-79.36-133.12-133.12c-7.68-23.041,7.68-48.641,30.72-56.32c33.28-10.24,43.52-53.76,17.92-79.36l-71.68-71.68 c-20.48-17.92-51.2-17.92-69.12,0l-48.64,48.64c-48.64,51.2,5.12,186.88,125.44,307.2c120.32,120.32,256,176.641,307.2,125.44 l48.64-48.64C517.581,425.6,517.581,394.88,499.66,376.96z"
                    },
                    new Classes.MenuItems()
                    {
                        Page= "Contacts",
                        PathData = "M26.90625 5C18.039063 5 14.746094 11.851563 17.28125 17.65625C16.964844 17.867188 16.457031 18.527344 16.5625 19.6875C16.773438 21.796875 17.722656 22.300781 18.25 22.40625C18.460938 24.410156 19.730469 26.746094 20.46875 27.0625C20.46875 28.433594 20.449219 29.582031 20.34375 31.0625C18.726563 35.308594 8.390625 34.324219 7.0625 41.9375C7.003906 42.269531 7.347656 43 8.03125 43L42 43C43.015625 43 42.996094 42.25 42.9375 41.90625C41.585938 34.324219 31.273438 35.304688 29.65625 31.0625C29.550781 29.476563 29.53125 28.433594 29.53125 27.0625C30.269531 26.746094 31.445313 24.304688 31.65625 22.40625C32.183594 22.40625 33.027344 21.769531 33.34375 19.65625C33.449219 18.496094 33.015625 17.761719 32.59375 17.65625C34.28125 15.546875 33.976563 6.90625 27.75 6.90625 Z M 15.15625 10C8.480469 10.113281 5.554688 15.425781 7.5625 19.96875C7.347656 20.074219 6.925781 20.632813 7.03125 21.59375C7.246094 23.304688 7.882813 23.71875 8.3125 23.71875C8.523438 25.320313 9.46875 27.117188 10 27.4375C10 28.503906 10.011719 28.472656 9.90625 29.75C9.039063 32.484375 0 32.28125 0 39C0 39 0 40 1 40L5.53125 40C7.058594 35.679688 11.210938 34.015625 14.34375 32.78125C16.070313 32.101563 17.839844 31.414063 18.375 30.5625C18.433594 29.632813 18.464844 28.84375 18.46875 27.96875C17.519531 26.882813 16.8125 25.191406 16.46875 23.75C15.65625 23.148438 14.777344 21.992188 14.5625 19.84375C14.46875 18.824219 14.683594 17.976563 15.03125 17.3125C14.238281 14.847656 14.289063 12.273438 15.15625 10 Z M 38 10C37.042969 10 35.914063 10.078125 34.84375 10.25C35.644531 12.597656 35.675781 15.402344 34.96875 17.40625C35.28125 18.09375 35.425781 18.929688 35.34375 19.84375L35.3125 19.90625L35.3125 19.96875C35.015625 21.933594 34.292969 23.128906 33.4375 23.78125C33.113281 25.132813 32.453125 26.839844 31.53125 27.9375C31.535156 28.796875 31.566406 29.570313 31.625 30.5625C32.160156 31.414063 33.933594 32.101563 35.65625 32.78125C38.785156 34.011719 42.9375 35.679688 44.46875 40L49 40C50 40 50 39.03125 50 39.03125C50.003906 31.296875 41.441406 33.128906 40.15625 29.59375C40.046875 28.414063 40.03125 28.523438 40.03125 27.34375C40.566406 27.023438 41.550781 25.222656 41.65625 23.71875C42.085938 23.71875 42.722656 23.277344 42.9375 21.5625C43.042969 20.707031 42.726563 20.074219 42.40625 19.96875C43.695313 18.253906 43.484375 11.5 38.65625 11.5Z"
                    },
                    new Classes.MenuItems()
                    {
                        Page= "NearBy",
                        PathData = "M255.999,0C152.786,0,68.817,85.478,68.817,190.545c0,58.77,29.724,130.103,88.349,212.017 c42.902,59.948,85.178,102.702,86.957,104.494c3.27,3.292,7.572,4.943,11.879,4.943c4.182,0,8.367-1.558,11.611-4.683 c1.783-1.717,44.166-42.74,87.149-101.86c58.672-80.701,88.421-153.007,88.421-214.912C443.181,85.478,359.21,0,255.999,0z M255.999,272.806c-50.46,0-91.511-41.052-91.511-91.511s41.052-91.511,91.511-91.511s91.511,41.052,91.511,91.511 S306.457,272.806,255.999,272.806z"
                    }, 
                    //To add space i am adding blank item
                    new Classes.MenuItems() {ListItemHeight = 300},
                    new Classes.MenuItems()
                    {
                        Page= "Settings",
                        PathData = "M9.6679688,2L9.1757812,4.5234375C8.3550224,4.8338012,7.5961042,5.2674041,6.9296875,5.8144531L4.5058594,4.9785156 2.1738281,9.0214844 4.1132812,10.707031C4.0445153,11.128986 4,11.558619 4,12 4,12.441381 4.0445153,12.871014 4.1132812,13.292969L2.1738281,14.978516 4.5058594,19.021484 6.9296875,18.185547C7.5961042,18.732596,8.3550224,19.166199,9.1757812,19.476562L9.6679688,22 14.332031,22 14.824219,19.476562C15.644978,19.166199,16.403896,18.732596,17.070312,18.185547L19.494141,19.021484 21.826172,14.978516 19.886719,13.292969C19.955485,12.871014 20,12.441381 20,12 20,11.558619 19.955485,11.128986 19.886719,10.707031L21.826172,9.0214844 19.494141,4.9785156 17.070312,5.8144531C16.403896,5.2674041,15.644978,4.8338012,14.824219,4.5234375L14.332031,2 9.6679688,2z M12,8C14.209,8 16,9.791 16,12 16,14.209 14.209,16 12,16 9.791,16 8,14.209 8,12 8,9.791 9.791,8 12,8z"
                    }
                };

                MenuListBox.ItemsSource = itemList;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void MenuListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Classes.MenuItems selectedGroup = (Classes.MenuItems)MenuListBox.SelectedItem;
                if (selectedGroup != null)
                {
                    if (selectedGroup.Page == "Chat")
                    {
                        if (ChatWindow.ChatWindowContext != null)
                        {
                            ChatWindow.ChatWindowContext.ContentsChatListGrid.Visibility = Visibility.Visible;
                            ChatWindow.ChatWindowContext.SearchGrid.Visibility = Visibility.Visible;
                            ChatWindow.ChatWindowContext.ContentsContactUserGrid.Visibility = Visibility.Collapsed;
                        }
                    }
                    else if (selectedGroup.Page == "Call")
                    {
                        CallWindows callWindows = new CallWindows();
                        callWindows.ShowDialog();
                    }
                    else if (selectedGroup.Page == "Contacts")
                    {
                        if (ChatWindow.ChatWindowContext != null)
                        {
                            ChatWindow.ChatWindowContext.ContentsChatListGrid.Visibility = Visibility.Collapsed;
                            ChatWindow.ChatWindowContext.SearchGrid.Visibility = Visibility.Collapsed;
                            ChatWindow.ChatWindowContext.ContentsContactUserGrid.Visibility = Visibility.Visible;
                        }
                    }
                    else if (selectedGroup.Page == "NearBy")
                    {
                        NearByWindows nearByWindows = new NearByWindows();
                        nearByWindows.ShowDialog();
                    }
                    else if (selectedGroup.Page == "Settings")
                    {
                        SettingsWindows settingsWindows = new SettingsWindows();
                        settingsWindows.ShowDialog();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}