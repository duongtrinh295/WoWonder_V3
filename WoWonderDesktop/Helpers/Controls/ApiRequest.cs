using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;

namespace WoWonderDesktop.Helpers.Controls
{
    internal static class ApiRequest
    {
        internal static readonly string ApiGetSearchGif = "https://api.giphy.com/v1/gifs/search?api_key=b9427ca5441b4f599efa901f195c9f58&limit=15&rating=g&q=";
        internal static readonly string ApiGeTrendingGif = "https://api.giphy.com/v1/gifs/trending?api_key=b9427ca5441b4f599efa901f195c9f58&limit=15&rating=g";
        internal static readonly string ApiGetTimeZone = "http://ip-api.com/json/";
          
        public static async Task<string> GetTimeZoneAsync()
        {
            try
            {
                if (Methods.CheckForInternetConnection() && Settings.AutoCodeTimeZone)
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync(ApiGetTimeZone);
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<TimeZoneObject>(json);
                    if(data != null)
                    {
                        UserDetails.Lat = data.Lat.ToString(CultureInfo.InvariantCulture);
                        UserDetails.Lng = data.Lon.ToString(CultureInfo.InvariantCulture);

                        return data?.Timezone;
                    }
                }
                return Settings.CodeTimeZone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Settings.CodeTimeZone;
            }
        }
         
        public static async Task<ObservableCollection<GifGiphyClass.Datum>> SearchGif(string searchKey, string offset)
        {
            try
            {
                if (!Methods.CheckForInternetConnection())
                {
                    MessageBox.Show(LocalResources.label_Please_check_your_internet, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return null;
                }
                else
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync(ApiGetSearchGif + searchKey + "&offset=" + offset);
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<GifGiphyClass>(json);

                    return response.StatusCode switch
                    {
                        HttpStatusCode.OK => data.DataMeta.Status == 200
                            ? new ObservableCollection<GifGiphyClass.Datum>(data.Data)
                            : null,
                        _ => null!
                    };
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public static async Task<ObservableCollection<GifGiphyClass.Datum>> TrendingGif(string offset)
        {
            try
            {
                if (!Methods.CheckForInternetConnection())
                {
                    MessageBox.Show(LocalResources.label_Please_check_your_internet, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return null;
                }
                else
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync(ApiGeTrendingGif + "&offset=" + offset);
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<GifGiphyClass>(json);

                    return response.StatusCode switch
                    {
                        HttpStatusCode.OK => data.DataMeta.Status == 200
                            ? new ObservableCollection<GifGiphyClass.Datum>(data.Data)
                            : null,
                        _ => null!
                    };
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }
          
        public static async Task<GetSiteSettingsObject.ConfigObject> GetSettings_Api()
        {
            try
            {
                if (Methods.CheckForInternetConnection())
                {
                    (var apiStatus, dynamic respond) = await Current.GetSettingsAsync();

                    if (apiStatus != 200 || respond is not GetSiteSettingsObject result || result.Config == null)
                        return Methods.DisplayReportResult(respond);
                 
                    //Products Categories
                    var listProducts = result.Config.ProductsCategories.Select(cat => new Classes.Categories
                    {
                        CategoriesId = cat.Key,
                        CategoriesName = cat.Value,
                        CategoriesColor = "#ffffff"
                    }).ToList();

                    ListUtils.ListCategoriesProducts.Clear();
                    ListUtils.ListCategoriesProducts = new ObservableCollection<Classes.Categories>(listProducts);
                     
                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertOrUpdateSettings(result.Config);
                 
                    return ListUtils.SettingsSiteList;
                }
                else
                {
                    MessageBox.Show(LocalResources.label_Please_check_your_internet, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return null;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }


        public static async Task Get_MyProfileData_Api(string userid)
        {
            if (Methods.CheckForInternetConnection())
            {
                (int apiStatus, var respond) = await RequestsAsync.Global.GetUserDataAsync(userid , "user_data");
                if (apiStatus == 200 && respond is GetUserDataObject result) 
                {
                    var dbt = ClassMapper.Mapper?.Map<UserDataFody>(result.UserData);
                    if (userid == UserDetails.UserId)
                    {
                        UserDetails.Avatar = result.UserData.Avatar;
                        UserDetails.Cover = result.UserData.Cover;
                        UserDetails.Username = result.UserData.Username;
                        UserDetails.FullName = result.UserData.Name;
                        UserDetails.Email = result.UserData.Email;
                         
                        ListUtils.MyProfileList = new ObservableCollection<UserDataFody>() { dbt };

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.Insert_Or_Update_To_MyProfileTable(dbt);

                        //ChatWindow?.My_Profile();

                        //if (result.Following?.Count > 0) 
                        //    ChatWindow?.UsersContact(result.Following);
                    }
                }
                else Methods.DisplayReportResult(respond);
            }
        }
         
        public static async Task GetArchivedChats()
        {
            if (Methods.CheckForInternetConnection() && !string.IsNullOrEmpty(UserDetails.AccessToken) && Settings.EnableChatArchive)
            {
                var (apiStatus, respond) = await RequestsAsync.Message.GetArchivedChatsAsync().ConfigureAwait(false);
                if (apiStatus != 200 || respond is not LastChatObject result || result.Data == null)
                {
                    //Methods.DisplayReportResult(respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data let check = ListUtils.ArchiveList.FirstOrDefault(a => a.ChatId == item.ChatId) where check == null select WoWonderTools.UserChatFilter(item))
                        {
                            ListUtils.ArchiveList?.Add(new Classes.LastChatArchive
                            {
                                ChatType = item.ChatType,
                                ChatId = item.ChatId,
                                UserId = item.UserId,
                                GroupId = item.GroupId,
                                PageId = item.PageId,
                                Name = item.Name,
                                IdLastMessage = item?.LastMessage.LastMessageClass?.Id.ToString() ?? "",
                                LastChat = item,
                            });
                        }
                         
                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.InsertORUpdateORDelete_ListArchive(ListUtils.ArchiveList.ToList());
                    }
                }
            }
        }

        public static async Task GetPinChats()
        {
            if (Methods.CheckForInternetConnection() && !string.IsNullOrEmpty(UserDetails.AccessToken) &&  Settings.EnableChatPin)
            {
                var (apiStatus, respond) = await RequestsAsync.Message.GetPinChatsAsync().ConfigureAwait(false);
                if (apiStatus != 200 || respond is not LastChatObject result || result.Data == null)
                {
                    //Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    { 
                        foreach (var item in from item in result.Data let check = ListUtils.PinList.FirstOrDefault(a => a.ChatId == item.ChatId) where check == null select WoWonderTools.UserChatFilter(item))
                        {
                            if (item.IsArchive)
                                continue;

                            ListUtils.PinList?.Add(new Classes.LastChatArchive
                            {
                                ChatType = item.ChatType,
                                ChatId = item.ChatId,
                                UserId = item.UserId,
                                GroupId = item.GroupId,
                                PageId = item.PageId,
                                Name = item.Name,
                                IdLastMessage = item?.LastMessage.LastMessageClass?.Id.ToString() ?? "",
                                LastChat = item,
                            });
                        }

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.InsertORUpdateORDelete_ListPin(ListUtils.PinList.ToList());
                    }
                }
            }
        }
         
        public static void Logout()
        {
            try
            {
                //close all window
                if (Methods.CheckForInternetConnection())
                {
                    SqLiteDatabase database = new SqLiteDatabase();
                    database.DropAll();

                    if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                    {
                        UserDetails.Socket?.Emit_loggedoutEvent(UserDetails.AccessToken);
                        UserDetails.Socket?.DisconnectSocket();
                    }

                    ListUtils.ClearAllList();

                    //Methods.Path.ClearFolder();
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() =>  RequestsAsync.Auth.DeleteTokenAsync(UserDetails.AccessToken) });
                }

                var assemblyName = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = assemblyName,
                        //RedirectStandardOutput = true,
                        //UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();

                Application.Current.Shutdown();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
} 