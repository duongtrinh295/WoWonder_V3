using WoWonderDesktop.SocketSystem;

namespace WoWonderDesktop.Helpers.Model
{
    public static class UserDetails
    {
        public static string AccessToken = "";
        public static string UserId = "";
        public static string Username = "";
        public static string FullName = "";
        public static string Password = "";
        public static string Email = "";
        public static string Cookie = "";
        public static string Status;
        public static string Avatar = "";
        public static string Cover = "";
        public static string DeviceId = "";
        public static string Lang = "";
        public static string Lat = "";
        public static string Lng = "";
        public static bool NotificationPopup { get; set; } = true;

        public static string Time = "";

        public static string SearchGender = "all";
        public static string SearchCountry = "all";
        public static string SearchStatus = "all";
        public static string SearchVerified = "all";
        public static string SearchProfilePicture = "all";
        public static string SearchFilterByAge = "off";
        public static string SearchAgeFrom = "10";
        public static string SearchAgeTo = "70";

        public static bool ModeDarkStlye = false;
        public static bool OnlineUsers = true;
        public static bool SoundControl = true;
         

        public static WoSocketHandler Socket { get; set; }
    }
}
