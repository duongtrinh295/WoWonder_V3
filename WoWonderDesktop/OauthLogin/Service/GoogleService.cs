using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWonderClient;
using WoWonderDesktop.Helpers.Utils;

namespace WoWonderDesktop.OauthLogin.Service
{
    public static class GoogleService 
    {
        public static string Clientid = "430795656343-679a7fus3pfr1ani0nr0gosotgcvq2s8.apps.googleusercontent.com";
        public static string Clientsecret = "xuTYDJwOivWG3qNhsYFOZ2jB";
        public static string CallbackUrl = InitializeWoWonder.WebsiteUrl + "/login-with.php?provider=Google";
        public static string Scope = "https://www.googleapis.com/auth/userinfo.email+https://www.googleapis.com/auth/userinfo.profile";

        public static string BeginAuthentication()
        {
            try
            {
                var requestUrl = "https://accounts.google.com/o/oauth2/auth"
                                 + "?scope=" + Scope
                                 + "&state=1"
                                 + "&redirect_uri=" + CallbackUrl
                                 + "&client_id=" + Clientid
                                 + "&response_type=code" 
                                 + "&approval_prompt=auto"
                                 + "&access_type=online";

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
                var requestUrl = "https://accounts.google.com/o/oauth2/token"
                                 + "?code=" + code
                                 + "&client_id=" + Clientid
                                 + "&client_secret=" + Clientsecret
                                 + "&redirect_uri=" + CallbackUrl
                                 + "&grant_type=authorization_code";

                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(requestUrl, null);
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                Console.WriteLine(data);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async void RequestUserProfile(string token)
        {
            string profileUrl = "https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + token;

            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(profileUrl, null);
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            Console.WriteLine(data); 
        }
    }

    public class GoogleUserData  
    { 
        public string Id { get; set; }
        public string Email { get; set; }
        public bool VerifiedEmail { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Link { get; set; }
        public string Picture { get; set; }
        public string Gender { get; set; }

        // override
        public string UserId { get { return Id; } } 
        public string FullName { get { return Name; } }

        // not implemented
        public string PhoneNumber { get { return null; } }
    }
}