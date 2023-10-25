using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using CommunityToolkit.WinUI.Notifications;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SocketIOClient;
using SocketIOClient.Ext_Json;
using WoWonderClient;
using WoWonderClient.Classes.Socket;
using WoWonderDesktop.Controls;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using Exception = System.Exception;
// ReSharper disable All

namespace WoWonderDesktop.SocketSystem
{
    public class WoSocketHandler
    {
        private SocketIO Client;
        public static bool IsJoined;
        private static ChatWindow GlobalContext;
        private int MTries;

        public void InitStart()
        {
            try
            {
                GlobalContext = ChatWindow.ChatWindowContext;

                DisconnectSocket();

                //string port = ListUtils.SettingsSiteList?.NodejsSsl == "1" ? ListUtils.SettingsSiteList?.NodejsSslPort : ListUtils.SettingsSiteList?.NodejsPort;

                var options = new SocketIOOptions
                {
                    // low-level engine options
                    Transport = Settings.Transport,
                    AutoUpgrade = false,
                    SslProtocols = Settings.WebExceptionSecurity,
                    Path = "/socket.io",
                    Query = null,
                    //ExtraHeaders = new Dictionary<string, string>()
                    //{
                    //    {"Connection", "Upgrade"},
                    //    {"Upgrade", "websocket"},
                    //    {"Sec-WebSocket-Version", "13"},
                    //    {"Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits"},
                    //},

                    // Manager options
                    EIO = EngineIO.V3,
                    Reconnection = true,
                    ReconnectionAttempts = int.MaxValue,
                    ReconnectionDelay = 1000,
                    ReconnectionDelayMax = 5000,
                    RandomizationFactor = 0.5,
                    ConnectionTimeout = TimeSpan.FromSeconds(20),

                    Auth = null,
                };

                string website = InitializeWoWonder.WebsiteUrl;
                char last = InitializeWoWonder.WebsiteUrl.Last();
                if (last.Equals('/'))
                {
                    website = InitializeWoWonder.WebsiteUrl.Remove(InitializeWoWonder.WebsiteUrl.Length - 1, 1);
                }

                Client = new SocketIO(new Uri($"{website}:{Settings.PortSocketServer}"), options);
                if (Client != null)
                {
                    var jsonSerializer = new NewtonsoftJsonSerializer
                    {
                        OptionsProvider = () => new JsonSerializerSettings
                        {
                            ContractResolver = new DefaultContractResolver
                            {
                                NamingStrategy = new CamelCaseNamingStrategy()
                            }
                        }
                    };
                    Client.JsonSerializer = jsonSerializer;

                    WoSocketEvents events = new WoSocketEvents();
                    events.InitEvents(Client);

                    Emit_Connect(UserDetails.Username, UserDetails.AccessToken);
                }
            }
            catch (OperationCanceledException e)
            {
                Methods.DisplayReportResultTrack(e);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }
        public async void Emit_Connect(string username, string accessToken)
        {
            try
            {
                if (Client != null)
                {
                    Client.OnConnected += null;
                    Client.OnConnected += (sender, args) =>
                    {
                        try
                        {
                            Console.WriteLine("Socket_OnConnected");
                            Console.WriteLine("Socket.Id:" + Client.Id);

                            //Add all On_ functions here 
                            //Socket_On_Alert(Client);
                            Socket_On_Private_Message(Client);
                            //Socket_On_Private_PageMessage(Client);
                            Socket_On_Private_GroupMessage(Client);
                            //Socket_On_Private_Message_page(Client); 
                            Socket_On_User_Status_Change(Client);
                            Socket_On_RecordingEvent(Client);
                            Socket_On_TypingEvent(Client);
                            Socket_On_StopTypingEvent(Client);
                            Socket_On_Seen_Messages(Client);
                            Socket_On_new_video_call(Client);
                            Socket_On_loggedintEvent(Client);
                            Socket_On_loggedoutEvent(Client);
                            Socket_On_message_reaction(Client);
                            //Socket_On_Private_PageMessage(Client);

                            Emit_Join(username, accessToken);
                        }
                        catch (Exception ex)
                        {
                            Methods.DisplayReportResultTrack(ex);
                        }
                    };
                    await Client.ConnectAsync();
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public async void Emit_Join(string username, string accessToken)
        {
            try
            {
                if (Client == null)
                    InitStart();

                if (Client != null)
                {
                    if (!Client.Connected)
                    {
                        Emit_Connect(username, accessToken);
                        return;
                    }

                    if (IsJoined)
                        return;

                    Dictionary<string, string> value = new Dictionary<string, string>
                    {
                        {"username", username}, {"user_id", accessToken}
                    };

                    await Client.EmitAsync("join", response =>
                    {
                        try
                        {
                            Console.WriteLine("Socket.Id:" + Client.Id);
                            Console.WriteLine("Socket_joined");
                            IsJoined = true;

                            var result = response;
                            Console.WriteLine(result);
                            MTries = 0;
                            Socket_ping_for_lastseen(UserDetails.AccessToken);

                            if (UserDetails.OnlineUsers)
                                Emit_loggedintEvent(UserDetails.AccessToken);
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    }, value);
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }
         
        #region User

        //======================= Emit Async ==========================

        //set type text
        public async void EmitAsync_RecordingEvent(string recipientId, string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync();

                Console.WriteLine("Socket.Id:" + Client?.Id);

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"recipient_id", recipientId}, {"user_id", accessToken}
                };

                await Client?.EmitAsync("recording", response =>
                {
                    try
                    {
                        var result = response.GetValue();
                        Console.WriteLine(result);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //set type text
        public async void EmitAsync_TypingEvent(string recipientId, string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync();

                Console.WriteLine("Socket.Id:" + Client?.Id);

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"recipient_id", recipientId}, {"user_id", accessToken}
                };

                await Client?.EmitAsync("typing", response =>
                {
                    try
                    {
                        var result = response.GetValue();
                        Console.WriteLine(result);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //set Stop text
        public async void EmitAsync_StoppedEvent(string recipientId, string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync();

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"recipient_id", recipientId}, {"user_id", accessToken}
                };

                await Client?.EmitAsync("typing_done", response =>
                {
                    try
                    {
                        var result = response.GetValue();
                        Console.WriteLine(result);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //set seen messages
        public async void EmitAsync_SendSeenMessages(string recipientId, string accessToken, string fromUserId)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync();

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"recipient_id", recipientId}, {"user_id", accessToken}, {"current_user_id", fromUserId}
                };

                await Client?.EmitAsync("seen_messages", value);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Send Message text
        public async void EmitAsync_SendMessage(string toId, string accessToken, string username, string msg, string color, string messageReplyId, string messageHashId, string storyId = "", string lat = "", string lng = "")
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync();

                if (string.IsNullOrEmpty(messageReplyId))
                    messageReplyId = "0";

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    { "to_id", toId },
                    { "from_id", accessToken },
                    { "username", username },
                    { "msg", msg },
                    { "color", color },
                    { "message_reply_id", messageReplyId },
                    { "story_id", storyId },
                    { "lat", lat },
                    { "lng", lng },
                    { "isSticker", "false" }
                };

                await Client?.EmitAsync("private_message", response =>
                {
                    try
                    {
                        var json = response.GetValue();
                        var result = response.GetValue<PrivateMessageObject>();
                        if (result != null)
                        {
                            Application.Current.Dispatcher?.Invoke(() =>
                            {
                                try
                                {
                                    var checker = ListUtils.ListMessages?.FirstOrDefault(a => a.Id == long.Parse(messageHashId));
                                    if (checker != null)
                                    {
                                        //Update data message and get type
                                        checker.Id = long.Parse(result.MessageId);
                                        checker.Seen = "0";

                                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                        dbDatabase.Insert_Or_Update_To_one_MessagesTable(checker);
 
                                        //GlobalContext.Updater(checker.MesData);

                                        //if (UserDetails.SoundControl)
                                        //    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");

                                        Task.Delay(1500);

                                        if (Methods.CheckForInternetConnection())
                                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GlobalContext.GetMessagesById(result.MessageId) });
                                    }

                                    if (Methods.CheckForInternetConnection())
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GlobalContext.LoadChatAsync });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //======================= On Async ==========================

        public static void Socket_On_Seen_Messages(SocketIO client)
        {
            try
            {
                if (client is { Connected: true })
                {

                    client?.On("seen_messages", response =>
                    {
                        try
                        {
                            var result = response.GetValue();
                            Console.WriteLine(result);
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });

                    client?.On("lastseen", response =>
                    {
                        try
                        {
                            var result = response.GetValue();
                            Console.WriteLine(result);
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Get new user in last user messages 
        public static void Socket_On_User_Status_Change(SocketIO client)
        {
            try
            {
                if (client is { Connected: true })
                    client?.On("user_status_change", response =>
                    {
                        try
                        {
                            var result = response.GetValue();

                            Console.WriteLine(result);
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Check who is recording now
        public void Socket_On_RecordingEvent(SocketIO client)
        {
            try
            {
                if (client is { Connected: true })
                    client?.On("recording", response =>
                    {
                        try
                        {
                            var json = response.GetValue();

                            var result = response.GetValue<ChatTypingObject>();
                            Console.WriteLine(result);
                            if (result != null)
                            {
                                var data = ListUtils.ListUsers?.FirstOrDefault(a => a.UserId == result.SenderId);
                                if (data != null)
                                {
                                    Console.WriteLine(data);
                                }

                                Application.Current.Dispatcher?.Invoke(delegate
                                {
                                    try
                                    {
                                        var typing = result.IsTyping;
                                        ViewModel.ModelInstance.LastSeen = typing == "200" ? LocalResources.label_Typping : GlobalContext.LastSeen ?? GlobalContext.LastSeen;
                                        ViewModel.ModelInstance.UpdateLastSeen();
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Check who is Typing now
        public void Socket_On_TypingEvent(SocketIO client)
        {
            try
            {
                if (client is { Connected: true })
                    client?.On("typing", response =>
                    {
                        try
                        {
                            var result = response.GetValue<ChatTypingObject>();
                            Console.WriteLine(result);
                            if (result != null)
                            {
                                var data = ListUtils.ListUsers?.FirstOrDefault(a => a.UserId == result.SenderId);
                                if (data != null)
                                {
                                    Console.WriteLine(data);
                                }

                                Application.Current.Dispatcher?.Invoke(delegate
                                {
                                    try
                                    {
                                        var typing = result.IsTyping;
                                        ViewModel.ModelInstance.LastSeen = typing == "200" ? LocalResources.label_Typping : GlobalContext.LastSeen ?? GlobalContext.LastSeen;
                                        ViewModel.ModelInstance.UpdateLastSeen();
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Check who is finish Typing
        public void Socket_On_StopTypingEvent(SocketIO client)
        {
            try
            {
                if (client is { Connected: true })
                    client?.On("typing_done", response =>
                    {
                        try
                        {
                            if (response != null)
                            {
                                var result = response.GetValue<ChatTypingObject>();
                                if (result != null)
                                {
                                    var data = ListUtils.ListUsers?.FirstOrDefault(a => a.UserId == result.SenderId);
                                    if (data != null)
                                    {
                                        Console.WriteLine(data);
                                    }

                                    Application.Current.Dispatcher?.Invoke(delegate
                                    {
                                        try
                                        {
                                            ViewModel.ModelInstance.LastSeen = GlobalContext.LastSeen;
                                            ViewModel.ModelInstance.UpdateLastSeen();
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Get New Message 
        public static void Socket_On_Private_Message(SocketIO client)
        {
            try
            {
                if (client is { Connected: true })
                    client?.On("private_message", response =>
                    {
                        try
                        {
                            var json = response.GetValue();
                            var result = response.GetValue<PrivateMessageObject>();
                            if (result != null)
                            {
                                if (Methods.CheckForInternetConnection())
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GlobalContext.LoadChatAsync });

                                if (!string.IsNullOrEmpty(ViewModel.ModelInstance.Name))
                                {
                                    if (ChatWindow.UserId == result.Sender)
                                    {
                                        GlobalContext.TaskWork = "Working";
                                        Application.Current.Dispatcher?.Invoke(GlobalContext.MessageUpdater);

                                        //Wael add data Messages and get type 
                                        UserDetails.Socket?.EmitAsync_SendSeenMessages(result.Sender, UserDetails.AccessToken, UserDetails.UserId);
                                    }
                                    else if (UserDetails.UserId == result.Sender)
                                    {
                                        GlobalContext.TaskWork = "Working";
                                        Application.Current.Dispatcher?.Invoke(GlobalContext.MessageUpdater);
                                    }
                                }
                                else
                                {
                                    //ListUtils.MessageUnreadList ??= new ObservableCollection<PrivateMessageObject>();

                                    var updaterUser = ListUtils.ListUsers?.FirstOrDefault(a => a.UserId == result.Sender && a.ChatType == "user");
                                    if (updaterUser != null)
                                    {
                                        if (result.IsMedia != null && result.IsMedia.Value)
                                        {
                                            //var data = ListUtils.MessageUnreadList.FirstOrDefault(a => a.Sender == result.Sender && a.Message == Methods.FunString.DecodeString(result.Message));
                                            //if (data == null)
                                            //{
                                            //    ListUtils.MessageUnreadList.Add(new PrivateMessageObject
                                            //    {
                                            //        Sender = result.Sender,
                                            //        Message = GlobalContext.GetText(Resource.String.Lbl_SendMessage)
                                            //    });
                                            //}

                                            // create Notifications 
                                            new ToastContentBuilder()
                                                .AddArgument("action", "viewConversation")
                                                .AddArgument("conversationId", result.Sender)
                                                .AddText(result.Username)
                                                .AddText("Send you a message")
                                                .AddAppLogoOverride(new Uri(result.Avatar, UriKind.Absolute), ToastGenericAppLogoCrop.Circle)
                                                .Show();
                                        }
                                        else
                                        {
                                            //var data = ListUtils.MessageUnreadList.FirstOrDefault(a => a.Sender == result.Sender && a.Message == Methods.FunString.DecodeString(result.Message));
                                            //if (data == null)
                                            //{
                                            //    ListUtils.MessageUnreadList.Add(new PrivateMessageObject
                                            //    {
                                            //        Sender = result.Sender,
                                            //        Message = Methods.FunString.DecodeString(result.Message)
                                            //    });
                                            //}

                                            // create Notifications 
                                            new ToastContentBuilder()
                                                .AddArgument("action", "viewConversation")
                                                .AddArgument("conversationId", result.Sender)
                                                .AddText(result.Username)
                                                .AddText(Methods.FunString.DecodeString(result.Message))
                                                .AddAppLogoOverride(new Uri(result.Avatar, UriKind.Absolute), ToastGenericAppLogoCrop.Circle)
                                                .Show();
                                        }
                                    }
                                    else if (UserDetails.UserId != result.Sender)
                                        // create Notifications 
                                        new ToastContentBuilder()
                                            .AddArgument("action", "viewConversation")
                                            .AddArgument("conversationId", result.Sender)
                                            .AddText(result.Username)
                                            .AddText(Methods.FunString.DecodeString(result.Message))
                                            .AddAppLogoOverride(new Uri(result.Avatar, UriKind.Absolute), ToastGenericAppLogoCrop.Circle)
                                            .Show();
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion
         
        #region Group

        //======================= Emit Async ==========================

        //Send Group Message text
        public async void EmitAsync_SendGroupMessage(string groupId, string accessToken, string username, string msg, string messageReplyId, string messageHashId)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync();

                if (string.IsNullOrEmpty(messageReplyId))
                    messageReplyId = "0";

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"group_id", groupId},
                    {"from_id", accessToken},
                    {"username", username},
                    {"msg", msg},
                    {"message_reply_id", messageReplyId},
                    {"isSticker", "false"}
                };

                await Client?.EmitAsync("group_message", response =>
                {
                    try
                    {
                        //var json = response.GetValue();
                        var result = response.GetValue<PrivateGroupMessageObject>();
                        if (result != null)
                        {
                            Application.Current.Dispatcher?.Invoke(() =>
                            {
                                try
                                {
                                    var checker = ListUtils.ListMessages?.FirstOrDefault(a => a.Id == long.Parse(messageHashId));
                                    if (checker != null)
                                    { 
                                        //Update data mesasage and get type
                                        checker.Id = result.NewMessage.Id;
                                        checker = WoWonderTools.MessageFilter(result.GroupData.GroupId, WoWonderTools.GetNameFinal(result.NewMessage.UserData), result.NewMessage, GlobalContext.ChatColor, "group");
                                        Console.WriteLine(checker);
                                        
                                        //if (UserDetails.SoundControl)
                                        //    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");

                                        Task.Delay(1500);

                                        if (Methods.CheckForInternetConnection())
                                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GlobalContext.GetGroupMessagesById(result.MessageId) });
                                    }

                                    if (Methods.CheckForInternetConnection())
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GlobalContext.LoadChatAsync });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //======================= On Async ==========================

        //Get New GroupMessage 
        public static void Socket_On_Private_GroupMessage(SocketIO client)
        {
            try
            {
                if (client is { Connected: true })
                    client?.On("group_message", response =>
                    {
                        try
                        {
                            var json = response.GetValue();

                            var result = response.GetValue<PrivateGroupMessageObject>();
                            if (result != null)
                            {
                                if (Methods.CheckForInternetConnection())
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GlobalContext.LoadChatAsync });

                                if (GlobalContext != null)
                                { 
                                    if (ChatWindow.GroupId == result.Id)
                                    {
                                        GlobalContext.TaskWork = "Working";
                                        Application.Current.Dispatcher?.Invoke(GlobalContext.MessageUpdater);

                                        //Wael add data Messages and get type 
                                        //UserDetails.Socket?.EmitAsync_SendSeenMessages(result.Sender, UserDetails.AccessToken, UserDetails.UserId);
                                    }
                                    else
                                    {
                                        GlobalContext.TaskWork = "Working";
                                        Application.Current.Dispatcher?.Invoke(GlobalContext.MessageUpdater);
                                    }
                                }
                                else
                                {
                                    //ListUtils.MessageUnreadList ??= new ObservableCollection<PrivateMessageObject>();

                                    var updaterUser = ListUtils.ListUsers?.FirstOrDefault(a => a.GroupId == result.Id && a.ChatType == "group");
                                    if (updaterUser != null)
                                    {
                                        if (!string.IsNullOrEmpty(result.NewMessage.Media))
                                        {
                                            //var data = ListUtils.MessageUnreadList.FirstOrDefault(a => a.Sender == result.Id);
                                            //if (data == null)
                                            //{
                                            //    ListUtils.MessageUnreadList.Add(new PrivateMessageObject
                                            //    {
                                            //        Sender = result.Id,
                                            //        Message = GlobalContext.GetText(Resource.String.Lbl_SendMessage)
                                            //    });
                                            //}

                                            // create Notifications 
                                            new ToastContentBuilder()
                                                .AddArgument("action", "viewConversation")
                                                .AddArgument("conversationId", result.Id)
                                                .AddText(Methods.FunString.DecodeString(updaterUser.Name))
                                                .AddText("Send you a message")
                                                .AddAppLogoOverride(new Uri(result.NewMessage.UserData.Avatar, UriKind.Absolute), ToastGenericAppLogoCrop.Circle)
                                                .Show();
                                        }
                                        else
                                        {
                                            //var data = ListUtils.MessageUnreadList.FirstOrDefault(a => a.Sender == result.Id && a.Message == Methods.FunString.DecodeString(result.NewMessage.Text));
                                            //if (data == null)
                                            //{
                                            //    ListUtils.MessageUnreadList.Add(new PrivateMessageObject
                                            //    {
                                            //        Sender = result.Id,
                                            //        Message = Methods.FunString.DecodeString(result.NewMessage.Text)
                                            //    });
                                            //}

                                            new ToastContentBuilder()
                                                .AddArgument("action", "viewConversation")
                                                .AddArgument("conversationId", result.Id)
                                                .AddText(Methods.FunString.DecodeString(updaterUser.Name))
                                                .AddText(Methods.FunString.DecodeString(result.NewMessage.Text))
                                                .AddAppLogoOverride(new Uri(result.NewMessage.UserData.Avatar, UriKind.Absolute), ToastGenericAppLogoCrop.Circle)
                                                .Show();
                                        }
                                    }
                                    else
                                        new ToastContentBuilder()
                                            .AddArgument("action", "viewConversation")
                                            .AddArgument("conversationId", result.Id)
                                            .AddText(Methods.FunString.DecodeString(result.GroupData.Name))
                                            .AddText(Methods.FunString.DecodeString(result.NewMessage.Text))
                                            .AddAppLogoOverride(new Uri(result.NewMessage.UserData.Avatar, UriKind.Absolute), ToastGenericAppLogoCrop.Circle)
                                            .Show();
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region call

        //Check new video call 
        public void Socket_On_new_video_call(SocketIO client)
        {
            try
            {
                if (client is { Connected: true })
                    client?.On("new_video_call", response =>
                    {
                        try
                        {
                            var result = response.GetValue<NewVideoCallObject>();
                            if (result != null)
                            {
                                if (Methods.CheckForInternetConnection())
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GlobalContext.LoadChatAsync });
                            }
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Check new video call 
        public async void EmitAsync_Create_callEvent(string recipientId)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync();

                //{to_id: toUSERID, type: 'create_video'}
                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"to_id", recipientId}, {"type", "create_video"}
                };

                await Client?.EmitAsync("user_notification", response =>
                {
                    try
                    {
                        var result = response.GetValue();
                        Console.WriteLine(result);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region Reaction

        public async void EmitAsync_message_reaction(string id, string reaction, string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync();

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"type", "messages"},
                    {"id", id},
                    {"reaction", reaction},
                    {"user_id", accessToken},
                };

                await Client?.EmitAsync("register_reaction", value);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public static void Socket_On_message_reaction(SocketIO client)
        {
            try
            {
                if (client is { Connected: true })
                    client?.On("register_reaction", response =>
                    {
                        try
                        {
                            var json = response.GetValue();
                            var result = response.GetValue<ReactionMessageObject>();
                            if (result != null)
                            {
                                Application.Current.Dispatcher?.Invoke(delegate
                                {
                                    try
                                    {
                                        if (ChatWindow.TypeChatPage == "user")
                                        {
                                            if (Methods.CheckForInternetConnection())
                                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GlobalContext.GetMessagesById(result.Id) });
                                        }
                                        else
                                        {
                                            if (Methods.CheckForInternetConnection())
                                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GlobalContext.GetGroupMessagesById(result.Id) });
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                }); 
                            }
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region General

        //======================= Emit Async ==========================

        //online
        public async void Emit_loggedintEvent(string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync();

                Dictionary<string, string> value = new Dictionary<string, string> { { "from_id", accessToken } };

                await Client?.EmitAsync("on_user_loggedin", response =>
                {
                    try
                    {
                        //var result = response.GetValue();

                        Console.WriteLine(response);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //offline
        public async void Emit_loggedoutEvent(string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync();

                Dictionary<string, string> value = new Dictionary<string, string> { { "from_id", accessToken } };

                await Client?.EmitAsync("on_user_loggedoff", response =>
                {
                    try
                    {
                        //var result = response.GetValue();

                        Console.WriteLine(response);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //======================= On Async ==========================

        //online
        public void Socket_On_loggedintEvent(SocketIO client)
        {
            try
            {
                client?.On("on_user_loggedin", response =>
                {
                    try
                    {
                        //var result = response.GetValue();

                        Console.WriteLine(response);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //offline
        public void Socket_On_loggedoutEvent(SocketIO client)
        {
            try
            {
                if (client is { Connected: true })
                    client?.On("on_user_loggedoff", response =>
                    {
                        try
                        {
                            //var result = response.GetValue();

                            Console.WriteLine(response);
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        private static Timer Timer;

        //UPDATE USER LAST SEEN
        public async void Socket_ping_for_lastseen(string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync();

                Console.WriteLine("Socket.Id:" + Client?.Id);

                if (Timer != null)
                    return;

                Timer = new Timer { Interval = 2000 };
                Timer.Elapsed += (o, eventArgs) =>
                {
                    try
                    {
                        Dictionary<string, string> valueDictionary = new Dictionary<string, string>
                        {
                            {"user_id", accessToken}
                        };

                        Client?.EmitAsync("ping_for_lastseen", valueDictionary);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };
                Timer.Enabled = true;
                Timer.Start();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void DisconnectSocket()
        {
            try
            {
                if (Client != null)
                {
                    //Client?.Off("alert");
                    Client?.Off("join");
                    Client?.Off("private_message");
                    Client?.Off("page_message");
                    Client?.Off("group_message");
                    Client?.Off("seen_messages");
                    Client?.Off("lastseen");
                    Client?.Off("user_status_change");
                    Client?.Off("recording");
                    Client?.Off("typing");
                    Client?.Off("typing_done");
                    Client?.Off("new_video_call");
                    Client?.Off("user_notification");
                    Client?.Off("on_user_loggedin");
                    Client?.Off("on_user_loggedoff");
                    Client?.Off("ping_for_lastseen");
                    Client?.Off("register_reaction");

                    Client?.DisconnectAsync();
                    Client = null;
                }

                if (Timer != null)
                {
                    Timer.Stop();
                    Timer = null;
                }

                IsJoined = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ReconnectSocket()
        {
            try
            {
                if (MTries < 5)
                {
                    MTries++;

                    DisconnectSocket();

                    //Connect to socket with access token
                    UserDetails.Socket?.Emit_Join(UserDetails.Username, UserDetails.AccessToken);
                }
                else
                {
                    MessageBox.Show(LocalResources.label5_Error_connectServer);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Get New Message 
        //public static void Socket_On_Private_Message_page(SocketIO client)
        //{
        //    try
        //    {
        //        client?.On("private_message_page", response =>
        //        {
        //            try
        //            {
        //                var json = response.GetValue();

        //                var result = response.GetValue<PrivateMessageObject>();
        //                if (result != null)
        //                {

        //                }
        //            }
        //            catch (Exception exception)
        //            {
        //                Methods.DisplayReportResultTrack(exception);
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Methods.DisplayReportResultTrack(ex);
        //    }
        //}

        //public static void Socket_On_Alert(SocketIO client)
        //{
        //    try
        //    { 
        //        client?.On("alert", response =>
        //        {
        //            try
        //            {
        //                var result = response.GetValue(); 
        //                Console.WriteLine(result);
        //            } 
        //            catch (Exception exception)
        //            {
        //                Methods.DisplayReportResultTrack(exception);
        //            }
        //        });
        //    } 
        //    catch (Exception ex)
        //    {
        //        Methods.DisplayReportResultTrack(ex);
        //    }
        //}

    }
}