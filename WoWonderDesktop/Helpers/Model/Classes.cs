using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;

namespace WoWonderDesktop.Helpers.Model
{
    //############# DONT'T MODIFY HERE #############
    public class Classes
    {
        public class Categories
        {
            public string CategoriesId { get; set; }
            public string CategoriesName { get; set; }
            public string CategoriesColor { get; set; }
            public List<SubCategories> SubList { get; set; }
        }

        public class OptionLastChat
        {
            public string ChatId { set; get; }
            public string PageId { set; get; }
            public string GroupId { set; get; }
            public string UserId { set; get; }
            public string Name { set; get; }
            public string ChatType { set; get; }
        }

        public class LastChatArchive : OptionLastChat
        {
            public string IdLastMessage { set; get; }

            public ChatObject LastChat { get; set; }
            public PageDataObject LastChatPage { get; set; }
        }
        
        public class SharedFile : INotifyPropertyChanged
        {
            public string FileName { set; get; }
            public string FileType { set; get; }
            public string FileDate { set; get; }
            public string FilePath { set; get; }
            public string FileExtension { set; get; }
            public string ImageUrl { set; get; }
            //style
            public Visibility VoiceFrameVisibility { set; get; }
            public Visibility VideoFrameVisibility { set; get; }
            public Visibility ImageFrameVisibility { set; get; }
            public Visibility FileFrameVisibility { set; get; }
            public Visibility EmptyLabelVisibility { set; get; }
            public string SColorBackground { set; get; }
            public string SColorForeground { set; get; }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
            {
                PropertyChanged?.Invoke(this, eventArgs);
            }
        }
         
        public class CallVideo : INotifyPropertyChanged
        {
            public string CallVideoCallId { set; get; }
            public string CallVideoUserId { set; get; }
            public string CallVideoTupeIcon { set; get; }
            public string CallVideoColorIcon { set; get; }
            public string CallVideoAvatar { set; get; }
            public string CallVideoUserName { set; get; }
            public string CallVideoUserDataTime { set; get; }
            //style
            public string SColorBackground { set; get; }
            public string SColorForeground { set; get; }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
            {
                PropertyChanged?.Invoke(this, eventArgs);
            }
        }
            
        public class MenuItems
        {
            public string Page { get; set; }
            public string PathData { get; set; }

            public int ListItemHeight { get; set; }

            public bool IsItemSelected { get; set; }
        }

        public class MoreOptionsMenu
        {
            public PathGeometry Icon { get; set; }
            public string MenuText { get; set; }
            public string BorderStroke { get; set; }
            public string Fill { get; set; }

        }
    } 
}
