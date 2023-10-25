using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using WoWonderClient.Classes.Event;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;

namespace WoWonderDesktop.Helpers.Model
{
    public class UserListFody : ChatObject, INotifyPropertyChanged
    {
        public new Visibility UVerified { set; get; }
        public new string SMessageFontWeight { set; get; }
        public new Visibility IsSeenIconCheck { set; get; } 
        public new Visibility IsPinIconVisibility { set; get; }
        public new Visibility IsMuteIconVisibility { set; get; }
        public new Visibility ChatColorCircleVisibility { set; get; }
        public new Visibility MessageCountVisibility { set; get; }
        public new Visibility MediaIconVisibility { set; get; }
        public new string MediaIconImage { set; get; }
        public new string AppMainLater { set; get; }
        public new string UsernameTwoLetters { set; get; }
        public new string SColorBackground { set; get; }
        public new string SColorForeground { set; get; }
        public new string LastMessageColor { set; get; }
        public new string LastMessageText { set; get; }
        public new ObservableCollection<string> MenuChatItems { set; get; }


        public event PropertyChangedEventHandler PropertyChanged;
       
        protected void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            PropertyChanged?.Invoke(this, eventArgs);
        }
    }

    public class UserDataFody : UserDataObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public new Visibility UVerified { set; get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
      
    public class SessionDataFody : FetchSessionsObject.SessionsDataObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public new Visibility SIpAddressVisibility { set; get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
     
    public class EventDataFody : EventDataObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public new UserDataFody UserData { set; get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
     
    public class MessagesDataFody : MessageData, INotifyPropertyChanged
    {
        public new int Progress_Value { set; get; }
        public new string sound_time { set; get; }
        public new int sound_slider_value { set; get; }
        public new Visibility Pause_Visibility { set; get; }
        public new Visibility Play_Visibility { set; get; }
        public new Visibility Download_Visibility { set; get; }
        public new Visibility Icon_File_Visibility { set; get; }
        public new Visibility Hlink_Download_Visibility { set; get; }
        public new Visibility Hlink_Open_Visibility { set; get; }
        public new string Img_user_message { set; get; }
        public new string Type_Icon_File { set; get; }
        public new Visibility ProgresSVisibility { set; get; }
        public new Visibility Save_img_Visibility { set; get; }
        public new string EndChatColor { set; get; }
        public new Visibility ReplyPanel_Visibility { set; get; }
        public new string Reply_TxtOwnerName { set; get; }
        public new string Reply_TxtMessageType { set; get; }
        public new string Reply_TxtShortMessage { set; get; }
        public new string Reply_MessageFileThumbnail { set; get; }
        public new Visibility Reply_MessageFileThumbnail_Visibility { set; get; }
        public new Visibility Reply_TxtMessageType_Visibility { set; get; }
        public new Visibility Reply_TxtShortMessage_Visibility { set; get; } 
        public new Visibility Forwarded_Visibility { set; get; }
        public new ImageSource ReactionImage { set; get; }
        public new Visibility ReactionVisibility { set; get; }
        public new Visibility FavoriteVisibility { set; get; }
        public new Visibility SeenVisibility { set; get; }
        public new string Seen { set; get; }

        public new ObservableCollection<string> MenuMessageItems { set; get; }
        public new ObservableCollection<ReactFody> ReactionMessageItems { set; get; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            PropertyChanged?.Invoke(this, eventArgs);
        }
    }
}