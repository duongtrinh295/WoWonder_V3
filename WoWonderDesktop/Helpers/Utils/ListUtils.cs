using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.SQLiteDB;
using WoWonderClient.Classes.Global;

namespace WoWonderDesktop.Helpers.Utils
{
    public static class ListUtils
    {
        public static DataTables.SettingsAppTb SettingsApp = new DataTables.SettingsAppTb();
        public static GetSiteSettingsObject.ConfigObject SettingsSiteList = new GetSiteSettingsObject.ConfigObject();
        public static ObservableCollection<DataTables.LoginTb> DataUserLoginList = new ObservableCollection<DataTables.LoginTb>();
        public static ObservableCollection<Classes.Categories> ListCategoriesProducts = new ObservableCollection<Classes.Categories>();
        public static ObservableCollection<UserDataFody> MyProfileList = new ObservableCollection<UserDataFody>();
        public static ObservableCollection<UserListFody> ListUsers = new ObservableCollection<UserListFody>();
        public static ObservableCollection<UserListFody> ListGroup = new ObservableCollection<UserListFody>();
        public static ObservableCollection<UserDataFody> ListSearch = new ObservableCollection<UserDataFody>();
        public static ObservableCollection<UserDataFody> ListUsersProfile = new ObservableCollection<UserDataFody>();
        public static ObservableCollection<UserDataFody> ListUsersBlock = new ObservableCollection<UserDataFody>();
        public static ObservableCollection<UserDataFody> ListNearbyUsers= new ObservableCollection<UserDataFody>();
        public static ObservableCollection<Classes.SharedFile> ListSharedFiles = new ObservableCollection<Classes.SharedFile>();
        public static ObservableCollection<Classes.SharedFile> LastSharedFiles = new ObservableCollection<Classes.SharedFile>();
        public static ObservableCollection<MessagesDataFody> ListMessages = new ObservableCollection<MessagesDataFody>();
        public static ObservableCollection<GifGiphyClass.Datum> ListGifs = new ObservableCollection<GifGiphyClass.Datum>();
        public static ObservableCollection<Classes.CallVideo> ListCall = new ObservableCollection<Classes.CallVideo>();

        public static ObservableCollection<Classes.OptionLastChat> MuteList = new ObservableCollection<Classes.OptionLastChat>();
        public static ObservableCollection<Classes.LastChatArchive> PinList = new ObservableCollection<Classes.LastChatArchive>();
        public static ObservableCollection<Classes.LastChatArchive> ArchiveList = new ObservableCollection<Classes.LastChatArchive>();

        public static ObservableCollection<UserDataFody> FriendRequestsList = new ObservableCollection<UserDataFody>();
        public static ObservableCollection<GroupChatRequest> GroupRequestsList = new ObservableCollection<GroupChatRequest>();
        public static ObservableCollection<UserDataFody> ListUsersForward = new ObservableCollection<UserDataFody>();
        public static ObservableCollection<MessagesDataFody> StartedMessageList = new ObservableCollection<MessagesDataFody>();
        public static ObservableCollection<MessagesDataFody> PinnedMessageList = new ObservableCollection<MessagesDataFody>();
        public static ObservableCollection<SessionDataFody> SessionsList = new ObservableCollection<SessionDataFody>();
        public static ObservableCollection<EventDataFody> EventsList = new ObservableCollection<EventDataFody>();
        public static ObservableCollection<UserDataFody> SelectMembersList = new ObservableCollection<UserDataFody>();
        public static ObservableCollection<DataTables.StickersTable> StickersList = new ObservableCollection<DataTables.StickersTable>();

        public static void ClearAllList()
        {
            try
            {
                SettingsSiteList = null;
                 
                DataUserLoginList = new ObservableCollection<DataTables.LoginTb>();
                ListCategoriesProducts = new ObservableCollection<Classes.Categories>();
                MyProfileList = new ObservableCollection<UserDataFody>();
                ListUsers = new ObservableCollection<UserListFody>();
                ListUsersBlock = new ObservableCollection<UserDataFody>();
                ListSearch = new ObservableCollection<UserDataFody>();
                ListUsersProfile = new ObservableCollection<UserDataFody>();
                ListSharedFiles = new ObservableCollection<Classes.SharedFile>();
                LastSharedFiles = new ObservableCollection<Classes.SharedFile>();
                ListMessages = new ObservableCollection<MessagesDataFody>();
                ListGifs = new ObservableCollection<GifGiphyClass.Datum>();
                ListCall = new ObservableCollection<Classes.CallVideo>(); 
                MuteList = new ObservableCollection<Classes.OptionLastChat>();
                PinList = new ObservableCollection<Classes.LastChatArchive>();
                ArchiveList = new ObservableCollection<Classes.LastChatArchive>();
                ListUsersForward = new ObservableCollection<UserDataFody>();
                StartedMessageList = new ObservableCollection<MessagesDataFody>();
                PinnedMessageList = new ObservableCollection<MessagesDataFody>();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void AddRange<T>(ObservableCollection<T> collection, IEnumerable<T> items)
        {
            try
            {
                items.ToList().ForEach(collection.Add);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }

        public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int n)
        {
            var enumerable = source as T[] ?? source.ToArray();

            return enumerable.Skip(Math.Max(0, enumerable.Count() - n));
        }

        public static void Copy<T>(T from, T to)
        {
            Type t = typeof(T);
            PropertyInfo[] props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in props)
            {
                try
                {
                    if (!p.CanRead || !p.CanWrite) continue;

                    var key = p.Name;
                    var val = p.GetGetMethod()?.Invoke(from, null);
                    var defaultVal = p.PropertyType.IsValueType ? Activator.CreateInstance(p.PropertyType) : null;
                    if (val != null && defaultVal != null && !val.Equals(defaultVal))
                    {
                        p.GetSetMethod()?.Invoke(to, new[] { val });
                    }
                    else
                    {
                        if (p.GetValue(from) == null)
                        {
                            continue;
                        }

                        try
                        {
                            to.GetType().GetProperty(key)?.SetValue(to, val);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }


        //public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        //{
        //    var list = new List<List<T>>();

        //    for (int i = 0; i < locations.Count; i += nSize)
        //    {
        //        list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
        //    }

        //    return list;
        //}

        //public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int n)
        //{
        //    var enumerable = source as T[] ?? source.ToArray();

        //    return enumerable.Skip(Math.Max(0, enumerable.Count() - n));
        //}


        //public static T Cast<T>(object myobj)
        //{
        //    Type objectType = myobj.GetType();
        //    Type target = typeof(UserDataObject);
        //    object x = Activator.CreateInstance(target, false);

        //    var z = from source in objectType.GetMembers().ToList()
        //            where source.MemberType == MemberTypes.Property
        //            select source;

        //    var d = from source in target.GetMembers().ToList()
        //            where source.MemberType == MemberTypes.Property
        //            select source;

        //    List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name).ToList().Contains(memberInfo.Name)).ToList();
        //    PropertyInfo propertyInfo;

        //    object value;
        //    foreach (var memberInfo in members)
        //    {
        //        propertyInfo = typeof(T).GetProperty(memberInfo.Name);
        //        value = myobj.GetType().GetProperty(memberInfo.Name)?.GetValue(myobj, null);

        //        if (propertyInfo != null)
        //            propertyInfo.SetValue(x, value, null);
        //    }
        //    return (T)x;
        //}

        public static int Remove<T>(this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            var itemsToRemove = coll.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }

        /// <summary>
        /// Extends ObservableCollection adding a RemoveAll method to remove elements based on a boolean condition function
        /// </summary>
        /// <typeparam name="T">The type contained by the collection</typeparam>
        /// <param name="observableCollection">The ObservableCollection</param>
        /// <param name="condition">A function that evaluates to true for elements that should be removed</param>
        /// <returns>The number of elements removed</returns>
        public static int RemoveAll<T>(this ObservableCollection<T> observableCollection, Func<T, bool> condition)
        {
            // Find all elements satisfying the condition, i.e. that will be removed
            var toRemove = observableCollection
                .Where(condition)
                .ToList();

            // Remove the elements from the original collection, using the Count method to iterate through the list, 
            // incrementing the count whenever there's a successful removal
            return toRemove.Count(observableCollection.Remove);
        }

        /// <summary>
        /// Extends ObservableCollection adding a RemoveAll method to remove elements based on a boolean condition function
        /// </summary>
        /// <typeparam name="T">The type contained by the collection</typeparam>
        /// <param name="observableCollection">The ObservableCollection</param>
        /// <param name="toRemove">Find all elements satisfying the condition, i.e. that will be removed</param>
        /// <returns>The number of elements removed</returns>
        public static int RemoveAll<T>(this ObservableCollection<T> observableCollection, List<T> toRemove)
        {
            // Remove the elements from the original collection, using the Count method to iterate through the list, 
            // incrementing the count whenever there's a successful removal
            return toRemove.Count(observableCollection.Remove);
        }
         
        public static void AddDoubleClickEventStyle(ListBox listBox, MouseButtonEventHandler mouseButtonEventHandler)
        {
            if (listBox.ItemContainerStyle == null)
                listBox.ItemContainerStyle = new Style(typeof(ListBoxItem));

            listBox.ItemContainerStyle.Setters.Add(new EventSetter()
            {
                Event = ListBoxItem.MouseDoubleClickEvent,
                Handler = mouseButtonEventHandler
            });
        } 
    }
} 