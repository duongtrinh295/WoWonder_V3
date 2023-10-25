using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WoWonderClient.Classes.GroupChat;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using UserControl = System.Windows.Controls.UserControl;

namespace WoWonderDesktop.Controls
{
    /// <summary>
    /// Interaction logic for AttachFilePopupMenu.xaml
    /// </summary>
    public partial class AttachFilePopupMenu : UserControl
    {
        public AttachFilePopupMenu()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button b)
                {
                    var text = b.Content?.ToString();

                    if (text == LocalResources.label5_Docs)
                    {
                        Btn_Attach_OnClick("Docs");
                    }
                    else if (text == LocalResources.label5_Camera)
                    {
                        Btn_Attach_OnClick("Image");
                    }
                    else if (text == LocalResources.label5_Gallery)
                    {
                        Btn_Attach_OnClick("Gallery");
                    }
                    else if (text == LocalResources.label5_Audio)
                    {
                        Btn_Attach_OnClick("Audio");
                    }
                    else if (text == LocalResources.label_location)
                    {
                        //wael
                    }
                    else if (text == LocalResources.label5_Contact)
                    {
                        Btn_Attach_OnClick("Contact");
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Btn_Attach_OnClick(string type)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog
                {
                    // Set filter for file extension and default file extension by default its select from 
                    DefaultExt = "All files"
                };

                try
                {
                    string extenstion = ListUtils.SettingsSiteList?.AllowedExtenstion.Replace("jpg", "*.jpg").Replace(",", ";*.") ?? "jpg,png,jpeg,gif,mkv,docx,zip,rar,pdf,doc,mp3,mp4,flv,wav,txt,mov,avi,webm,wav,mpeg".Replace("jpg", "*.jpg").Replace(",", ";*.");
                    //dlg.Filter = "All files (Documents,Images,Media,Archive)|" + extenstion + "";

                    switch (type)
                    {
                        case "Docs":
                            dlg.Filter = "Documents (*.txt;*.pdf;.rar;*.zip;*.iso;*.tar;*.gz)|*.txt;*.pdf;.rar;*.zip;*.iso;*.tar;*.gz";
                            break;
                        case "Image":
                            dlg.Filter = "Image files (*.jpg;*.png;*.gif;*.jpeg)|*.jpg;*.png;*.gif;*.jpeg";
                            break;
                        case "Gallery":
                            dlg.Filter = "Media files (*.jpg;*.png;*.gif;*.jpeg;*.mp4;*.mov;*.mpeg;*.flv;*.avi;*.webm;*.quicktime)|*.jpg;*.png;*.gif;*.jpeg;*.mp4;*.mov;*.mpeg;*.flv;*.avi;*.webm;*.quicktime";
                            break;
                        case "Audio":
                            dlg.Filter = "Audio files (*.wav;*.mpeg;*.mp3)|*.wav;*.mpeg;*.mp3";
                            break;
                        case "Contact":
                            dlg.Filter = "Contact files (*.csv;*.vcf)|*.csv;*.vcf";
                            break;
                        default:
                            dlg.Filter = "All files (" + extenstion + ")|" + extenstion;
                            break;
                    }
                }
                catch
                {
                    dlg.Filter = "Documents (.txt;*.pdf)|*.txt;*.pdf|Image files (*.png;*.png;*.gif;*.ico;*.jpeg)|*.png;*.png;*.gif;*.ico;*.jpeg|Media files (*.mp4;*.mp3;*.avi;*.3gp;*.mp2;*.wmv;*.mkv;*.mpg;*.flv;*.wav)|*.mp4;*.mp3;*.avi;*.3gp;*.mp2;*.wmv;*.mkv;*.mpg;*.flv;*.wav|Archive files (.rar;*.zip;*.iso;*.tar;*.gz)|.rar;*.zip;*.iso;*.tar;*.gz|All files (*.*)|*.*";
                }

                // Display OpenFileDialog by calling ShowDialog method
                var result = dlg.ShowDialog();

                // Get the selected file name and display in a TextBox
                if (result == true)
                {
                    // Open document
                    string filename = dlg.FileName;
                    string fileNameMedia = filename.Split('\\').Last();

                    long size = new FileInfo(dlg.FileName).Length;
                    double totalSize = size / 1024.0F / 1024.0F;
                    ChatWindow.SizeFile = totalSize.ToString("0.### KB");

                    string filecopy = Methods.FilesDestination + ChatWindow.UserId;
                    string checkExtension = Methods.Check_FileExtension(filename);

                    string copyedfile = "";

                    if (checkExtension == "File")
                    {
                        copyedfile = filecopy + "\\" + "file\\" + fileNameMedia;
                        if (!File.Exists(copyedfile))
                        {
                            File.Copy(filename, copyedfile, true);
                        }
                    }

                    if (checkExtension == "Image")
                    {
                        copyedfile = filecopy + "\\" + "images\\" + fileNameMedia;
                        if (!File.Exists(copyedfile))
                        {
                            File.Copy(filename, copyedfile, true);
                        }
                    }

                    if (checkExtension == "Video")
                    {
                        copyedfile = filecopy + "\\" + "video\\" + fileNameMedia;
                        if (!File.Exists(copyedfile))
                        {
                            File.Copy(filename, copyedfile, true);
                        }
                    }

                    if (checkExtension == "Audio")
                    {
                        copyedfile = filecopy + "\\" + "sound\\" + fileNameMedia;
                        if (!File.Exists(copyedfile))
                        {
                            File.Copy(filename, copyedfile, true);
                        }
                    }
                    string[] f = copyedfile.Split(new char[] { '.' });
                    string[] allowedExtenstion = ListUtils.SettingsSiteList?.AllowedExtenstion.Split(new char[] { ',' }) ?? "jpg,png,jpeg,gif,mkv,docx,zip,rar,pdf,doc,mp3,mp4,flv,wav,txt,mov,avi,webm,wav,mpeg".Split(new char[] { ',' });

                    if (ChatWindow.TypeChatPage == "user")
                    {
                        if (Methods.CheckForInternetConnection())
                        {
                            int index = ListUtils.ListUsers.IndexOf(ListUtils.ListUsers.FirstOrDefault(a => a.UserId == ChatWindow.UserId));
                            if (index > -1 && index != 0)
                            {
                                ListUtils.ListUsers.Move(index, 0);
                            }

                            if (allowedExtenstion.Contains(f.Last()))
                            {
                                try
                                {
                                    Dispatcher?.Invoke(async delegate // <--- HERE
                                    {
                                        try
                                        {
                                            int index = ListUtils.ListUsers.IndexOf(ListUtils.ListUsers.FirstOrDefault(a => a.UserId == ChatWindow.UserId));
                                            if (index > -1 && index != 0)
                                            {
                                                ListUtils.ListUsers.Move(index, 0);
                                            }

                                            //Uplouding.Visibility = Visibility.Visible;
                                            //Uplouding.IsIndeterminate = true;
                                            //NoMessagePanel.Visibility = Visibility.Hidden;
                                            var conversation = ChatWindow.ChatWindowContext.MethodToSendAttachmentMessages(copyedfile);

                                            var (apiStatus, respond) = await RequestsAsync.Message.SendMessageAsync(ChatWindow.UserId, conversation.Id.ToString(), "", "", copyedfile, "", "", "", "", "", "", "", ChatWindow.ReplyId);
                                            if (apiStatus == 200)
                                            {
                                                if (respond is SendMessageObject res)
                                                {
                                                    //MethodToSendAttachmentMessages(copyedfile);

                                                    var lastMessage = ListUtils.ListMessages.LastOrDefault(a => a.Id == conversation.Id);
                                                    if (lastMessage != null)
                                                    {
                                                        lastMessage.Id = res.MessageData[0].Id;
                                                        lastMessage = WoWonderTools.MessageFilter(ChatWindow.UserId, ViewModel.ModelInstance.Name, res.MessageData.FirstOrDefault(), ChatWindow.ChatWindowContext.ChatColor);
                                                        Console.WriteLine(lastMessage.Id);

                                                        SqLiteDatabase database = new SqLiteDatabase();
                                                        database.Insert_Or_Update_To_one_MessagesTable(lastMessage);

                                                        ViewModel.ModelInstance.UpdateChangedMessage();
                                                        ViewModel.ModelInstance.UpdateChatAllList(conversation);

                                                        //Update
                                                        ViewModel.ModelInstance.UpdateMessage(true);
                                                    }
                                                }
                                            }
                                            ViewModel.ModelInstance.CancelReply();
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
                            else
                            {
                                MessageBox.Show(LocalResources.label5_ErrorSelectedFileExtenstion, LocalResources.label5_Error, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }
                        }
                        else
                        {
                            MessageBox.Show(LocalResources.label_Please_check_your_internet);
                        }
                    }
                    else
                    {
                        if (Methods.CheckForInternetConnection())
                        {
                            int index = ListUtils.ListGroup.IndexOf(ListUtils.ListGroup.FirstOrDefault(a => a.GroupId == ChatWindow.GroupId));
                            if (index > -1 && index != 0)
                            {
                                ListUtils.ListGroup.Move(index, 0);
                            }

                            if (allowedExtenstion.Contains(f.Last()))
                            {
                                try
                                {
                                    Dispatcher?.Invoke(async delegate // <--- HERE
                                    {
                                        try
                                        {
                                            int index = ListUtils.ListGroup.IndexOf(ListUtils.ListGroup.FirstOrDefault(a => a.GroupId == ChatWindow.GroupId));
                                            if (index > -1 && index != 0)
                                            {
                                                ListUtils.ListGroup.Move(index, 0);
                                            }

                                            //Uplouding.Visibility = Visibility.Visible;
                                            //Uplouding.IsIndeterminate = true;
                                            //NoMessagePanel.Visibility = Visibility.Hidden;
                                            var conversation = ChatWindow.ChatWindowContext.MethodToSendAttachmentMessages(copyedfile);

                                            var (apiStatus, respond) = await RequestsAsync.GroupChat.Send_MessageToGroupChatAsync(ChatWindow.GroupId, conversation.Id.ToString(), "", "", copyedfile, "", "", "", "", "", ChatWindow.ReplyId);
                                            if (apiStatus == 200)
                                            {
                                                if (respond is GroupSendMessageObject res)
                                                {
                                                    //MethodToSendAttachmentMessages(copyedfile);

                                                    var lastMessage = ListUtils.ListMessages.LastOrDefault(a => a.Id == conversation.Id);
                                                    if (lastMessage != null)
                                                    {
                                                        var item = res.Data.FirstOrDefault();
                                                        if (item == null)
                                                            return;

                                                        lastMessage = WoWonderTools.MessageFilter(ChatWindow.GroupId, WoWonderTools.GetNameFinal(item.UserData), item, ChatWindow.ChatWindowContext.ChatColor, "group");
                                                        lastMessage.Id = item.Id;

                                                        ViewModel.ModelInstance.UpdateChangedMessage();
                                                        ViewModel.ModelInstance.UpdateChatAllList(conversation);

                                                        //Update
                                                        ViewModel.ModelInstance.UpdateMessage(true);
                                                    }
                                                }
                                            }
                                            ViewModel.ModelInstance.CancelReply();
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
                            else
                            {
                                MessageBox.Show(LocalResources.label5_ErrorSelectedFileExtenstion, LocalResources.label5_Error, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }
                        }
                        else
                        {
                            MessageBox.Show(LocalResources.label_Please_check_your_internet);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}