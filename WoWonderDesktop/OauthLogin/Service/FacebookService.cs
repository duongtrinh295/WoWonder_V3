using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWonderDesktop.Helpers.Utils;

namespace WoWonderDesktop.OauthLogin.Service
{
    public static class FacebookService  
    {
        public static string Clientid = "2805434832884289";
        public static string Clientsecret = "9bff5e8c9b7bdd3c782d2f9f1ce9641c";
        public static string CallbackUrl = /*Client.WebsiteUrl + */"https://www.facebook.com/connect/login_success.html";
        public static string Scope = "public_profile,email";

        public static string BeginAuthentication()
        {
            try
            {
                var requestUrl = "https://www.facebook.com/dialog/oauth"
                                 + "?client_id=" + Clientid
                                 + "&redirect_uri=" + CallbackUrl
                                 + "&state="
                                 + "&scope=" + Scope
                                 + "&display=popup";

                return requestUrl; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }

        public static async Task RequestToken(string code)
        {
            try
            {
                var requestUrl = "https://graph.facebook.com/oauth/access_token"
                                 + "?code=" + code
                                 + "&client_id=" + Clientid
                                 + "&redirect_uri=" + CallbackUrl 
                                 + "&client_secret=" + Clientsecret;

                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(requestUrl, null);
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                Console.WriteLine(data);

                //RequestUserProfile
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            } 
        }
         
        public static async void RequestUserProfile(string token)
        {
            var fields = "id,email,name,first_name,last_name,link,gender,locale,timezone,verified";
            var profileUrl = "https://graph.facebook.com/me?fields="+ fields +"&access_token=" + token;

            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(profileUrl, null);
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            Console.WriteLine(data);
        }
    }

    public class FacebookUserData 
    { 
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Link { get; set; }
        public string Gender { get; set; }
        public string Picture { get; set; }
        public string Locale { get; set; }
        public int Timezone { get; set; }
        public bool Verified { get; set; }

        // override
        public  string UserId { get { return Id; } } 
        public  string FullName { get { return Name; } }
                
        public  string PhoneNumber { get { return null; } }
    }
}