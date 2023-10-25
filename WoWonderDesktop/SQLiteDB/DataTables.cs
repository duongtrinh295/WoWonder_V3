using SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderDesktop.Helpers.Model;

namespace WoWonderDesktop.SQLiteDB
{
    public class DataTables
    {
        [Table("LoginTb")]
        public class LoginTb
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdLogin { get; set; }

            public string UserId { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string AccessToken { get; set; }
            public string Cookie { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
            public string Lang { get; set; }
            public string DeviceId { get; set; }
        }

        [Table("SettingsTb")]
        public class SettingsTb : GetSiteSettingsObject.ConfigObject
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdSettings { get; set; }

            public new string CurrencyArray { get; set; }
            public new string CurrencySymbolArray { get; set; }
            public new string PageCategories { get; set; }
            public new string GroupCategories { get; set; }
            public new string BlogCategories { get; set; }
            public new string ProductsCategories { get; set; }
            public new string JobCategories { get; set; }
            public new string Genders { get; set; }
            public new string Family { get; set; }
            public new string MovieCategory { get; set; }
            public new string PostColors { get; set; }
            public new string Fields { get; set; }
            public new string PageSubCategories { get; set; }
            public new string GroupSubCategories { get; set; }
            public new string ProductsSubCategories { get; set; }
            public new string PageCustomFields { get; set; }
            public new string GroupCustomFields { get; set; }
            public new string ProductCustomFields { get; set; }
            public new string PostReactionsTypes { get; set; }
            public new string ProPackages { get; set; } 
        }

        [Table("SettingsAppTb")]
        public class SettingsAppTb  
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdSettingsApp { get; set; }

            public string NotificationDesktop { get; set; } 
            public string NotificationPlaysound { get; set; } 
            public string BackgroundChatsImages { get; set; } 
            public string LangResources { get; set; } 
            public string DarkMode { get; set; }
        }

        [Table("MyContactsTb")]
        public class MyContactsTb : UserDataObject
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdMyFollowing { get; set; }

            public new long UserId { get; set; }
            public new string ApiNotificationSettings { get; set; }
            public new string Details { get; set; }
        }

        [Table("MyProfileTb")]
        public class MyProfileTb : UserDataObject
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdMyProfile { get; set; }

            public new string ApiNotificationSettings { get; set; }
            public new string Details { get; set; }
        }

        [Table("LastUsersTb")]
        public class LastUsersTb : ChatObject
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdLastUsers { get; set; }

            public new string ApiNotificationSettings { get; set; }
            public new string Details { get; set; }
            public new string UserData { get; set; }
            public new string LastMessage { get; set; }
            public new string Parts { get; set; }
            public new string Mute { get; set; }
            public new string MenuChatItems { get; set; }
        }
         
        [Table("MessageTb")]
        public class MessageTb : MessageData
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdMessage { get; set; }

            public new string Product { get; set; }
            public new string MessageUser { get; set; }
            public new string UserData { get; set; }
            public new string ToData { get; set; }
            public new string Reaction { get; set; }
            public new string Reply { get; set; }
            public new string Story { get; set; }
            public new string MenuMessageItems { get; set; }
            public new string ReactionMessageItems { get; set; }
        }

        [Table("StickersTable")]
        public class StickersTable
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string PackageId { get; set; }
            public string SName { get; set; }
            public string SCount { get; set; }
            public bool SVisibility { get; set; }

            public string StickerList { get; set; }
        }


        [Table("CallVideoTable")]
        public class CallVideoTable
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string CallVideoCallId { set; get; }
            public string CallVideoUserId { set; get; }
            public string CallVideoTupeIcon { set; get; }
            public string CallVideoColorIcon { set; get; }
            public string CallVideoAvatar { set; get; }
            public string CallVideoUserName { set; get; }
            public string CallVideoUserDataTime { set; get; }
        }
         
        [Table("MuteTb")]
        public class MuteTb : Classes.OptionLastChat
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdMute { get; set; }

        }

        [Table("PinTb")]
        public class PinTb : Classes.LastChatArchive
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdPin { get; set; }

            public new string LastChat { get; set; }
            public new string LastChatPage { get; set; }
        }

        [Table("ArchiveTb")]
        public class ArchiveTb : Classes.LastChatArchive
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdArchive { get; set; }
             
            public new string LastChat { get; set; }
            public new string LastChatPage { get; set; }
        }
         
    }
} 