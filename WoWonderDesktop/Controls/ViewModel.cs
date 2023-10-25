using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using WoWonderClient;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using WoWonderDesktop.Annotations;
using WoWonderDesktop.Commands;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using WoWonderDesktop.WindowsPages;

namespace WoWonderDesktop.Controls
{
    public class ViewModel : INotifyPropertyChanged
    {
        //Initializing resource dictionary file
        public static readonly ResourceDictionary Dictionary = Application.LoadComponent(new Uri(Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Styles/Icons.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary;

        public string Name { get; set; }
        public string AvatarUser { get; set; }
        public string LastSeen { get; set; }
        public static ViewModel ModelInstance { get; private set; }

        public ViewModel()
        {
            try
            {
                ModelInstance = this;

                PinnedChats = new ObservableCollection<UserListFody>();
                ArchivedChats = new ObservableCollection<UserListFody>();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Chats List

        protected ObservableCollection<UserListFody> mPinnedChats;
        protected ObservableCollection<UserListFody> _archivedChats;

        public ObservableCollection<UserListFody> Chats
        {
            get => ListUtils.ListUsers;
            set
            {
                try
                {
                    //To Change the list
                    if (ListUtils.ListUsers == value)
                        return;

                    //To Update the list
                    ListUtils.ListUsers = value;

                    //Updating filtered chats to match
                    FilteredChats = new ObservableCollection<UserListFody>(ListUtils.ListUsers);
                    OnPropertyChanged(nameof(Chats));
                    OnPropertyChanged(nameof(FilteredChats));
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public ObservableCollection<UserListFody> PinnedChats
        {
            get => mPinnedChats;
            set
            {
                //To Change the list
                if (mPinnedChats == value)
                    return;

                //To Update the list
                mPinnedChats = value;

                //Updating filtered chats to match
                FilteredPinnedChats = new ObservableCollection<UserListFody>(mPinnedChats);
                OnPropertyChanged(nameof(PinnedChats));
                OnPropertyChanged(nameof(FilteredPinnedChats));
            }
        }

        public ObservableCollection<UserListFody> ArchivedChats
        {
            get => _archivedChats;
            set
            {
                _archivedChats = value;
                OnPropertyChanged();
            }
        }

        //Filtering Chats & Pinned Chats
        public ObservableCollection<UserListFody> FilteredChats { get; set; }
        public ObservableCollection<UserListFody> FilteredPinnedChats { get; set; }

        protected int ChatPosition { get; set; }

        public void LoadChats(ObservableCollection<UserListFody> collection)
        {
            try
            {
                //Loading data from Database
                Chats ??= new ObservableCollection<UserListFody>();
                FilteredChats ??= new ObservableCollection<UserListFody>();

                //Transfer data
                Chats = collection;
                FilteredChats = collection;

                if (ListUtils.PinList.Count > 0)
                {
                    foreach (var item in from item in ListUtils.PinList let check = PinnedChats.FirstOrDefault(a => a.ChatId == item.ChatId) where check == null select WoWonderTools.UserChatFilter(item.LastChat))
                    { 
                        PinnedChats.Add(item);
                        FilteredPinnedChats.Add(item);
                        OnPropertyChanged(nameof(PinnedChats));
                        OnPropertyChanged(nameof(FilteredPinnedChats));
                        item.IsPin = true;

                        //Remove selected chat from all chats / unpinned chats
                        //Store position of chat before pinning so that when we unpin or unarchive we get it on same original position...
                        ChatPosition = Chats.IndexOf(item);
                        Chats.Remove(item);
                        FilteredChats.Remove(item);
                        OnPropertyChanged(nameof(Chats));
                        OnPropertyChanged(nameof(FilteredChats));
                         
                        //Remember, Chat will be removed from Pinned List when Archived.. and Vice Versa...
                        //Fixed
                        if (ArchivedChats != null)
                        {
                            if (ArchivedChats.Contains(item))
                            {
                                ArchivedChats.Remove(item);
                                item.IsArchive = false;
                            }
                        }
                    }
                }

                if (ListUtils.ArchiveList.Count > 0)
                {
                    foreach (var item in from item in ListUtils.ArchiveList let check = ArchivedChats.FirstOrDefault(a => a.ChatId == item.ChatId) where check == null select WoWonderTools.UserChatFilter(item.LastChat))
                    {
                        ArchivedChats.Add(item);
                        item.IsArchive = true;
                        item.IsPin = false;

                        //Remove Chat from Pinned & Unpinned Chat List
                        Chats.Remove(item);
                        FilteredChats.Remove(item);
                        PinnedChats.Remove(item);
                        FilteredPinnedChats.Remove(item);

                        //Update Lists
                        OnPropertyChanged(nameof(Chats));
                        OnPropertyChanged(nameof(FilteredChats));
                        OnPropertyChanged(nameof(PinnedChats));
                        OnPropertyChanged(nameof(FilteredPinnedChats));
                        OnPropertyChanged(nameof(ArchivedChats));
                    }
                }

                //Update
                OnPropertyChanged(nameof(Chats));
                OnPropertyChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ClearChats()
        {
            try
            {
                //Loading data from Database
                Chats?.Clear();
                FilteredChats?.Clear();

                ListUtils.PinList?.Clear();
                ListUtils.ArchiveList?.Clear();

                //Update 
                OnPropertyChanged(nameof(Chats));
                OnPropertyChanged(nameof(FilteredChats));
                OnPropertyChanged(nameof(PinnedChats));
                OnPropertyChanged(nameof(FilteredPinnedChats));
                OnPropertyChanged(nameof(ArchivedChats));
                OnPropertyChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UpdateLastChatAllList(UserListFody conversation)
        {
            UpdateLastChatAndMoveUp(ListUtils.ListUsers, conversation);
            UpdateLastChatAndMoveUp(Chats, conversation);
            UpdateLastChatAndMoveUp(PinnedChats, conversation);
            UpdateLastChatAndMoveUp(FilteredChats, conversation);
            UpdateLastChatAndMoveUp(FilteredPinnedChats, conversation);
            UpdateLastChatAndMoveUp(ArchivedChats, conversation);
        }

        //Move the chat contact on top when new message is sent or received
        public void UpdateLastChatAndMoveUp(ObservableCollection<UserListFody> chatList, UserListFody conversation)
        {
            try
            {
                //Check contact...
                var chat = chatList.FirstOrDefault(x => x.ChatId == conversation.ChatId);
                //if found..then..
                if (chat != null)
                {
                    var index = chatList.IndexOf(chat);

                    //Update Contact Chat Last Message and Message Time..
                    chat.LastMessageText = conversation.LastMessageText;
                    chat.LastSeenTimeText = conversation.LastSeenTimeText;

                    //Move Chat on top when new message is received/sent...
                    if (index > 0)
                        chatList.Move(chatList.IndexOf(chat), 0);

                    //Update Collections
                    OnPropertyChanged(nameof(Chats));
                    OnPropertyChanged(nameof(PinnedChats));
                    OnPropertyChanged(nameof(FilteredChats));
                    OnPropertyChanged(nameof(FilteredPinnedChats));
                    OnPropertyChanged(nameof(ArchivedChats));
                    
                    //ICollectionView view = CollectionViewSource.GetDefaultView(chatList);
                    //view?.Refresh();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //To get the ContactName of selected chat so that we can open corresponding conversation
        protected ICommand _getSelectedChatCommand;

        public ICommand GetSelectedChatCommand => _getSelectedChatCommand ??= new RelayCommand(parameter =>
        {
            if (parameter is UserListFody v)
            {  
                if (v.Name == Name || !WoWonderTools.ChatIsAllowed(v))
                    return;

                //getting Name from selected chat
                Name = v.Name;

                var time = v.ChatTime ?? v.Time;
                bool success = int.TryParse(time, out var number);
                LastSeen = success ? LocalResources.label_last_seen + " " + Methods.Time.TimeAgo(number, false) : LocalResources.label_last_seen + " " + time;

                OnPropertyChanged(nameof(Name));

                //getting Avatar from selected chat
                AvatarUser = v.Avatar;
                OnPropertyChanged(nameof(AvatarUser));

                //Get message 
                LoadMessagesDataFody(v, "user");
            }
        });

        public void UpdateLastSeen()
        {
            if (!string.IsNullOrEmpty(Name))
                OnPropertyChanged(nameof(Name));
        }

        //To Pin Chat on Pin Button Click
        protected ICommand _pinChatCommand;

        public ICommand PinChatCommand => _pinChatCommand ??= new RelayCommand(parameter =>
        {
            if (parameter is UserListFody v)
            {
                if (!FilteredPinnedChats.Contains(v))
                {
                    //Add selected chat to pin chat

                    PinnedChats.Add(v);
                    FilteredPinnedChats.Add(v);
                    OnPropertyChanged(nameof(PinnedChats));
                    OnPropertyChanged(nameof(FilteredPinnedChats));
                    v.IsPin = true;

                    //Remove selected chat from all chats / unpinned chats
                    //Store position of chat before pinning so that when we unpin or unarchive we get it on same original position...
                    ChatPosition = Chats.IndexOf(v);
                    Chats.Remove(v);
                    FilteredChats.Remove(v);
                    OnPropertyChanged(nameof(Chats));
                    OnPropertyChanged(nameof(FilteredChats));


                    //Remember, Chat will be removed from Pinned List when Archived.. and Vice Versa...
                    //Fixed
                    if (ArchivedChats != null)
                    {
                        if (ArchivedChats.Contains(v))
                        {
                            ArchivedChats.Remove(v);
                            v.IsArchive = false;
                        }
                    }

                    var check2 = ListUtils.ListUsers.FirstOrDefault(a => a.ChatId == v.ChatId);
                    if (check2 != null)
                    {
                        check2.IsArchive = false;
                        check2.IsPin = true;
                    }

                    //Send Api
                    var dictionary = new Dictionary<string, string>
                    {
                        {"pin", "yes"},
                    };

                    //if (v.Mute != null)
                    //{
                    //    dictionary.Add("call_chat", v.Mute.CallChat);
                    //    dictionary.Add("archive", v.Mute.Archive);
                    //    dictionary.Add("notify", v.Mute.Notify);
                    //}

                    if (Methods.CheckForInternetConnection())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.MuteChatsInfoAsync(v.ChatId, v.ChatType, dictionary) });

                    var item = v.MenuChatItems.FirstOrDefault(s => s.Contains(LocalResources.label5_UnPin) || s.Contains(LocalResources.label5_Pin));
                    var indexItem = v.MenuChatItems.IndexOf(item);
                    if (indexItem > -1)
                    {
                        if (check2 != null)
                        {
                            check2.MenuChatItems[indexItem] = v.IsPin ? LocalResources.label5_UnPin : LocalResources.label5_Pin;
                        }

                        v.MenuChatItems[indexItem] = v.IsPin ? LocalResources.label5_UnPin : LocalResources.label5_Pin;
                    }

                    var check = ListUtils.PinList.FirstOrDefault(a => a.ChatId == v.ChatId);
                    if (check == null)
                    {
                        ListUtils.PinList?.Add(new Classes.LastChatArchive
                        {
                            ChatType = v.ChatType,
                            ChatId = v.ChatId,
                            UserId = v.UserId,
                            GroupId = v.GroupId,
                            PageId = v.PageId,
                            Name = v.Name,
                            IdLastMessage = v?.LastMessage.LastMessageClass?.Id.ToString() ?? "",
                            LastChat = v,
                        });

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.InsertORDelete_Pin(ListUtils.PinList.LastOrDefault());
                    }
                }
            }
        });

        //To Pin Chat on Pin Button Click
        protected ICommand _unPinChatCommand;

        public ICommand UnPinChatCommand => _unPinChatCommand ??= new RelayCommand(parameter =>
        {
            if (parameter is UserListFody v)
            {
                if (!FilteredChats.Contains(v))
                {
                    //Add selected chat to Normal Unpinned chat list

                    if (!FilteredChats.Contains(v) /*&& !Chats.Contains(v)*/)
                    {
                        //Chats.Add(v);
                        FilteredChats.Add(v);
                    }
                     
                    //Restore position of chat before pinning so that when we unpin or unarchive we get it on same original position...
                    if (ChatPosition == -1)
                        ChatPosition = 0;

                    Chats.Move(Chats.Count - 1, ChatPosition);
                    FilteredChats.Move(Chats.Count - 1, ChatPosition);

                    //Update
                    OnPropertyChanged(nameof(Chats));
                    OnPropertyChanged(nameof(FilteredChats));

                    //Remove selected pinned chats list
                    PinnedChats.Remove(v);
                    FilteredPinnedChats.Remove(v);
                    OnPropertyChanged(nameof(PinnedChats));
                    OnPropertyChanged(nameof(FilteredPinnedChats));
                    v.IsPin = false;

                    //Send Api
                    var dictionary = new Dictionary<string, string>
                    {
                        { "pin", "no"},
                    };

                    //if (v.Mute != null)
                    //{
                    //    dictionary.Add("call_chat", v.Mute.CallChat);
                    //    dictionary.Add("archive", v.Mute.Archive);
                    //    dictionary.Add("notify", v.Mute.Notify);
                    //}

                    if (Methods.CheckForInternetConnection())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.MuteChatsInfoAsync(v.ChatId, v.ChatType, dictionary) });

                    var check2 = ListUtils.ListUsers.FirstOrDefault(a => a.ChatId == v.ChatId);
                    if (check2 != null)
                    {
                        check2.IsPin = false; 
                    }

                    var item = v.MenuChatItems.FirstOrDefault(s => s.Contains(LocalResources.label5_UnPin) || s.Contains(LocalResources.label5_Pin));
                    var indexItem = v.MenuChatItems.IndexOf(item);
                    if (indexItem > -1)
                    {
                        if (check2 != null)
                            check2.MenuChatItems[indexItem] = v.IsPin ? LocalResources.label5_UnPin : LocalResources.label5_Pin;
                      
                        v.MenuChatItems[indexItem] = v.IsPin ? LocalResources.label5_UnPin : LocalResources.label5_Pin;
                    }

                    var check = ListUtils.PinList.FirstOrDefault(a => a.ChatId == v.ChatId);
                    if (check != null)
                    {
                        ListUtils.PinList.Remove(check);

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.InsertORDelete_Pin(check);
                    }
                }
            }
        });

        /// <summary>
        /// Archive Chat Command
        /// </summary>
        protected ICommand _archiveChatCommand;

        public ICommand ArchiveChatCommand => _archiveChatCommand ??= new RelayCommand(parameter =>
        {
            if (parameter is UserListFody v)
            {
                if (!ArchivedChats.Contains(v))
                {
                    //Remember, Chat will be removed from Pinned List when Archived.. and Vice Versa...                    

                    //Add Chat in Archive List 
                    ArchivedChats.Add(v);
                    v.IsArchive = true;
                    v.IsPin = false;

                    //Remove Chat from Pinned & Unpinned Chat List
                    Chats.Remove(v);
                    FilteredChats.Remove(v);
                    PinnedChats.Remove(v);
                    FilteredPinnedChats.Remove(v);

                    //Update Lists
                    OnPropertyChanged(nameof(Chats));
                    OnPropertyChanged(nameof(FilteredChats));
                    OnPropertyChanged(nameof(PinnedChats));
                    OnPropertyChanged(nameof(FilteredPinnedChats));
                    OnPropertyChanged(nameof(ArchivedChats));

                    //Send Api
                    var dictionary = new Dictionary<string, string>
                    {
                        {"archive", "yes"},
                    };

                    if (v.Mute != null)
                    {
                        dictionary.Add("call_chat", v.Mute.CallChat);
                        dictionary.Add("pin", "no");
                        dictionary.Add("notify", v.Mute.Notify);
                    }

                    if (Methods.CheckForInternetConnection())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.MuteChatsInfoAsync(v.ChatId, v.ChatType, dictionary) });

                    var check2 = ListUtils.ListUsers.FirstOrDefault(a => a.ChatId == v.ChatId);
                    if (check2 != null)
                    {
                        check2.IsArchive = true;
                        check2.IsPin = false;
                    }

                    var item = v.MenuChatItems.FirstOrDefault(s => s.Contains(LocalResources.label5_UnArchive) || s.Contains(LocalResources.label5_Archive));
                    var indexItem = v.MenuChatItems.IndexOf(item);
                    if (indexItem > -1)
                    {
                        if (check2 != null)
                        {
                            check2.MenuChatItems[indexItem] = v.IsArchive ? LocalResources.label5_UnArchive : LocalResources.label5_Archive;
                        }

                        v.MenuChatItems[indexItem] = v.IsArchive ? LocalResources.label5_UnArchive : LocalResources.label5_Archive;
                    }

                    var check = ListUtils.ArchiveList.FirstOrDefault(a => a.ChatId == v.ChatId);
                    if (check == null)
                    {
                        ListUtils.ArchiveList.Add(new Classes.LastChatArchive()
                        {
                            ChatType = v.ChatType,
                            ChatId = v.ChatId,
                            UserId = v.UserId,
                            GroupId = v.GroupId,
                            PageId = v.PageId,
                            Name = v.Name,
                            IdLastMessage = v?.LastMessage.LastMessageClass?.Id.ToString() ?? "",
                            LastChat = v,
                        });

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.InsertORDelete_Archive(ListUtils.ArchiveList.LastOrDefault());
                    }
                }
            }
        });

        /// <summary>
        /// UnArchive Chat Command
        /// </summary>
        protected ICommand _UnArchiveChatCommand;

        public ICommand UnArchiveChatCommand => _UnArchiveChatCommand ??= new RelayCommand(parameter =>
        {
            if (parameter is UserListFody v)
            {
                if (!FilteredChats.Contains(v) /*&& !Chats.Contains(v)*/)
                {
                    //Chats.Add(v);
                    FilteredChats.Add(v);
                }
                
                ArchivedChats.Remove(v);
                v.IsArchive = false;
                v.IsPin = false;

                OnPropertyChanged(nameof(Chats));
                OnPropertyChanged(nameof(FilteredChats));
                OnPropertyChanged(nameof(ArchivedChats));

                //Send Api
                var dictionary = new Dictionary<string, string>
                {
                    {"archive", "no"},
                };

                if (v.Mute != null)
                {
                    dictionary.Add("call_chat", v.Mute.CallChat);
                    dictionary.Add("pin", "yes");
                    dictionary.Add("notify", v.Mute.Notify);
                }

                if (Methods.CheckForInternetConnection())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.MuteChatsInfoAsync(v.ChatId, v.ChatType, dictionary) });

                var check2 = ListUtils.ListUsers.FirstOrDefault(a => a.ChatId == v.ChatId);
                if (check2 != null)
                {
                    check2.IsArchive = false;
                    check2.IsPin = false;
                }

                var item = v.MenuChatItems.FirstOrDefault(s => s.Contains(LocalResources.label5_UnArchive) || s.Contains(LocalResources.label5_Archive));
                var indexItem = v.MenuChatItems.IndexOf(item);
                if (indexItem > -1)
                {
                    if (check2 != null)
                    {
                        check2.MenuChatItems[indexItem] = v.IsArchive ? LocalResources.label5_UnArchive : LocalResources.label5_Archive;
                    }

                    v.MenuChatItems[indexItem] = v.IsArchive ? LocalResources.label5_UnArchive : LocalResources.label5_Archive;
                }

                var check = ListUtils.ArchiveList.FirstOrDefault(a => a.ChatId == v.ChatId);
                if (check == null)
                {
                    ListUtils.ArchiveList.Remove(check);

                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertORDelete_Archive(check);
                } 
            }
        });

        #endregion

        //=========================================

        #region Search Chats

        protected bool _isSearchBoxOpen;

        public bool IsSearchBoxOpen
        {
            get => _isSearchBoxOpen;
            set
            {
                if (_isSearchBoxOpen == value)
                    return;

                _isSearchBoxOpen = value;


                if (_isSearchBoxOpen == false)
                    //Clear Search Box
                    SearchText = string.Empty;
                OnPropertyChanged(nameof(IsSearchBoxOpen));
                OnPropertyChanged(nameof(SearchText));
            }
        }

        protected string LastSearchText { get; set; }
        protected string mSearchText { get; set; }

        public string SearchText
        {
            get => mSearchText;
            set
            {

                //checked if value is different
                if (mSearchText == value)
                    return;

                //Update Value
                mSearchText = value;

                //if search text is empty restore messages
                if (string.IsNullOrEmpty(SearchText))
                    Search();
            }
        }

        public void OpenSearchBox()
        {
            IsSearchBoxOpen = true;
        }

        public void ClearSearchBox()
        {
            if (!string.IsNullOrEmpty(SearchText))
                SearchText = string.Empty;
            else
                CloseSearchBox();
        }

        public void CloseSearchBox() => IsSearchBoxOpen = false;

        public void Search()
        {
            //To avoid re searching same text again
            if (string.IsNullOrEmpty(LastSearchText) && string.IsNullOrEmpty(SearchText) || string.Equals(LastSearchText, SearchText))
                return;

            //If searchbox is empty or chats is null pr chat cound less than 0
            if (string.IsNullOrEmpty(SearchText) || Chats == null || Chats.Count <= 0)
            {
                FilteredChats = new ObservableCollection<UserListFody>(Chats ?? Enumerable.Empty<UserListFody>());
                OnPropertyChanged(nameof(FilteredChats));

                FilteredPinnedChats = new ObservableCollection<UserListFody>(PinnedChats ?? Enumerable.Empty<UserListFody>());
                OnPropertyChanged(nameof(FilteredPinnedChats));
                //Update Last search Text
                LastSearchText = SearchText;

                return;
            }

            //Now, to find all chats that contain the text in our search box

            //if that chat is in Normal Unpinned Chat list find there...
            FilteredChats = new ObservableCollection<UserListFody>(Chats.Where(
                chat => chat.Name.ToLower().Contains(SearchText) //if ContactName Contains SearchText then add it in filtered chat list
                        || chat.LastMessageText != null && chat.LastMessageText.ToLower().Contains(SearchText) //if Message Contains SearchText then add it in filtered chat list
            ));
            OnPropertyChanged(nameof(FilteredChats));

            //else if not found in Normal Unpinned Chat list, find in pinned chats list
            FilteredPinnedChats = new ObservableCollection<UserListFody>(PinnedChats.Where(pinnedchat => pinnedchat.Name.ToLower().Contains(SearchText) //if ContactName Contains SearchText then add it in filtered chat list
                                                                                                                                                        //|| pinnedchat.LastMessageText != null && pinnedchat.LastMessageText.ToLower().Contains(SearchText) //if Message Contains SearchText then add it in filtered chat list
                    ));

            OnPropertyChanged(nameof(FilteredPinnedChats));

            //Update Last search Text
            LastSearchText = SearchText;
        }



        /// <summary>
        /// Search Command
        /// </summary>
        protected ICommand _openSearchCommand;

        public ICommand OpenSearchCommand
        {
            get
            {
                if (_openSearchCommand == null)
                    _openSearchCommand = new CommandViewModel(OpenSearchBox);
                return _openSearchCommand;
            }
            set { _openSearchCommand = value; }
        }

        /// <summary>
        /// Clear Search Command
        /// </summary>
        protected ICommand _clearSearchCommand;

        public ICommand ClearSearchCommand
        {
            get
            {
                if (_clearSearchCommand == null)
                    _clearSearchCommand = new CommandViewModel(ClearSearchBox);
                return _clearSearchCommand;
            }
            set { _clearSearchCommand = value; }
        }

        /// <summary>
        /// Close Search Command
        /// </summary>
        protected ICommand _closeSearchCommand;

        public ICommand CloseSearchCommand
        {
            get
            {
                if (_closeSearchCommand == null)
                    _closeSearchCommand = new CommandViewModel(CloseSearchBox);
                return _closeSearchCommand;
            }
            set { _closeSearchCommand = value; }
        }

        /// <summary>
        /// Search Command
        /// </summary>
        protected ICommand _searchCommand;

        public ICommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                    _searchCommand = new CommandViewModel(Search);
                return _searchCommand;
            }
            set { _searchCommand = value; }
        }
        #endregion

        #region Window: More options Popup Chat 

        //This is our list containing the Window Options..
        private ObservableCollection<Classes.MoreOptionsMenu> _windowMoreOptionsMenuList;

        public ObservableCollection<Classes.MoreOptionsMenu> WindowMoreOptionsMenuList
        {
            get { return _windowMoreOptionsMenuList; }
            set { _windowMoreOptionsMenuList = value; }
        }

        void WindowMoreOptionsMenu()
        {
            WindowMoreOptionsMenuList = new ObservableCollection<Classes.MoreOptionsMenu>()
            { 
                new Classes.MoreOptionsMenu()
                {
                    Icon = (PathGeometry) Dictionary["refresh"],
                    MenuText = LocalResources.label_Refresh
                }, 
                new Classes.MoreOptionsMenu()
                {
                    Icon = (PathGeometry) Dictionary["settings"],
                    MenuText = LocalResources.label_Settings
                },
            };
            OnPropertyChanged(nameof(WindowMoreOptionsMenuList));
        }

        protected ICommand _windowsMoreOptionsCommand;

        public ICommand WindowsMoreOptionsCommand
        {
            get
            {
                if (_windowsMoreOptionsCommand == null)
                    _windowsMoreOptionsCommand = new CommandViewModel(WindowMoreOptionsMenu);
                return _windowsMoreOptionsCommand;
            }
            set { _windowsMoreOptionsCommand = value; }
        }

        //======================= Popup  Conversation ==========================

        void ConversationScreenMoreOptionsMenu()
        {  
            //To populate menu items for conversation screen options list..
            WindowMoreOptionsMenuList = new ObservableCollection<Classes.MoreOptionsMenu>()
            {
                new Classes.MoreOptionsMenu()
                {
                    Icon = ChatWindow.TypeChatPage == "user" ? (PathGeometry) Dictionary["allmedia"] : (PathGeometry) Dictionary["info"],
                    MenuText = ChatWindow.TypeChatPage == "user" ? LocalResources.label5_AllMedia : LocalResources.label5_InfoGroup
                } 
               
                //new Classes.MoreOptionsMenu()
                //{
                //    Icon = (PathGeometry) Dictionary["report"],
                //    MenuText = "Report"
                //},
               
                //new Classes.MoreOptionsMenu()
                //{
                //    Icon = (PathGeometry) Dictionary["exportchat"],
                //    MenuText = "Export Chat"
                //},
            };

            if (ChatWindow.TypeChatPage == "group")
            {
                WindowMoreOptionsMenuList.Add(new Classes.MoreOptionsMenu()
                {
                    Icon = (PathGeometry)Dictionary["exit"],
                    MenuText = LocalResources.label5_ExitGroup
                });
            }
             
            WindowMoreOptionsMenuList.Add(new Classes.MoreOptionsMenu()
            {
                Icon = (PathGeometry)Dictionary["wallpaper"],
                MenuText = LocalResources.label5_ChangeWallpaper
            });

            if (ChatWindow.TypeChatPage == "user")
            {
                WindowMoreOptionsMenuList.Add(new Classes.MoreOptionsMenu()
                {
                    Icon = (PathGeometry)Dictionary["starredmessages"],
                    MenuText = LocalResources.label5_StartedMessages
                });
                 
                WindowMoreOptionsMenuList.Add(new Classes.MoreOptionsMenu()
                {
                    Icon = (PathGeometry)Dictionary["block"],
                    MenuText = LocalResources.label5_Block
                }); 
                 
                WindowMoreOptionsMenuList.Add(new Classes.MoreOptionsMenu()
                {
                    Icon = (PathGeometry)Dictionary["clearchat"],
                    MenuText = LocalResources.label5_ClearChat
                });
            } 
            
            OnPropertyChanged(nameof(WindowMoreOptionsMenuList));
        }

        protected ICommand _conversationScreenMoreOptionsCommand;

        public ICommand ConversationScreenMoreOptionsCommand
        {
            get
            {
                if (_conversationScreenMoreOptionsCommand == null)
                    _conversationScreenMoreOptionsCommand = new CommandViewModel(ConversationScreenMoreOptionsMenu);
                return _conversationScreenMoreOptionsCommand;
            }
            set { _conversationScreenMoreOptionsCommand = value; }
        }

        #endregion

        #region Attachment message

        //This is our list containing the Attachment Menu Options..
        private ObservableCollection<Classes.MoreOptionsMenu> _attachmentOptionsMenuList;

        public ObservableCollection<Classes.MoreOptionsMenu> AttachmentOptionsMenuList
        {
            get { return _attachmentOptionsMenuList; }
            set { _attachmentOptionsMenuList = value; }
        }


        protected ICommand _attachmentOptionsCommand;

        public ICommand AttachmentOptionsCommand
        {
            get
            {
                if (_attachmentOptionsCommand == null)
                    _attachmentOptionsCommand = new CommandViewModel(AttachmentOptionsMenu);
                return _attachmentOptionsCommand;
            }
            set { _attachmentOptionsCommand = value; }
        }


        void AttachmentOptionsMenu()
        {
            //To populate menu items for Attachment Menu options list..
            AttachmentOptionsMenuList = new ObservableCollection<Classes.MoreOptionsMenu>()
            {
                new Classes.MoreOptionsMenu()
                {
                    Icon = (PathGeometry) Dictionary["docs"],
                    MenuText = LocalResources.label5_Docs,
                    BorderStroke = "#3F3990",
                    Fill = "#CFCEEC"
                },
                new Classes.MoreOptionsMenu()
                {
                    Icon = (PathGeometry) Dictionary["camera"],
                    MenuText = LocalResources.label5_Camera,
                    BorderStroke = "#2C5A71",
                    Fill = "#C5E7F8"
                },
                new Classes.MoreOptionsMenu()
                {
                    Icon = (PathGeometry) Dictionary["gallery"],
                    MenuText =LocalResources. label5_Gallery,
                    BorderStroke = "#EA2140",
                    Fill = "#F3BEBE"
                },
                new Classes.MoreOptionsMenu()
                {
                    Icon = (PathGeometry) Dictionary["audio"],
                    MenuText = LocalResources.label5_Audio,
                    BorderStroke = "#E67E00",
                    Fill = "#F7D5AC"
                },
                //new Classes.MoreOptionsMenu()
                //{
                //    Icon = (PathGeometry) Dictionary["location"],
                //    MenuText = LocalResources.label_location,
                //    BorderStroke = "#28C58F",
                //    Fill = "#E3F5EF"
                //},
                new Classes.MoreOptionsMenu()
                {
                    Icon = (PathGeometry) Dictionary["contact"],
                    MenuText = LocalResources.label5_Contact,
                    BorderStroke = "#0093E0",
                    Fill = "#DDF1FB"
                }
            };
            
            OnPropertyChanged(nameof(AttachmentOptionsMenuList));
        }
         
        #endregion

        #region Conversations , Search , Reply

        protected bool _isConversationSearchBoxOpen;

        public bool IsConversationSearchBoxOpen
        {
            get => _isConversationSearchBoxOpen;
            set
            {
                if (_isConversationSearchBoxOpen == value)
                    return;

                _isConversationSearchBoxOpen = value;


                if (_isConversationSearchBoxOpen == false)
                    //Clear Search Box
                    SearchConversationText = string.Empty;
                OnPropertyChanged(nameof(IsConversationSearchBoxOpen));
                OnPropertyChanged(nameof(SearchConversationText));
            }
        }

        public ObservableCollection<MessagesDataFody> Conversations
        {
            get => ListUtils.ListMessages;
            set
            {
                //To Change the list
                if (ListUtils.ListMessages == value)
                    return;

                //To Update the list
                ListUtils.ListMessages = value;

                //Updating filtered chats to match
                FilteredConversations = new ObservableCollection<MessagesDataFody>(ListUtils.ListMessages);
                OnPropertyChanged(nameof(Conversations));
                OnPropertyChanged(nameof(FilteredConversations));
            }
        }

        /// <summary>
        /// Filter Conversation
        /// </summary>
        public ObservableCollection<MessagesDataFody> FilteredConversations { get; set; }

        protected string LastSearchConversationText;
        protected string mSearchConversationText;

        public string SearchConversationText
        {
            get => mSearchConversationText;
            set
            {

                //checked if value is different
                if (mSearchConversationText == value)
                    return;

                //Update Value
                mSearchConversationText = value;

                //if search text is empty restore messages
                if (string.IsNullOrEmpty(SearchConversationText))
                    SearchInConversation();
            }
        }

        public bool FocusMessageBox { get; set; }
        public bool IsThisAReplyMessage { get; set; }
        public string MessageToReplyText { get; set; }

        protected bool _isSearchConversationBoxOpen;

        public bool IsSearchConversationBoxOpen
        {
            get => _isSearchConversationBoxOpen;
            set
            {
                if (_isSearchConversationBoxOpen == value)
                    return;

                _isSearchConversationBoxOpen = value;
                
                if (_isSearchConversationBoxOpen == false)
                    //Clear Search Box
                    SearchConversationText = string.Empty;

                OnPropertyChanged(nameof(IsSearchConversationBoxOpen));
                OnPropertyChanged(nameof(SearchConversationText));
            }
        }

        public void OpenConversationSearchBox()
        {
            IsSearchConversationBoxOpen = true;
        }

        public void ClearConversationSearchBox()
        {
            if (!string.IsNullOrEmpty(SearchConversationText))
                SearchConversationText = string.Empty;
            else
                CloseConversationSearchBox();
        }

        public void CloseConversationSearchBox() => IsSearchConversationBoxOpen = false;

        public async void LoadMessagesDataFody(UserListFody chat, string type)
        {
            ChatWindow.ChatWindowContext.MessagesTabItem.IsSelected = true;

            Conversations ??= new ObservableCollection<MessagesDataFody>();
            FilteredConversations ??= new ObservableCollection<MessagesDataFody>();

            Conversations.Clear();
            FilteredConversations.Clear();
             
            //Reset reply message text when the new chat is fetched
            MessageToReplyText = string.Empty;
            OnPropertyChanged(nameof(MessageToReplyText));

            await ChatWindow.ChatWindowContext.OpenChat(chat, type);

            UpdateMessage(true);
        }

        public void UpdateMessage(bool scrollToBottom)
        {
            Conversations = ListUtils.ListMessages;
            FilteredConversations = ListUtils.ListMessages;

            UpdateChangedMessage();

            //Reset reply message text when the new chat is fetched
            CancelReply();

            //Scroll Down >> 
            if (scrollToBottom && ListUtils.ListMessages.Count > 0)
            {
                var list = ConversationControl.ControlInstance.ChatMessgaeslistBox;

                list.SelectedIndex = ListUtils.ListMessages.Count - 1;
                list.ScrollIntoView(list.SelectedItem);
            }
        }

        void SearchInConversation()
        {
            //To avoid re searching same text again
            if (string.IsNullOrEmpty(LastSearchConversationText) && string.IsNullOrEmpty(SearchConversationText) || string.Equals(LastSearchConversationText, SearchConversationText))
                return;

            //If searchBox is empty or Conversations is null pr chat cound less than 0
            if (string.IsNullOrEmpty(SearchConversationText) || Conversations == null || Conversations.Count <= 0)
            {
                FilteredConversations = new ObservableCollection<MessagesDataFody>(Conversations ?? Enumerable.Empty<MessagesDataFody>());
                OnPropertyChanged(nameof(FilteredConversations));

                //Update Last search Text
                LastSearchConversationText = SearchConversationText;

                return;
            }

            //Now, to find all Conversations that contain the text in our search box 
            FilteredConversations = new ObservableCollection<MessagesDataFody>(Conversations.Where(chat => chat.Text.ToLower().Contains(SearchConversationText) || chat.Text.ToLower().Contains(SearchConversationText)));
            OnPropertyChanged(nameof(FilteredConversations));

            //Update Last search Text
            LastSearchConversationText = SearchConversationText;
        }

        public void CancelReply()
        {
            ChatWindow.ReplyId = string.Empty;

            IsThisAReplyMessage = false;
            //Reset Reply Message Text
            MessageToReplyText = string.Empty;
            OnPropertyChanged(nameof(MessageToReplyText));
        }

        public void OpenReply()
        {
            MessageToReplyText = "aaa";

            //update
            OnPropertyChanged(nameof(MessageToReplyText));

            //Set focus on Message Box when user clicks reply button
            FocusMessageBox = true;
            OnPropertyChanged(nameof(FocusMessageBox));

            //Flag this message as reply message
            IsThisAReplyMessage = true;
            OnPropertyChanged(nameof(IsThisAReplyMessage));
        }

        public void SendMessage()
        { 
            try
            {
                //Send message only when the textbox is not empty..
                if (Methods.CheckForInternetConnection())
                {
                    TextRange textRange = new TextRange(ChatWindow.ChatWindowContext.MessageBoxText.Document.ContentStart, ChatWindow.ChatWindowContext.MessageBoxText.Document.ContentEnd);
                    string textMsg = textRange.Text;

                    if (string.IsNullOrEmpty(textMsg) || string.IsNullOrWhiteSpace(textMsg) || textMsg.Contains(LocalResources.label_MessageBoxText))
                    {
                    }
                    else
                    {
                        App.Current.Dispatcher?.Invoke((Action)delegate // <--- HERE
                        {
                            try
                            {

                                StringWriter myWriter = new StringWriter();
                                HttpUtility.HtmlDecode(textRange.Text, myWriter);
                                textMsg = myWriter.ToString();

                                if (ChatWindow.TypeChatPage == "user")
                                {
                                    int index = ListUtils.ListUsers.IndexOf(ListUtils.ListUsers.FirstOrDefault(a => a.UserId == ChatWindow.UserId));
                                    if (index > -1 && index != 0)
                                    {
                                        ListUtils.ListUsers.Move(index, 0);
                                    }

                                    var updater2 = ListUtils.ListUsers.FirstOrDefault(d => d.UserId == ChatWindow.UserId);
                                    if (updater2 != null)
                                    {
                                        updater2.LastMessage.LastMessageClass.Text = textMsg;

                                        //Update
                                        OnPropertyChanged(nameof(Chats));
                                        OnPropertyChanged();
                                    }

                                    //NoMessagePanel.Visibility = Visibility.Hidden;

                                    if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                                    {
                                        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                        string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);
                                        string time = DateTime.Now.ToString("hh:mm");

                                        var conversation = new MessagesDataFody()
                                        {
                                            Id = unixTimestamp,
                                            FromId = UserDetails.UserId,
                                            Img_user_message = UserDetails.Avatar,
                                            ToId = ChatWindow.UserId,

                                            Text = textMsg,

                                            Time = time,
                                            TimeText = time,
                                            Position = "right",
                                            ModelType = MessageModelType.RightText,
                                            ChatColor = ChatWindow.ChatWindowContext.ChatColor,
                                            EndChatColor = Settings.ColorMessageThemeGradient ? WoWonderTools.GetColorEnd(ChatWindow.ChatWindowContext.ChatColor) : ChatWindow.ChatWindowContext.ChatColor,

                                            Seen = "-1",
                                            SendFile = true,
                                            ErrorSendMessage = false,
                                            ReplyId = ChatWindow.ReplyId,
                                        };

                                        //Add message to conversation list
                                        ListUtils.ListMessages.Add(WoWonderTools.MessageFilter(ChatWindow.UserId, ViewModel.ModelInstance.Name, conversation, conversation.ChatColor));

                                        UpdateChatAllList(conversation);

                                        UserDetails.Socket?.EmitAsync_SendMessage(ChatWindow.UserId, UserDetails.AccessToken, UserDetails.Username, textMsg, ChatWindow.ChatWindowContext.ChatColor, ChatWindow.ReplyId, time2);

                                        //Update
                                        UpdateMessage(true);
                                    }
                                    else
                                        ChatWindow.ChatWindowContext.MethodToSendMessages(textMsg);

                                }
                                else
                                {
                                    int index = ListUtils.ListGroup.IndexOf(ListUtils.ListGroup.FirstOrDefault(a => a.GroupId == ChatWindow.GroupId));
                                    if (index > -1 && index != 0)
                                    {
                                        ListUtils.ListGroup.Move(index, 0);
                                    }
                                     
                                    //NoMessagePanel.Visibility = Visibility.Hidden;

                                    if (Settings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                                    {
                                        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                        string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);
                                        string time = DateTime.Now.ToString("hh:mm");

                                        var conversation = new MessagesDataFody()
                                        {
                                            Id = unixTimestamp,
                                            FromId = UserDetails.UserId,
                                            Img_user_message = UserDetails.Avatar,
                                            GroupId = ChatWindow.GroupId,
                                            ToId = ChatWindow.UserId,

                                            Text = textMsg,

                                            Time = time,
                                            TimeText = time,
                                            Position = "right",
                                            ModelType = MessageModelType.RightText,
                                            ChatColor = ChatWindow.ChatWindowContext.ChatColor,
                                            EndChatColor = Settings.ColorMessageThemeGradient ? WoWonderTools.GetColorEnd(ChatWindow.ChatWindowContext.ChatColor) : ChatWindow.ChatWindowContext.ChatColor,

                                            Seen = "-1",
                                            SendFile = true,
                                            ErrorSendMessage = false,
                                            ReplyId = ChatWindow.ReplyId,
                                        };

                                        //Add message to conversation list
                                        ListUtils.ListMessages.Add(WoWonderTools.MessageFilter(ChatWindow.GroupId, UserDetails.FullName, conversation, conversation.ChatColor , "group"));

                                        UpdateChatAllList(conversation);

                                        UserDetails.Socket?.EmitAsync_SendGroupMessage(ChatWindow.GroupId, UserDetails.AccessToken, UserDetails.Username, textMsg, ChatWindow.ReplyId, time2);

                                        //Update
                                        UpdateMessage(true);
                                    }
                                    else
                                        ChatWindow.ChatWindowContext.MethodToSendMessages(textMsg);
                                }
                                 
                                ChatWindow.ChatWindowContext.MessageBoxText.Document.Blocks.Clear();
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                    }
                }
                else
                {
                    MessageBox.Show(LocalResources.label_Please_check_your_internet);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void UpdateChatAllList(MessagesDataFody conversation)
        {
            UpdateChatAndMoveUp(Chats, conversation);
            UpdateChatAndMoveUp(PinnedChats, conversation);
            UpdateChatAndMoveUp(FilteredChats, conversation);
            UpdateChatAndMoveUp(FilteredPinnedChats, conversation);
            UpdateChatAndMoveUp(ArchivedChats, conversation);
        }

        public void UpdateChangedMessage()
        {
            //Update
            OnPropertyChanged(nameof(FilteredConversations));
            OnPropertyChanged(nameof(Conversations));
            OnPropertyChanged(nameof(IsThisAReplyMessage));
            OnPropertyChanged(nameof(MessageToReplyText));
        }

        //Move the chat contact on top when new message is sent or received
        public void UpdateChatAndMoveUp(ObservableCollection<UserListFody> chatList, MessagesDataFody conversation)
        {
            Application.Current.Dispatcher?.Invoke(delegate
            {
                try
                {
                    //Check if the message sent is to the selected contact or not...
                    var chat = chatList.FirstOrDefault(x => x.Name == Name);
                    //wael 
                    //if found..then..
                    if (chat != null)
                    {
                        //Update Contact Chat Last Message and Message Time..
                        chat.LastMessageText = conversation.Text;
                        chat.LastSeenTimeText = conversation.TimeText;

                        //Move Chat on top when new message is received/sent...
                        chatList.Move(chatList.IndexOf(chat), 0);

                        //Update Collections
                        OnPropertyChanged(nameof(Chats));
                        OnPropertyChanged(nameof(PinnedChats));
                        OnPropertyChanged(nameof(FilteredChats));
                        OnPropertyChanged(nameof(FilteredPinnedChats));
                        OnPropertyChanged(nameof(ArchivedChats));
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        /// <summary>
        /// Search Command
        /// </summary>
        protected ICommand _openConversationSearchCommand;

        public ICommand OpenConversationSearchCommand
        {
            get
            {
                if (_openConversationSearchCommand == null)
                    _openConversationSearchCommand = new CommandViewModel(OpenConversationSearchBox);
                return _openConversationSearchCommand;
            }
            set { _openConversationSearchCommand = value; }
        }

        /// <summary>
        /// Clear Search Command
        /// </summary>
        protected ICommand _clearConversationSearchCommand;

        public ICommand ClearConversationSearchCommand
        {
            get
            {
                if (_clearConversationSearchCommand == null)
                    _clearConversationSearchCommand = new CommandViewModel(ClearConversationSearchBox);
                return _clearConversationSearchCommand;
            }
            set { _clearConversationSearchCommand = value; }
        }

        /// <summary>
        /// Close Search Command
        /// </summary>
        protected ICommand _closeConversationSearchCommand;

        public ICommand CloseConversationSearchCommand
        {
            get
            {
                if (_closeConversationSearchCommand == null)
                    _closeConversationSearchCommand = new CommandViewModel(CloseConversationSearchBox);
                return _closeConversationSearchCommand;
            }
            set { _closeConversationSearchCommand = value; }
        }

        protected ICommand _searchConversationCommand;

        public ICommand SearchConversationCommand
        {
            get
            {
                if (_searchConversationCommand == null)
                    _searchConversationCommand = new CommandViewModel(SearchInConversation);
                return _searchConversationCommand;
            }
            set { _searchConversationCommand = value; }
        }

        protected ICommand _cancelReplyCommand;

        public ICommand CancelReplyCommand
        {
            get
            {
                if (_cancelReplyCommand == null)
                    _cancelReplyCommand = new CommandViewModel(CancelReply);
                return _cancelReplyCommand;
            }
            set { _cancelReplyCommand = value; }
        }

        protected ICommand _sendMessageCommand;

        public ICommand SendMessageCommand
        {
            get
            {
                if (_sendMessageCommand == null)
                    _sendMessageCommand = new CommandViewModel(SendMessage);
                return _sendMessageCommand;
            }
            set { _sendMessageCommand = value; }
        }

        #endregion

        #region ContactInfo

        protected bool _IsContactInfoOpen;

        public bool IsContactInfoOpen
        {
            get => _IsContactInfoOpen;
            set
            {
                _IsContactInfoOpen = value;
                OnPropertyChanged(nameof(IsContactInfoOpen));
            }
        }

        public void OpenContactInfo()
        {
            try
            {
                if (ChatWindow.TypeChatPage == "user")
                {
                    IsContactInfoOpen = true;
                }
                else
                {
                    CloseContactInfo();

                    if (ChatWindow.ChatWindowContext.SelectedChatData != null)
                    {
                        if (ChatWindow.ChatWindowContext.SelectedChatData.Owner != null && ChatWindow.ChatWindowContext.SelectedChatData.Owner.Value)
                        {
                            EditGroupChatWindow editGroupChatWindow = new EditGroupChatWindow(ChatWindow.ChatWindowContext.SelectedChatData);
                            editGroupChatWindow.ShowDialog();
                        }
                        else
                        {
                            GroupChatProfileWindow chatProfileWindow = new GroupChatProfileWindow(ChatWindow.ChatWindowContext.SelectedChatData);
                            chatProfileWindow.ShowDialog();
                        }
                    }
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e); 
            }
        }

        public void CloseContactInfo() => IsContactInfoOpen = false;

        /// <summary>
        /// Open ContactInfo Command
        /// </summary>
        protected ICommand _openContactInfoCommand;

        public ICommand OpenContactinfoCommand
        {
            get
            {
                if (_openContactInfoCommand == null)
                    _openContactInfoCommand = new CommandViewModel(OpenContactInfo);
                return _openContactInfoCommand;
            }
            set { _openContactInfoCommand = value; }
        }

        /// <summary>
        /// Open ContactInfo Command
        /// </summary>
        protected ICommand _closeontactInfoCommand;

        public ICommand CloseContactinfoCommand
        {
            get
            {
                if (_closeontactInfoCommand == null)
                    _closeontactInfoCommand = new CommandViewModel(CloseContactInfo);
                return _closeontactInfoCommand;
            }
            set { _closeontactInfoCommand = value; }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}