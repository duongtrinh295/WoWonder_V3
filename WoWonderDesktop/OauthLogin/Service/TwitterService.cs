using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWonderClient;

namespace WoWonderDesktop.OauthLogin.Service
{
    // Twitter support is technically not Oauth2
    // 4 years worth of fun: 
    // https://twittercommunity.com/t/oauth-2-0-support/253
    // https://twittercommunity.com/t/how-to-get-email-from-twitter-user-using-oauthtokens/558/155

    // ALSO REALLY IMPORTANT: 
    // Twitter QueryStringBuilder.Build parameters MUST BE ALPHABETICALLY ORDERED
    // Yeah, talk about idiotic API...
    // https://dev.twitter.com/oauth/overview/creating-signatures#note-lexigraphically
    public class TwitterService
    {
        public static string ClientId = "xxx_CLIENT_ID_xxx";
        public static string ClientSecret = "xxx_CLIENT_SECRET_xxx";
        public static string CallbackUrl = InitializeWoWonder.WebsiteUrl + "/login-with.php?provider=Twitter";
        public static string Token = "";
        public static string TokenSecret = "";

        private readonly OAuth2Util Util = new OAuth2Util();

        public async Task<string> BeginAuthentication()
        {
            var requestUrl = "https://api.twitter.com/oauth/request_token";
            var qstring = "?oauth_consumer_key=" + ClientId
                          + "&oauth_nonce=" + Util.GetNonce()
                          + "&oauth_signature_method=HMAC-SHA1"
                          + "&oauth_timestamp=" + Util.GetTimeStamp()
                          + "&oauth_version=1.0";

            var signature = Util.GetSha1Signature("POST", requestUrl, qstring, ClientSecret);
            var lastLink = requestUrl + qstring + "&oauth_signature=" + Uri.EscapeDataString(signature);

            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(lastLink, null);
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            var twitterAuthResp = new TwitterAuthResponse(json);
            Console.WriteLine(twitterAuthResp.OAuthToken);

            return requestUrl;
        }

        // TODO: Refactor
        public class TwitterAuthResponse
        {
            public string OAuthToken { get; set; }
            public string OAuthTokenSecret { get; set; }
            public bool OAuthCallbackConfirmed { get; set; }
            public string OAuthAuthorizeUrl { get; set; }
            public long UserId { get; set; }
            public string ScreenName { get; set; }

            public TwitterAuthResponse(string responseText)
            {
                string[] keyValPairs = responseText.Split('&');

                for (int i = 0; i < keyValPairs.Length; i++)
                {
                    string[] splits = keyValPairs[i].Split('=');
                    switch (splits[0])
                    {
                        case "oauth_token":
                            OAuthToken = splits[1];
                            break;
                        case "oauth_token_secret":
                            OAuthTokenSecret = splits[1];
                            break;
                        case "oauth_callback_confirmed":
                            OAuthCallbackConfirmed = splits[1] == "true";
                            break;
                        case "xoauth_request_auth_url":
                            OAuthAuthorizeUrl = splits[1];
                            break;
                        case "user_id":
                            UserId = long.Parse(splits[1]);
                            break;
                        case "screen_name":
                            ScreenName = splits[1];
                            break;
                    }
                }
            }
        }

        public async Task<string> RequestToken(string oauthToken, string oauthVerifier)
        {
            var requestUrl = "https://api.twitter.com/oauth/request_token";
            var qstring = "?oauth_consumer_key=" + ClientId
                                                 + "&oauth_nonce=" + Util.GetNonce()
                                                 + "&oauth_signature_method=HMAC-SHA1"
                                                 + "&oauth_timestamp=" + Util.GetTimeStamp()
                                                 + "&oauth_token=" + oauthToken
                                                 + "&oauth_verifier=" + oauthVerifier
                                                 + "&oauth_version=1.0";

            var signature = Util.GetSha1Signature("POST", requestUrl, qstring, ClientSecret);
            var lastLink = requestUrl + qstring + "&oauth_signature=" + Uri.EscapeDataString(signature);

            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(lastLink, null);
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            var twitterAuthResp = new TwitterAuthResponse(json);
            Console.WriteLine(twitterAuthResp.OAuthToken);
             
            Token = twitterAuthResp.OAuthToken;
            TokenSecret = twitterAuthResp.OAuthTokenSecret;

            return twitterAuthResp.OAuthToken;
        }

        public async void RequestUserProfile()
        {
            const string callingUrl = "https://api.twitter.com/1.1/account/verify_credentials.json";

            var qstring = GeneratePayload("GET", callingUrl);

            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(callingUrl + "?" + qstring);
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<TwitterUserData>(json);

            Console.WriteLine(data);
        }

        public async Task<bool> VerifyFollowing(string screenName)
        {
            const string callingUrl = "https://api.twitter.com/1.1/friendships/lookup.json";

            var qstring = GeneratePayload("GET", callingUrl, "screen_name", screenName);

            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(callingUrl + "?" + qstring);
            var json = await response.Content.ReadAsStringAsync();
            var friendshipList = JsonConvert.DeserializeObject<List<Friendship>>(json);
            
            switch (friendshipList.Count)
            {
                case 1 when friendshipList[0].Connections.Contains("following"):
                    return true;
                default:
                    return false;
            }
        }

        public async Task<TwitterUserData> FollowUser(string screenName)
        {
            const string callingUrl = "https://api.twitter.com/1.1/friendships/create.json";

            var qstring = GeneratePayload("POST", callingUrl, "follow", "true", "screen_name", screenName);
             
            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(callingUrl, new StringContent(qstring));
            var json = await response.Content.ReadAsStringAsync();
            var twitterUser = JsonConvert.DeserializeObject<TwitterUserData>(json);
            return twitterUser;
        }

        private string GeneratePayload(string mode, string callingUrl, params object[] pars)
        {
            var paramsList = new Dictionary<string, object>
            {
                {"oauth_consumer_key", ClientId},
                {"oauth_nonce", Util.GetNonce()},
                {"oauth_signature_method", "HMAC-SHA1"},
                {"oauth_timestamp", Util.GetTimeStamp()},
                {"oauth_token", Token},
                {"oauth_version", "1.0"}
            };

            for (int i = 0; i < pars.Length; i += 2)
            {
                var key = pars[i];
                var val = pars[i + 1];

                paramsList.Add((string)key, val);
            }

            var qstring = JsonConvert.SerializeObject(paramsList);

            var signature = Util.GetSha1Signature(mode, callingUrl, qstring, ClientSecret, TokenSecret);
            qstring += "&oauth_signature=" + Uri.EscapeDataString(signature);

            return qstring;
        }
         
    }

    public class Friendship
    {
        public string Name { get; set; }
        public string ScreenName { get; set; }
        public long Id { get; set; }
        public string IdStr { get; set; }
        public List<string> Connections { get; set; }
    }

    public class TwitterUserData
    {
        public long Id { get; set; }
        public string IdStr { get; set; }
        public string Name { get; set; }
        public string ScreenName { get; set; }
        public string Location { get; set; }
        public string ProfileLocation { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }

        public bool Protected { get; set; }
        public bool Verified { get; set; }

        public int FollowersCount { get; set; }
        public int FriendsCount { get; set; }
        public int ListedCount { get; set; }
        public int FavouritesCount { get; set; }
        public int StatusesCount { get; set; }

        public bool Following { get; set; }
        public bool FollowingRequestSent { get; set; }
        public bool Notifications { get; set; }

        // override
        public string UserId { get { return IdStr; } }
        public string FullName { get { return Name; } }

        // not shared by twitter
        public string Email { get { return null; } }
        public string PhoneNumber { get { return null; } }
    }
}