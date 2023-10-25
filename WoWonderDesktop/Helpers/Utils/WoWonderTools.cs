using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using GoogleMaps.LocationServices;
using WoWonderClient;
using WoWonderClient.Classes.Event;
using WoWonderClient.Classes.Global;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.language;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Story;
using WoWonderClient.Classes.Socket;

namespace WoWonderDesktop.Helpers.Utils
{
    public static class WoWonderTools
    {
        public static string GetNameFinal(UserDataObject dataUser)
        {
            try
            {
                if (!string.IsNullOrEmpty(dataUser?.Name) && !string.IsNullOrWhiteSpace(dataUser?.Name))
                    return Methods.FunString.DecodeString(dataUser.Name);

                return Methods.FunString.DecodeString(dataUser?.Username);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Methods.FunString.DecodeString(dataUser?.Username);
            }
        }

        public static string GetAboutFinal(UserDataObject dataUser)
        {
            try
            {
                if (!string.IsNullOrEmpty(dataUser?.About) && !string.IsNullOrWhiteSpace(dataUser?.About))
                    return Methods.FunString.DecodeString(dataUser.About);

                return LocalResources.label_ProfileUserAbout + " " + Settings.ApplicationName; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return LocalResources.label_ProfileUserAbout + " " + Settings.ApplicationName;
            }
        }

        public static bool GetStatusOnline(int lastSeen, string isShowOnline)
        {
            try
            {
                string time = Methods.Time.TimeAgo(lastSeen, false);
                bool status = /*isShowOnline == "on" &&*/ time == Methods.Time.LblJustNow;
                return status;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static (string, string) GetCurrency(string idCurrency)
        {
            try
            {
                switch (Settings.CurrencyStatic)
                {
                    case true:
                        return (Settings.CurrencyCodeStatic, Settings.CurrencyIconStatic);
                }

                string currency = Settings.CurrencyCodeStatic;
                bool success = int.TryParse(idCurrency, out var number);
                if (success)
                {
                    Console.WriteLine("Converted '{0}' to {1}.", idCurrency, number);
                    if (ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count > 0 && ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count > number)
                        currency = ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList[number] ?? Settings.CurrencyCodeStatic;
                    else if (ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.Count > 0)
                    {
                        currency = ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.FirstOrDefault(a => a.Key == number.ToString()).Value ?? Settings.CurrencyCodeStatic;
                    }
                }
                else
                {
                    Console.WriteLine("Attempted conversion of '{0}' failed.", idCurrency ?? "<null>");
                    currency = idCurrency;
                }

                if (ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList != null)
                {
                    string currencyIcon = ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.FirstOrDefault(a => a.Key == currency?.ToUpper()).Value ?? "";
                     
                    //switch (currency?.ToUpper())
                    //{
                    //    case "USD":
                    //        currencyIcon = !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Usd)
                    //                ? ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Usd ?? "$" : "$";
                    //        break;
                    //    case "Jpy":
                    //        currencyIcon = !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Jpy)
                    //                ? ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Jpy ?? "¥"
                    //                : "¥";
                    //        break;
                    //    case "EUR":
                    //        currencyIcon = !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Eur)
                    //                ? ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Eur ?? "€"
                    //                : "€";
                    //        break;
                    //    case "TRY":
                    //        currencyIcon = !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Try)
                    //                ? ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Try ?? "₺"
                    //                : "₺";
                    //        break;
                    //    case "GBP":
                    //        currencyIcon =
                    //            !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Gbp)
                    //                ? ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Gbp ?? "£"
                    //                : "£";
                    //        break;
                    //    case "RUB":
                    //        currencyIcon =
                    //            !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Rub)
                    //                ? ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Rub ?? "₽"
                    //                : "₽";
                    //        break;
                    //    case "PLN":
                    //        currencyIcon =
                    //            !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Pln)
                    //                ? ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Pln ?? "zł"
                    //                : "zł";
                    //        break;
                    //    case "ILS":
                    //        currencyIcon =
                    //            !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Ils)
                    //                ? ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Ils ?? "₪"
                    //                : "₪";
                    //        break;
                    //    case "BRL":
                    //        currencyIcon =
                    //            !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Brl)
                    //                ? ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Brl ?? "R$"
                    //                : "R$";
                    //        break;
                    //    case "INR":
                    //        currencyIcon =
                    //            !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Inr)
                    //                ? ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Inr ?? "₹"
                    //                : "₹";
                    //        break;
                    //    default:
                    //        currencyIcon =
                    //            !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Usd)
                    //                ? ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.Usd ?? "$"
                    //                : "$";
                    //        break;
                    //}

                    return (currency, currencyIcon);
                }

                return (Settings.CurrencyCodeStatic, Settings.CurrencyIconStatic);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return (Settings.CurrencyCodeStatic, Settings.CurrencyIconStatic);
            }
        }

        public static string GetRelationship(int index)
        {
            try
            {
                var relationshipLocal = new List<string> { "None", "Single", "In a relationship", "Married", "Engaged" };

                switch (index)
                {
                    case > -1:
                        {
                            string name = relationshipLocal[index];
                            return name;
                        }
                    default:
                        return relationshipLocal?.First() ?? "";
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }

        public static MessageModelType GetTypeModel(MessageData item)
        {
            try
            {
                MessageModelType modelType;

                if (item.FromId == UserDetails.UserId) // right
                {
                    item.Position = "right";
                }
                else if (item.ToId == UserDetails.UserId) // left
                {
                    item.Position = "left";
                }

                string imageUrl = "", text = "";
                if (!string.IsNullOrEmpty(item.Stickers))
                {
                    item.Stickers = item.Stickers.Replace(".mp4", ".gif");
                    imageUrl = item.Stickers;
                }

                if (!string.IsNullOrEmpty(item.Media))
                    imageUrl = item.Media;

                if (!string.IsNullOrEmpty(item.Text))
                    text = ChatUtils.GetMessage(item.Text, item.Time);

                if (!string.IsNullOrEmpty(text))
                    modelType = item.TypeTwo == "contact" ? item.Position == "left" ? MessageModelType.LeftContact : MessageModelType.RightContact : item.Position == "left" ? MessageModelType.LeftText : MessageModelType.RightText;
                else if (item.Product?.ProductClass != null && !string.IsNullOrEmpty(item.ProductId) && item.ProductId != "0")
                    modelType = item.Position == "left" ? MessageModelType.LeftProduct : MessageModelType.RightProduct;
                else if (!string.IsNullOrEmpty(item.Lat) && !string.IsNullOrEmpty(item.Lng) && item.Lat != "0" && item.Lng != "0")
                    modelType = item.Position == "left" ? MessageModelType.LeftMap : MessageModelType.RightMap;
                else if (!string.IsNullOrEmpty(imageUrl))
                {
                    var type = Methods.Check_FileExtension(imageUrl);
                    switch (type)
                    {
                        case "Audio":
                            modelType = item.Position == "left" ? MessageModelType.LeftAudio : MessageModelType.RightAudio;
                            break;
                        case "Video":
                            modelType = item.Position == "left" ? MessageModelType.LeftVideo : MessageModelType.RightVideo;
                            break;
                        case "Image" when !string.IsNullOrEmpty(item.Media) && !item.Media.Contains(".gif"):
                            modelType = item.Media.Contains("sticker") ? item.Position == "left" ? MessageModelType.LeftSticker : MessageModelType.RightSticker : item.Position == "left" ? MessageModelType.LeftImage : MessageModelType.RightImage;
                            break;
                        case "File" when !string.IsNullOrEmpty(item.Stickers) && item.Stickers.Contains(".gif"):
                        case "File" when !string.IsNullOrEmpty(item.Media) && item.Media.Contains(".gif"):
                        case "Image" when !string.IsNullOrEmpty(item.Stickers) && item.Stickers.Contains(".gif"):
                        case "Image" when !string.IsNullOrEmpty(item.Media) && item.Media.Contains(".gif"):
                            modelType = item.Position == "left" ? MessageModelType.LeftGif : MessageModelType.RightGif;
                            break;
                        case "File":
                            modelType = item.Position == "left" ? MessageModelType.LeftFile : MessageModelType.RightFile;
                            break;
                        default:
                            modelType = MessageModelType.None;
                            break;
                    }
                }
                else
                {
                    modelType = MessageModelType.None;
                }

                return modelType;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return MessageModelType.None;
            }
        }

        public static MessageModelType GetTypeModel(PrivateMessageObject item)
        {
            try
            {
                MessageModelType modelType;

                if (item.Sender == UserDetails.UserId) // right
                {
                    item.Position = "right";
                }
                else if (item.Receiver == UserDetails.UserId) // left
                {
                    item.Position = "left";
                }

                string text = "";

                if (!string.IsNullOrEmpty(item.Message))
                    text = ChatUtils.GetMessage(item.Message, item.Time);

                if (!string.IsNullOrEmpty(text))
                    modelType = item.Position == "left" ? MessageModelType.LeftText : MessageModelType.RightText;

                else if (!string.IsNullOrEmpty(item.Lat) && !string.IsNullOrEmpty(item.Lng) && item.Lat != "0" && item.Lng != "0")
                    modelType = item.Position == "left" ? MessageModelType.LeftMap : MessageModelType.RightMap;

                //if (item == "contact")
                //    modelType = item.Position == "left" ? MessageModelType.LeftContact : MessageModelType.RightContact;

                else if (!string.IsNullOrEmpty(item.MediaLink))
                {
                    var typeFile = Methods.Check_FileExtension(item.MediaLink);
                    switch (typeFile)
                    {
                        case "Audio":
                            modelType = item.Position == "left" ? MessageModelType.LeftAudio : MessageModelType.RightAudio;
                            break;
                        case "Video":
                            modelType = item.Position == "left" ? MessageModelType.LeftVideo : MessageModelType.RightVideo;
                            break;
                        case "Image" when !string.IsNullOrEmpty(item.MediaLink) && !item.MediaLink.Contains(".gif") && !item.MessagesHtml.Contains(".gif"):
                            modelType = item.MediaLink.Contains("sticker") ? item.Position == "left" ? MessageModelType.LeftSticker : MessageModelType.RightSticker : item.Position == "left" ? MessageModelType.LeftImage : MessageModelType.RightImage;
                            break;
                        case "Image" when !string.IsNullOrEmpty(item.MediaLink) && item.MediaLink.Contains(".gif") || item.MessagesHtml.Contains(".gif"):
                            modelType = item.Position == "left" ? MessageModelType.LeftGif : MessageModelType.RightGif;
                            break;
                        case "File":
                            modelType = item.Position == "left" ? MessageModelType.LeftFile : MessageModelType.RightFile;
                            break;
                        default:
                            modelType = MessageModelType.None;
                            break;
                    }
                }
                else
                {
                    modelType = MessageModelType.None;
                }

                return modelType;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return MessageModelType.None;
            }
        }

        public static MessagesDataFody MessageFilter(string userid, string username, MessageData item, string chatColor, string typeChat = "user")
        {
            try
            {
                item.Media ??= "";
                item.Stickers ??= "";

                item.Text ??= "";

                item.Stickers = item.Stickers.Replace(".mp4", ".gif");

                if (!string.IsNullOrEmpty(item.Text) && !string.IsNullOrWhiteSpace(item.Text))
                    item.Text = ChatUtils.GetMessage(item.Text, item.Time);

                bool success = int.TryParse(item.Time, out var number);
                item.TimeText = success ? Methods.Time.TimeAgo(number) : item.Time;
              
                Visibility seenVisibility = Visibility.Collapsed;
                item.UserName = username;
                if (item.FromId == UserDetails.UserId) // right
                {
                    item.Position = "right";
                    item.UserName = GetNameFinal(ListUtils.MyProfileList.FirstOrDefault());
                     
                    if (item.Seen == "-1")
                    {
                        item.IconSeen = "ClockTimeFourOutline";
                        seenVisibility = Visibility.Visible;
                    }
                    else if (item.Seen == "0")
                    {
                        item.IconSeen = "Check"; 
                        seenVisibility = Visibility.Visible;
                    }
                    else if (item.Seen != "-1" && item.Seen != "0" && !string.IsNullOrEmpty(item.Seen))
                    {
                        item.IconSeen = "CheckAll";
                        seenVisibility = Visibility.Visible;
                    } 
                }
                else if (item.ToId == UserDetails.UserId) // left
                {
                    item.UserName = username;
                    item.Position = "left"; 
                    seenVisibility = Visibility.Collapsed;
                }
                 
                string colorBoxMessage = "", typeIconFile = "";
                Visibility playVisibility = Visibility.Collapsed;
                Visibility pauseVisibility = Visibility.Collapsed;
                Visibility progresSVisibility = Visibility.Collapsed;
                Visibility downloadVisibility = Visibility.Visible;
                Visibility iconFileVisibility = Visibility.Collapsed;
                Visibility hlinkDownloadVisibility = Visibility.Visible;
                Visibility hlinkOpenVisibility = Visibility.Collapsed;
                Visibility saveImgVisibility = Visibility.Visible;

                var modelType = GetTypeModel(item);
                item.ModelType = modelType;
                 
                switch (modelType)
                {
                    case MessageModelType.RightFile:
                    case MessageModelType.RightSticker:
                    case MessageModelType.RightVideo:
                    case MessageModelType.RightContact:
                    case MessageModelType.RightAudio:
                    case MessageModelType.RightMap:
                    case MessageModelType.RightImage:
                    case MessageModelType.RightText:
                    case MessageModelType.RightGif:
                    case MessageModelType.RightProduct:
                        colorBoxMessage = chatColor;
                        break;
                }

                switch (modelType)
                {
                    case MessageModelType.LeftProduct:
                    case MessageModelType.RightProduct:
                        {
                            if (item.Product?.ProductClass != null)
                            {
                                string imageUrl = item.Product.Value.ProductClass.Images[0]?.Image ?? "";
                                var fileName = imageUrl.Split('/').Last();
                                item.Media = Methods.MultiMedia.GetFile(userid, fileName, "images", imageUrl);

                                item.P_Name = Methods.FunString.DecodeString(item.Product.Value.ProductClass.Name);

                                item.P_Category = ListUtils.ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == item.Product.Value.ProductClass.Category)?.CategoriesName;

                                var (currency, currencyIcon) = GetCurrency(item.Product.Value.ProductClass.Currency);
                                item.P_PriceWithCurrency = currencyIcon + " " + item.Product.Value.ProductClass.Price;
                                Console.WriteLine(currency);
                            }

                            break;
                        }
                    case MessageModelType.LeftGif:
                    case MessageModelType.RightGif:
                        {
                            //https://media1.giphy.com/media/l0ExncehJzexFpRHq/200.gif?cid=b4114d905d3e926949704872410ec12a&rid=200.gif   
                            //https://media3.giphy.com/media/LSKVlAGSnuXxVdp5wN/200.gif?cid=b4114d90pvb2jy1t65c2dap0se0uc7qef6atvtsxom4cmoi2&rid=200.gif&ct=g
                            string imageUrl = "";
                            if (!string.IsNullOrEmpty(item.Stickers))
                                imageUrl = item.Stickers;
                            else if (!string.IsNullOrEmpty(item.Media))
                                imageUrl = item.Media;
                            else if (!string.IsNullOrEmpty(item.MediaFileName))
                                imageUrl = item.MediaFileName;

                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                string[] fileName = imageUrl.Split(new[] { "/", "200.gif?cid=", "&rid=200", "&rid=200.gif&ct=g" }, StringSplitOptions.RemoveEmptyEntries);
                                var lastFileName = fileName.Last();
                                var name = fileName[3] + lastFileName;

                                item.Media = Methods.MultiMedia.GetFile(userid, name, "images", imageUrl);
                            }
                            break;
                        }
                    case MessageModelType.LeftText:
                    case MessageModelType.RightText:

                        if (!string.IsNullOrEmpty(item.Text) && !string.IsNullOrWhiteSpace(item.Text))
                            item.Text = Methods.FunString.DecodeString(item.Text);

                        //var dbt = ClassMapper.Mapper?.Map<MessagesDataFody>(item);
                        //return dbt;
                        break;
                    case MessageModelType.LeftMap:
                    case MessageModelType.RightMap:
                        {
                            try
                            {
                                var locationService = new GoogleLocationService(Settings.GoogleMapsKey);
                                var addressData = locationService.GetAddressFromLatLang(Convert.ToDouble(item.Lat), Convert.ToDouble(item.Lng));
                                if (addressData != null)
                                {
                                    string imageUrlMap = "https://maps.googleapis.com/maps/api/staticmap?";
                                    if (!string.IsNullOrEmpty(addressData.Address) && !string.IsNullOrWhiteSpace(addressData.Address))
                                    {
                                        imageUrlMap += "center=" + addressData.Address;
                                        imageUrlMap += "&zoom=13";
                                        imageUrlMap += "&scale=2";
                                        imageUrlMap += "&size=150x150";
                                        imageUrlMap += "&maptype=roadmap";
                                        imageUrlMap += "&key=" + Settings.GoogleMapsKey;
                                        imageUrlMap += "&format=png";
                                        imageUrlMap += "&visual_refresh=true";
                                        imageUrlMap += "&markers=size:small|color:0xff0000|label:1|" + addressData.Address;

                                        item.Media = imageUrlMap;
                                    }
                                    else
                                        item.Media = @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Image_Map.png";

                                    saveImgVisibility = Visibility.Collapsed;
                                }
                            }
                            catch
                            {
                                item.Media = @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Image_Map.png";
                                saveImgVisibility = Visibility.Collapsed;
                            }
                        }
                        break;
                    case MessageModelType.LeftImage:
                    case MessageModelType.RightImage:
                        {
                            var fileName = item.Media.Split('/').Last();
                            item.Media = Methods.MultiMedia.GetFile(userid, fileName, "images", item.Media);
                            break;
                        }
                    case MessageModelType.LeftAudio:
                    case MessageModelType.RightAudio:
                        {
                            var fileName = item.Media.Split('/').Last();
                            item.Media = Methods.MultiMedia.GetFile(userid, fileName, "sound", item.Media);

                            playVisibility = Visibility.Visible;
                            pauseVisibility = Visibility.Collapsed;
                            progresSVisibility = Visibility.Collapsed;
                            downloadVisibility = Visibility.Collapsed;
                            break;
                        }
                    case MessageModelType.LeftContact:
                    case MessageModelType.RightContact:
                        {
                            if (item.Text.Contains("{&quot;Key&quot;") || item.Text.Contains("{key:") || item.Text.Contains("{key:^qu") || item.Text.Contains("{^key:^qu") || item.Text.Contains("{Key:") || item.Text.Contains("&quot;"))
                            {
                                string[] stringSeparators = { "," };
                                var name = item.Text.Split(stringSeparators, StringSplitOptions.None);
                                var stringName = Methods.FunString.DecodeString(name[0]).Replace("{key:", "").Replace("{Key:", "").Replace("Value:", "").Replace("{", "").Replace("}", "");
                                var stringNumber = Methods.FunString.DecodeString(name[1]).Replace("{key:", "").Replace("{Key:", "").Replace("Value:", "").Replace("{", "").Replace("}", "");
                                item.ContactName = stringName;
                                item.ContactNumber = stringNumber;
                                item.Text = stringName + "\r\n" + stringNumber;

                            }
                            break;
                        }
                    case MessageModelType.LeftVideo:
                    case MessageModelType.RightVideo:
                        {
                            var dbt = ClassMapper.Mapper?.Map<MessagesDataFody>(item);
                            item.MediaFileName = Methods.MultiMedia.Rename_Video(item.MediaFileName);
                            item.ImageVideo = Methods.MultiMedia.Get_ThumbnailVideo_messages(item.Media, userid, dbt);

                            playVisibility = Visibility.Visible;
                            progresSVisibility = Visibility.Collapsed;
                            downloadVisibility = Visibility.Collapsed;
                            break;
                        }
                    case MessageModelType.LeftSticker:
                    case MessageModelType.RightSticker:
                        {
                            var dbt = ClassMapper.Mapper?.Map<MessagesDataFody>(item);
                            var fileName = item.Media.Split('/').Last();
                            item.Media = Methods.MultiMedia.Get_Sticker_messages(fileName, item.Media, dbt);
                            break;
                        }
                    case MessageModelType.LeftFile:
                    case MessageModelType.RightFile:
                        {
                            var fileName = item.Media.Split('/').Last();
                            item.Media = Methods.MultiMedia.GetFile(userid, fileName, "file", item.Media);

                            typeIconFile = GetIconFile(item.Media);  

                            switch (item.MediaFileName.Length > 25)
                            {
                                case true:
                                    item.MediaFileName = Methods.FunString.SubStringCutOf(item.MediaFileName, 20) + "." + item.MediaFileName.Split('.').Last();
                                    break;
                            }

                            if (File.Exists(item.Media))
                            {
                                long size = new FileInfo(item.Media).Length;
                                double totalSize = size / 1024.0F / 1024.0F;
                                item.FileSize = totalSize.ToString("0.### KB");
                            }
                           
                            iconFileVisibility = Visibility.Visible;
                            progresSVisibility = Visibility.Collapsed;
                            downloadVisibility = Visibility.Collapsed;
                            hlinkDownloadVisibility = Visibility.Collapsed;
                            hlinkOpenVisibility = Visibility.Visible;
                            break;
                        }
                }

                var db = ClassMapper.Mapper?.Map<MessagesDataFody>(item);
                if (db.Reply?.ReplyClass == null)
                {
                    db.Reply = new ReplyUnion
                    {
                        ReplyClass = new MessageData()
                    };
                }

                if (db.Story?.StoryClass == null)
                {
                    db.Story = new StoryUnion
                    {
                        StoryClass = new StoryDataObject.Story()
                    };
                }

                if (db.Reply?.ReplyClass?.Id != null && !string.IsNullOrEmpty(db.ReplyId) && db.ReplyId != "0")
                {
                    var type = GetTypeModel(item);
                    if (type != MessageModelType.None)
                    {
                        var msgReply = MessageFilter(userid, username, db.Reply?.ReplyClass, chatColor, typeChat);
                        msgReply = ReplyItems(msgReply.ModelType, msgReply, userid, username);

                        db.Reply = new ReplyUnion
                        {
                            ReplyClass = msgReply
                        };

                        db.ReplyPanel_Visibility = msgReply.ReplyPanel_Visibility;
                        db.Reply_TxtOwnerName = msgReply.Reply_TxtOwnerName;
                        db.Reply_TxtMessageType = msgReply.Reply_TxtMessageType;
                        db.Reply_TxtShortMessage = msgReply.Reply_TxtShortMessage;
                        db.Reply_MessageFileThumbnail = msgReply.Reply_MessageFileThumbnail;
                        db.Reply_MessageFileThumbnail_Visibility = msgReply.Reply_MessageFileThumbnail_Visibility;
                        db.Reply_TxtMessageType_Visibility = msgReply.Reply_TxtMessageType_Visibility;
                        db.Reply_TxtShortMessage_Visibility = msgReply.Reply_TxtShortMessage_Visibility;
                    }
                }
                else
                {
                    if (db.Story?.StoryClass?.Id != null && !string.IsNullOrEmpty(db.StoryId) && db.StoryId != "0")
                    {
                        db = ReplyStoryItems(db, db.Story?.StoryClass);
                    }
                    else
                    {
                        db.ReplyPanel_Visibility = Visibility.Collapsed;
                    }
                }

                if (ListUtils.StartedMessageList?.Count > 0 && Settings.EnableFavoriteMessageSystem)
                {
                    var cec = ListUtils.StartedMessageList?.FirstOrDefault(a => a.Id == item.Id);
                    if (cec?.Fav == "yes")
                    {
                        item.Fav = "yes";
                    }
                }

                if (db.Forward != null && db.Forward.Value == 1)
                {
                    db.Forwarded_Visibility = Visibility.Visible;
                }
                else
                {
                    db.Forwarded_Visibility = Visibility.Collapsed;
                }

                //style
                db.ChatColor = colorBoxMessage;
                db.EndChatColor = Settings.ColorMessageThemeGradient ? GetColorEnd(colorBoxMessage) : colorBoxMessage;
                if (db.UserData != null)
                    db.Img_user_message = Methods.MultiMedia.Get_image(userid, db.UserData.Avatar.Split('/').Last(), db.UserData.Avatar);
                else if (db.MessageUser?.UserDataClass != null)
                    db.Img_user_message = Methods.MultiMedia.Get_image(userid, db.MessageUser?.UserDataClass.Avatar.Split('/').Last(), db.MessageUser?.UserDataClass.Avatar);
                 
                db.ProgresSVisibility = progresSVisibility;
                db.Download_Visibility = downloadVisibility;
                db.SeenVisibility = seenVisibility;
                db.Play_Visibility = playVisibility;
                db.Pause_Visibility = pauseVisibility;
                db.Icon_File_Visibility = iconFileVisibility;
                db.Hlink_Download_Visibility = hlinkDownloadVisibility;
                db.Hlink_Open_Visibility = hlinkOpenVisibility;
                db.Type_Icon_File = typeIconFile;
                db.sound_slider_value = 0;
                db.Progress_Value = 0;
                db.sound_time = "";
                db.Save_img_Visibility = saveImgVisibility;
              
                if (typeChat == "user")
                    db.FavoriteVisibility = db.Fav == "yes" ? Visibility.Visible : Visibility.Collapsed;
                else
                    db.FavoriteVisibility =  Visibility.Collapsed; 

                db.ReactionMessageItems = new ObservableCollection<ReactFody>()
                {
                    new ReactFody()
                    {
                        React_Image =  @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_like.gif",
                        React_Type = "like",
                        React_Name = LocalResources.label5_Like,
                    },
                    new ReactFody()
                    {
                        React_Image =  @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_love.gif",
                        React_Type = "love",
                        React_Name = LocalResources.label5_Love,
                    },
                    new ReactFody()
                    {
                        React_Image =  @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_haha.gif",
                        React_Type = "haha",
                        React_Name = LocalResources.label5_HaHa,
                    },
                    new ReactFody()
                    {
                        React_Image =  @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_wow.gif",
                        React_Type = "wow",
                        React_Name = LocalResources.label5_Wow,
                    },
                    new ReactFody()
                    {
                        React_Image =  @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_sad.gif",
                        React_Type = "sad",
                        React_Name = LocalResources.label5_Sad,
                    },
                    new ReactFody()
                    {
                        React_Image =  @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_angry.gif",
                        React_Type = "angry",
                        React_Name = LocalResources.label5_Angry,
                    },
                };

                if (db.Reaction != null)
                {
                    if (db.Reaction.IsReacted != null && db.Reaction.IsReacted.Value)
                    {
                        if (!string.IsNullOrEmpty(db.Reaction.Type))
                        {
                            Application.Current.Dispatcher?.Invoke(delegate // <--- HERE
                            {
                                try
                                {
                                    var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == db.Reaction.Type).Value?.Id ?? "";
                                    switch (react)
                                    {
                                        case "1":
                                            db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_like.gif"));
                                            break;
                                        case "2":
                                            db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_love.gif"));
                                            break;
                                        case "3":
                                            db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_haha.gif"));
                                            break;
                                        case "4":
                                            db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_wow.gif"));
                                            break;
                                        case "5":
                                            db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_sad.gif"));
                                            break;
                                        case "6":
                                            db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_angry.gif"));
                                            break;
                                        default:
                                            if (db.Reaction.Count > 0)
                                                db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_like.gif"));
                                            break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });

                            db.ReactionVisibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        if (db.Reaction.Count > 0)
                        {
                            if (db.Reaction.Like != null)
                            {
                                db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_like.gif"));
                            }
                            else if (db.Reaction.Love != null)
                            {
                                db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_love.gif"));
                            }
                            else if (db.Reaction.HaHa != null)
                            {
                                db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_haha.gif"));
                            }
                            else if (db.Reaction.Wow != null)
                            {
                                db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_wow.gif"));
                            }
                            else if (db.Reaction.Sad != null)
                            {
                                db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_sad.gif"));
                            }
                            else if (db.Reaction.Angry != null)
                            {
                                db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_angry.gif"));
                            }
                            else
                            {
                                db.ReactionImage = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Reaction/gif_like.gif"));
                            }
                            db.ReactionVisibility = Visibility.Visible;
                        }
                        else
                        {
                            db.ReactionVisibility = Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    db.ReactionVisibility = Visibility.Collapsed;
                }

                db.MenuMessageItems = new ObservableCollection<string>();
                if (db.Position == "right")
                {
                    db.MenuMessageItems.Add(LocalResources.label5_MessageInfo);
                    db.MenuMessageItems.Add(LocalResources.label5_DeleteMessage);
                }

                if (Settings.EnableReplyMessageSystem)
                {
                    db.MenuMessageItems.Add(LocalResources.label5_Reply);
                }

                if (Settings.EnableForwardMessageSystem)
                {
                    db.MenuMessageItems.Add(LocalResources.label5_Forward);
                }

                if (Settings.EnablePinMessageSystem && typeChat == "user")
                {
                    db.MenuMessageItems.Add(db.Pin == "no" ? LocalResources.label5_Pin : LocalResources.label5_UnPin);
                }

                if (Settings.EnableFavoriteMessageSystem && typeChat == "user")
                {
                    db.MenuMessageItems.Add(db.Fav == "yes" ? LocalResources.label5_UnFavorite : LocalResources.label5_Favorite);
                }

                return db;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                var db = ClassMapper.Mapper?.Map<MessagesDataFody>(item);
                return db;
            }
        }

        //Reply Messages
        public static MessagesDataFody ReplyItems(MessageModelType typeView, MessagesDataFody message, string userid, string name)
        {
            try
            {
                if (!string.IsNullOrEmpty(message?.Id.ToString()))
                {
                    message.ReplyPanel_Visibility = Visibility.Visible;
                    message.Reply_TxtOwnerName = message.MessageUser?.UserDataClass?.UserId == UserDetails.UserId ? "You" : name;

                    message.Reply_TxtShortMessage_Visibility = Visibility.Visible;

                    if (typeView is MessageModelType.LeftText or MessageModelType.RightText)
                    {
                        message.Reply_MessageFileThumbnail_Visibility = Visibility.Collapsed;
                        message.Reply_TxtMessageType_Visibility = Visibility.Collapsed;
                        message.Reply_TxtShortMessage = message.Text;
                    }
                    else
                    {
                        message.Reply_MessageFileThumbnail_Visibility = Visibility.Visible;
                        var fileName = message.Media.Split('/').Last();
                        switch (typeView)
                        {
                            case MessageModelType.LeftVideo:
                            case MessageModelType.RightVideo:
                                {
                                    message.Reply_TxtMessageType_Visibility = Visibility.Collapsed;
                                    message.Reply_TxtShortMessage = LocalResources.label_cuont_video;

                                    if (string.IsNullOrEmpty(message.ImageVideo))
                                    {
                                        message.Reply_MessageFileThumbnail = message.ImageVideo;
                                    }
                                    else
                                    {
                                        message.Reply_MessageFileThumbnail = Methods.MultiMedia.Get_ThumbnailVideo_messages(message.Media, userid, message);
                                    }

                                    break;
                                }
                            case MessageModelType.LeftGif:
                            case MessageModelType.RightGif:
                                {
                                    message.Reply_TxtMessageType_Visibility = Visibility.Collapsed;
                                    message.Reply_TxtShortMessage = LocalResources.label_gift;

                                    message.Reply_MessageFileThumbnail = Methods.MultiMedia.GetFile(userid, fileName, "images", message.Media);

                                    break;
                                }
                            case MessageModelType.LeftSticker:
                            case MessageModelType.RightSticker:
                                {
                                    message.Reply_TxtMessageType_Visibility = Visibility.Collapsed;
                                    message.Reply_TxtShortMessage = LocalResources.label5_Sticker;

                                    message.Reply_MessageFileThumbnail = Methods.MultiMedia.Get_Sticker_messages(fileName, message.Media, message);

                                    break;
                                }
                            case MessageModelType.LeftImage:
                            case MessageModelType.RightImage:
                                {
                                    message.Reply_TxtMessageType_Visibility = Visibility.Collapsed;
                                    message.Reply_TxtShortMessage = LocalResources.label_cuont_images;

                                    message.Reply_MessageFileThumbnail = Methods.MultiMedia.GetFile(userid, fileName, "images", message.Media);
                                    break;
                                }
                            case MessageModelType.LeftAudio:
                            case MessageModelType.RightAudio:
                                {
                                    message.Reply_TxtMessageType_Visibility = Visibility.Collapsed;
                                    message.Reply_TxtShortMessage = LocalResources.label_cuont_sounds;

                                    message.Reply_MessageFileThumbnail = @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Audio_File.png";
                                    break;
                                }
                            case MessageModelType.LeftFile:
                            case MessageModelType.RightFile:
                                {
                                    //message.Reply_TxtShortMessage_Visibility = Visibility.Collapsed;
                                    message.Reply_TxtMessageType = LocalResources.label_cuont_file;

                                    var fileNameWithoutExtension = fileName.Split('.').First();
                                    var fileNameExtension = fileName.Split('.').Last();

                                    message.Reply_TxtShortMessage = Methods.FunString.SubStringCutOf(fileNameWithoutExtension, 10) + fileNameExtension;
                                    message.Reply_MessageFileThumbnail = @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Image_File.png";

                                    break;
                                }
                            case MessageModelType.LeftMap:
                            case MessageModelType.RightMap:
                                {
                                    message.Reply_TxtShortMessage_Visibility = Visibility.Collapsed;
                                    message.Reply_TxtMessageType = LocalResources.label_location;
                                    message.Reply_MessageFileThumbnail = @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Image_Map.png";
                                    break;
                                }
                            case MessageModelType.LeftContact:
                            case MessageModelType.RightContact:
                                {
                                    message.Reply_TxtMessageType = LocalResources.label5_Contact;
                                    message.Reply_TxtShortMessage = message.ContactName;
                                    message.Reply_MessageFileThumbnail = @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/no_profile_image_circle.png";

                                    break;
                                }
                            case MessageModelType.LeftProduct:
                            case MessageModelType.RightProduct:
                                {
                                    message.Reply_TxtMessageType_Visibility = Visibility.Collapsed;
                                    message.Reply_TxtShortMessage = LocalResources.label5_Product;
                                    string imageUrl = !string.IsNullOrEmpty(message.Media) ? message.Media : message.Product?.ProductClass?.Images?.FirstOrDefault()?.Image;

                                    message.Reply_MessageFileThumbnail = Methods.MultiMedia.GetFile(userid, fileName, "images", imageUrl);

                                    break;
                                }
                            case MessageModelType.LeftText:
                            case MessageModelType.RightText:
                            case MessageModelType.None:
                                break;
                        }
                    }
                }
                return message;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return message;
            }
        }

        //Reply Story Messages
        public static MessagesDataFody ReplyStoryItems(MessagesDataFody message, StoryDataObject.Story story)
        {
            try
            {
                if (!string.IsNullOrEmpty(story?.Id))
                {
                    message.ReplyPanel_Visibility = Visibility.Visible;
                    message.Reply_TxtOwnerName = LocalResources.label5_Story;

                    message.Reply_MessageFileThumbnail_Visibility = Visibility.Visible;

                    var mediaFile = !story.Thumbnail.Contains("avatar") && story.Videos.Count == 0 ? story.Thumbnail : story.Videos[0].Filename;
                    var fileName = mediaFile.Split('/').Last();

                    var typeView = Methods.Check_FileExtension(mediaFile);
                    switch (typeView)
                    {
                        case "Video":
                            {
                                message.Reply_TxtMessageType_Visibility = Visibility.Collapsed;
                                message.Reply_TxtShortMessage = LocalResources.label_cuont_video;

                                message.Reply_MessageFileThumbnail = Methods.MultiMedia.Get_ThumbnailVideo_messages(mediaFile, story.UserId, message);

                                break;
                            }
                        case "Image":
                            {
                                message.Reply_TxtMessageType_Visibility = Visibility.Collapsed;
                                message.Reply_TxtShortMessage = LocalResources.label_cuont_images;

                                message.Reply_MessageFileThumbnail = Methods.MultiMedia.GetFile(story.UserId, fileName, "images", mediaFile);
                                break;
                            }
                    }
                }
                else
                {
                    message.ReplyPanel_Visibility = Visibility.Collapsed;
                }
                return message;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return message;
            }
        }

        public static string GetColorEnd(string color)
        {
            try
            {
                if (color == null)
                    return Settings.MainColor;

                if (color != null && color.Contains("b582af"))
                {
                    return "#db69ce";
                }
                else if (color.Contains("a52729"))
                {
                    return "#e85657";
                }
                else if (color.Contains("f9c270"))
                {
                    return "#fab44b";
                }
                else if (color.Contains("70a0e0"))
                {
                    return "#5794e6";
                }
                else if (color.Contains("56c4c5"))
                {
                    return "#31a7a8";
                }
                else if (color.Contains("f33d4c"))
                {
                    return "#bf0010";
                }
                else if (color.Contains("a1ce79"))
                {
                    return "#7eba49";
                }
                else if (color.Contains("a085e2"))
                {
                    return "#652af5";
                }
                else if (color.Contains("ed9e6a"))
                {
                    return "#f78439";
                }
                else if (color.Contains("2b87ce"))
                {
                    return "#34a2f7";
                }
                else if (color.Contains("f2812b"))
                {
                    return "#fa750f";
                }
                else if (color.Contains("0ba05d"))
                {
                    return "#25db89";
                }
                else if (color.Contains("0e71ea"))
                {
                    return "#2065ba";
                }
                else if (color.Contains("aa2294"))
                {
                    return "#f03ad2";
                }
                else if (color.Contains("f9a722"))
                {
                    return "#bf8424";
                }
                else if (color.Contains("008484"))
                {
                    return "#2fbdbd";
                }
                else if (color.Contains("5462a5"))
                {
                    return "#002cff";
                }
                else if (color.Contains("fc9cde"))
                {
                    return "#e058b6";
                }
                else if (color.Contains("51bcbc"))
                {
                    return "#018f8f";
                }
                else if (color.Contains("c9605e"))
                {
                    return "#fa6e6b";
                }
                else if (color.Contains("01a5a5"))
                {
                    return "#4fdbdb";
                }
                else if (color.Contains("056bba"))
                {
                    return "#3994db";
                }
                else
                {
                    //Default Color >> AppSettings.MainColor
                    return Settings.MainColor;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Settings.MainColor;
            }
        }

        //Get Shared Files Profile user in Right Side "images, Media, file"
        public static async Task GetSharedFiles(string userId)
        {
            try
            {
                if (ListUtils.ListSharedFiles.Count > 0)
                    ListUtils.ListSharedFiles.Clear();

                var imagePath = Methods.Path.FolderDiskMyApp + "\\" + userId + "\\Images\\";
                var videoPath = Methods.Path.FolderDiskMyApp + "\\" + userId + "\\Video\\";
                var soundsPath = Methods.Path.FolderDiskMyApp + "\\" + userId + "\\Sound\\";
                var otherPath = Methods.Path.FolderDiskMyApp + "\\" + userId + "\\File\\";
                var gifPath = Methods.Path.FolderDiskMyApp + "\\" + userId + "\\Gif\\";
                var stickerPath = Methods.Path.FolderDiskMyApp + "\\" + userId + "\\Sticker\\";

                //Check for folder if exists
                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);

                if (!Directory.Exists(stickerPath))
                    Directory.CreateDirectory(stickerPath);

                if (!Directory.Exists(gifPath))
                    Directory.CreateDirectory(gifPath);

                if (!Directory.Exists(soundsPath))
                    Directory.CreateDirectory(soundsPath);

                if (!Directory.Exists(videoPath))
                    Directory.CreateDirectory(videoPath);

                if (!Directory.Exists(otherPath))
                    Directory.CreateDirectory(otherPath);

                var imageFiles = new DirectoryInfo(imagePath).GetFiles().OrderByDescending(f => f.LastWriteTime).ToList();
                var stickerFiles = new DirectoryInfo(stickerPath).GetFiles().OrderByDescending(f => f.LastWriteTime).ToList();
                var gifFiles = new DirectoryInfo(gifPath).GetFiles().OrderByDescending(f => f.LastWriteTime).ToList();
                var soundsFiles = new DirectoryInfo(soundsPath).GetFiles().OrderByDescending(f => f.LastWriteTime).ToList();
                var videoFiles = new DirectoryInfo(videoPath).GetFiles().OrderByDescending(f => f.LastWriteTime).ToList();
                var otherFiles = new DirectoryInfo(otherPath).GetFiles().OrderByDescending(f => f.LastWriteTime).ToList();

                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (imageFiles.Count > 0)
                            foreach (var dir in imageFiles)
                            {
                                var dir1 = dir;
                                var check = ListUtils.ListSharedFiles.FirstOrDefault(a => a.FileName == dir1.Name);
                                if (check == null)
                                {
                                    ListUtils.ListSharedFiles.Add(new Classes.SharedFile
                                    {
                                        FileType = "Image",
                                        FileName = Methods.FunString.SubStringCutOf(dir.Name, 20),
                                        FileDate = dir.LastWriteTime.Millisecond.ToString(),
                                        FilePath = dir.FullName,
                                        ImageUrl = GetIconFile(dir.FullName),
                                        FileExtension = dir.Extension,
                                        EmptyLabelVisibility = Visibility.Collapsed,
                                        ImageFrameVisibility = Visibility.Visible,
                                        VoiceFrameVisibility = Visibility.Collapsed,
                                        VideoFrameVisibility = Visibility.Collapsed,
                                        FileFrameVisibility = Visibility.Collapsed
                                    });
                                }
                            }

                        if (stickerFiles.Count > 0)
                            foreach (var file in stickerFiles)
                            {
                                var file1 = file;
                                var check = ListUtils.ListSharedFiles.FirstOrDefault(a => a.FileName == file1.Name);
                                if (check == null)
                                {
                                    ListUtils.ListSharedFiles.Add(new Classes.SharedFile
                                    {
                                        FileType = "Sticker",
                                        FileName = Methods.FunString.SubStringCutOf(file.Name, 20),
                                        FileDate = file.LastWriteTime.Millisecond.ToString(),
                                        FilePath = file.FullName,
                                        ImageUrl = @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/File/File_sticker.png",
                                        FileExtension = file.Extension,
                                        EmptyLabelVisibility = Visibility.Collapsed,
                                        ImageFrameVisibility = Visibility.Collapsed,
                                        VoiceFrameVisibility = Visibility.Collapsed,
                                        VideoFrameVisibility = Visibility.Collapsed,
                                        FileFrameVisibility = Visibility.Visible
                                    });
                                }
                            }

                        if (gifFiles.Count > 0)
                            foreach (var file in gifFiles)
                            {
                                var file1 = file;
                                var check = ListUtils.ListSharedFiles.FirstOrDefault(a => a.FileName == file1.Name);
                                if (check == null)
                                {
                                    ListUtils.ListSharedFiles.Add(new Classes.SharedFile
                                    {
                                        FileType = "Gif",
                                        FileName = Methods.FunString.SubStringCutOf(file.Name, 20),
                                        FileDate = file.LastWriteTime.Millisecond.ToString(),
                                        FilePath = file.FullName,
                                        ImageUrl = GetIconFile(file.FullName),
                                        FileExtension = file.Extension,
                                        EmptyLabelVisibility = Visibility.Collapsed,
                                        ImageFrameVisibility = Visibility.Collapsed,
                                        VoiceFrameVisibility = Visibility.Collapsed,
                                        VideoFrameVisibility = Visibility.Collapsed,
                                        FileFrameVisibility = Visibility.Visible
                                    });
                                }
                            }

                        if (soundsFiles.Count > 0)
                            foreach (var dir in soundsFiles)
                            {
                                var dir1 = dir;
                                var check = ListUtils.ListSharedFiles.FirstOrDefault(a => a.FileName == dir1.Name);
                                if (check == null)
                                {
                                    ListUtils.ListSharedFiles.Add(new Classes.SharedFile
                                    {
                                        FileType = "Media",
                                        FileName = Methods.FunString.SubStringCutOf(dir.Name, 20),
                                        FilePath = dir.FullName,
                                        FileExtension = dir.Extension,
                                        ImageUrl = GetIconFile(dir.FullName),
                                        FileDate = dir.LastWriteTime.Millisecond.ToString(),
                                        EmptyLabelVisibility = Visibility.Collapsed,
                                        ImageFrameVisibility = Visibility.Collapsed,
                                        VoiceFrameVisibility = Visibility.Visible,
                                        VideoFrameVisibility = Visibility.Collapsed,
                                        FileFrameVisibility = Visibility.Collapsed
                                    });
                                }
                            }

                        if (videoFiles.Count > 0)
                            foreach (var dir in videoFiles)
                            {
                                var dir1 = dir;
                                var check = ListUtils.ListSharedFiles.FirstOrDefault(a => a.FileName == dir1.Name);
                                if (check == null)
                                {
                                    ListUtils.ListSharedFiles.Add(new Classes.SharedFile
                                    {
                                        FileType = "Media",
                                        FileName = Methods.FunString.SubStringCutOf(dir.Name, 20),
                                        FilePath = dir.FullName,
                                        FileExtension = dir.Extension,
                                        ImageUrl = GetIconFile(dir.FullName),
                                        FileDate = dir.LastWriteTime.Millisecond.ToString(),
                                        EmptyLabelVisibility = Visibility.Collapsed,
                                        ImageFrameVisibility = Visibility.Collapsed,
                                        VoiceFrameVisibility = Visibility.Collapsed,
                                        VideoFrameVisibility = Visibility.Visible,
                                        FileFrameVisibility = Visibility.Collapsed
                                    });
                                }
                            }

                        if (otherFiles.Count > 0)
                            foreach (var dir in otherFiles)
                            {
                                var dir1 = dir;
                                var check = ListUtils.ListSharedFiles.FirstOrDefault(a => a.FileName == dir1.Name);
                                if (check == null)
                                {
                                    ListUtils.ListSharedFiles.Add(new Classes.SharedFile
                                    {
                                        FileType = "File",
                                        FileName = Methods.FunString.SubStringCutOf(dir.Name, 20),
                                        FilePath = dir.FullName,
                                        FileExtension = dir.Extension,
                                        ImageUrl = GetIconFile(dir.FullName),
                                        FileDate = dir.LastWriteTime.Millisecond.ToString(),
                                        EmptyLabelVisibility = Visibility.Collapsed,
                                        ImageFrameVisibility = Visibility.Collapsed,
                                        VoiceFrameVisibility = Visibility.Collapsed,
                                        VideoFrameVisibility = Visibility.Collapsed,
                                        FileFrameVisibility = Visibility.Visible
                                    });
                                }
                            }

                        if (ListUtils.ListSharedFiles.Count > 0)
                        {
                            //Last 50 File
                            List<Classes.SharedFile> orderByDateList = ListUtils.ListSharedFiles.OrderBy(T => T.FileName).Take(8).ToList();
                            ListUtils.LastSharedFiles = new ObservableCollection<Classes.SharedFile>(orderByDateList);
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static string GetIconFile(string mediaFile)
        {
            try
            {
                var type = Methods.Check_FileExtension(mediaFile);
                var extension = mediaFile.Split('.').LastOrDefault();

                if (type == "Audio")
                {
                    return @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/File/File_music.png";
                }
                else if (type == "Video")
                {
                    return @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/File/File_video.png";
                }
                else if (type == "Image")
                {
                    if (extension.Contains("gif") || extension.Contains("GIF"))
                    {
                        return @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/File/File_gif.png";
                    }
                    else  
                    {
                        return @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/File/File_image.png";
                    } 
                }
                else if (type == "File")
                {
                    if (extension.Contains("rar") || extension.Contains("RAR") || extension.Contains("zip") || extension.Contains("ZIP"))
                    {
                        return @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/File/File_zip.png";
                    }
                    else if (extension.Contains("txt") || extension.Contains("TXT"))
                    {
                        return @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/File/File_txt.png";
                    }
                    else if (extension.Contains("docx") || extension.Contains("DOCX") || extension.Contains("doc") || extension.Contains("DOC"))
                    {
                        return @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/File/File_doc.png";
                    }
                    else if (extension.Contains("apk") || extension.Contains("APK"))
                    {
                        return @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/File/File_apk.png";
                    }
                    else if (extension.Contains("pdf") || extension.Contains("PDF"))
                    {
                        return @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/File/File_pdf.png";
                    }
                    else
                    {
                        return @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/File/File_text.png";
                    }
                }
                else
                {
                    return @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/File/File_text.png";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/File/File_text.png";
            }
        }

        public static bool CheckMute(string id, string type, Mute mute)
        {
            try
            {
                if (mute?.Notify == "yes")
                {
                    return true;
                }

                var check = ListUtils.MuteList?.FirstOrDefault(a => a.ChatId == id && a.ChatType == type);
                return check != null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static bool CheckPin(string id, string type, Mute mute)
        {
            try
            {
                if (mute?.Pin == "yes")
                {
                    return true;
                }

                var check = ListUtils.PinList?.FirstOrDefault(a => a.ChatId == id && a.ChatType == type);
                return check != null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static bool ChatIsAllowed(UserDataObject dataObject)
        {
            try
            {
                if (dataObject.MessagePrivacy == "2") //No_body
                {
                    MessageBox.Show(LocalResources.label5_ChatNotAllowed);
                    return false;
                }
                else if (dataObject.MessagePrivacy == "1") //People_i_Follow
                {
                    if (dataObject.IsFollowing == "1")
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show(LocalResources.label5_ChatNotAllowed);
                        return false;
                    }
                }
                else if (dataObject.MessagePrivacy == "0") //Everyone
                {
                    return true;
                }
                 
                return true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return true;

            }
        }

        public static (Classes.LastChatArchive, bool) CheckArchive(string id, string type, Mute mute)
        {
            try
            {
                Classes.LastChatArchive check = ListUtils.ArchiveList?.FirstOrDefault(a => a.ChatId == id && a.ChatType == type) ?? null;

                if (mute?.Archive == "yes")
                {
                    return (check, true);
                }

                return (check, check != null);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return (null, false);
            }
        }

        public static UserListFody UserChatFilter(ChatObject data)
        {
            try
            {
                Visibility isVerified = Visibility.Visible;
                string messageFontWeight = "Light";
                string messageColor = "#7E7E7E";
                
                Visibility chekIcon = Visibility.Collapsed;
                Visibility chatColorCircleVisibility = Visibility.Collapsed;
                string mediaIconImage = "Image";
                Visibility mediaIconVisibility = Visibility.Collapsed;
                 
                data.IsOnline = GetStatusOnline(Convert.ToInt32(data.LastseenUnixTime), data.LastseenStatus);
                data.About = GetAboutFinal(data);
                
                switch (data.Verified)
                {
                    case "0":
                        isVerified = Visibility.Hidden;
                        break;
                }

                var image = data.Avatar?.Split('/').Last();
                if (image != null && !image.Contains("d-avatar"))
                {
                    data.Avatar = Methods.MultiMedia.Get_image(data.UserId, image, data.Avatar);
                }

                switch (data.ChatType)
                {
                    case "user":
                        data.Name = GetNameFinal(data);
                        break;
                    case "page":
                        var userAdminPage = data.UserId;
                        if (userAdminPage == data.LastMessage.LastMessageClass?.ToData.UserId)
                        {
                            //var userId = LastMessage.UserData.UserId;
                            var name = data.LastMessage.LastMessageClass?.UserData.Name + " (" + data.PageName + ")";
                            data.PageName = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(name), 30);
                        }
                        else
                        {
                            //var userId = LastMessage.ToData.UserId;
                            var name = data.LastMessage.LastMessageClass?.ToData.Name + " (" + data.PageName + ")";
                            data.PageName = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(name), 30);
                        }

                        data.PageName = GetNameFinal(data);
                        break;
                    case "group":
                        data.GroupName = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(data.GroupName), 30);
                        break;
                }

                data.IsMute = CheckMute(data.ChatId, data.ChatType, data.Mute);
                data.IsPin = CheckPin(data.ChatId, data.ChatType, data.Mute);
                data.IsArchive = CheckArchive(data.ChatId, data.ChatType, data.Mute).Item2;

                bool success = int.TryParse(!string.IsNullOrEmpty(data.ChatTime) ? data.ChatTime : data.Time, out var number);
                if (success)
                {
                    data.LastSeenTimeText = Methods.Time.TimeAgo(number, true);
                }
                else
                {
                    data.LastSeenTimeText = Methods.Time.ReplaceTime(!string.IsNullOrEmpty(data.ChatTime) ? data.ChatTime : data.Time);
                }
                 
                if (data.LastMessage.LastMessageClass == null)
                    data.LastMessage = new LastMessageUnion
                    {
                        LastMessageClass = new MessageData()
                    };

                if (!string.IsNullOrEmpty(data.LastMessage.LastMessageClass.Id.ToString()))
                {
                    data.LastMessage.LastMessageClass.Media ??= "";
                    data.LastMessage.LastMessageClass.Stickers ??= "";
                    data.LastMessage.LastMessageClass.Text ??= "";

                    data.LastMessage.LastMessageClass.Stickers = data.LastMessage.LastMessageClass.Stickers != null ? data.LastMessage.LastMessageClass.Stickers.Replace(".mp4", ".gif") : "";

                    data.LastMessage.LastMessageClass.Seen = data.LastMessage.LastMessageClass.Seen;

                    switch (data.LastMessage.LastMessageClass.Seen)
                    {
                        case "0":
                            messageFontWeight = "SemiBold";
                            messageColor = Settings.MainColor;
                            break;
                    }

                    if (data.ChatColor != null && data.ChatColor.Contains("rgb"))
                    {
                        try
                        {
                            Regex regex = new Regex(@"([0-9]+)");
                            string colorData = data.ChatColor;

                            MatchCollection matches = regex.Matches(colorData);

                            int colorR = Convert.ToInt32(matches[0].ToString());
                            int colorG = Convert.ToInt32(matches[1].ToString());
                            int colorB = Convert.ToInt32(matches[2].ToString());

                            data.ChatColor = data.LastMessageColor = Methods.FunString.HexFromRgb(colorR, colorG, colorB);
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                            data.ChatColor = Settings.MainColor;
                        }
                    }
                    else
                    {
                        data.ChatColor = data.LastMessageColor = data.ChatColor ?? Settings.MainColor;
                    }

                    //data.ChatColor = data.LastMessage.LastMessageClass.ChatColor = data.LastMessage.LastMessageClass?.ChatColor ?? Settings.MainColor;

                    if (!string.IsNullOrEmpty(data.LastMessage.LastMessageClass.Text))
                        data.LastMessageText = data.LastMessage.LastMessageClass.Text = ChatUtils.GetMessage(data.LastMessage.LastMessageClass.Text, data.LastMessage.LastMessageClass.Time);

                    if (!string.IsNullOrEmpty(data.LastMessage.LastMessageClass.Text))
                        data.LastMessageText = data.LastMessage.LastMessageClass.Text = Methods.FunString.DecodeString(data.LastMessage.LastMessageClass.Text);

                    switch (string.IsNullOrEmpty(data.LastMessage.LastMessageClass.Media))
                    {
                        //If message contains Media files 
                        case false when data.LastMessage.LastMessageClass.Media.Contains("image"):
                            mediaIconImage = "Image";
                            mediaIconVisibility = Visibility.Visible;
                            data.LastMessageText = data.LastMessage.LastMessageClass.Text = LocalResources.label_Send_you_an_image_file;
                            break;
                        case false when data.LastMessage.LastMessageClass.Media.Contains("video"):
                            mediaIconImage = "Beats";
                            mediaIconVisibility = Visibility.Visible;
                            data.LastMessageText = data.LastMessage.LastMessageClass.Text = LocalResources.label_Send_you_an_video_file;
                            break;
                        case false when data.LastMessage.LastMessageClass.Media.Contains("sticker"):
                            mediaIconImage = "Face";
                            mediaIconVisibility = Visibility.Visible;
                            data.LastMessageText = data.LastMessage.LastMessageClass.Text = LocalResources.label_Send_you_an_sticker_file;
                            break;
                        case false when data.LastMessage.LastMessageClass.Media.Contains("sounds"):
                            mediaIconImage = "MusicCircle";
                            mediaIconVisibility = Visibility.Visible;
                            data.LastMessageText = data.LastMessage.LastMessageClass.Text = LocalResources.label_Send_you_an_audio_file;
                            break;
                        case false when data.LastMessage.LastMessageClass.Media.Contains("file"):
                            mediaIconImage = "FileChart";
                            mediaIconVisibility = Visibility.Visible;
                            data.LastMessageText = data.LastMessage.LastMessageClass.Text = LocalResources.label_Send_you_an_file;
                            break;
                        default:
                            {
                                if (!string.IsNullOrEmpty(data.LastMessage.LastMessageClass.Stickers) && data.LastMessage.LastMessageClass.Stickers.Contains(".gif"))
                                {
                                    data.LastMessageText = data.LastMessage.LastMessageClass.Text = LocalResources.label_Send_you_an_gif_image;
                                    chatColorCircleVisibility = Visibility.Collapsed;
                                }
                                else if (!string.IsNullOrEmpty(data.LastMessage.LastMessageClass.ProductId) && data.LastMessage.LastMessageClass.ProductId != "0")
                                {
                                    data.LastMessageText = data.LastMessage.LastMessageClass.Text = LocalResources.label5_Product;
                                    chatColorCircleVisibility = Visibility.Collapsed;
                                }
                                else if (!string.IsNullOrEmpty(data.LastMessage.LastMessageClass.Lat) && !string.IsNullOrEmpty(data.LastMessage.LastMessageClass.Lng) && data.LastMessage.LastMessageClass.Lat != "0" && data.LastMessage.LastMessageClass.Lng != "0")
                                {
                                    mediaIconImage = "GoogleMaps";
                                    mediaIconVisibility = Visibility.Visible;
                                    data.LastMessageText = data.LastMessage.LastMessageClass.Text = LocalResources.label5_Send_you_an_location;

                                }
                                else
                                {
                                    switch (string.IsNullOrEmpty(data.LastMessage.LastMessageClass.Text))
                                    {
                                        //if (!string.IsNullOrEmpty(LastMessage.Text) && LastMessage.Text.Contains("http"))
                                        //{
                                        //    data.LastMessage.LastMessageClass.Text = Methods.FunString.SubStringCutOf(LastMessage.Text, 30);
                                        //}
                                        //else
                                        case false:
                                            {
                                                if (data.LastMessage.LastMessageClass.TypeTwo == "contact" || data.LastMessage.LastMessageClass.Text.Contains("{&quot;Key&quot;") || data.LastMessage.LastMessageClass.Text.Contains("{key:") || data.LastMessage.LastMessageClass.Text.Contains("{key:^qu") ||
                                                    data.LastMessage.LastMessageClass.Text.Contains("{^key:^qu") || data.LastMessage.LastMessageClass.Text.Contains("{Key:") || data.LastMessage.LastMessageClass.Text.Contains("&quot;"))
                                                {
                                                    mediaIconImage = "Contact";
                                                    mediaIconVisibility = Visibility.Visible;
                                                    data.LastMessageText = data.LastMessage.LastMessageClass.Text = LocalResources.label5_Send_you_an_contact;
                                                }
                                                else
                                                {
                                                    data.LastMessageText = data.LastMessage.LastMessageClass.Text = Methods.FunString.DecodeString(data.LastMessage.LastMessageClass.Text);
                                                    chatColorCircleVisibility = Visibility.Collapsed;
                                                }
                                                break;
                                            }
                                        default:
                                            messageColor = data.ChatColor;
                                            data.LastMessageText = data.LastMessage.LastMessageClass.Text = LocalResources.label_Changed_his_chat_color;
                                            chatColorCircleVisibility = Visibility.Visible;
                                            break;
                                    }
                                }

                                break;
                            }
                    }

                    if (data.LastMessage.LastMessageClass.FromId == UserDetails.UserId)
                    {
                        if (data.LastMessage.LastMessageClass.Seen != "0")
                        {
                            chekIcon = Visibility.Visible;
                        }
                    }
                }

                data.LastMessageText = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(data.LastMessageText, 25));

                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        data.SColorBackground = "#232323";
                        data.SColorForeground = "#efefef";
                        break;
                    default:
                        data.SColorBackground = "#ffffff";
                        data.SColorForeground = "#444444";
                        break;
                }

                var db = ClassMapper.Mapper?.Map<UserListFody>(data);

                db.MenuChatItems = new ObservableCollection<string>();

                if (Settings.EnableChatArchive)
                    db.MenuChatItems.Add(db.IsArchive ? LocalResources.label5_UnArchive : LocalResources.label5_Archive);

                db.MenuChatItems.Add(LocalResources.label_Btn_delete_chat);

                if (Settings.EnableChatPin)
                    db.MenuChatItems.Add(db.IsPin ? LocalResources.label5_UnPin : LocalResources.label5_Pin);

                if (Settings.EnableChatMute)
                    db.MenuChatItems.Add(db.IsMute ? LocalResources.label5_UnMuteNotification : LocalResources.label5_MuteNotification);

                db.MenuChatItems.Add(LocalResources.label_Btn_Block_User);

                if (Settings.VideoCall)
                {
                    var dataSettings = ListUtils.SettingsSiteList;
                    if (dataSettings?.WhoCall == "pro") //just pro user can chat 
                    {
                        var dataUser = ListUtils.MyProfileList?.FirstOrDefault()?.IsPro;
                        if (dataUser == "0") // Not Pro remove call
                        {

                        }
                        else
                        {
                            db.MenuChatItems.Add(LocalResources.label_Video_call);
                        }
                    }
                    else //all users can chat
                    {
                        if (dataSettings?.VideoChat == "0" || !Settings.VideoCall)
                        {
                            //VideoCallButton.Visibility = ViewStates.Gone;
                            Settings.VideoCall = false;
                        }
                        else
                        {
                            db.MenuChatItems.Add(LocalResources.label_Video_call);
                        }
                    }
                }

                db.MenuChatItems.Add(LocalResources.label_ProfileViewLinkButton);

                db.UVerified = isVerified;
                db.SMessageFontWeight = messageFontWeight;
                db.LastMessageColor = messageColor;
                db.MediaIconVisibility = mediaIconVisibility;
                db.IsSeenIconCheck = chekIcon;
                db.IsPinIconVisibility = db.IsPin ? Visibility.Visible : Visibility.Collapsed;
                db.IsMuteIconVisibility = db.IsMute ? Visibility.Visible : Visibility.Collapsed;
                db.ChatColorCircleVisibility = chatColorCircleVisibility;
                db.MediaIconImage = mediaIconImage;
                db.MediaIconVisibility = mediaIconVisibility;
                db.MessageCountVisibility = string.IsNullOrEmpty(db.MessageCount) || db.MessageCount == "0" ? Visibility.Collapsed : Visibility.Visible;
                db.UsernameTwoLetters = Methods.FunString.GetoLettersfromString(Methods.FunString.SubStringCutOf(data.Name, 15));

                return db;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                var db = ClassMapper.Mapper?.Map<UserListFody>(data);
                return db;
            }
        }
        
        public static UserDataFody UserFilter(UserDataObject data)
        {
            try
            { 
                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        data.SColorBackground = "#232323";
                        data.SColorForeground = "#efefef";
                        break;
                    default:
                        data.SColorBackground = "#ffffff";
                        data.SColorForeground = "#444444";
                        break;
                }

                string avatarSplit = data.Avatar?.Split('/').Last();

                data.IsOnline = GetStatusOnline(Convert.ToInt32(data.LastseenUnixTime), data.LastseenStatus);
                data.Avatar = Methods.MultiMedia.Get_image(data.UserId, avatarSplit, data.Avatar);
                data.Name = Methods.FunString.SubStringCutOf(GetNameFinal(data), 30);
                data.About = GetAboutFinal(data);
                
                Visibility isVerified = Visibility.Visible;
                switch (data.Verified)
                {
                    case "0":
                        isVerified = Visibility.Hidden;
                        break;
                }
               
                switch (data.IsFollowing)
                {
                    //>> Not Friend
                    case "0":
                        data.TextColorFollowing = Settings.MainColor;
                        data.TextFollowing = Settings.ConnectivitySystem == 1 ? LocalResources.label5_Follow : LocalResources.label5_AddFriends;
                        data.ColorFollow = "#efefef";
                        break;
                    //>> Friend
                    case "1":
                        data.TextColorFollowing = "#efefef";
                        data.TextFollowing = Settings.ConnectivitySystem == 1 ? LocalResources.label5_Following : LocalResources.label5_Friends;
                        data.ColorFollow = (Settings.MainColor);
                        break;
                    //>> Request
                    case "2":
                        data.TextColorFollowing = Settings.MainColor;
                        data.TextFollowing = LocalResources.label5_Request;
                        data.ColorFollow = "#efefef";
                        break;
                    default:
                        data.TextColorFollowing = Settings.MainColor;
                        data.TextFollowing = Settings.ConnectivitySystem == 1 ? LocalResources.label5_Follow : LocalResources.label5_AddFriends;
                        data.ColorFollow = "#efefef";
                        data.IsFollowing = "0";
                        break;
                }

                //Variables 
                string gender = "GenderMale";
                string genderColor = "#0000ff"; //blue 

                if (data.Gender != "male")
                {
                    gender = "GenderFemale";
                    genderColor = "#cc5490"; //pink
                } 

                data.ColorGender = genderColor;
                data.ColorGender = genderColor;
                data.Gender = gender;

                data.LastSeenTimeText = LocalResources.label_last_seen + " " + Methods.Time.TimeAgo(int.Parse(data.LastseenUnixTime), false);

                var db = ClassMapper.Mapper?.Map<UserDataFody>(data);
                db.UVerified = isVerified;
                return db;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                var db = ClassMapper.Mapper?.Map<UserDataFody>(data);
                return db;
            }
        }
         
        public static SessionDataFody SessionFilter(FetchSessionsObject.SessionsDataObject data)
        {
            try
            { 
                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        data.SColorBackground = "#232323";
                        data.SColorForeground = "#efefef";
                        break;
                    default:
                        data.SColorBackground = "#ffffff";
                        data.SColorForeground = "#444444";
                        break;
                }

                if (data.SessionId == Current.AccessToken)
                {
                    data.SPlatform = data.Platform + " ( This Device )";
                }

                data.SPlatform = data.Platform;
                data.SBrowser =  "Browser : " + data.Browser;
                data.STime = "Last seen : " + data.Time;

                Visibility ipAddressVisibility = Visibility.Hidden;
                switch (string.IsNullOrEmpty(data.IpAddress))
                {
                    case false:
                        data.SIpAddress =  "Ip Address : " + data.IpAddress;
                        ipAddressVisibility = Visibility.Visible;
                        break;
                }

                if (data.Browser != null)
                {
                    //var drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(35).EndConfig().BuildRound(item.Browser.Substring(0, 1), Color.ParseColor(AppSettings.MainColor));
                    //holder.Image.SetImageDrawable(drawable);
                    data.SPlatformName = data.Browser.Substring(0, 1);
                }
                 
                var db = ClassMapper.Mapper?.Map<SessionDataFody>(data);
                db.SIpAddressVisibility = ipAddressVisibility;
                return db;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                var db = ClassMapper.Mapper?.Map<SessionDataFody>(data);
                return db;
            }
        }
         
        public static UserDataFody ForwardMessageFilter(UserDataObject data)
        {
            try
            {
                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        data.SColorBackground = "#232323";
                        data.SColorForeground = "#efefef";
                        break;
                    default:
                        data.SColorBackground = "#ffffff";
                        data.SColorForeground = "#444444";
                        break;
                }

                //Variables 
                string gender = "GenderMale";
                string genderColor = "#0000ff"; //blue 
                string avatarSplit = data.Avatar.Split('/').Last();

                if (data.Gender != "male")
                {
                    gender = "GenderFemale";
                    genderColor = "#cc5490"; //pink
                }

                data.IsOnline = GetStatusOnline(Convert.ToInt32(data.LastseenUnixTime), data.LastseenStatus);
                data.Avatar = Methods.MultiMedia.Get_image(data.UserId, avatarSplit, data.Avatar);
                Methods.FunString.SubStringCutOf(GetNameFinal(data), 30);
                data.About = GetAboutFinal(data);

                data.TextColorFollowing = Settings.MainColor;
                data.TextFollowing = LocalResources.label_Send;
                data.ColorFollow = "#efefef";

                data.ColorGender = genderColor;
                data.ColorGender = genderColor;
                data.Gender = gender;

                Visibility isverified = Visibility.Visible;
                switch (data.Verified)
                {
                    case "0":
                        isverified = Visibility.Hidden;
                        break;
                }

                var db = ClassMapper.Mapper?.Map<UserDataFody>(data);
                db.UVerified = isverified;
                return db;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                var db = ClassMapper.Mapper?.Map<UserDataFody>(data);
                return db;
            }
        }

        public static EventDataFody EventFilter(EventDataObject data)
        {
            try
            {
                switch (UserDetails.ModeDarkStlye)
                {
                    case true:
                        data.SColorBackground = "#232323";
                        data.SColorForeground = "#efefef";
                        break;
                    default:
                        data.SColorBackground = "#ffffff";
                        data.SColorForeground = "#444444";
                        break;
                }

                //Variables 
                data.Name = Methods.FunString.DecodeString(data.Name);
                data.Description = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(data.Description),50);

                data.UserData = UserFilter(data.UserData);

                var db = ClassMapper.Mapper?.Map<EventDataFody>(data);
                return db;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                var db = ClassMapper.Mapper?.Map<EventDataFody>(data);
                return db;
            }
        }
         
        public static UserListFody ConvertToUserList(UserDataFody info)
        {
            try
            {
                UserListFody infoObject = new UserListFody
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
                    ApiNotificationSettings = info.ApiNotificationSettings,
                    Details = info.Details,
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
                    ChatType = "user",
                    LastMessage = new LastMessageUnion()
                    {
                        LastMessageClass = new MessageData()
                    }
                };
                 
                var item = UserChatFilter(infoObject); 
                return item;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }
    }
}