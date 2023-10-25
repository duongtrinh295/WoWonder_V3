using System;
using System.Windows;
using System.Windows.Controls;
using WoWonderDesktop.Helpers.Utils;
using WoWonderClient.Classes.Message;

namespace WoWonderDesktop.Helpers.Controls
{
    public class MessageDataTemplate : DataTemplateSelector
    {
        public  DataTemplate TextDataTemplate { get; set; }
        public DataTemplate ImageDataTemplate { get; set; }
        public DataTemplate SoundDataTemplate { get; set; }
        public DataTemplate VideoDataTemplate { get; set; }
        public DataTemplate ContactDataTemplate { get; set; }
        public DataTemplate StickerDataTemplate { get; set; }
        public DataTemplate FileDataTemplate { get; set; }
        public DataTemplate GifsDataTemplate { get; set; }
        public DataTemplate ProductsDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            try
            {

                var msg = item as MessageData;

                var modelType = WoWonderTools.GetTypeModel(msg);

                switch (modelType)
                {
                    case MessageModelType.LeftText:
                    case MessageModelType.RightText:
                        return TextDataTemplate;

                    case MessageModelType.RightMap:
                    case MessageModelType.RightImage:
                    case MessageModelType.LeftMap:
                    case MessageModelType.LeftImage:
                        return ImageDataTemplate;

                    case MessageModelType.RightFile:
                    case MessageModelType.LeftFile:
                        return FileDataTemplate;

                    case MessageModelType.RightVideo:
                    case MessageModelType.LeftVideo:
                        return VideoDataTemplate;

                    case MessageModelType.RightAudio:
                    case MessageModelType.LeftAudio:
                        return SoundDataTemplate;

                    case MessageModelType.RightContact:
                    case MessageModelType.LeftContact:
                        return ContactDataTemplate;

                    case MessageModelType.RightSticker:
                    case MessageModelType.LeftSticker:
                        return StickerDataTemplate;

                    case MessageModelType.RightGif:
                    case MessageModelType.LeftGif:
                        return GifsDataTemplate;

                    case MessageModelType.RightProduct:
                    case MessageModelType.LeftProduct:
                        return ProductsDataTemplate;
                    default:
                        Methods.DisplayReportResult(msg);
                        return null;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }
    }
}
