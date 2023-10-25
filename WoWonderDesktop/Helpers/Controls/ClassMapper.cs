using System;
using AutoMapper;
using WoWonderClient.Classes.Event;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.SQLiteDB;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;

namespace WoWonderDesktop.Helpers.Controls
{
    public static class ClassMapper
    {
        public static IMapper Mapper;
        public static void SetMappers()
        {
            try
            {
                var configuration = new MapperConfiguration(cfg =>
                {
                    try
                    {
                        cfg.AllowNullCollections = true;
                         
                        cfg.CreateMap<UserDataFody, UserDataObject>();
                        cfg.CreateMap<UserDataObject, UserDataFody>();
                         
                        cfg.CreateMap<MessagesDataFody, MessageData>().ForMember(x => x.ReactionMessageItems, opt => opt.Ignore()).ForMember(x => x.ReactionImage, opt => opt.Ignore());
                        cfg.CreateMap<MessageData, MessagesDataFody>().ForMember(x => x.ReactionMessageItems, opt => opt.Ignore()).ForMember(x => x.ReactionImage, opt => opt.Ignore());

                        cfg.CreateMap<ChatObject, UserListFody>();
                        cfg.CreateMap<UserListFody, ChatObject>();

                        cfg.CreateMap<FetchSessionsObject.SessionsDataObject, SessionDataFody>();
                        cfg.CreateMap<SessionDataFody, FetchSessionsObject.SessionsDataObject>();

                        cfg.CreateMap<EventDataObject, EventDataFody>();
                        cfg.CreateMap<EventDataFody, EventDataObject>();

                        cfg.CreateMap<GetSiteSettingsObject.ConfigObject, DataTables.SettingsTb>().ForMember(x => x.AutoIdSettings, opt => opt.Ignore());
                        cfg.CreateMap<UserDataObject, DataTables.MyProfileTb>().ForMember(x => x.AutoIdMyProfile, opt => opt.Ignore());
                        cfg.CreateMap<UserDataObject, DataTables.MyContactsTb>().ForMember(x => x.AutoIdMyFollowing, opt => opt.Ignore());
                        cfg.CreateMap<ChatObject, DataTables.LastUsersTb>().ForMember(x => x.AutoIdLastUsers, opt => opt.Ignore());
                        cfg.CreateMap<MessageData, DataTables.MessageTb>().ForMember(x => x.AutoIdMessage, opt => opt.Ignore());
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                
                // only during development, validate your mappings; remove it before release
                //configuration.AssertConfigurationIsValid();

                Mapper = configuration.CreateMapper();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    } 
}