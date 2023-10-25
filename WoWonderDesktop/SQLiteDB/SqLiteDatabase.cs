using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SQLite;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.Story;

namespace WoWonderDesktop.SQLiteDB
{
    public class SqLiteDatabase
    {
        //############# DON'T MODIFY HERE #############
        private static readonly string Folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + Settings.ApplicationName + "\\Database\\";
        private static readonly string PathCombine = Path.Combine(Folder, Settings.DatabaseName + "_.db3");
         
        //Open Connection in Database
        //*********************************************************

        #region Connection

        private SQLiteConnection OpenConnection()
        {
            try
            {
                if (!Directory.Exists(Folder))
                    Directory.CreateDirectory(Folder);

                var connection = new SQLiteConnection(new SQLiteConnectionString(PathCombine, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true));
                return connection;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public void CheckTablesStatus()
        {
            try
            {
                using var connection = OpenConnection();
                connection?.CreateTable<DataTables.LoginTb>();
                connection?.CreateTable<DataTables.SettingsAppTb>();
                connection?.CreateTable<DataTables.SettingsTb>();
                connection?.CreateTable<DataTables.MyContactsTb>();
                connection?.CreateTable<DataTables.MyProfileTb>();
                connection?.CreateTable<DataTables.CallVideoTable>();
                connection?.CreateTable<DataTables.LastUsersTb>();
                connection?.CreateTable<DataTables.MessageTb>();
                connection?.CreateTable<DataTables.StickersTable>();

                connection?.CreateTable<DataTables.MuteTb>();
                connection?.CreateTable<DataTables.PinTb>();
                connection?.CreateTable<DataTables.ArchiveTb>(); 
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    CheckTablesStatus();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }
         
        //Delete table 
        public void DropAll()
        {
            try
            {
                using var connection = OpenConnection();
                connection?.DropTable<DataTables.LoginTb>();
                connection?.DropTable<DataTables.SettingsAppTb>();
                connection?.DropTable<DataTables.SettingsTb>();
                connection?.DropTable<DataTables.MyContactsTb>();
                connection?.DropTable<DataTables.MyProfileTb>();
                connection?.DropTable<DataTables.CallVideoTable>();
                connection?.DropTable<DataTables.LastUsersTb>();
                connection?.DropTable<DataTables.MessageTb>();
                connection?.DropTable<DataTables.StickersTable>();

                connection?.DropTable<DataTables.MuteTb>();
                connection?.DropTable<DataTables.PinTb>();
                connection?.DropTable<DataTables.ArchiveTb>(); 
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DropAll();
                else
                    Methods.DisplayReportResultTrack(e);

            }
        }

        #endregion

        //########################## End SQLite_Entity ##########################

        //Start SQL_Commander >>  Custom 
        //*********************************************************

        #region Login

        //Insert Or Update data Login
        public void InsertOrUpdateLogin(DataTables.LoginTb db)
        {
            try
            {
                using var connection = OpenConnection();
                var dataUser = connection.Table<DataTables.LoginTb>().FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.UserId = UserDetails.UserId;
                    dataUser.AccessToken = UserDetails.AccessToken;
                    dataUser.Cookie = UserDetails.Cookie;
                    dataUser.Username = UserDetails.Username;
                    dataUser.Password = UserDetails.Password;
                    dataUser.Status = UserDetails.Status;
                    dataUser.Lang = Settings.LangResources;
                    dataUser.Email = UserDetails.Email;

                    Current.AccessToken = UserDetails.AccessToken;

                    connection.Update(dataUser);
                }
                else
                {
                    connection.Insert(db);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateLogin(db);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data Login
        public DataTables.LoginTb Get_data_Login()
        {
            try
            {
                using var connection = OpenConnection();
                var dataUser = connection.Table<DataTables.LoginTb>().FirstOrDefault();
                if (dataUser != null)
                {
                    UserDetails.Username = dataUser.Username;
                    UserDetails.FullName = dataUser.Username;
                    UserDetails.Password = dataUser.Password;
                    UserDetails.AccessToken = dataUser.AccessToken;
                    UserDetails.UserId = dataUser.UserId;
                    UserDetails.Status = dataUser.Status;
                    UserDetails.Cookie = dataUser.Cookie;
                    UserDetails.Email = dataUser.Email;

                    Settings.LangResources = dataUser.Lang;
                    UserDetails.DeviceId = dataUser.DeviceId;

                    Current.AccessToken = UserDetails.AccessToken = dataUser.AccessToken;

                    ListUtils.DataUserLoginList.Clear();
                    ListUtils.DataUserLoginList.Add(dataUser);

                    return dataUser;
                }

                return null;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_data_Login();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null;
                }
            }
        }

        #endregion

        #region Settings

        public void InsertOrUpdateSettings(GetSiteSettingsObject.ConfigObject settingsData)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;

                if (settingsData != null)
                {
                    var select = connection.Table<DataTables.SettingsTb>().FirstOrDefault();
                    if (select == null)
                    { 
                        var db = ClassMapper.Mapper?.Map<DataTables.SettingsTb>(settingsData);

                        if (db != null)
                        {
                            db.CurrencyArray = JsonConvert.SerializeObject(settingsData.CurrencyArray.CurrencyList);
                            db.CurrencySymbolArray = JsonConvert.SerializeObject(settingsData.CurrencySymbolArray.CurrencyList);
                            db.PageCategories = JsonConvert.SerializeObject(settingsData.PageCategories);
                            db.GroupCategories = JsonConvert.SerializeObject(settingsData.GroupCategories);
                            db.BlogCategories = JsonConvert.SerializeObject(settingsData.BlogCategories);
                            db.ProductsCategories = JsonConvert.SerializeObject(settingsData.ProductsCategories);
                            db.JobCategories = JsonConvert.SerializeObject(settingsData.JobCategories);
                            db.Genders = JsonConvert.SerializeObject(settingsData.Genders);
                            db.Family = JsonConvert.SerializeObject(settingsData.Family);
                            db.MovieCategory = JsonConvert.SerializeObject(settingsData.MovieCategory);
                            db.PostColors = JsonConvert.SerializeObject(settingsData.PostColors?.PostColorsList);
                            db.Fields = JsonConvert.SerializeObject(settingsData.Fields);
                            db.PostReactionsTypes = JsonConvert.SerializeObject(settingsData.PostReactionsTypes);
                            db.PageSubCategories = JsonConvert.SerializeObject(settingsData.PageSubCategories?.SubCategoriesList);
                            db.GroupSubCategories = JsonConvert.SerializeObject(settingsData.GroupSubCategories?.SubCategoriesList);
                            db.ProductsSubCategories = JsonConvert.SerializeObject(settingsData.ProductsSubCategories?.SubCategoriesList);
                            db.PageCustomFields = JsonConvert.SerializeObject(settingsData.PageCustomFields);
                            db.GroupCustomFields = JsonConvert.SerializeObject(settingsData.GroupCustomFields);
                            db.ProductCustomFields = JsonConvert.SerializeObject(settingsData.ProductCustomFields);
                            db.ProPackages = JsonConvert.SerializeObject(settingsData.ProPackages); 
                             
                            connection.Insert(db);
                        }

                        ListUtils.SettingsSiteList = db;
                    }
                    else
                    {
                        select = ClassMapper.Mapper?.Map<DataTables.SettingsTb>(settingsData);
                        if (select != null)
                        {  
                            select.CurrencyArray = JsonConvert.SerializeObject(settingsData.CurrencyArray.CurrencyList);
                            select.CurrencySymbolArray = JsonConvert.SerializeObject(settingsData.CurrencySymbolArray.CurrencyList);
                            select.PageCategories = JsonConvert.SerializeObject(settingsData.PageCategories);
                            select.GroupCategories = JsonConvert.SerializeObject(settingsData.GroupCategories);
                            select.BlogCategories = JsonConvert.SerializeObject(settingsData.BlogCategories);
                            select.ProductsCategories = JsonConvert.SerializeObject(settingsData.ProductsCategories);
                            select.JobCategories = JsonConvert.SerializeObject(settingsData.JobCategories);
                            select.Genders = JsonConvert.SerializeObject(settingsData.Genders);
                            select.Family = JsonConvert.SerializeObject(settingsData.Family);
                            select.MovieCategory = JsonConvert.SerializeObject(settingsData.MovieCategory);
                            select.PostColors = JsonConvert.SerializeObject(settingsData.PostColors?.PostColorsList);
                            select.Fields = JsonConvert.SerializeObject(settingsData.Fields);
                            select.PostReactionsTypes = JsonConvert.SerializeObject(settingsData.PostReactionsTypes);
                            select.PageSubCategories = JsonConvert.SerializeObject(settingsData.PageSubCategories?.SubCategoriesList);
                            select.GroupSubCategories = JsonConvert.SerializeObject(settingsData.GroupSubCategories?.SubCategoriesList);
                            select.ProductsSubCategories = JsonConvert.SerializeObject(settingsData.ProductsSubCategories?.SubCategoriesList);
                            select.PageCustomFields = JsonConvert.SerializeObject(settingsData.PageCustomFields);
                            select.GroupCustomFields = JsonConvert.SerializeObject(settingsData.GroupCustomFields);
                            select.ProductCustomFields = JsonConvert.SerializeObject(settingsData.ProductCustomFields);
                            select.ProPackages = JsonConvert.SerializeObject(settingsData.ProPackages); 

                            connection.Update(select);

                            ListUtils.SettingsSiteList = select;
                        } 
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateSettings(settingsData);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get Settings
        public GetSiteSettingsObject.ConfigObject GetSettings()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return null!;

                var settingsData = connection.Table<DataTables.SettingsTb>().FirstOrDefault();
                if (settingsData != null)
                {
                   
                    var db = ClassMapper.Mapper?.Map<GetSiteSettingsObject.ConfigObject>(settingsData);
                    if (db != null)
                    {
                        GetSiteSettingsObject.ConfigObject asd = db;

                        asd.CurrencyArray = new GetSiteSettingsObject.CurrencyArray();
                        asd.CurrencySymbolArray = new GetSiteSettingsObject.CurrencySymbol();
                        asd.PageCategories = new Dictionary<string, string>();
                        asd.GroupCategories = new Dictionary<string, string>();
                        asd.BlogCategories = new Dictionary<string, string>();
                        asd.ProductsCategories = new Dictionary<string, string>();
                        asd.JobCategories = new Dictionary<string, string>();
                        asd.Genders = new Dictionary<string, string>();
                        asd.Family = new Dictionary<string, string>();
                        asd.MovieCategory = new Dictionary<string, string>();
                        asd.PostColors = new Dictionary<string, PostColorsObject>();
                        asd.Fields = new List<Field>();
                        asd.PostReactionsTypes = new Dictionary<string, PostReactionsType>();
                        asd.PageSubCategories = new GetSiteSettingsObject.SubCategoriesUnion
                        {
                            SubCategoriesList = new Dictionary<string, List<SubCategories>>()
                        };
                        asd.GroupSubCategories = new GetSiteSettingsObject.SubCategoriesUnion
                        {
                            SubCategoriesList = new Dictionary<string, List<SubCategories>>()
                        };
                        asd.ProductsSubCategories = new GetSiteSettingsObject.SubCategoriesUnion
                        {
                            SubCategoriesList = new Dictionary<string, List<SubCategories>>()
                        };
                        asd.PageCustomFields = new List<CustomField>();
                        asd.GroupCustomFields = new List<CustomField>();
                        asd.ProductCustomFields = new List<CustomField>();

                        asd.ProPackages = new Dictionary<string, DataProPackages>();
                      
                        asd.CurrencyArray = string.IsNullOrEmpty(settingsData.CurrencyArray) switch
                        {
                            false => new GetSiteSettingsObject.CurrencyArray
                            {
                                CurrencyList = JsonConvert.DeserializeObject<List<string>>(settingsData.CurrencyArray)
                            },
                            _ => asd.CurrencyArray
                        };

                        asd.CurrencySymbolArray = string.IsNullOrEmpty(settingsData.CurrencySymbolArray) switch
                        {
                            false => new GetSiteSettingsObject.CurrencySymbol
                            {
                                CurrencyList = JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsData.CurrencySymbolArray),
                            },
                            _ => asd.CurrencySymbolArray
                        };

                        asd.PageCategories = string.IsNullOrEmpty(settingsData.PageCategories) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsData.PageCategories),
                            _ => asd.PageCategories
                        };

                        asd.GroupCategories = string.IsNullOrEmpty(settingsData.GroupCategories) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsData.GroupCategories),
                            _ => asd.GroupCategories
                        };

                        asd.BlogCategories = string.IsNullOrEmpty(settingsData.BlogCategories) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsData.BlogCategories),
                            _ => asd.BlogCategories
                        };

                        asd.ProductsCategories = string.IsNullOrEmpty(settingsData.ProductsCategories) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(
                                settingsData.ProductsCategories),
                            _ => asd.ProductsCategories
                        };

                        asd.JobCategories = string.IsNullOrEmpty(settingsData.JobCategories) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsData.JobCategories),
                            _ => asd.JobCategories
                        };

                        asd.Genders = string.IsNullOrEmpty(settingsData.Genders) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsData.Genders),
                            _ => asd.Genders
                        };

                        asd.Family = string.IsNullOrEmpty(settingsData.Family) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsData.Family),
                            _ => asd.Family
                        };

                        asd.MovieCategory = string.IsNullOrEmpty(settingsData.MovieCategory) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsData.MovieCategory),
                            _ => asd.MovieCategory
                        };

                        asd.PostColors = string.IsNullOrEmpty(settingsData.PostColors) switch
                        {
                            false => new GetSiteSettingsObject.PostColorUnion
                            {
                                PostColorsList =
                                    JsonConvert.DeserializeObject<Dictionary<string, PostColorsObject>>(
                                        settingsData.PostColors)
                            },
                            _ => asd.PostColors
                        };

                        asd.PostReactionsTypes = string.IsNullOrEmpty(settingsData.PostReactionsTypes) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, PostReactionsType>>(
                                settingsData.PostReactionsTypes),
                            _ => asd.PostReactionsTypes
                        };

                        asd.Fields = string.IsNullOrEmpty(settingsData.Fields) switch
                        {
                            false => JsonConvert.DeserializeObject<List<Field>>(settingsData.Fields),
                            _ => asd.Fields
                        };

                        asd.PageSubCategories = string.IsNullOrEmpty(settingsData.PageSubCategories) switch
                        {
                            false => new GetSiteSettingsObject.SubCategoriesUnion
                            {
                                SubCategoriesList =
                                    JsonConvert.DeserializeObject<Dictionary<string, List<SubCategories>>>(
                                        settingsData.PageSubCategories)
                            },
                            _ => asd.PageSubCategories
                        };

                        asd.GroupSubCategories = string.IsNullOrEmpty(settingsData.GroupSubCategories) switch
                        {
                            false => new GetSiteSettingsObject.SubCategoriesUnion
                            {
                                SubCategoriesList =
                                    JsonConvert.DeserializeObject<Dictionary<string, List<SubCategories>>>(
                                        settingsData.GroupSubCategories)
                            },
                            _ => asd.GroupSubCategories
                        };

                        asd.ProductsSubCategories = string.IsNullOrEmpty(settingsData.ProductsSubCategories) switch
                        {
                            false => new GetSiteSettingsObject.SubCategoriesUnion
                            {
                                SubCategoriesList =
                                    JsonConvert.DeserializeObject<Dictionary<string, List<SubCategories>>>(
                                        settingsData.ProductsSubCategories)
                            },
                            _ => asd.ProductsSubCategories
                        };

                        asd.PageCustomFields = string.IsNullOrEmpty(settingsData.PageCustomFields) switch
                        {
                            false => JsonConvert.DeserializeObject<List<CustomField>>(settingsData.PageCustomFields),
                            _ => asd.PageCustomFields
                        };

                        asd.GroupCustomFields = string.IsNullOrEmpty(settingsData.GroupCustomFields) switch
                        {
                            false => JsonConvert.DeserializeObject<List<CustomField>>(settingsData.GroupCustomFields),
                            _ => asd.GroupCustomFields
                        };

                        asd.ProductCustomFields = string.IsNullOrEmpty(settingsData.ProductCustomFields) switch
                        {
                            false => JsonConvert.DeserializeObject<List<CustomField>>(settingsData.ProductCustomFields),
                            _ => asd.ProductCustomFields
                        };

                        asd.ProPackages = string.IsNullOrEmpty(settingsData.ProPackages) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, DataProPackages>>(
                                settingsData.ProPackages),
                            _ => asd.ProPackages
                        };
                         
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                if (asd.ProductsCategories != null)
                                {
                                    //Products Categories
                                    var listProducts = asd.ProductsCategories.Select(cat => new Classes.Categories
                                    {
                                        CategoriesId = cat.Key,
                                        CategoriesName = Methods.FunString.DecodeString(cat.Value),
                                        CategoriesColor = "#ffffff",
                                        SubList = new List<SubCategories>()
                                    }).ToList();

                                    ListUtils.ListCategoriesProducts.Clear();
                                    ListUtils.ListCategoriesProducts = new ObservableCollection<Classes.Categories>(listProducts);

                                    if (asd.ProductsSubCategories?.SubCategoriesList?.Count > 0)
                                        //Sub Categories Products
                                        foreach (var sub in asd.ProductsSubCategories?.SubCategoriesList)
                                        {
                                            var subCategories = asd.ProductsSubCategories?.SubCategoriesList?.FirstOrDefault(a => a.Key == sub.Key).Value;
                                            if (subCategories?.Count > 0)
                                            {
                                                var cat = ListUtils.ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == sub.Key);
                                                if (cat != null)
                                                {
                                                    foreach (var pairs in subCategories)
                                                    {
                                                        cat.SubList.Add(pairs);
                                                    }
                                                }
                                            }
                                        }
                                } 
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });

                        ListUtils.SettingsSiteList = asd;

                        return asd;
                    }
                    else
                    {
                        return null!;
                    }
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetSettings();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                }
            }
        }

        #endregion

        #region SettingsApp

        public void InsertOrUpdateSettingsApp(DataTables.SettingsAppTb settingsData)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;

                if (settingsData != null)
                {
                    var select = connection.Table<DataTables.SettingsAppTb>().FirstOrDefault();
                    if (select == null)
                    { 
                        connection.Insert(settingsData); 
                    }
                    else
                    {
                        select.BackgroundChatsImages = settingsData.BackgroundChatsImages;
                        select.DarkMode = settingsData.DarkMode;
                        select.LangResources = settingsData.LangResources;
                        select.NotificationDesktop = settingsData.NotificationDesktop;
                        select.NotificationPlaysound = settingsData.NotificationPlaysound;

                        connection.Update(select); 
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateSettingsApp(settingsData);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get SettingsApp
        public DataTables.SettingsAppTb GetSettingsApp()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return null!;

                var settingsData = connection.Table<DataTables.SettingsAppTb>().FirstOrDefault();
                if (settingsData != null)
                {
                    var db = new DataTables.SettingsAppTb
                    {
                        BackgroundChatsImages = settingsData.BackgroundChatsImages,
                        DarkMode = settingsData.DarkMode,
                        LangResources = settingsData.LangResources,
                        NotificationDesktop = settingsData.NotificationDesktop,
                        NotificationPlaysound = settingsData.NotificationPlaysound
                    };

                    return db;
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetSettingsApp();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                }
            }
        }

        #endregion

        #region My Contacts >> Following

        //Insert data To My Contact Table
        public void Insert_Or_Replace_MyContactTable(ObservableCollection<UserDataFody> usersContactList)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var result = connection.Table<DataTables.MyContactsTb>().ToList();
                List<DataTables.MyContactsTb> list = new List<DataTables.MyContactsTb>();

                connection.BeginTransaction();

                var lists = new List<UserDataFody>(usersContactList);
                foreach (var info in lists)
                {
                    var db = ClassMapper.Mapper?.Map<DataTables.MyContactsTb>(info);
                    if (info.Details.DetailsClass != null && db != null)
                    {
                        db.UserId = long.Parse(info.UserId);
                        db.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);
                        db.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);
                        list.Add(db);
                    }

                    var update = result.FirstOrDefault(a => a.UserId == long.Parse(info.UserId));
                    if (update != null)
                    {
                        update = ClassMapper.Mapper?.Map<DataTables.MyContactsTb>(info);
                        if (info.Details.DetailsClass != null && update != null)
                        {
                            update.UserId = long.Parse(info.UserId);
                            update.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);
                            update.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);
                            connection.Update(update);
                        }
                    }
                }

                switch (list.Count)
                {
                    case <= 0:
                        return;
                }


                //Bring new  
                var newItemList = list.Where(c => !result.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                switch (newItemList.Count)
                {
                    case > 0:
                        connection.InsertAll(newItemList);
                        break;
                }

                result = connection.Table<DataTables.MyContactsTb>().ToList();
                var deleteItemList = result.Where(c => !list.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                switch (deleteItemList.Count)
                {
                    case > 0:
                        {
                            foreach (var delete in deleteItemList)
                                connection.Delete(delete);
                            break;
                        }
                }

                connection.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Replace_MyContactTable(usersContactList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        // Get data To My Contact Table
        public ObservableCollection<UserDataFody> Get_MyContact(int id = 0, int nSize = 15)
        {
            try
            {
                using var connection = OpenConnection();
                var query = connection.Table<DataTables.MyContactsTb>().Where(w => w.UserId > id).OrderBy(q => q.AutoIdMyFollowing).Take(nSize).ToList();

                //var select = connection.Table<DataTables.MyContactsTb>().Take(20).ToList();
                switch (query.Count > 0)
                {
                    case true:
                        {
                            var list = new ObservableCollection<UserDataFody>();

                            foreach (var info in query)
                            {
                                UserDataFody infoObject = new UserDataFody
                                {
                                    UserId = info.UserId.ToString(),
                                    Username = info.Username,
                                    Email = info.Email,
                                    FirstName = info.FirstName,
                                    LastName = info.LastName,
                                    Avatar = info.Avatar,
                                    Cover = info.Cover,
                                    BackgroundImage = info.BackgroundImage,
                                    RelationshipId = info.RelationshipId,
                                    Address = info.Address,
                                    Working = info.Working,
                                    Gender = info.Gender,
                                    Facebook = info.Facebook,
                                    Google = info.Google,
                                    Twitter = info.Twitter,
                                    Linkedin = info.Linkedin,
                                    Website = info.Website,
                                    Instagram = info.Instagram,
                                    WebDeviceId = info.WebDeviceId,
                                    Language = info.Language,
                                    IpAddress = info.IpAddress,
                                    PhoneNumber = info.PhoneNumber,
                                    Timezone = info.Timezone,
                                    Lat = info.Lat,
                                    Lng = info.Lng,
                                    Time = info.Time,
                                    About = info.About,
                                    Birthday = info.Birthday,
                                    Registered = info.Registered,
                                    Lastseen = info.Lastseen,
                                    LastLocationUpdate = info.LastLocationUpdate,
                                    Balance = info.Balance,
                                    Verified = info.Verified,
                                    Status = info.Status,
                                    Active = info.Active,
                                    Admin = info.Admin,
                                    IsPro = info.IsPro,
                                    ProType = info.ProType,
                                    School = info.School,
                                    Name = info.Name,
                                    AndroidMDeviceId = info.AndroidMDeviceId,
                                    ECommented = info.ECommented,
                                    AndroidNDeviceId = info.AndroidMDeviceId,
                                    AvatarFull = info.AvatarFull,
                                    BirthPrivacy = info.BirthPrivacy,
                                    CanFollow = info.CanFollow,
                                    ConfirmFollowers = info.ConfirmFollowers,
                                    CountryId = info.CountryId,
                                    EAccepted = info.EAccepted,
                                    EFollowed = info.EFollowed,
                                    EJoinedGroup = info.EJoinedGroup,
                                    ELastNotif = info.ELastNotif,
                                    ELiked = info.ELiked,
                                    ELikedPage = info.ELikedPage,
                                    EMentioned = info.EMentioned,
                                    EProfileWallPost = info.EProfileWallPost,
                                    ESentmeMsg = info.ESentmeMsg,
                                    EShared = info.EShared,
                                    EVisited = info.EVisited,
                                    EWondered = info.EWondered,
                                    EmailNotification = info.EmailNotification,
                                    FollowPrivacy = info.FollowPrivacy,
                                    FriendPrivacy = info.FriendPrivacy,
                                    GenderText = info.GenderText,
                                    InfoFile = info.InfoFile,
                                    IosMDeviceId = info.IosMDeviceId,
                                    IosNDeviceId = info.IosNDeviceId,
                                    IsBlocked = info.IsBlocked,
                                    IsFollowing = info.IsFollowing,
                                    IsFollowingMe = info.IsFollowingMe,
                                    LastAvatarMod = info.LastAvatarMod,
                                    LastCoverMod = info.LastCoverMod,
                                    LastDataUpdate = info.LastDataUpdate,
                                    LastFollowId = info.LastFollowId,
                                    LastLoginData = info.LastLoginData,
                                    LastseenStatus = info.LastseenStatus,
                                    LastSeenTimeText = info.LastSeenTimeText,
                                    LastseenUnixTime = info.LastseenUnixTime,
                                    MessagePrivacy = info.MessagePrivacy,
                                    NewEmail = info.NewEmail,
                                    NewPhone = info.NewPhone,
                                    NotificationsSound = info.NotificationsSound,
                                    OrderPostsBy = info.OrderPostsBy,
                                    PaypalEmail = info.PaypalEmail,
                                    PostPrivacy = info.PostPrivacy,
                                    Referrer = info.Referrer,
                                    ShareMyData = info.ShareMyData,
                                    ShareMyLocation = info.ShareMyLocation,
                                    ShowActivitiesPrivacy = info.ShowActivitiesPrivacy,
                                    TwoFactor = info.TwoFactor,
                                    TwoFactorVerified = info.TwoFactorVerified,
                                    Url = info.Url,
                                    VisitPrivacy = info.VisitPrivacy,
                                    Vk = info.Vk,
                                    Wallet = info.Wallet,
                                    WorkingLink = info.WorkingLink,
                                    Youtube = info.Youtube,
                                    City = info.City,
                                    Points = info.Points,
                                    DailyPoints = info.DailyPoints,
                                    PointDayExpire = info.PointDayExpire,
                                    State = info.State,
                                    Zip = info.Zip,
                                    CashfreeSignature = info.CashfreeSignature,
                                    IsAdmin = info.IsAdmin,
                                    MemberId = info.MemberId,
                                    ChatColor = info.ChatColor,
                                    PaystackRef = info.PaystackRef,
                                    RefUserId = info.RefUserId,
                                    SchoolCompleted = info.SchoolCompleted,
                                    AvatarPostId = info.AvatarPostId,
                                    CodeSent = info.CodeSent,
                                    CoverPostId = info.CoverPostId,
                                    Discord = info.Discord,
                                    IsReported = info.IsReported,
                                    IsStoryMuted = info.IsStoryMuted,
                                    Mailru = info.Mailru,
                                    NotificationSettings = info.NotificationSettings,
                                    IsNotifyStopped = info.IsNotifyStopped,
                                    Qq = info.Qq,
                                    StripeSessionId = info.StripeSessionId,
                                    TimeCodeSent = info.TimeCodeSent,
                                    Wechat = info.Wechat,
                                    ApiNotificationSettings = new NotificationSettingsUnion(),
                                    Details = new DetailsUnion(),
                                    SColorBackground = info.SColorBackground,
                                    SColorForeground = info.SColorForeground,
                                    UserPlatform = info.UserPlatform,
                                    WeatherUnit = info.WeatherUnit,
                                    IsMute = info.IsMute,
                                    ColorFollow = info.ColorFollow,
                                    ColorGender = info.ColorGender,
                                    IsArchive = info.IsArchive,
                                    IsPin = info.IsPin,
                                    Selected = info.Selected,
                                    TextColorFollowing = info.TextColorFollowing,
                                    TextFollowing = info.TextFollowing,
                                    Type = info.Type,
                                    Banned = info.Banned,
                                    BannedReason = info.BannedReason,
                                    CoinbaseCode = info.CoinbaseCode,
                                    CoinbaseHash = info.CoinbaseHash,
                                    ConversationId = info.ConversationId,
                                    CurrentlyWorking = info.CurrentlyWorking,
                                    IsOnline = info.IsOnline,
                                    IsOpenToWork = info.IsOpenToWork,
                                    IsProvidingService = info.IsProvidingService,
                                    Languages = info.Languages,
                                    Okru = info.Okru,
                                    OpenToWorkData = info.OpenToWorkData,
                                    Permission = info.Permission,
                                    ProvidingService = info.ProvidingService,
                                    SAppMainLater = info.SAppMainLater,
                                    SColorOnOff = info.SColorOnOff,
                                    SecurionpayKey = info.SecurionpayKey,
                                    Skills = info.Skills,
                                    YoomoneyHash = info.YoomoneyHash,
                                    
                                };

                                infoObject.Details = string.IsNullOrEmpty(info.Details) switch
                                {
                                    false => new DetailsUnion { DetailsClass = JsonConvert.DeserializeObject<Details>(info.Details) },
                                    _ => infoObject.Details
                                };

                                infoObject.ApiNotificationSettings = string.IsNullOrEmpty(info.ApiNotificationSettings) switch
                                {
                                    false => new NotificationSettingsUnion { NotificationSettingsClass = JsonConvert.DeserializeObject<NotificationSettings>(info.ApiNotificationSettings) },
                                    _ => infoObject.ApiNotificationSettings
                                };

                                list.Add(infoObject);
                            }

                            return list;
                        }
                    default:
                        return new ObservableCollection<UserDataFody>();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_MyContact(id, nSize);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<UserDataFody>();
                }
            }
        }
         
        public void Insert_Or_Replace_OR_Delete_UsersContact(UserDataObject info, string type)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var user = connection.Table<DataTables.MyContactsTb>().FirstOrDefault(c => c.UserId == long.Parse(info.UserId));
                if (user != null)
                {
                    switch (type)
                    {
                        case "Delete":
                            connection.Delete(user);
                            break;
                        default: // Update
                            {
                                user = ClassMapper.Mapper?.Map<DataTables.MyContactsTb>(info);
                                user.UserId = long.Parse(info.UserId);

                                if (info.Details.DetailsClass != null)
                                    user.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);

                                if (info.ApiNotificationSettings.NotificationSettingsClass != null)
                                    user.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);

                                connection.Update(user);
                                break;
                            }
                    }
                }
                else
                {
                    DataTables.MyContactsTb db = new DataTables.MyContactsTb
                    {
                        UserId = long.Parse(info.UserId),
                        Username = info.Username,
                        Email = info.Email,
                        FirstName = info.FirstName,
                        LastName = info.LastName,
                        Avatar = info.Avatar,
                        Cover = info.Cover,
                        BackgroundImage = info.BackgroundImage,
                        RelationshipId = info.RelationshipId,
                        Address = info.Address,
                        Working = info.Working,
                        Gender = info.Gender,
                        Facebook = info.Facebook,
                        Google = info.Google,
                        Twitter = info.Twitter,
                        Linkedin = info.Linkedin,
                        Website = info.Website,
                        Instagram = info.Instagram,
                        WebDeviceId = info.WebDeviceId,
                        Language = info.Language,
                        IpAddress = info.IpAddress,
                        PhoneNumber = info.PhoneNumber,
                        Timezone = info.Timezone,
                        Lat = info.Lat,
                        Lng = info.Lng,
                        Time = info.Time,
                        About = info.About,
                        Birthday = info.Birthday,
                        Registered = info.Registered,
                        Lastseen = info.Lastseen,
                        LastLocationUpdate = info.LastLocationUpdate,
                        Balance = info.Balance,
                        Verified = info.Verified,
                        Status = info.Status,
                        Active = info.Active,
                        Admin = info.Admin,
                        IsPro = info.IsPro,
                        ProType = info.ProType,
                        School = info.School,
                        Name = info.Name,
                        AndroidMDeviceId = info.AndroidMDeviceId,
                        ECommented = info.ECommented,
                        AndroidNDeviceId = info.AndroidMDeviceId,
                        AvatarFull = info.AvatarFull,
                        BirthPrivacy = info.BirthPrivacy,
                        CanFollow = info.CanFollow,
                        ConfirmFollowers = info.ConfirmFollowers,
                        CountryId = info.CountryId,
                        EAccepted = info.EAccepted,
                        EFollowed = info.EFollowed,
                        EJoinedGroup = info.EJoinedGroup,
                        ELastNotif = info.ELastNotif,
                        ELiked = info.ELiked,
                        ELikedPage = info.ELikedPage,
                        EMentioned = info.EMentioned,
                        EProfileWallPost = info.EProfileWallPost,
                        ESentmeMsg = info.ESentmeMsg,
                        EShared = info.EShared,
                        EVisited = info.EVisited,
                        EWondered = info.EWondered,
                        EmailNotification = info.EmailNotification,
                        FollowPrivacy = info.FollowPrivacy,
                        FriendPrivacy = info.FriendPrivacy,
                        GenderText = info.GenderText,
                        InfoFile = info.InfoFile,
                        IosMDeviceId = info.IosMDeviceId,
                        IosNDeviceId = info.IosNDeviceId,
                        IsBlocked = info.IsBlocked,
                        IsFollowing = info.IsFollowing,
                        IsFollowingMe = info.IsFollowingMe,
                        LastAvatarMod = info.LastAvatarMod,
                        LastCoverMod = info.LastCoverMod,
                        LastDataUpdate = info.LastDataUpdate,
                        LastFollowId = info.LastFollowId,
                        LastLoginData = info.LastLoginData,
                        LastseenStatus = info.LastseenStatus,
                        LastSeenTimeText = info.LastSeenTimeText,
                        LastseenUnixTime = info.LastseenUnixTime,
                        MessagePrivacy = info.MessagePrivacy,
                        NewEmail = info.NewEmail,
                        NewPhone = info.NewPhone,
                        NotificationsSound = info.NotificationsSound,
                        OrderPostsBy = info.OrderPostsBy,
                        PaypalEmail = info.PaypalEmail,
                        PostPrivacy = info.PostPrivacy,
                        Referrer = info.Referrer,
                        ShareMyData = info.ShareMyData,
                        ShareMyLocation = info.ShareMyLocation,
                        ShowActivitiesPrivacy = info.ShowActivitiesPrivacy,
                        TwoFactor = info.TwoFactor,
                        TwoFactorVerified = info.TwoFactorVerified,
                        Url = info.Url,
                        VisitPrivacy = info.VisitPrivacy,
                        Vk = info.Vk,
                        Wallet = info.Wallet,
                        WorkingLink = info.WorkingLink,
                        Youtube = info.Youtube,
                        City = info.City,
                        Points = info.Points,
                        DailyPoints = info.DailyPoints,
                        PointDayExpire = info.PointDayExpire,
                        State = info.State,
                        Zip = info.Zip,
                        CashfreeSignature = info.CashfreeSignature,
                        IsAdmin = info.IsAdmin,
                        MemberId = info.MemberId,
                        ChatColor = info.ChatColor,
                        PaystackRef = info.PaystackRef,
                        RefUserId = info.RefUserId,
                        SchoolCompleted = info.SchoolCompleted,
                        AvatarPostId = info.AvatarPostId,
                        CodeSent = info.CodeSent,
                        CoverPostId = info.CoverPostId,
                        Discord = info.Discord,
                        IsArchive = info.IsArchive,
                        IsMute = info.IsMute,
                        IsPin = info.IsPin,
                        IsReported = info.IsReported,
                        IsStoryMuted = info.IsStoryMuted,
                        Mailru = info.Mailru,
                        NotificationSettings = info.NotificationSettings,
                        IsNotifyStopped = info.IsNotifyStopped,
                        Qq = info.Qq,
                        StripeSessionId = info.StripeSessionId,
                        TimeCodeSent = info.TimeCodeSent,
                        Wechat = info.Wechat,
                        ApiNotificationSettings = string.Empty,
                        Details = string.Empty,
                        Selected = false,
                        Banned = info.Banned,
                        BannedReason = info.BannedReason,
                        CoinbaseCode = info.CoinbaseCode,
                        CoinbaseHash = info.CoinbaseHash,
                        ConversationId = info.ConversationId,
                        CurrentlyWorking = info.CurrentlyWorking,
                        IsOnline = info.IsOnline,
                        IsOpenToWork = info.IsOpenToWork,
                        IsProvidingService = info.IsProvidingService,
                        Languages = info.Languages,
                        Okru = info.Okru,
                        OpenToWorkData = info.OpenToWorkData,
                        Permission = info.Permission,
                        ProvidingService = info.ProvidingService,
                        SAppMainLater = info.SAppMainLater,
                        SColorOnOff = info.SColorOnOff,
                        SecurionpayKey = info.SecurionpayKey,
                        Skills = info.Skills,
                        YoomoneyHash = info.YoomoneyHash,
                        ColorFollow = info.ColorFollow,
                        ColorGender = info.ColorGender,
                        SColorBackground = info.SColorBackground,
                        SColorForeground = info.SColorForeground,
                        TextFollowing = info.TextFollowing,
                        TextColorFollowing = info.TextColorFollowing,
                        Type = info.Type,
                        UserPlatform = info.UserPlatform,
                        WeatherUnit = info.WeatherUnit, 
                    };

                    if (info.Details.DetailsClass != null)
                        db.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);

                    if (info.ApiNotificationSettings.NotificationSettingsClass != null)
                        db.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);

                    connection.Insert(db);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Replace_OR_Delete_UsersContact(info, type);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        // Get data One user To My Contact Table
        public UserDataFody Get_DataOneUser(string userName)
        {
            try
            {
                using var connection = OpenConnection();
                var info = connection.Table<DataTables.MyContactsTb>().FirstOrDefault(a => a.Username == userName || a.Name == userName);
                if (info != null)
                {
                    UserDataFody infoObject = new UserDataFody
                    {
                        UserId = info.UserId.ToString(),
                        Username = info.Username,
                        Email = info.Email,
                        FirstName = info.FirstName,
                        LastName = info.LastName,
                        Avatar = info.Avatar,
                        Cover = info.Cover,
                        BackgroundImage = info.BackgroundImage,
                        RelationshipId = info.RelationshipId,
                        Address = info.Address,
                        Working = info.Working,
                        Gender = info.Gender,
                        Facebook = info.Facebook,
                        Google = info.Google,
                        Twitter = info.Twitter,
                        Linkedin = info.Linkedin,
                        Website = info.Website,
                        Instagram = info.Instagram,
                        WebDeviceId = info.WebDeviceId,
                        Language = info.Language,
                        IpAddress = info.IpAddress,
                        PhoneNumber = info.PhoneNumber,
                        Timezone = info.Timezone,
                        Lat = info.Lat,
                        Lng = info.Lng,
                        Time = info.Time,
                        About = info.About,
                        Birthday = info.Birthday,
                        Registered = info.Registered,
                        Lastseen = info.Lastseen,
                        LastLocationUpdate = info.LastLocationUpdate,
                        Balance = info.Balance,
                        Verified = info.Verified,
                        Status = info.Status,
                        Active = info.Active,
                        Admin = info.Admin,
                        IsPro = info.IsPro,
                        ProType = info.ProType,
                        School = info.School,
                        Name = info.Name,
                        AndroidMDeviceId = info.AndroidMDeviceId,
                        ECommented = info.ECommented,
                        AndroidNDeviceId = info.AndroidMDeviceId,
                        AvatarFull = info.AvatarFull,
                        BirthPrivacy = info.BirthPrivacy,
                        CanFollow = info.CanFollow,
                        ConfirmFollowers = info.ConfirmFollowers,
                        CountryId = info.CountryId,
                        EAccepted = info.EAccepted,
                        EFollowed = info.EFollowed,
                        EJoinedGroup = info.EJoinedGroup,
                        ELastNotif = info.ELastNotif,
                        ELiked = info.ELiked,
                        ELikedPage = info.ELikedPage,
                        EMentioned = info.EMentioned,
                        EProfileWallPost = info.EProfileWallPost,
                        ESentmeMsg = info.ESentmeMsg,
                        EShared = info.EShared,
                        EVisited = info.EVisited,
                        EWondered = info.EWondered,
                        EmailNotification = info.EmailNotification,
                        FollowPrivacy = info.FollowPrivacy,
                        FriendPrivacy = info.FriendPrivacy,
                        GenderText = info.GenderText,
                        InfoFile = info.InfoFile,
                        IosMDeviceId = info.IosMDeviceId,
                        IosNDeviceId = info.IosNDeviceId,
                        IsBlocked = info.IsBlocked,
                        IsFollowing = info.IsFollowing,
                        IsFollowingMe = info.IsFollowingMe,
                        LastAvatarMod = info.LastAvatarMod,
                        LastCoverMod = info.LastCoverMod,
                        LastDataUpdate = info.LastDataUpdate,
                        LastFollowId = info.LastFollowId,
                        LastLoginData = info.LastLoginData,
                        LastseenStatus = info.LastseenStatus,
                        LastSeenTimeText = info.LastSeenTimeText,
                        LastseenUnixTime = info.LastseenUnixTime,
                        MessagePrivacy = info.MessagePrivacy,
                        NewEmail = info.NewEmail,
                        NewPhone = info.NewPhone,
                        NotificationsSound = info.NotificationsSound,
                        OrderPostsBy = info.OrderPostsBy,
                        PaypalEmail = info.PaypalEmail,
                        PostPrivacy = info.PostPrivacy,
                        Referrer = info.Referrer,
                        ShareMyData = info.ShareMyData,
                        ShareMyLocation = info.ShareMyLocation,
                        ShowActivitiesPrivacy = info.ShowActivitiesPrivacy,
                        TwoFactor = info.TwoFactor,
                        TwoFactorVerified = info.TwoFactorVerified,
                        Url = info.Url,
                        VisitPrivacy = info.VisitPrivacy,
                        Vk = info.Vk,
                        Wallet = info.Wallet,
                        WorkingLink = info.WorkingLink,
                        Youtube = info.Youtube,
                        City = info.City,
                        Points = info.Points,
                        DailyPoints = info.DailyPoints,
                        PointDayExpire = info.PointDayExpire,
                        State = info.State,
                        Zip = info.Zip,
                        CashfreeSignature = info.CashfreeSignature,
                        IsAdmin = info.IsAdmin,
                        MemberId = info.MemberId,
                        ChatColor = info.ChatColor,
                        PaystackRef = info.PaystackRef,
                        RefUserId = info.RefUserId,
                        SchoolCompleted = info.SchoolCompleted,
                        AvatarPostId = info.AvatarPostId,
                        CodeSent = info.CodeSent,
                        CoverPostId = info.CoverPostId,
                        Discord = info.Discord,
                        IsReported = info.IsReported,
                        IsStoryMuted = info.IsStoryMuted,
                        Mailru = info.Mailru,
                        NotificationSettings = info.NotificationSettings,
                        IsNotifyStopped = info.IsNotifyStopped,
                        Qq = info.Qq,
                        StripeSessionId = info.StripeSessionId,
                        TimeCodeSent = info.TimeCodeSent,
                        Wechat = info.Wechat,
                        ApiNotificationSettings = new NotificationSettingsUnion(),
                        Details = new DetailsUnion(),
                        SColorBackground = info.SColorBackground,
                        SColorForeground = info.SColorForeground,
                        UserPlatform = info.UserPlatform,
                        WeatherUnit = info.WeatherUnit,
                        IsMute = info.IsMute,
                        ColorFollow = info.ColorFollow,
                        ColorGender = info.ColorGender,
                        IsArchive = info.IsArchive,
                        IsPin = info.IsPin,
                        Selected = info.Selected,
                        TextColorFollowing = info.TextColorFollowing,
                        TextFollowing = info.TextFollowing,
                        Type = info.Type,
                        Banned = info.Banned,
                        BannedReason = info.BannedReason,
                        CoinbaseCode = info.CoinbaseCode,
                        CoinbaseHash = info.CoinbaseHash,
                        ConversationId = info.ConversationId,
                        CurrentlyWorking = info.CurrentlyWorking,
                        IsOnline = info.IsOnline,
                        IsOpenToWork = info.IsOpenToWork,
                        IsProvidingService = info.IsProvidingService,
                        Languages = info.Languages,
                        Okru = info.Okru,
                        OpenToWorkData = info.OpenToWorkData,
                        Permission = info.Permission,
                        ProvidingService = info.ProvidingService,
                        SAppMainLater = info.SAppMainLater,
                        SColorOnOff = info.SColorOnOff,
                        SecurionpayKey = info.SecurionpayKey,
                        Skills = info.Skills,
                        YoomoneyHash = info.YoomoneyHash, 
                    };

                    infoObject.Details = string.IsNullOrEmpty(info.Details) switch
                    {
                        false => new DetailsUnion { DetailsClass = JsonConvert.DeserializeObject<Details>(info.Details) },
                        _ => infoObject.Details
                    };

                    infoObject.ApiNotificationSettings = string.IsNullOrEmpty(info.ApiNotificationSettings) switch
                    {
                        false => new NotificationSettingsUnion { NotificationSettingsClass = JsonConvert.DeserializeObject<NotificationSettings>(info.ApiNotificationSettings) },
                        _ => infoObject.ApiNotificationSettings
                    };

                    return infoObject;
                }
                return null;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_DataOneUser(userName);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null;
                }
            }
        }

        // Get data One user To My Contact Table
        public void RemoveUser_All_Table(string fromId, string toId)
        {
            try
            {
                using var connection = OpenConnection();
                var chatActivity = connection.Table<DataTables.LastUsersTb>().FirstOrDefault(a => a.UserId == toId);
                if (chatActivity != null)
                {
                    connection.Delete(chatActivity);
                }

                var usersCall = connection.Table<DataTables.CallVideoTable>().FirstOrDefault(a => a.CallVideoUserId == toId);
                if (usersCall != null)
                {
                    connection.Delete(usersCall);
                }

                var usersContact = connection.Table<DataTables.MyContactsTb>().FirstOrDefault(a => a.UserId == long.Parse(toId));
                if (usersContact != null)
                {
                    connection.Delete(usersContact);
                }

                DeleteAllMessagesUser(fromId, toId);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    RemoveUser_All_Table(fromId, toId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #region My Profile

        //Insert Or Update data My Profile Table
        public void Insert_Or_Update_To_MyProfileTable(UserDataFody info)
        {
            try
            {
                using var connection = OpenConnection();
                var item = connection.Table<DataTables.MyProfileTb>().FirstOrDefault();
                if (item != null)
                {
                    item.UserId = info.UserId;
                    item.Username = info.Username;
                    item.Email = info.Email;
                    item.FirstName = info.FirstName;
                    item.LastName = info.LastName;
                    item.Avatar = info.Avatar;
                    item.Cover = info.Cover;
                    item.BackgroundImage = info.BackgroundImage;
                    item.RelationshipId = info.RelationshipId;
                    item.Address = info.Address;
                    item.Working = info.Working;
                    item.Gender = info.Gender;
                    item.Facebook = info.Facebook;
                    item.Google = info.Google;
                    item.Twitter = info.Twitter;
                    item.Linkedin = info.Linkedin;
                    item.Website = info.Website;
                    item.Instagram = info.Instagram;
                    item.WebDeviceId = info.WebDeviceId;
                    item.Language = info.Language;
                    item.IpAddress = info.IpAddress;
                    item.PhoneNumber = info.PhoneNumber;
                    item.Timezone = info.Timezone;
                    item.Lat = info.Lat;
                    item.Lng = info.Lng;
                    item.Time = info.Time;
                    item.About = info.About;
                    item.Birthday = info.Birthday;
                    item.Registered = info.Registered;
                    item.Lastseen = info.Lastseen;
                    item.LastLocationUpdate = info.LastLocationUpdate;
                    item.Balance = info.Balance;
                    item.Verified = info.Verified;
                    item.Status = info.Status;
                    item.Active = info.Active;
                    item.Admin = info.Admin;
                    item.IsPro = info.IsPro;
                    item.ProType = info.ProType;
                    item.School = info.School;
                    item.Name = info.Name;
                    item.AndroidMDeviceId = info.AndroidMDeviceId;
                    item.ECommented = info.ECommented;
                    item.AndroidNDeviceId = info.AndroidMDeviceId;
                    item.AvatarFull = info.AvatarFull;
                    item.BirthPrivacy = info.BirthPrivacy;
                    item.CanFollow = info.CanFollow;
                    item.ConfirmFollowers = info.ConfirmFollowers;
                    item.CountryId = info.CountryId;
                    item.EAccepted = info.EAccepted;
                    item.EFollowed = info.EFollowed;
                    item.EJoinedGroup = info.EJoinedGroup;
                    item.ELastNotif = info.ELastNotif;
                    item.ELiked = info.ELiked;
                    item.ELikedPage = info.ELikedPage;
                    item.EMentioned = info.EMentioned;
                    item.EProfileWallPost = info.EProfileWallPost;
                    item.ESentmeMsg = info.ESentmeMsg;
                    item.EShared = info.EShared;
                    item.EVisited = info.EVisited;
                    item.EWondered = info.EWondered;
                    item.EmailNotification = info.EmailNotification;
                    item.FollowPrivacy = info.FollowPrivacy;
                    item.FriendPrivacy = info.FriendPrivacy;
                    item.GenderText = info.GenderText;
                    item.InfoFile = info.InfoFile;
                    item.IosMDeviceId = info.IosMDeviceId;
                    item.IosNDeviceId = info.IosNDeviceId;
                    item.IsBlocked = info.IsBlocked;
                    item.IsFollowing = info.IsFollowing;
                    item.IsFollowingMe = info.IsFollowingMe;
                    item.LastAvatarMod = info.LastAvatarMod;
                    item.LastCoverMod = info.LastCoverMod;
                    item.LastDataUpdate = info.LastDataUpdate;
                    item.LastFollowId = info.LastFollowId;
                    item.LastLoginData = info.LastLoginData;
                    item.LastseenStatus = info.LastseenStatus;
                    item.LastSeenTimeText = info.LastSeenTimeText;
                    item.LastseenUnixTime = info.LastseenUnixTime;
                    item.MessagePrivacy = info.MessagePrivacy;
                    item.NewEmail = info.NewEmail;
                    item.NewPhone = info.NewPhone;
                    item.NotificationsSound = info.NotificationsSound;
                    item.OrderPostsBy = info.OrderPostsBy;
                    item.PaypalEmail = info.PaypalEmail;
                    item.PostPrivacy = info.PostPrivacy;
                    item.Referrer = info.Referrer;
                    item.ShareMyData = info.ShareMyData;
                    item.ShareMyLocation = info.ShareMyLocation;
                    item.ShowActivitiesPrivacy = info.ShowActivitiesPrivacy;
                    item.TwoFactor = info.TwoFactor;
                    item.TwoFactorVerified = info.TwoFactorVerified;
                    item.Url = info.Url;
                    item.VisitPrivacy = info.VisitPrivacy;
                    item.Vk = info.Vk;
                    item.Wallet = info.Wallet;
                    item.WorkingLink = info.WorkingLink;
                    item.Youtube = info.Youtube;
                    item.City = info.City;
                    item.Points = info.Points;
                    item.DailyPoints = info.DailyPoints;
                    item.PointDayExpire = info.PointDayExpire;
                    item.State = info.State;
                    item.Zip = info.Zip;
                    item.CashfreeSignature = info.CashfreeSignature;
                    item.IsAdmin = info.IsAdmin;
                    item.MemberId = info.MemberId;
                    item.ChatColor = info.ChatColor;
                    item.PaystackRef = info.PaystackRef;
                    item.RefUserId = info.RefUserId;
                    item.SchoolCompleted = info.SchoolCompleted;
                    item.AvatarPostId = info.AvatarPostId;
                    item.CodeSent = info.CodeSent;
                    item.CoverPostId = info.CoverPostId;
                    item.Discord = info.Discord;
                    item.IsReported = info.IsReported;
                    item.IsStoryMuted = info.IsStoryMuted;
                    item.Mailru = info.Mailru;
                    item.NotificationSettings = info.NotificationSettings;
                    item.IsNotifyStopped = info.IsNotifyStopped;
                    item.Qq = info.Qq;
                    item.StripeSessionId = info.StripeSessionId;
                    item.TimeCodeSent = info.TimeCodeSent;
                    item.Wechat = info.Wechat;
                    item.ApiNotificationSettings = string.Empty;
                    item.Details = string.Empty;
                    item.SColorBackground = info.SColorBackground;
                    item.SColorForeground = info.SColorForeground;
                    item.UserPlatform = info.UserPlatform;
                    item.WeatherUnit = info.WeatherUnit;
                    item.IsMute = info.IsMute;
                    item.ColorFollow = info.ColorFollow;
                    item.ColorGender = info.ColorGender;
                    item.IsArchive = info.IsArchive;
                    item.IsPin = info.IsPin;
                    item.Selected = info.Selected;
                    item.TextColorFollowing = info.TextColorFollowing;
                    item.TextFollowing = info.TextFollowing;
                    item.Type = info.Type;
                    item.Banned = info.Banned;
                    item.BannedReason = info.BannedReason;
                    item.CoinbaseCode = info.CoinbaseCode;
                    item.CoinbaseHash = info.CoinbaseHash;
                    item.ConversationId = info.ConversationId;
                    item.CurrentlyWorking = info.CurrentlyWorking;
                    item.IsOnline = info.IsOnline;
                    item.IsOpenToWork = info.IsOpenToWork;
                    item.IsProvidingService = info.IsProvidingService;
                    item.Languages = info.Languages;
                    item.Okru = info.Okru;
                    item.OpenToWorkData = info.OpenToWorkData;
                    item.Permission = info.Permission;
                    item.ProvidingService = info.ProvidingService;
                    item.SAppMainLater = info.SAppMainLater;
                    item.SColorOnOff = info.SColorOnOff;
                    item.SecurionpayKey = info.SecurionpayKey;
                    item.Skills = info.Skills;
                    item.YoomoneyHash = info.YoomoneyHash;
                   
                    if (info.Details.DetailsClass != null)
                        item.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);

                    if (info.ApiNotificationSettings.NotificationSettingsClass != null)
                        item.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);

                    //db.Details = JsonConvert.SerializeObject(info.Details);
                    connection.Update(item);
                }
                else
                {
                    DataTables.MyProfileTb db = new DataTables.MyProfileTb
                    {
                        UserId = info.UserId,
                        Username = info.Username,
                        Email = info.Email,
                        FirstName = info.FirstName,
                        LastName = info.LastName,
                        Avatar = info.Avatar,
                        Cover = info.Cover,
                        BackgroundImage = info.BackgroundImage,
                        RelationshipId = info.RelationshipId,
                        Address = info.Address,
                        Working = info.Working,
                        Gender = info.Gender,
                        Facebook = info.Facebook,
                        Google = info.Google,
                        Twitter = info.Twitter,
                        Linkedin = info.Linkedin,
                        Website = info.Website,
                        Instagram = info.Instagram,
                        WebDeviceId = info.WebDeviceId,
                        Language = info.Language,
                        IpAddress = info.IpAddress,
                        PhoneNumber = info.PhoneNumber,
                        Timezone = info.Timezone,
                        Lat = info.Lat,
                        Lng = info.Lng,
                        Time = info.Time,
                        About = info.About,
                        Birthday = info.Birthday,
                        Registered = info.Registered,
                        Lastseen = info.Lastseen,
                        LastLocationUpdate = info.LastLocationUpdate,
                        Balance = info.Balance,
                        Verified = info.Verified,
                        Status = info.Status,
                        Active = info.Active,
                        Admin = info.Admin,
                        IsPro = info.IsPro,
                        ProType = info.ProType,
                        School = info.School,
                        Name = info.Name,
                        AndroidMDeviceId = info.AndroidMDeviceId,
                        ECommented = info.ECommented,
                        AndroidNDeviceId = info.AndroidMDeviceId,
                        AvatarFull = info.AvatarFull,
                        BirthPrivacy = info.BirthPrivacy,
                        CanFollow = info.CanFollow,
                        ConfirmFollowers = info.ConfirmFollowers,
                        CountryId = info.CountryId,
                        EAccepted = info.EAccepted,
                        EFollowed = info.EFollowed,
                        EJoinedGroup = info.EJoinedGroup,
                        ELastNotif = info.ELastNotif,
                        ELiked = info.ELiked,
                        ELikedPage = info.ELikedPage,
                        EMentioned = info.EMentioned,
                        EProfileWallPost = info.EProfileWallPost,
                        ESentmeMsg = info.ESentmeMsg,
                        EShared = info.EShared,
                        EVisited = info.EVisited,
                        EWondered = info.EWondered,
                        EmailNotification = info.EmailNotification,
                        FollowPrivacy = info.FollowPrivacy,
                        FriendPrivacy = info.FriendPrivacy,
                        GenderText = info.GenderText,
                        InfoFile = info.InfoFile,
                        IosMDeviceId = info.IosMDeviceId,
                        IosNDeviceId = info.IosNDeviceId,
                        IsBlocked = info.IsBlocked,
                        IsFollowing = info.IsFollowing,
                        IsFollowingMe = info.IsFollowingMe,
                        LastAvatarMod = info.LastAvatarMod,
                        LastCoverMod = info.LastCoverMod,
                        LastDataUpdate = info.LastDataUpdate,
                        LastFollowId = info.LastFollowId,
                        LastLoginData = info.LastLoginData,
                        LastseenStatus = info.LastseenStatus,
                        LastSeenTimeText = info.LastSeenTimeText,
                        LastseenUnixTime = info.LastseenUnixTime,
                        MessagePrivacy = info.MessagePrivacy,
                        NewEmail = info.NewEmail,
                        NewPhone = info.NewPhone,
                        NotificationsSound = info.NotificationsSound,
                        OrderPostsBy = info.OrderPostsBy,
                        PaypalEmail = info.PaypalEmail,
                        PostPrivacy = info.PostPrivacy,
                        Referrer = info.Referrer,
                        ShareMyData = info.ShareMyData,
                        ShareMyLocation = info.ShareMyLocation,
                        ShowActivitiesPrivacy = info.ShowActivitiesPrivacy,
                        TwoFactor = info.TwoFactor,
                        TwoFactorVerified = info.TwoFactorVerified,
                        Url = info.Url,
                        VisitPrivacy = info.VisitPrivacy,
                        Vk = info.Vk,
                        Wallet = info.Wallet,
                        WorkingLink = info.WorkingLink,
                        Youtube = info.Youtube,
                        City = info.City,
                        Points = info.Points,
                        DailyPoints = info.DailyPoints,
                        PointDayExpire = info.PointDayExpire,
                        State = info.State,
                        Zip = info.Zip,
                        CashfreeSignature = info.CashfreeSignature,
                        IsAdmin = info.IsAdmin,
                        MemberId = info.MemberId,
                        ChatColor = info.ChatColor,
                        PaystackRef = info.PaystackRef,
                        RefUserId = info.RefUserId,
                        SchoolCompleted = info.SchoolCompleted,
                        AvatarPostId = info.AvatarPostId,
                        CodeSent = info.CodeSent,
                        CoverPostId = info.CoverPostId,
                        Discord = info.Discord,
                        IsReported = info.IsReported,
                        IsStoryMuted = info.IsStoryMuted,
                        Mailru = info.Mailru,
                        NotificationSettings = info.NotificationSettings,
                        IsNotifyStopped = info.IsNotifyStopped,
                        Qq = info.Qq,
                        StripeSessionId = info.StripeSessionId,
                        TimeCodeSent = info.TimeCodeSent,
                        Wechat = info.Wechat,
                        ApiNotificationSettings = string.Empty,
                        Details = string.Empty,
                        SColorBackground = info.SColorBackground,
                        SColorForeground = info.SColorForeground,
                        UserPlatform = info.UserPlatform,
                        WeatherUnit = info.WeatherUnit,
                        IsMute = info.IsMute,
                        ColorFollow = info.ColorFollow,
                        ColorGender = info.ColorGender,
                        IsArchive = info.IsArchive,
                        IsPin = info.IsPin,
                        Selected = info.Selected,
                        TextColorFollowing = info.TextColorFollowing,
                        TextFollowing = info.TextFollowing,
                        Type = info.Type,
                        Banned = info.Banned,
                        BannedReason = info.BannedReason,
                        CoinbaseCode = info.CoinbaseCode,
                        CoinbaseHash = info.CoinbaseHash,
                        ConversationId = info.ConversationId,
                        CurrentlyWorking = info.CurrentlyWorking,
                        IsOnline = info.IsOnline,
                        IsOpenToWork = info.IsOpenToWork,
                        IsProvidingService = info.IsProvidingService,
                        Languages = info.Languages,
                        Okru = info.Okru,
                        OpenToWorkData = info.OpenToWorkData,
                        Permission = info.Permission,
                        ProvidingService = info.ProvidingService,
                        SAppMainLater = info.SAppMainLater,
                        SColorOnOff = info.SColorOnOff,
                        SecurionpayKey = info.SecurionpayKey,
                        Skills = info.Skills,
                        YoomoneyHash = info.YoomoneyHash,
                    };

                    if (info.Details.DetailsClass != null)
                        db.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);

                    if (info.ApiNotificationSettings.NotificationSettingsClass != null)
                        db.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);

                    //db.Details = JsonConvert.SerializeObject(info.Details);
                    connection.Insert(db);
                }

                UserDetails.Avatar = info.Avatar;
                UserDetails.Cover = info.Cover;
                UserDetails.Username = info.Username;
                UserDetails.FullName = info.Name;
                UserDetails.Email = info.Email;

                ListUtils.MyProfileList = new ObservableCollection<UserDataFody> { info };
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Update_To_MyProfileTable(info);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        // Get data To My Profile Table
        public UserDataFody Get_MyProfile()
        {
            try
            {
                using var connection = OpenConnection();
                var info = connection.Table<DataTables.MyProfileTb>().FirstOrDefault();
                if (info != null)
                {
                    UserDataFody infoObject = new UserDataFody
                    {
                        UserId = info.UserId,
                        Username = info.Username,
                        Email = info.Email,
                        FirstName = info.FirstName,
                        LastName = info.LastName,
                        Avatar = info.Avatar,
                        Cover = info.Cover,
                        BackgroundImage = info.BackgroundImage,
                        RelationshipId = info.RelationshipId,
                        Address = info.Address,
                        Working = info.Working,
                        Gender = info.Gender,
                        Facebook = info.Facebook,
                        Google = info.Google,
                        Twitter = info.Twitter,
                        Linkedin = info.Linkedin,
                        Website = info.Website,
                        Instagram = info.Instagram,
                        WebDeviceId = info.WebDeviceId,
                        Language = info.Language,
                        IpAddress = info.IpAddress,
                        PhoneNumber = info.PhoneNumber,
                        Timezone = info.Timezone,
                        Lat = info.Lat,
                        Lng = info.Lng,
                        Time = info.Time,
                        About = info.About,
                        Birthday = info.Birthday,
                        Registered = info.Registered,
                        Lastseen = info.Lastseen,
                        LastLocationUpdate = info.LastLocationUpdate,
                        Balance = info.Balance,
                        Verified = info.Verified,
                        Status = info.Status,
                        Active = info.Active,
                        Admin = info.Admin,
                        IsPro = info.IsPro,
                        ProType = info.ProType,
                        School = info.School,
                        Name = info.Name,
                        AndroidMDeviceId = info.AndroidMDeviceId,
                        ECommented = info.ECommented,
                        AndroidNDeviceId = info.AndroidMDeviceId,
                        AvatarFull = info.AvatarFull,
                        BirthPrivacy = info.BirthPrivacy,
                        CanFollow = info.CanFollow,
                        ConfirmFollowers = info.ConfirmFollowers,
                        CountryId = info.CountryId,
                        EAccepted = info.EAccepted,
                        EFollowed = info.EFollowed,
                        EJoinedGroup = info.EJoinedGroup,
                        ELastNotif = info.ELastNotif,
                        ELiked = info.ELiked,
                        ELikedPage = info.ELikedPage,
                        EMentioned = info.EMentioned,
                        EProfileWallPost = info.EProfileWallPost,
                        ESentmeMsg = info.ESentmeMsg,
                        EShared = info.EShared,
                        EVisited = info.EVisited,
                        EWondered = info.EWondered,
                        EmailNotification = info.EmailNotification,
                        FollowPrivacy = info.FollowPrivacy,
                        FriendPrivacy = info.FriendPrivacy,
                        GenderText = info.GenderText,
                        InfoFile = info.InfoFile,
                        IosMDeviceId = info.IosMDeviceId,
                        IosNDeviceId = info.IosNDeviceId,
                        IsBlocked = info.IsBlocked,
                        IsFollowing = info.IsFollowing,
                        IsFollowingMe = info.IsFollowingMe,
                        LastAvatarMod = info.LastAvatarMod,
                        LastCoverMod = info.LastCoverMod,
                        LastDataUpdate = info.LastDataUpdate,
                        LastFollowId = info.LastFollowId,
                        LastLoginData = info.LastLoginData,
                        LastseenStatus = info.LastseenStatus,
                        LastSeenTimeText = info.LastSeenTimeText,
                        LastseenUnixTime = info.LastseenUnixTime,
                        MessagePrivacy = info.MessagePrivacy,
                        NewEmail = info.NewEmail,
                        NewPhone = info.NewPhone,
                        NotificationsSound = info.NotificationsSound,
                        OrderPostsBy = info.OrderPostsBy,
                        PaypalEmail = info.PaypalEmail,
                        PostPrivacy = info.PostPrivacy,
                        Referrer = info.Referrer,
                        ShareMyData = info.ShareMyData,
                        ShareMyLocation = info.ShareMyLocation,
                        ShowActivitiesPrivacy = info.ShowActivitiesPrivacy,
                        TwoFactor = info.TwoFactor,
                        TwoFactorVerified = info.TwoFactorVerified,
                        Url = info.Url,
                        VisitPrivacy = info.VisitPrivacy,
                        Vk = info.Vk,
                        Wallet = info.Wallet,
                        WorkingLink = info.WorkingLink,
                        Youtube = info.Youtube,
                        City = info.City,
                        Points = info.Points,
                        DailyPoints = info.DailyPoints,
                        PointDayExpire = info.PointDayExpire,
                        State = info.State,
                        Zip = info.Zip,
                        CashfreeSignature = info.CashfreeSignature,
                        IsAdmin = info.IsAdmin,
                        MemberId = info.MemberId,
                        ChatColor = info.ChatColor,
                        PaystackRef = info.PaystackRef,
                        RefUserId = info.RefUserId,
                        SchoolCompleted = info.SchoolCompleted,
                        AvatarPostId = info.AvatarPostId,
                        CodeSent = info.CodeSent,
                        CoverPostId = info.CoverPostId,
                        Discord = info.Discord,
                        IsReported = info.IsReported,
                        IsStoryMuted = info.IsStoryMuted,
                        Mailru = info.Mailru,
                        NotificationSettings = info.NotificationSettings,
                        IsNotifyStopped = info.IsNotifyStopped,
                        Qq = info.Qq,
                        StripeSessionId = info.StripeSessionId,
                        TimeCodeSent = info.TimeCodeSent,
                        Wechat = info.Wechat,
                        ApiNotificationSettings = new NotificationSettingsUnion(),
                        Details = new DetailsUnion(),
                        SColorBackground = info.SColorBackground,
                        SColorForeground = info.SColorForeground,
                        UserPlatform = info.UserPlatform,
                        WeatherUnit = info.WeatherUnit,
                        IsMute = info.IsMute,
                        ColorFollow = info.ColorFollow,
                        ColorGender = info.ColorGender,
                        IsArchive = info.IsArchive,
                        IsPin = info.IsPin,
                        Selected = info.Selected,
                        TextColorFollowing = info.TextColorFollowing,
                        TextFollowing = info.TextFollowing,
                        Type = info.Type,
                        Banned = info.Banned,
                        BannedReason = info.BannedReason,
                        CoinbaseCode = info.CoinbaseCode,
                        CoinbaseHash = info.CoinbaseHash,
                        ConversationId = info.ConversationId,
                        CurrentlyWorking = info.CurrentlyWorking,
                        IsOnline = info.IsOnline,
                        IsOpenToWork = info.IsOpenToWork,
                        IsProvidingService = info.IsProvidingService,
                        Languages = info.Languages,
                        Okru = info.Okru,
                        OpenToWorkData = info.OpenToWorkData,
                        Permission = info.Permission,
                        ProvidingService = info.ProvidingService,
                        SAppMainLater = info.SAppMainLater,
                        SColorOnOff = info.SColorOnOff,
                        SecurionpayKey = info.SecurionpayKey,
                        Skills = info.Skills,
                        YoomoneyHash = info.YoomoneyHash,
                    };

                    infoObject.Details = string.IsNullOrEmpty(info.Details) switch
                    {
                        false => new DetailsUnion { DetailsClass = JsonConvert.DeserializeObject<Details>(info.Details) },
                        _ => infoObject.Details
                    };

                    infoObject.ApiNotificationSettings = string.IsNullOrEmpty(info.ApiNotificationSettings) switch
                    {
                        false => new NotificationSettingsUnion { NotificationSettingsClass = JsonConvert.DeserializeObject<NotificationSettings>(info.ApiNotificationSettings) },
                        _ => infoObject.ApiNotificationSettings
                    };

                    UserDetails.Avatar = info.Avatar;
                    UserDetails.Cover = info.Cover;
                    UserDetails.Username = info.Username;
                    UserDetails.FullName = info.Name;
                    UserDetails.Email = info.Email;

                    ListUtils.MyProfileList = new ObservableCollection<UserDataFody> { infoObject };

                    return infoObject;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_MyProfile();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null;
                }
            }
        }

        #endregion

        #region Message

        //Insert data To Message Table
        public void Insert_Or_Replace_MessagesTable(ObservableCollection<MessagesDataFody> messageList)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;

                List<DataTables.MessageTb> listOfDatabaseForInsert = new List<DataTables.MessageTb>();

                // get data from database
                var resultMessage = connection.Table<DataTables.MessageTb>().ToList();

                foreach (var messages in messageList)
                {
                    var maTb = ClassMapper.Mapper?.Map<DataTables.MessageTb>(messages);
                    maTb.SendFile = false;

                    maTb.Product = JsonConvert.SerializeObject(messages.Product?.ProductClass);

                    maTb.MessageUser = JsonConvert.SerializeObject(messages.MessageUser?.UserDataClass);
                    maTb.UserData = JsonConvert.SerializeObject(messages.UserData);
                    maTb.ToData = JsonConvert.SerializeObject(messages.ToData);
                    maTb.Reaction = JsonConvert.SerializeObject(messages.Reaction);
                    maTb.Reply = JsonConvert.SerializeObject(messages.Reply?.ReplyClass);
                    maTb.Story = JsonConvert.SerializeObject(messages.Story?.StoryClass);

                    var dataCheck = resultMessage.FirstOrDefault(a => a.Id == messages.Id);
                    if (dataCheck != null)
                    {
                        var checkForUpdate = resultMessage.FirstOrDefault(a => a.Id == dataCheck.Id);
                        if (checkForUpdate != null)
                        {
                            checkForUpdate = ClassMapper.Mapper?.Map<DataTables.MessageTb>(messages);
                            //checkForUpdate.SendFile = false;
                            checkForUpdate.ChatColor = messages.ChatColor;

                            checkForUpdate.Product = JsonConvert.SerializeObject(messages.Product?.ProductClass);

                            checkForUpdate.MessageUser = JsonConvert.SerializeObject(messages.MessageUser?.UserDataClass);
                            checkForUpdate.UserData = JsonConvert.SerializeObject(messages.UserData);
                            checkForUpdate.ToData = JsonConvert.SerializeObject(messages.ToData);
                            checkForUpdate.Reaction = JsonConvert.SerializeObject(messages.Reaction);
                            checkForUpdate.Reply = JsonConvert.SerializeObject(messages.Reply?.ReplyClass);
                            checkForUpdate.Story = JsonConvert.SerializeObject(messages.Story?.StoryClass);

                            connection.Update(checkForUpdate);

                            //var cec = ChatWindowActivity.GetInstance()?.StartedMessageList?.FirstOrDefault(a => a.Id == Long.ParseLong(dataCheck.Id));
                            //if (cec != null)
                            //{
                            //    cec.MesData = messages.MesData;
                            //    cec.MesData.IsStarted = true;
                            //    cec.TypeView = messages.TypeView;
                            //}
                        }
                        else
                        {
                            listOfDatabaseForInsert.Add(maTb);
                        }
                    }
                    else
                    {
                        listOfDatabaseForInsert.Add(maTb);
                    }
                }

                connection.BeginTransaction();

                //Bring new  
                if (listOfDatabaseForInsert.Count > 0)
                {
                    connection.InsertAll(listOfDatabaseForInsert);
                }

                connection.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Replace_MessagesTable(messageList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Update one Messages Table
        public void Insert_Or_Update_To_one_MessagesTable(MessagesDataFody item)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;

                var data = connection.Table<DataTables.MessageTb>().FirstOrDefault(a => a.Id == item.Id);
                if (data != null)
                {
                    data = ClassMapper.Mapper?.Map<DataTables.MessageTb>(item);
                    //data.SendFile = false;

                    data.Product = JsonConvert.SerializeObject(item.Product?.ProductClass);

                    data.MessageUser = JsonConvert.SerializeObject(item.MessageUser?.UserDataClass);
                    data.UserData = JsonConvert.SerializeObject(item.UserData);
                    data.ToData = JsonConvert.SerializeObject(item.ToData);
                    data.Reaction = JsonConvert.SerializeObject(item.Reaction);
                    data.Reply = JsonConvert.SerializeObject(item.Reply?.ReplyClass);
                    data.Story = JsonConvert.SerializeObject(item.Story?.StoryClass);

                    connection.Update(data);
                }
                else
                {
                    var maTb = ClassMapper.Mapper?.Map<DataTables.MessageTb>(item);
                    //maTb.SendFile = false;

                    maTb.Product = JsonConvert.SerializeObject(item.Product?.ProductClass);

                    maTb.MessageUser = JsonConvert.SerializeObject(item.MessageUser?.UserDataClass);
                    maTb.UserData = JsonConvert.SerializeObject(item.UserData);
                    maTb.ToData = JsonConvert.SerializeObject(item.ToData);
                    maTb.Reaction = JsonConvert.SerializeObject(item.Reaction);
                    maTb.Reply = JsonConvert.SerializeObject(item.Reply?.ReplyClass);
                    maTb.Story = JsonConvert.SerializeObject(item.Story?.StoryClass);

                    //Insert  one Messages Table
                    connection.Insert(maTb);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Update_To_one_MessagesTable(item);
                else
                    Methods.DisplayReportResultTrack(e);
            } 
        }

        //Get data To Messages
        public ObservableCollection<MessagesDataFody> GetMessages_List(string fromId, string toId, long beforeMessageId)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return null;

                var data = connection.Table<DataTables.MessageTb>().FirstOrDefault(a => a.FromId == toId || a.ToId == toId);
                if (data == null)
                    return new ObservableCollection<MessagesDataFody>();
                 
                List<DataTables.MessageTb> query3;

                if (beforeMessageId == 0)
                {
                    query3 = connection.Table<DataTables.MessageTb>().Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId).ToList();
                }
                else
                {
                    query3 = connection.Table<DataTables.MessageTb>().Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId && w.Id < beforeMessageId && w.Id != beforeMessageId).ToList();
                }
                 
                List<DataTables.MessageTb> query = query3.Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId).OrderBy(q => q.Time).TakeLast(35).ToList();
                ObservableCollection<MessagesDataFody> listMessages = new ObservableCollection<MessagesDataFody>();
                if (query.Count > 0)
                {
                    foreach (var item in from item in query let check = listMessages?.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                    {
                        var maTb = ClassMapper.Mapper?.Map<MessagesDataFody>(item);
                        if (maTb != null)
                        { 
                            maTb.Product = new ProductUnion();
                            maTb.MessageUser = new MessageUserUnion();
                            maTb.UserData = new UserDataObject();
                            maTb.ToData = new UserDataObject();
                            maTb.Reaction = new Reaction();
                            maTb.Reply = new ReplyUnion();
                            maTb.Story = new StoryUnion();

                            if (!string.IsNullOrEmpty(item.Product))
                                maTb.Product = new ProductUnion
                                {
                                    ProductClass = JsonConvert.DeserializeObject<ProductDataObject>(item.Product)
                                };

                            if (!string.IsNullOrEmpty(item.MessageUser))
                                maTb.MessageUser = new MessageUserUnion
                                {
                                    UserDataClass = JsonConvert.DeserializeObject<UserDataObject>(item.MessageUser)
                                };

                            if (!string.IsNullOrEmpty(item.UserData))
                                maTb.UserData = JsonConvert.DeserializeObject<UserDataObject>(item.UserData);

                            if (!string.IsNullOrEmpty(item.ToData))
                                maTb.ToData = JsonConvert.DeserializeObject<UserDataObject>(item.ToData);

                            if (!string.IsNullOrEmpty(item.Reaction))
                                maTb.Reaction = JsonConvert.DeserializeObject<Reaction>(item.Reaction);

                            if (!string.IsNullOrEmpty(item.Reply))
                                maTb.Reply = new ReplyUnion
                                {
                                    ReplyClass = JsonConvert.DeserializeObject<MessageData>(item.Reply)
                                };

                            if (!string.IsNullOrEmpty(item.Story))
                                maTb.Story = new StoryUnion
                                {
                                    StoryClass = JsonConvert.DeserializeObject<StoryDataObject.Story>(item.Story)
                                };

                            var type = WoWonderTools.GetTypeModel(maTb);
                            if (type == MessageModelType.None)
                                continue;

                            if (beforeMessageId == 0)
                            {
                                listMessages.Add(WoWonderTools.MessageFilter(toId, "", maTb, maTb.ChatColor));
                            }
                            else
                            {
                                listMessages.Insert(0, WoWonderTools.MessageFilter(toId, "", maTb, maTb.ChatColor));
                            }
                        }
                    }
                    return listMessages;
                }

                return listMessages;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetMessages_List(fromId, toId, beforeMessageId);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<MessagesDataFody>();
                }
            }
        }

        //Get data To where first Messages >> load more
        public ObservableCollection<MessagesDataFody> GetMessageList(string fromId, string toId, long beforeMessageId)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return null;
                  
                List<DataTables.MessageTb> query3;

                if (beforeMessageId == 0)
                {
                    query3 = connection.Table<DataTables.MessageTb>().Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId).ToList();
                }
                else
                {
                    query3 = connection.Table<DataTables.MessageTb>().Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId && w.Id < beforeMessageId && w.Id != beforeMessageId).ToList();
                }
                 
                List<DataTables.MessageTb> query = query3.Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId).OrderBy(q => q.Time).TakeLast(15).ToList();

                ObservableCollection<MessagesDataFody> listMessages = new ObservableCollection<MessagesDataFody>();
                query.Reverse();

                if (query.Count > 0)
                {
                    foreach (var item in from item in query let check = ListUtils.ListMessages?.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                    {
                        var maTb = ClassMapper.Mapper?.Map<MessagesDataFody>(item);
                        if (maTb != null)
                        { 
                            maTb.Product = new ProductUnion();
                            maTb.MessageUser = new MessageUserUnion();
                            maTb.UserData = new UserDataObject();
                            maTb.ToData = new UserDataObject();
                            maTb.Reaction = new Reaction();
                            maTb.Reply = new ReplyUnion();
                            maTb.Story = new StoryUnion();

                            if (!string.IsNullOrEmpty(item.Product))
                                maTb.Product = new ProductUnion
                                {
                                    ProductClass = JsonConvert.DeserializeObject<ProductDataObject>(item.Product)
                                };

                            if (!string.IsNullOrEmpty(item.MessageUser))
                                maTb.MessageUser = new MessageUserUnion
                                {
                                    UserDataClass = JsonConvert.DeserializeObject<UserDataObject>(item.MessageUser)
                                };

                            if (!string.IsNullOrEmpty(item.UserData))
                                maTb.UserData = JsonConvert.DeserializeObject<UserDataObject>(item.UserData);

                            if (!string.IsNullOrEmpty(item.ToData))
                                maTb.ToData = JsonConvert.DeserializeObject<UserDataObject>(item.ToData);

                            if (!string.IsNullOrEmpty(item.Reaction))
                                maTb.Reaction = JsonConvert.DeserializeObject<Reaction>(item.Reaction);

                            if (!string.IsNullOrEmpty(item.Reply))
                                maTb.Reply = new ReplyUnion
                                {
                                    ReplyClass = JsonConvert.DeserializeObject<MessageData>(item.Reply)
                                };

                            if (!string.IsNullOrEmpty(item.Story))
                                maTb.Story = new StoryUnion
                                {
                                    StoryClass = JsonConvert.DeserializeObject<StoryDataObject.Story>(item.Story)
                                };

                            var type = WoWonderTools.GetTypeModel(maTb);
                            if (type == MessageModelType.None)
                                continue;

                            listMessages.Add(WoWonderTools.MessageFilter(toId, "", maTb, maTb.ChatColor));
                        }
                    }

                    return listMessages;
                }

                return listMessages;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetMessageList(fromId, toId, beforeMessageId);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<MessagesDataFody>();
                }
            } 
        }

        //Remove data To Messages Table
        public void Delete_OneMessageUser(long messageId)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                var user = connection.Table<DataTables.MessageTb>().FirstOrDefault(c => c.Id == messageId);
                if (user != null)
                {
                    connection.Delete(user);
                }  
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Delete_OneMessageUser(messageId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public void DeleteAllMessagesUser(string fromId, string toId)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                var query = connection.Query<DataTables.MessageTb>("Delete FROM MessageTb WHERE ((FromId =" + fromId + " AND ToId=" + toId + ") OR (FromId =" + toId + " AND ToId=" + fromId + "))");
                Console.WriteLine(query);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteAllMessagesUser(fromId, toId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public void ClearAll_Messages()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                connection.DeleteAll<DataTables.MessageTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    ClearAll_Messages();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Last_User_Chat

        //Insert Or Update data To Users Table
        public void Insert_Or_Update_LastUsersChat(ObservableCollection<UserListFody> lastUsersList)
        {
            try
            {
                using var connection = OpenConnection();
                var result = connection.Table<DataTables.LastUsersTb>().ToList();

                List<DataTables.LastUsersTb> list = new List<DataTables.LastUsersTb>();
                foreach (var item in lastUsersList)
                {
                    DataTables.LastUsersTb db = new DataTables.LastUsersTb
                    {
                        Id = item.Id,
                        AvatarOrg = item.AvatarOrg,
                        BackgroundImageStatus = item.BackgroundImageStatus,
                        Boosted = item.Boosted,
                        CallActionType = item.CallActionType,
                        CallActionTypeUrl = item.CallActionTypeUrl,
                        Category = item.Category,
                        ChatTime = item.ChatTime,
                        ChatType = item.ChatType,
                        Company = item.Company,
                        CoverFull = item.CoverFull,
                        CoverOrg = item.CoverOrg,
                        CssFile = item.CssFile,
                        EmailCode = item.EmailCode,
                        GroupId = item.GroupId,
                        Instgram = item.Instgram,
                        Joined = item.Joined,
                        LastEmailSent = item.LastEmailSent,
                        PageCategory = item.PageCategory,
                        PageDescription = item.PageDescription,
                        PageId = item.PageId,
                        PageName = item.PageName,
                        PageTitle = item.PageTitle,
                        Phone = item.Phone,
                        GroupName = item.GroupName,
                        ProTime = item.ProTime,
                        Rating = item.Rating,
                        Showlastseen = item.Showlastseen,
                        SidebarData = item.SidebarData,
                        SmsCode = item.SmsCode,
                        SocialLogin = item.SocialLogin,
                        Src = item.Src,
                        StartUp = item.StartUp,
                        StartupFollow = item.StartupFollow,
                        StartupImage = item.StartupImage,
                        StartUpInfo = item.StartUpInfo,
                        UserId = item.UserId,
                        Username = item.Username,
                        Email = item.Email,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Avatar = item.Avatar,
                        Cover = item.Cover,
                        BackgroundImage = item.BackgroundImage,
                        RelationshipId = item.RelationshipId,
                        Address = item.Address,
                        Working = item.Working,
                        Gender = item.Gender,
                        Facebook = item.Facebook,
                        Google = item.Google,
                        Twitter = item.Twitter,
                        Linkedin = item.Linkedin,
                        Website = item.Website,
                        Instagram = item.Instagram,
                        WebDeviceId = item.WebDeviceId,
                        Language = item.Language,
                        IpAddress = item.IpAddress,
                        PhoneNumber = item.PhoneNumber,
                        Timezone = item.Timezone,
                        Lat = item.Lat,
                        Lng = item.Lng,
                        About = item.About,
                        Birthday = item.Birthday,
                        Registered = item.Registered,
                        Lastseen = item.Lastseen,
                        LastLocationUpdate = item.LastLocationUpdate,
                        Balance = item.Balance,
                        Verified = item.Verified,
                        Status = item.Status,
                        Active = item.Active,
                        Admin = item.Admin,
                        IsPro = item.IsPro,
                        ProType = item.ProType,
                        School = item.School,
                        Name = item.Name,
                        AndroidMDeviceId = item.AndroidMDeviceId,
                        ECommented = item.ECommented,
                        AndroidNDeviceId = item.AndroidMDeviceId,
                        AvatarFull = item.AvatarFull,
                        BirthPrivacy = item.BirthPrivacy,
                        CanFollow = item.CanFollow,
                        ConfirmFollowers = item.ConfirmFollowers,
                        CountryId = item.CountryId,
                        EAccepted = item.EAccepted,
                        EFollowed = item.EFollowed,
                        EJoinedGroup = item.EJoinedGroup,
                        ELastNotif = item.ELastNotif,
                        ELiked = item.ELiked,
                        ELikedPage = item.ELikedPage,
                        EMentioned = item.EMentioned,
                        EProfileWallPost = item.EProfileWallPost,
                        ESentmeMsg = item.ESentmeMsg,
                        EShared = item.EShared,
                        EVisited = item.EVisited,
                        EWondered = item.EWondered,
                        EmailNotification = item.EmailNotification,
                        FollowPrivacy = item.FollowPrivacy,
                        FriendPrivacy = item.FriendPrivacy,
                        GenderText = item.GenderText,
                        InfoFile = item.InfoFile,
                        IosMDeviceId = item.IosMDeviceId,
                        IosNDeviceId = item.IosNDeviceId,
                        IsBlocked = item.IsBlocked,
                        IsFollowing = item.IsFollowing,
                        IsFollowingMe = item.IsFollowingMe,
                        LastAvatarMod = item.LastAvatarMod,
                        LastCoverMod = item.LastCoverMod,
                        LastDataUpdate = item.LastDataUpdate,
                        LastFollowId = item.LastFollowId,
                        LastLoginData = item.LastLoginData,
                        LastseenStatus = item.LastseenStatus,
                        LastSeenTimeText = item.LastSeenTimeText,
                        LastseenUnixTime = item.LastseenUnixTime,
                        MessagePrivacy = item.MessagePrivacy,
                        NewEmail = item.NewEmail,
                        NewPhone = item.NewPhone,
                        NotificationsSound = item.NotificationsSound,
                        OrderPostsBy = item.OrderPostsBy,
                        PaypalEmail = item.PaypalEmail,
                        PostPrivacy = item.PostPrivacy,
                        Referrer = item.Referrer,
                        ShareMyData = item.ShareMyData,
                        ShareMyLocation = item.ShareMyLocation,
                        ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                        TwoFactor = item.TwoFactor,
                        TwoFactorVerified = item.TwoFactorVerified,
                        Url = item.Url,
                        VisitPrivacy = item.VisitPrivacy,
                        Vk = item.Vk,
                        Wallet = item.Wallet,
                        WorkingLink = item.WorkingLink,
                        Youtube = item.Youtube,
                        City = item.City,
                        State = item.State,
                        Zip = item.Zip,
                        Points = item.Points,
                        DailyPoints = item.DailyPoints,
                        PointDayExpire = item.PointDayExpire,
                        CashfreeSignature = item.CashfreeSignature,
                        IsAdmin = item.IsAdmin,
                        MemberId = item.MemberId,
                        ChatColor = item.ChatColor,
                        PaystackRef = item.PaystackRef,
                        RefUserId = item.RefUserId,
                        SchoolCompleted = item.SchoolCompleted,
                        Type = item.Type,
                        UserPlatform = item.UserPlatform,
                        WeatherUnit = item.WeatherUnit,
                        AvatarPostId = item.AvatarPostId,
                        CodeSent = item.CodeSent,
                        CoverPostId = item.CoverPostId,
                        Discord = item.Discord,
                        IsArchive = item.IsArchive,
                        IsMute = item.IsMute,
                        IsPin = item.IsPin,
                        IsReported = item.IsReported,
                        IsStoryMuted = item.IsStoryMuted,
                        Mailru = item.Mailru,
                        NotificationSettings = item.NotificationSettings,
                        IsNotifyStopped = item.IsNotifyStopped,
                        Qq = item.Qq,
                        StripeSessionId = item.StripeSessionId,
                        Time = item.Time,
                        TimeCodeSent = item.TimeCodeSent,
                        Wechat = item.Wechat,
                        AlbumData = item.AlbumData,
                        ChatId = item.ChatId,
                        IsPageOnwer = item.IsPageOnwer,
                        MessageCount = item.MessageCount,
                        Selected = false,
                        Owner = Convert.ToBoolean(item.Owner),
                        LastMessageText = item.LastMessageText,
                        AppMainLater = item.AppMainLater,
                        ColorFollow = item.ColorFollow,
                        ColorGender = item.ColorGender,
                        IsOnline = item.IsOnline,
                        MediaIconImage = item.MediaIconImage,
                        LastMessageColor = item.LastMessageColor,
                        PageSubCategory = item.PageSubCategory,
                        SColorBackground = item.SColorBackground,
                        SColorForeground = item.SColorForeground,
                        SMessageFontWeight = item.SMessageFontWeight,
                        SubCategory = item.SubCategory,
                        TextColorFollowing = item.TextColorFollowing,
                        TextFollowing = item.TextFollowing,
                        UsernameTwoLetters = item.UsernameTwoLetters,
                        UsersPost = item.UsersPost,
                        Banned = item.Banned,
                        BannedReason = item.BannedReason,
                        CoinbaseCode = item.CoinbaseCode,
                        CoinbaseHash = item.CoinbaseHash,
                        ConversationId = item.ConversationId,
                        CurrentlyWorking = item.CurrentlyWorking,
                        IsOpenToWork = item.IsOpenToWork,
                        IsProvidingService = item.IsProvidingService,
                        Languages = item.Languages,
                        Okru = item.Okru,
                        OpenToWorkData = item.OpenToWorkData,
                        Permission = item.Permission,
                        ProvidingService = item.ProvidingService,
                        SAppMainLater = item.SAppMainLater,
                        SColorOnOff = item.SColorOnOff,
                        SecurionpayKey = item.SecurionpayKey,
                        Skills = item.Skills,
                        YoomoneyHash = item.YoomoneyHash, 
                    };

                    db.OrderId = item.LastMessage.LastMessageClass.Time;
                     
                    db.UserData = JsonConvert.SerializeObject(item.UserData);
                    db.LastMessage = JsonConvert.SerializeObject(item.LastMessage.LastMessageClass);
                    db.Parts = JsonConvert.SerializeObject(item.Parts);
                    db.Details = JsonConvert.SerializeObject(item.Details.DetailsClass);
                    db.ApiNotificationSettings = JsonConvert.SerializeObject(item.ApiNotificationSettings.NotificationSettingsClass);
                    db.Mute = JsonConvert.SerializeObject(item.Mute);

                    list.Add(db);

                    var update = result.FirstOrDefault(a => a.UserId == item.UserId);
                    if (update != null)
                    {
                        update.Id = item.Id;
                        update.AvatarOrg = item.AvatarOrg;
                        update.BackgroundImageStatus = item.BackgroundImageStatus;
                        update.Boosted = item.Boosted;
                        update.CallActionType = item.CallActionType;
                        update.CallActionTypeUrl = item.CallActionTypeUrl;
                        update.Category = item.Category;
                        update.ChatTime = item.ChatTime;
                        update.ChatType = item.ChatType;
                        update.Company = item.Company;
                        update.CoverFull = item.CoverFull;
                        update.CoverOrg = item.CoverOrg;
                        update.CssFile = item.CssFile;
                        update.EmailCode = item.EmailCode;
                        update.GroupId = item.GroupId;
                        update.Instgram = item.Instgram;
                        update.Joined = item.Joined;
                        update.LastEmailSent = item.LastEmailSent;
                        update.PageCategory = item.PageCategory;
                        update.PageDescription = item.PageDescription;
                        update.PageId = item.PageId;
                        update.PageName = item.PageName;
                        update.PageTitle = item.PageTitle;
                        update.Phone = item.Phone;
                        update.GroupName = item.GroupName;
                        update.ProTime = item.ProTime;
                        update.Rating = item.Rating;
                        update.Showlastseen = item.Showlastseen;
                        update.SidebarData = item.SidebarData;
                        update.SmsCode = item.SmsCode;
                        update.SocialLogin = item.SocialLogin;
                        update.Src = item.Src;
                        update.StartUp = item.StartUp;
                        update.StartupFollow = item.StartupFollow;
                        update.StartupImage = item.StartupImage;
                        update.StartUpInfo = item.StartUpInfo;
                        update.UserId = item.UserId;
                        update.Username = item.Username;
                        update.Email = item.Email;
                        update.FirstName = item.FirstName;
                        update.LastName = item.LastName;
                        update.Avatar = item.Avatar;
                        update.Cover = item.Cover;
                        update.BackgroundImage = item.BackgroundImage;
                        update.RelationshipId = item.RelationshipId;
                        update.Address = item.Address;
                        update.Working = item.Working;
                        update.Gender = item.Gender;
                        update.Facebook = item.Facebook;
                        update.Google = item.Google;
                        update.Twitter = item.Twitter;
                        update.Linkedin = item.Linkedin;
                        update.Website = item.Website;
                        update.Instagram = item.Instagram;
                        update.WebDeviceId = item.WebDeviceId;
                        update.Language = item.Language;
                        update.IpAddress = item.IpAddress;
                        update.PhoneNumber = item.PhoneNumber;
                        update.Timezone = item.Timezone;
                        update.Lat = item.Lat;
                        update.Lng = item.Lng;
                        update.About = item.About;
                        update.Birthday = item.Birthday;
                        update.Registered = item.Registered;
                        update.Lastseen = item.Lastseen;
                        update.LastLocationUpdate = item.LastLocationUpdate;
                        update.Balance = item.Balance;
                        update.Verified = item.Verified;
                        update.Status = item.Status;
                        update.Active = item.Active;
                        update.Admin = item.Admin;
                        update.IsPro = item.IsPro;
                        update.ProType = item.ProType;
                        update.School = item.School;
                        update.Name = item.Name;
                        update.AndroidMDeviceId = item.AndroidMDeviceId;
                        update.ECommented = item.ECommented;
                        update.AndroidNDeviceId = item.AndroidMDeviceId;
                        update.AvatarFull = item.AvatarFull;
                        update.BirthPrivacy = item.BirthPrivacy;
                        update.CanFollow = item.CanFollow;
                        update.ConfirmFollowers = item.ConfirmFollowers;
                        update.CountryId = item.CountryId;
                        update.EAccepted = item.EAccepted;
                        update.EFollowed = item.EFollowed;
                        update.EJoinedGroup = item.EJoinedGroup;
                        update.ELastNotif = item.ELastNotif;
                        update.ELiked = item.ELiked;
                        update.ELikedPage = item.ELikedPage;
                        update.EMentioned = item.EMentioned;
                        update.EProfileWallPost = item.EProfileWallPost;
                        update.ESentmeMsg = item.ESentmeMsg;
                        update.EShared = item.EShared;
                        update.EVisited = item.EVisited;
                        update.EWondered = item.EWondered;
                        update.EmailNotification = item.EmailNotification;
                        update.FollowPrivacy = item.FollowPrivacy;
                        update.FriendPrivacy = item.FriendPrivacy;
                        update.GenderText = item.GenderText;
                        update.InfoFile = item.InfoFile;
                        update.IosMDeviceId = item.IosMDeviceId;
                        update.IosNDeviceId = item.IosNDeviceId;
                        update.IsBlocked = item.IsBlocked;
                        update.IsFollowing = item.IsFollowing;
                        update.IsFollowingMe = item.IsFollowingMe;
                        update.LastAvatarMod = item.LastAvatarMod;
                        update.LastCoverMod = item.LastCoverMod;
                        update.LastDataUpdate = item.LastDataUpdate;
                        update.LastFollowId = item.LastFollowId;
                        update.LastLoginData = item.LastLoginData;
                        update.LastseenStatus = item.LastseenStatus;
                        update.LastSeenTimeText = item.LastSeenTimeText;
                        update.LastseenUnixTime = item.LastseenUnixTime;
                        update.MessagePrivacy = item.MessagePrivacy;
                        update.NewEmail = item.NewEmail;
                        update.NewPhone = item.NewPhone;
                        update.NotificationsSound = item.NotificationsSound;
                        update.OrderPostsBy = item.OrderPostsBy;
                        update.PaypalEmail = item.PaypalEmail;
                        update.PostPrivacy = item.PostPrivacy;
                        update.Referrer = item.Referrer;
                        update.ShareMyData = item.ShareMyData;
                        update.ShareMyLocation = item.ShareMyLocation;
                        update.ShowActivitiesPrivacy = item.ShowActivitiesPrivacy;
                        update.TwoFactor = item.TwoFactor;
                        update.TwoFactorVerified = item.TwoFactorVerified;
                        update.Url = item.Url;
                        update.VisitPrivacy = item.VisitPrivacy;
                        update.Vk = item.Vk;
                        update.Wallet = item.Wallet;
                        update.WorkingLink = item.WorkingLink;
                        update.Youtube = item.Youtube;
                        update.City = item.City;
                        update.State = item.State;
                        update.Zip = item.Zip;
                        update.Points = item.Points;
                        update.DailyPoints = item.DailyPoints;
                        update.PointDayExpire = item.PointDayExpire;
                        update.CashfreeSignature = item.CashfreeSignature;
                        update.IsAdmin = item.IsAdmin;
                        update.MemberId = item.MemberId;
                        update.ChatColor = item.ChatColor;
                        update.PaystackRef = item.PaystackRef;
                        update.RefUserId = item.RefUserId;
                        update.SchoolCompleted = item.SchoolCompleted;
                        update.Type = item.Type;
                        update.UserPlatform = item.UserPlatform;
                        update.WeatherUnit = item.WeatherUnit;
                        update.AvatarPostId = item.AvatarPostId;
                        update.CodeSent = item.CodeSent;
                        update.CoverPostId = item.CoverPostId;
                        update.Discord = item.Discord;
                        update.IsArchive = item.IsArchive;
                        update.IsMute = item.IsMute;
                        update.IsPin = item.IsPin;
                        update.IsReported = item.IsReported;
                        update.IsStoryMuted = item.IsStoryMuted;
                        update.Mailru = item.Mailru;
                        update.NotificationSettings = item.NotificationSettings;
                        update.IsNotifyStopped = item.IsNotifyStopped;
                        update.Qq = item.Qq;
                        update.StripeSessionId = item.StripeSessionId;
                        update.Time = item.Time;
                        update.TimeCodeSent = item.TimeCodeSent;
                        update.Wechat = item.Wechat;
                        update.AlbumData = item.AlbumData;
                        update.ChatId = item.ChatId;
                        update.IsPageOnwer = item.IsPageOnwer;
                        update.MessageCount = item.MessageCount;
                        update.Selected = false;
                        update.Owner = Convert.ToBoolean(item.Owner);
                        update.LastMessageText = item.LastMessageText;
                        update.AppMainLater = item.AppMainLater;
                        update.ColorFollow = item.ColorFollow;
                        update.ColorGender = item.ColorGender;
                        update.IsOnline = item.IsOnline;
                        update.MediaIconImage = item.MediaIconImage;
                        update.LastMessageColor = item.LastMessageColor;
                        update.PageSubCategory = item.PageSubCategory;
                        update.SColorBackground = item.SColorBackground;
                        update.SColorForeground = item.SColorForeground;
                        update.SMessageFontWeight = item.SMessageFontWeight;
                        update.SubCategory = item.SubCategory;
                        update.TextColorFollowing = item.TextColorFollowing;
                        update.TextFollowing = item.TextFollowing;
                        update.UsernameTwoLetters = item.UsernameTwoLetters;
                        update.UsersPost = item.UsersPost;
                        update.Banned = item.Banned;
                        update.BannedReason = item.BannedReason;
                        update.CoinbaseCode = item.CoinbaseCode;
                        update.CoinbaseHash = item.CoinbaseHash;
                        update.ConversationId = item.ConversationId;
                        update.CurrentlyWorking = item.CurrentlyWorking;
                        update.IsOpenToWork = item.IsOpenToWork;
                        update.IsProvidingService = item.IsProvidingService;
                        update.Languages = item.Languages;
                        update.Okru = item.Okru;
                        update.OpenToWorkData = item.OpenToWorkData;
                        update.Permission = item.Permission;
                        update.ProvidingService = item.ProvidingService;
                        update.SAppMainLater = item.SAppMainLater;
                        update.SColorOnOff = item.SColorOnOff;
                        update.SecurionpayKey = item.SecurionpayKey;
                        update.Skills = item.Skills;
                        update.YoomoneyHash = item.YoomoneyHash;

                        update.OrderId = item.LastMessage.LastMessageClass.Time;

                        update.UserData = JsonConvert.SerializeObject(item.UserData);
                        update.LastMessage = JsonConvert.SerializeObject(item.LastMessage.LastMessageClass);
                        update.Parts = JsonConvert.SerializeObject(item.Parts);
                        update.Details = JsonConvert.SerializeObject(item.Details.DetailsClass);
                        update.ApiNotificationSettings = JsonConvert.SerializeObject(item.ApiNotificationSettings.NotificationSettingsClass);
                        update.Mute = JsonConvert.SerializeObject(item.Mute);

                        connection.Update(update);
                    }
                }

                switch (list.Count <= 0)
                {
                    case true:
                        return;
                }

                connection.BeginTransaction();
                //Bring new  
                var newItemList = list.Where(c => !result.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                switch (newItemList.Count > 0)
                {
                    case true:
                        connection.InsertAll(newItemList);
                        break;
                }

               // connection.UpdateAll(list);

                result = connection.Table<DataTables.LastUsersTb>().ToList();
                var deleteItemList = result.Where(c => !list.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                switch (deleteItemList.Count > 0)
                {
                    case true:
                        {
                            foreach (var delete in deleteItemList)
                                connection.Delete(delete);
                            break;
                        }
                }

                connection.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Update_LastUsersChat(lastUsersList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        // Get data To Last Users Chat
        public ObservableCollection<UserListFody> Get_LastUsersChat_List()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return null;
                var select = connection.Table<DataTables.LastUsersTb>().ToList();
                if (select.Count > 0)
                {
                    var list = new List<UserListFody>();

                    foreach (DataTables.LastUsersTb item in select)
                    {
                        //Classes.LastChatArchive archiveObject = null;
                        UserListFody db = new UserListFody
                        {
                            Id = item.Id,
                            AvatarOrg = item.AvatarOrg,
                            BackgroundImageStatus = item.BackgroundImageStatus,
                            Boosted = item.Boosted,
                            CallActionType = item.CallActionType,
                            CallActionTypeUrl = item.CallActionTypeUrl,
                            Category = item.Category,
                            ChatTime = item.ChatTime,
                            ChatType = item.ChatType,
                            Company = item.Company,
                            CoverFull = item.CoverFull,
                            CoverOrg = item.CoverOrg,
                            CssFile = item.CssFile,
                            EmailCode = item.EmailCode,
                            GroupId = item.GroupId,
                            Instgram = item.Instgram,
                            Joined = item.Joined,
                            LastEmailSent = item.LastEmailSent,
                            PageCategory = item.PageCategory,
                            PageDescription = item.PageDescription,
                            PageId = item.PageId,
                            PageName = item.PageName,
                            PageTitle = item.PageTitle,
                            Phone = item.Phone,
                            GroupName = item.GroupName,
                            ProTime = item.ProTime,
                            Rating = item.Rating,
                            Showlastseen = item.Showlastseen,
                            SidebarData = item.SidebarData,
                            SmsCode = item.SmsCode,
                            SocialLogin = item.SocialLogin,
                            Src = item.Src,
                            StartUp = item.StartUp,
                            StartupFollow = item.StartupFollow,
                            StartupImage = item.StartupImage,
                            StartUpInfo = item.StartUpInfo,
                            UserId = item.UserId,
                            Username = item.Username,
                            Email = item.Email,
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Avatar = item.Avatar,
                            Cover = item.Cover,
                            BackgroundImage = item.BackgroundImage,
                            RelationshipId = item.RelationshipId,
                            Address = item.Address,
                            Working = item.Working,
                            Gender = item.Gender,
                            Facebook = item.Facebook,
                            Google = item.Google,
                            Twitter = item.Twitter,
                            Linkedin = item.Linkedin,
                            Website = item.Website,
                            Instagram = item.Instagram,
                            WebDeviceId = item.WebDeviceId,
                            Language = item.Language,
                            IpAddress = item.IpAddress,
                            PhoneNumber = item.PhoneNumber,
                            Timezone = item.Timezone,
                            Lat = item.Lat,
                            Lng = item.Lng,
                            About = item.About,
                            Birthday = item.Birthday,
                            Registered = item.Registered,
                            Lastseen = item.Lastseen,
                            LastLocationUpdate = item.LastLocationUpdate,
                            Balance = item.Balance,
                            Verified = item.Verified,
                            Status = item.Status,
                            Active = item.Active,
                            Admin = item.Admin,
                            IsPro = item.IsPro,
                            ProType = item.ProType,
                            School = item.School,
                            Name = item.Name,
                            AndroidMDeviceId = item.AndroidMDeviceId,
                            ECommented = item.ECommented,
                            AndroidNDeviceId = item.AndroidMDeviceId,
                            AvatarFull = item.AvatarFull,
                            BirthPrivacy = item.BirthPrivacy,
                            CanFollow = item.CanFollow,
                            ConfirmFollowers = item.ConfirmFollowers,
                            CountryId = item.CountryId,
                            EAccepted = item.EAccepted,
                            EFollowed = item.EFollowed,
                            EJoinedGroup = item.EJoinedGroup,
                            ELastNotif = item.ELastNotif,
                            ELiked = item.ELiked,
                            ELikedPage = item.ELikedPage,
                            EMentioned = item.EMentioned,
                            EProfileWallPost = item.EProfileWallPost,
                            ESentmeMsg = item.ESentmeMsg,
                            EShared = item.EShared,
                            EVisited = item.EVisited,
                            EWondered = item.EWondered,
                            EmailNotification = item.EmailNotification,
                            FollowPrivacy = item.FollowPrivacy,
                            FriendPrivacy = item.FriendPrivacy,
                            GenderText = item.GenderText,
                            InfoFile = item.InfoFile,
                            IosMDeviceId = item.IosMDeviceId,
                            IosNDeviceId = item.IosNDeviceId,
                            IsBlocked = item.IsBlocked,
                            IsFollowing = item.IsFollowing,
                            IsFollowingMe = item.IsFollowingMe,
                            LastAvatarMod = item.LastAvatarMod,
                            LastCoverMod = item.LastCoverMod,
                            LastDataUpdate = item.LastDataUpdate,
                            LastFollowId = item.LastFollowId,
                            LastLoginData = item.LastLoginData,
                            LastseenStatus = item.LastseenStatus,
                            LastSeenTimeText = item.LastSeenTimeText,
                            LastseenUnixTime = item.LastseenUnixTime,
                            MessagePrivacy = item.MessagePrivacy,
                            NewEmail = item.NewEmail,
                            NewPhone = item.NewPhone,
                            NotificationsSound = item.NotificationsSound,
                            OrderPostsBy = item.OrderPostsBy,
                            PaypalEmail = item.PaypalEmail,
                            PostPrivacy = item.PostPrivacy,
                            Referrer = item.Referrer,
                            ShareMyData = item.ShareMyData,
                            ShareMyLocation = item.ShareMyLocation,
                            ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                            TwoFactor = item.TwoFactor,
                            TwoFactorVerified = item.TwoFactorVerified,
                            Url = item.Url,
                            VisitPrivacy = item.VisitPrivacy,
                            Vk = item.Vk,
                            Wallet = item.Wallet,
                            WorkingLink = item.WorkingLink,
                            Youtube = item.Youtube,
                            City = item.City,
                            State = item.State,
                            Zip = item.Zip,
                            Points = item.Points,
                            DailyPoints = item.DailyPoints,
                            PointDayExpire = item.PointDayExpire,
                            CashfreeSignature = item.CashfreeSignature,
                            IsAdmin = item.IsAdmin,
                            MemberId = item.MemberId,
                            ChatColor = item.ChatColor,
                            PaystackRef = item.PaystackRef,
                            RefUserId = item.RefUserId,
                            SchoolCompleted = item.SchoolCompleted,
                            Type = item.Type,
                            UserPlatform = item.UserPlatform,
                            WeatherUnit = item.WeatherUnit,
                            AvatarPostId = item.AvatarPostId,
                            CodeSent = item.CodeSent,
                            CoverPostId = item.CoverPostId,
                            Discord = item.Discord,
                            IsArchive = item.IsArchive,
                            IsMute = item.IsMute,
                            IsPin = item.IsPin,
                            IsReported = item.IsReported,
                            IsStoryMuted = item.IsStoryMuted,
                            Mailru = item.Mailru,
                            NotificationSettings = item.NotificationSettings,
                            IsNotifyStopped = item.IsNotifyStopped,
                            Qq = item.Qq,
                            StripeSessionId = item.StripeSessionId,
                            Time = item.Time,
                            TimeCodeSent = item.TimeCodeSent,
                            Wechat = item.Wechat,
                            AlbumData = item.AlbumData,
                            ChatId = item.ChatId,
                            IsPageOnwer = item.IsPageOnwer,
                            MessageCount = item.MessageCount,
                            Selected = false,
                            Owner = Convert.ToBoolean(item.Owner),
                            LastMessageText = item.LastMessageText,
                            AppMainLater = item.AppMainLater,
                            ColorFollow = item.ColorFollow,
                            ColorGender = item.ColorGender,
                            IsOnline = item.IsOnline,
                            MediaIconImage = item.MediaIconImage,
                            LastMessageColor = item.LastMessageColor,
                            PageSubCategory = item.PageSubCategory,
                            SColorBackground = item.SColorBackground,
                            SColorForeground = item.SColorForeground,
                            SMessageFontWeight = item.SMessageFontWeight,
                            SubCategory = item.SubCategory,
                            TextColorFollowing = item.TextColorFollowing,
                            TextFollowing = item.TextFollowing,
                            UsernameTwoLetters = item.UsernameTwoLetters,
                            UsersPost = item.UsersPost,
                            Banned = item.Banned,
                            BannedReason = item.BannedReason,
                            CoinbaseCode = item.CoinbaseCode,
                            CoinbaseHash = item.CoinbaseHash,
                            ConversationId = item.ConversationId,
                            CurrentlyWorking = item.CurrentlyWorking,
                            IsOpenToWork = item.IsOpenToWork,
                            IsProvidingService = item.IsProvidingService,
                            Languages = item.Languages,
                            Okru = item.Okru,
                            OpenToWorkData = item.OpenToWorkData,
                            Permission = item.Permission,
                            ProvidingService = item.ProvidingService,
                            SAppMainLater = item.SAppMainLater,
                            SColorOnOff = item.SColorOnOff,
                            SecurionpayKey = item.SecurionpayKey,
                            Skills = item.Skills,
                            YoomoneyHash = item.YoomoneyHash,

                            Details = new DetailsUnion(),
                            ApiNotificationSettings = new NotificationSettingsUnion(),
                            LastMessage = new LastMessageUnion
                            {
                                LastMessageClass = new MessageData()
                            },
                            Parts = new List<UserDataObject>(),
                            UserData = new UserDataObject(),
                            Mute = new Mute()
                        };

                        if (!string.IsNullOrEmpty(item.UserData))
                            db.UserData = JsonConvert.DeserializeObject<UserDataObject>(item.UserData);

                        if (!string.IsNullOrEmpty(item.LastMessage))
                        {
                            var sss = JsonConvert.DeserializeObject<MessageData>(item.LastMessage);
                            if (sss != null)
                            {
                                db.LastMessage = new LastMessageUnion
                                {
                                    LastMessageClass = sss
                                };
                            }
                        }

                        if (!string.IsNullOrEmpty(item.Parts))
                            db.Parts = JsonConvert.DeserializeObject<List<UserDataObject>>(item.Parts);

                        if (!string.IsNullOrEmpty(item.Mute))
                            db.Mute = JsonConvert.DeserializeObject<Mute>(item.Mute);

                        db = WoWonderTools.UserChatFilter(db);

                        if (!string.IsNullOrEmpty(item.Details))
                        {
                            var sss = JsonConvert.DeserializeObject<Details>(item.Details);
                            if (sss != null)
                            {
                                db.Details = new DetailsUnion
                                {
                                    DetailsClass = sss
                                };
                            }
                        }

                        if (!string.IsNullOrEmpty(item.ApiNotificationSettings))
                        {
                            var sss = JsonConvert.DeserializeObject<NotificationSettings>(item.ApiNotificationSettings);
                            if (sss != null)
                            {
                                db.ApiNotificationSettings = new NotificationSettingsUnion
                                {
                                    NotificationSettingsClass = sss
                                };
                            }
                        }
                        //Old
                        //if (db.IsPin)
                        //{
                        //    list.Insert(0, db);
                        //}
                        //else
                        //{
                        //    list.Add(db);
                        //}

                        list.Add(db);
                    }

                    return new ObservableCollection<UserListFody>(list);
                }
                else
                {
                    return new ObservableCollection<UserListFody>();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_LastUsersChat_List();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<UserListFody>();
                }
            } 
        }

        //Remove data from Users Table
        public void Delete_LastUsersChat(string userId)
        {
            try
            {
                using var connection = OpenConnection();
                var user = connection.Table<DataTables.LastUsersTb>().FirstOrDefault(c => c.UserId == userId);
                if (user != null)
                {
                    connection.Delete(user);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Delete_LastUsersChat(userId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Remove All data To Users Table
        public void ClearAll_LastUsersChat()
        {
            try
            {
                using var connection = OpenConnection();
                connection.DeleteAll<DataTables.LastUsersTb>();

                DeleteAllPin();
                DeleteAllMute();
                DeleteAllArchive();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    ClearAll_LastUsersChat();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Stickers

        //Insert data To Stickers Table
        public void Insert_To_StickersTable()
        {
            try
            {
                using var connection = OpenConnection();
                var data = connection.Table<DataTables.StickersTable>().ToList()?.Count;
                if (data == 0)
                {
                    var stickersList = new ObservableCollection<DataTables.StickersTable>();
                    DataTables.StickersTable s1 = new DataTables.StickersTable
                    {
                        PackageId = "1",
                        SName = "Rappit",
                        SVisibility = true,
                        SCount = StickersModel.Locally.StickerList1.Count.ToString()
                    };
                    stickersList.Add(s1);

                    DataTables.StickersTable s2 = new DataTables.StickersTable
                    {
                        PackageId = "2",
                        SName = "Water Drop",
                        SVisibility = true,
                        SCount = StickersModel.Locally.StickerList2.Count.ToString()
                    };
                    stickersList.Add(s2);

                    DataTables.StickersTable s3 = new DataTables.StickersTable
                    {
                        PackageId = "3",
                        SName = "Monster",
                        SVisibility = true,
                        SCount = StickersModel.Locally.StickerList3.Count.ToString()
                    };
                    stickersList.Add(s3);

                    DataTables.StickersTable s4 = new DataTables.StickersTable
                    {
                        PackageId = "4",
                        SName = "NINJA Nyankko",
                        SVisibility = false,
                        SCount = StickersModel.Locally.StickerList4.Count.ToString()
                    };
                    stickersList.Add(s4);

                    DataTables.StickersTable s5 = new DataTables.StickersTable
                    {
                        PackageId = "5",
                        SName = "So Much Love",
                        SVisibility = false,
                        SCount = StickersModel.Locally.StickerList5.Count.ToString()
                    };
                    stickersList.Add(s5);

                    DataTables.StickersTable s6 = new DataTables.StickersTable
                    {
                        PackageId = "6",
                        SName = "Sukkara chan",
                        SVisibility = false,
                        SCount = StickersModel.Locally.StickerList6.Count.ToString()
                    };  
                    stickersList.Add(s6);

                    DataTables.StickersTable s7 = new DataTables.StickersTable
                    {
                        PackageId = "7",
                        SName = "Flower Hijab",
                        SVisibility = false,
                        SCount = StickersModel.Locally.StickerList7.Count.ToString()
                    };
                    stickersList.Add(s7);

                    DataTables.StickersTable s8 = new DataTables.StickersTable
                    {
                        PackageId = "8",
                        SName = "Trendy boy",
                        SVisibility = false,
                        SCount = StickersModel.Locally.StickerList8.Count.ToString()
                    };
                    stickersList.Add(s8);

                    DataTables.StickersTable s9 = new DataTables.StickersTable
                    {
                        PackageId = "9",
                        SName = "The stickman",
                        SVisibility = false,
                        SCount = StickersModel.Locally.StickerList9.Count.ToString()
                    };
                    stickersList.Add(s9);

                    DataTables.StickersTable s10 = new DataTables.StickersTable
                    {
                        PackageId = "10",
                        SName = "Chip Dale Animated",
                        SVisibility = false,
                        SCount = StickersModel.Locally.StickerList10.Count.ToString()
                    };
                    stickersList.Add(s10);

                    DataTables.StickersTable s11 = new DataTables.StickersTable
                    {
                        PackageId = "11",
                        SName = Settings.ApplicationName + " Stickers",
                        SVisibility = false,
                        SCount = StickersModel.Locally.StickerList11.Count.ToString()
                    };
                    stickersList.Add(s11);

                    connection.InsertAll(stickersList);

                    ListUtils.StickersList = new ObservableCollection<DataTables.StickersTable>(stickersList);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("database is locked"))
                    Insert_To_StickersTable();
                else
                    Methods.DisplayReportResultTrack(ex);
            }
        }

        //Get  data To Stickers Table
        public ObservableCollection<DataTables.StickersTable> Get_From_StickersTable()
        {
            try
            {
                using var connection = OpenConnection();
                var stickersList = new ObservableCollection<DataTables.StickersTable>();
                var data = connection.Table<DataTables.StickersTable>().ToList();

                foreach (var s in data.Select(item => new DataTables.StickersTable
                {
                    PackageId = item.PackageId,
                    SName = item.SName,
                    SVisibility = item.SVisibility,
                    SCount = item.SCount
                }))
                {
                    stickersList.Add(s);
                }

                ListUtils.StickersList = new ObservableCollection<DataTables.StickersTable>(stickersList);

                return stickersList;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_From_StickersTable();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                }
            }
        }

        //Update data To Stickers Table
        public void Update_To_StickersTable(string typeName, bool visibility)
        {
            try
            {
                using var connection = OpenConnection();
                var data = connection.Table<DataTables.StickersTable>().FirstOrDefault(a => a.SName == typeName);
                if (data != null)
                {
                    data.SVisibility = visibility;
                }
                connection.Update(data);

                var data2 = ListUtils.StickersList.FirstOrDefault(a => a.SName == typeName);
                if (data2 != null)
                {
                    data2.SVisibility = visibility;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("database is locked"))
                    Update_To_StickersTable(typeName, visibility);
                else
                    Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion
         
        #region Call

        //Insert data To Call Video Table
        public void Insert_To_CallVideoTable(Classes.CallVideo data)
        {
            try
            {
                using var connection = OpenConnection();
                DataTables.CallVideoTable cv = new DataTables.CallVideoTable
                {
                    CallVideoUserId = data.CallVideoUserId,
                    CallVideoUserName = data.CallVideoUserName,
                    CallVideoAvatar = data.CallVideoAvatar,
                    CallVideoCallId = data.CallVideoCallId,
                    CallVideoTupeIcon = data.CallVideoTupeIcon,
                    CallVideoColorIcon = data.CallVideoColorIcon,
                    CallVideoUserDataTime = data.CallVideoUserDataTime,
                };

                connection.Insert(cv);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_To_CallVideoTable(data);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get  data To Call Video Table
        public ObservableCollection<Classes.CallVideo> get_data_CallVideo()
        {
            try
            {
                using var connection = OpenConnection();
                var listcCallVideos = new ObservableCollection<Classes.CallVideo>();
                var data = connection.Table<DataTables.CallVideoTable>().ToList();

                foreach (var cv in data.Select(item => new Classes.CallVideo
                {
                    CallVideoUserId = item.CallVideoUserId,
                    CallVideoUserName = item.CallVideoUserName,
                    CallVideoAvatar = item.CallVideoAvatar,
                    CallVideoCallId = item.CallVideoCallId,
                    CallVideoTupeIcon = item.CallVideoTupeIcon,
                    CallVideoColorIcon = item.CallVideoColorIcon,
                    CallVideoUserDataTime = item.CallVideoUserDataTime,
                    SColorBackground = UserDetails.ModeDarkStlye ? "#232323" : "#ffffff",
                    SColorForeground = UserDetails.ModeDarkStlye ? "#ffffff" : "#444444",
                }))
                {
                    listcCallVideos.Add(cv);
                }
                return listcCallVideos;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return get_data_CallVideo();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null;
                }
            }
        }

        //Remove data Call Video Table
        public void remove_data_CallVideo(string callId)
        {
            try
            {
                using var connection = OpenConnection();
                var callVideo = connection.Table<DataTables.CallVideoTable>().FirstOrDefault(a => a.CallVideoCallId == callId);
                if (callVideo != null)
                {
                    connection.Delete(callVideo);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    remove_data_CallVideo(callId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
          
        #region Mute

        public void InsertORDelete_Mute(Classes.OptionLastChat item)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                var result = connection.Table<DataTables.MuteTb>().ToList();
                var check = result.FirstOrDefault(a => a.ChatId == item.ChatId && a.ChatType == item.ChatType);
                if (check == null)
                {
                    DataTables.MuteTb cv = new DataTables.MuteTb
                    {
                        ChatType = item.ChatType,
                        ChatId = item.ChatId,
                        UserId = item.UserId,
                        GroupId = item.GroupId,
                        PageId = item.PageId,
                        Name = item.Name
                    };
                    connection.Insert(cv);
                }
                else
                {
                    connection.Delete(check);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertORDelete_Mute(item);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public ObservableCollection<Classes.OptionLastChat> Get_MuteList()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return new ObservableCollection<Classes.OptionLastChat>();
                var list = new ObservableCollection<Classes.OptionLastChat>();
                var result = connection.Table<DataTables.MuteTb>().ToList();
                if (result.Count <= 0) return new ObservableCollection<Classes.OptionLastChat>();
                foreach (var cv in result.Select(item => new Classes.OptionLastChat
                {
                    ChatType = item.ChatType,
                    ChatId = item.ChatId,
                    UserId = item.UserId,
                    GroupId = item.GroupId,
                    PageId = item.PageId,
                    Name = item.Name
                }))
                {
                    list.Add(cv);
                }

                return new ObservableCollection<Classes.OptionLastChat>(list);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_MuteList();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<Classes.OptionLastChat>();
                }
            }
        }

        public void DeleteAllMute()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                connection.DeleteAll<DataTables.MuteTb>();

                ListUtils.MuteList?.Clear();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteAllMute();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Pin

        public void InsertORDelete_Pin(Classes.LastChatArchive item)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                var result = connection.Table<DataTables.PinTb>().ToList();
                var check = result.FirstOrDefault(a => a.ChatId == item.ChatId && a.ChatType == item.ChatType);
                if (check == null)
                {
                    DataTables.PinTb cv = new DataTables.PinTb
                    {
                        ChatType = item.ChatType,
                        ChatId = item.ChatId,
                        UserId = item.UserId,
                        GroupId = item.GroupId,
                        PageId = item.PageId,
                        Name = item.Name,
                        IdLastMessage = item.IdLastMessage,
                        LastChat = JsonConvert.SerializeObject(item.LastChat),
                        LastChatPage = JsonConvert.SerializeObject(item.LastChatPage),
                    };
                    connection.Insert(cv);
                }
                else
                {
                    connection.Delete(check);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertORDelete_Pin(item);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public void InsertORUpdateORDelete_ListPin(List<Classes.LastChatArchive> lastChatPin)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var result = connection.Table<DataTables.PinTb>().ToList();
                List<DataTables.PinTb> list = new List<DataTables.PinTb>();

                connection.BeginTransaction();

                foreach (var item in lastChatPin)
                {
                    var db = new DataTables.PinTb
                    {
                        ChatType = item.ChatType,
                        ChatId = item.ChatId,
                        UserId = item.UserId,
                        GroupId = item.GroupId,
                        PageId = item.PageId,
                        Name = item.Name,
                        IdLastMessage = item.IdLastMessage,
                        LastChat = JsonConvert.SerializeObject(item.LastChat),
                        LastChatPage = JsonConvert.SerializeObject(item.LastChatPage),
                    };
                    list.Add(db);

                    var update = result.FirstOrDefault(a => a.ChatId == item.ChatId);
                    if (update != null)
                    {
                        update.ChatType = item.ChatType;
                        update.ChatId = item.ChatId;
                        update.UserId = item.UserId;
                        update.GroupId = item.GroupId;
                        update.PageId = item.PageId;
                        update.Name = item.Name;
                        update.IdLastMessage = item.IdLastMessage;
                        update.LastChat = JsonConvert.SerializeObject(item.LastChat);
                        update.LastChatPage = JsonConvert.SerializeObject(item.LastChatPage);

                        connection.Update(update);
                    }
                }

                switch (list.Count)
                {
                    case <= 0:
                        return;
                }

                //Bring new  
                var newItemList = list.Where(c => !result.Select(fc => fc.ChatId).Contains(c.ChatId)).ToList();
                switch (newItemList.Count)
                {
                    case > 0:
                        connection.InsertAll(newItemList);
                        break;
                }

                result = connection.Table<DataTables.PinTb>().ToList();
                var deleteItemList = result.Where(c => !list.Select(fc => fc.ChatId).Contains(c.ChatId)).ToList();
                switch (deleteItemList.Count)
                {
                    case > 0:
                        {
                            foreach (var delete in deleteItemList)
                                connection.Delete(delete);
                            break;
                        }
                }

                connection.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertORUpdateORDelete_ListArchive(lastChatPin);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public ObservableCollection<Classes.LastChatArchive> Get_PinList()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return new ObservableCollection<Classes.LastChatArchive>();
                var list = new ObservableCollection<Classes.LastChatArchive>();
                var result = connection.Table<DataTables.PinTb>().ToList();
                if (result.Count <= 0) return new ObservableCollection<Classes.LastChatArchive>();
                foreach (var cv in result.Select(item => new Classes.LastChatArchive
                {
                    ChatType = item.ChatType,
                    ChatId = item.ChatId,
                    UserId = item.UserId,
                    GroupId = item.GroupId,
                    PageId = item.PageId,
                    Name = item.Name,
                    IdLastMessage = item.IdLastMessage,
                    LastChat = !string.IsNullOrEmpty(item.LastChat) ? JsonConvert.DeserializeObject<ChatObject>(item.LastChat) : new ChatObject(),
                    LastChatPage = !string.IsNullOrEmpty(item.LastChatPage) ? JsonConvert.DeserializeObject<PageDataObject>(item.LastChatPage) : new PageDataObject(),
                }))
                {
                    list.Add(cv);
                }

                return new ObservableCollection<Classes.LastChatArchive>(list);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_PinList();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<Classes.LastChatArchive>();
                }
            }
        }

        public void DeleteAllPin()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                connection.DeleteAll<DataTables.PinTb>();

                ListUtils.PinList?.Clear();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteAllPin();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
        
        #region Archive

        public void InsertORDelete_Archive(Classes.LastChatArchive item)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                var result = connection.Table<DataTables.ArchiveTb>().ToList();
                var check = result.FirstOrDefault(a => a.ChatId == item.ChatId && a.ChatType == item.ChatType);
                if (check == null)
                {
                    DataTables.ArchiveTb cv = new DataTables.ArchiveTb
                    {
                        ChatType = item.ChatType,
                        ChatId = item.ChatId,
                        UserId = item.UserId,
                        GroupId = item.GroupId,
                        PageId = item.PageId,
                        Name = item.Name,
                        IdLastMessage = item.IdLastMessage,
                        LastChat = JsonConvert.SerializeObject(item.LastChat),
                        LastChatPage = JsonConvert.SerializeObject(item.LastChatPage),
                    };
                    connection.Insert(cv);
                }
                else
                {
                    connection.Delete(check);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertORDelete_Archive(item);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public void InsertORUpdateORDelete_ListArchive(List<Classes.LastChatArchive> lastChatArchive)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var result = connection.Table<DataTables.ArchiveTb>().ToList();
                List<DataTables.ArchiveTb> list = new List<DataTables.ArchiveTb>();

                connection.BeginTransaction();

                foreach (var item in lastChatArchive)
                {
                    var db = new DataTables.ArchiveTb
                    {
                        ChatType = item.ChatType,
                        ChatId = item.ChatId,
                        UserId = item.UserId,
                        GroupId = item.GroupId,
                        PageId = item.PageId,
                        Name = item.Name,
                        IdLastMessage = item.IdLastMessage,
                        LastChat = JsonConvert.SerializeObject(item.LastChat),
                        LastChatPage = JsonConvert.SerializeObject(item.LastChatPage),
                    };
                    list.Add(db);

                    var update = result.FirstOrDefault(a => a.ChatId == item.ChatId);
                    if (update != null)
                    {
                        update.ChatType = item.ChatType;
                        update.ChatId = item.ChatId;
                        update.UserId = item.UserId;
                        update.GroupId = item.GroupId;
                        update.PageId = item.PageId;
                        update.Name = item.Name;
                        update.IdLastMessage = item.IdLastMessage;
                        update.LastChat = JsonConvert.SerializeObject(item.LastChat);
                        update.LastChatPage = JsonConvert.SerializeObject(item.LastChatPage);

                        connection.Update(update);
                    }
                }

                switch (list.Count)
                {
                    case <= 0:
                        return;
                }

                //Bring new  
                var newItemList = list.Where(c => !result.Select(fc => fc.ChatId).Contains(c.ChatId)).ToList();
                switch (newItemList.Count)
                {
                    case > 0:
                        connection.InsertAll(newItemList);
                        break;
                }

                result = connection.Table<DataTables.ArchiveTb>().ToList();
                var deleteItemList = result.Where(c => !list.Select(fc => fc.ChatId).Contains(c.ChatId)).ToList();
                switch (deleteItemList.Count)
                {
                    case > 0:
                        {
                            foreach (var delete in deleteItemList)
                                connection.Delete(delete);
                            break;
                        }
                }

                connection.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertORUpdateORDelete_ListArchive(lastChatArchive);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public ObservableCollection<Classes.LastChatArchive> Get_ArchiveList()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return new ObservableCollection<Classes.LastChatArchive>();
                var list = new ObservableCollection<Classes.LastChatArchive>();
                var result = connection.Table<DataTables.ArchiveTb>().ToList();
                if (result.Count <= 0) return new ObservableCollection<Classes.LastChatArchive>();
                foreach (var cv in result.Select(item => new Classes.LastChatArchive
                {
                    ChatType = item.ChatType,
                    ChatId = item.ChatId,
                    UserId = item.UserId,
                    GroupId = item.GroupId,
                    PageId = item.PageId,
                    Name = item.Name,
                    IdLastMessage = item.IdLastMessage,
                    LastChat = !string.IsNullOrEmpty(item.LastChat) ? JsonConvert.DeserializeObject<ChatObject>(item.LastChat) : new ChatObject(),
                    LastChatPage = !string.IsNullOrEmpty(item.LastChatPage) ? JsonConvert.DeserializeObject<PageDataObject>(item.LastChatPage) : new PageDataObject(),
                }))
                {
                    list.Add(cv);
                }

                return new ObservableCollection<Classes.LastChatArchive>(list);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_ArchiveList();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<Classes.LastChatArchive>();
                }
            }
        }

        public void DeleteAllArchive()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                connection.DeleteAll<DataTables.ArchiveTb>();

                ListUtils.PinList?.Clear();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteAllArchive();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
        
    }
}