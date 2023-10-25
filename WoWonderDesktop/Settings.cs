using SocketIOClient.Transport;
using WoWonderClient;

namespace WoWonderDesktop
{
    //WOWONDER DESKTOP MESSENGER V3.4
    //*********************************************************
    //1- Add Your name application From Solution Explorer go to WoWonderDesktop > ChatWindow.xaml > Rename Title = "Wowonder"  in lines(16 - 67)
    //2- Add Your logo From Solution Explorer go to WoWonderDesktop > Images
    //3- Right click on Resources folder > Add > Existing Item > pick your own logo.png
    //Same thing for all images and icons

    internal static class Settings
    {
		//Main Settings >>>>>
		//*********************************************************
		public static readonly string TripleDesAppServiceProvider = "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";

		public static string Version = "3.4";

        public static readonly string ApplicationName = "WoWonder Desktop";
        public static readonly string DatabaseName = "WoWonderDesktop";

        public static readonly InitializeWoWonder.ConnectionType ConnectionTypeChat = InitializeWoWonder.ConnectionType.Socket; //New 
        public static readonly TransportProtocol Transport = TransportProtocol.Polling;

        public static readonly string PortSocketServer = "449"; //New
         
        // Friend system = 0 , follow system = 1
        public static readonly int ConnectivitySystem = 1;

        //Language Settings >>>>>
        //*********************************************************
        public static bool FlowDirectionRightToLeft = false;
        public static string LangResources = ""; // >>>>  default language
         
        //Main Colors 
        //*********************************************************
        public static readonly string MainColor = "#a52729";
        public static readonly string SeconderyColor = "#444444";

        //Dark Colors >> 
        public static readonly string DarkBackground_Color = "#232323";
        public static readonly string LigthBackground_Color = "#444444";
        public static readonly string WhiteBackground_Color = "#efefef";

        //Ligth Colors >> 
        public static string DarkBackgroundColor = "#444444";
        public static string LigthBackgroundColor = "#f8f8f8";
        public static string WhiteBackgroundColor = "#ffffff";

        public static readonly bool ColorMessageThemeGradient = true; //New 

        // Options List Chat
        public static readonly bool EnableChatArchive = true; //#New
        public static readonly bool EnableChatPin = true; //#New
        public static readonly bool EnableChatMute = true; //#New

        public static readonly bool EnableReplyMessageSystem = true; //New 
        public static readonly bool EnableForwardMessageSystem = true; //New 
        public static readonly bool EnablePinMessageSystem = true; //New  
        public static readonly bool EnableFavoriteMessageSystem = true; //New 

        /// <summary>
        /// https://developers.google.com/maps/documentation/maps-static/intro
        /// </summary>
        public static readonly string GoogleMapsKey = "AIzaSyAgsF7RPNJeiqECwlAoNwvAIE9phnHOudM"; //New 
         
        /// <summary>
        /// https://dashboard.stipop.io/
        /// you can get api key from here https://prnt.sc/26ofmq9
        /// </summary>
        public static readonly string StickersApikey = "0a441b19287cad752e87f6072bb914c0"; //#New
         
        //Bypass Web Erros (OnLogin crach or error set it to true)
        //*********************************************************
        public static readonly bool WebExceptionSecurity = true; // >>>>  Security Protocol Type ssl
         
        //Messages Control 
        ///*********************************************************
        public static readonly int RefreshChatActivitiesPer = 6000; // 6 Seconds

        public static readonly int UpdateMessageReceiverInt = 4000; // 4 Seconds

        // login using social media
        //*********************************************************
        public static readonly bool FacebookIcon = false; //>> Next version
        public static readonly bool TwitterIcon = false; //>> Next version
        public static bool VkIcon = false; //>> Next version
        public static readonly bool GoogleIcon = false; //>> Next version
        public static readonly bool InstagramIcon = false; //>> Next version

        //Login Icons 
        //*********************************************************
        public static string LogoImg = "Images/icon.ico";
        public static readonly string LoginImg = "signup_bg.png";
        public static readonly string RegisterImg = "44.jpg";

        //=============== Message Popup Window Style ==========================
        public static readonly string PopUpBackroundColor = "White";
        public static readonly string PopUpTextFromcolor = "#444444";
        public static readonly string PopUpMsgTextcolor = "#444444";

        //=============== Call ==========================
        public static bool VideoCall = true;

        //=============== Notification ==========================
        public static string NotificationDesktop = "true";
        public static string NotificationPlaysound = "true";

        // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

        //Code Time Zone (true => Get from Internet , false => Get From #CodeTimeZone )
        //*********************************************************
        public static readonly bool AutoCodeTimeZone = true;
        public static readonly string CodeTimeZone = "UTC";

        //Error Report Mode
        //*********************************************************
        public static readonly bool SetApisReportMode = true;

        /// <summary>
        ///  Currency
        /// CurrencyStatic = true : get currency from app not api 
        /// CurrencyStatic = false : get currency from api (default)
        /// </summary>
        public static readonly bool CurrencyStatic = false;

        public static readonly string CurrencyIconStatic = "$";
        public static readonly string CurrencyCodeStatic = "USD";
          
        public static readonly bool ShowTextWithSpace = false; //New 

        public static readonly bool EnableChatGroup = true; //New 
        public static readonly bool EnableChatPage = false; //New 

        public static readonly bool EnableEvent = true; //New 
    }
}