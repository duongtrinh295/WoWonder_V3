using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Storage;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderDesktop.Helpers.Controls;

namespace WoWonderDesktop.Helpers.Utils
{
    public static class Methods
    {
        //Cach folder Destinations
        public static readonly string MainDestination = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + Settings.ApplicationName + "\\";
        public static string FilesDestination = MainDestination + "Users\\";

        // Check For Internet Connection
        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int description, int reservedValue);
        public static bool CheckForInternetConnection()
        {
            try
            { 
                int description;
                return InternetGetConnectedState(out description, 0); 
            }
            catch (Exception e)
            {
                DisplayReportResultTrack(e);
                return false; 
            }
        }

        public static void DisplayReportResultTrack(Exception exception, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                if (!exception.Message.Contains("com.android.okhttp") || !exception.Message.Contains("while sending the request"))
                {
                    Trace.WriteLine("\n ========================= ReportMode Start ========================= \n");
                    Trace.WriteLine("ReportMode >> Message: " + exception.Message + " \n  " + exception.StackTrace);
                    Trace.WriteLine("ReportMode >> Member name: " + memberName);
                    Trace.WriteLine("ReportMode >> Source file path: " + sourceFilePath);
                    Trace.WriteLine("ReportMode >> Source line number: " + sourceLineNumber);
                    Trace.WriteLine("\n ========================= ReportMode End ========================= \n");
                }

                if (Settings.SetApisReportMode)
                {
                    string text = " ReportMode >> message: " + exception.Message + " \n  " + exception.StackTrace;
                    text += "\n ReportMode >> member name: " + memberName;
                    text += "\n ReportMode >> source file path: " + sourceFilePath;
                    text += "\n ReportMode >> source line number: " + sourceLineNumber;
                    text += "\n ReportMode >> Date : " + DateTime.Now;
                    text += "\n ============================================== \n \n";
                     
                    GenerateNoteOnSD(text);
                } 
            }
            catch (Exception xx)
            {
                Console.WriteLine(xx);
            }
        }

        private static void GenerateNoteOnSD(string sBody)
        {
            try
            {
                string folderDestination = MainDestination + "Logs\\";
                string fileName = folderDestination + "\\ErrorReportMode.txt";

                if (Directory.Exists(folderDestination) == false)
                {
                    Directory.CreateDirectory(folderDestination);
                }

                if (!File.Exists(fileName))
                {
                    using FileStream fs = File.Create(fileName, 1024, FileOptions.WriteThrough);
                    // Add some text to file    
                    byte[] title = new UTF8Encoding(true).GetBytes("New Text File");
                    fs.Write(title, 0, title.Length);
                    byte[] author = new UTF8Encoding(true).GetBytes("Mahesh Chand");
                    fs.Write(author, 0, author.Length);
                    //Console.WriteLine("Adding access control entry for " + fileName);
                    //Console.WriteLine("Done.");
                }
                 
                // Open the stream and read it back. 
                var contents = File.ReadAllText(fileName);
                contents += sBody;
                File.WriteAllText(fileName, contents);
            }
            catch (IOException e)
            {
                DisplayReportResultTrack(e);
            }
        }
         
        // Functions Check File Extension */Audio, Image, Video\*
        public static string Check_FileExtension(string filename)
        {
            var mime = MimeTypeMap.GetMimeType(filename.Split('.').LastOrDefault());
            if (string.IsNullOrEmpty(mime)) return "Forbidden";
            if (mime.Contains("audio"))
            {
                return "Audio";
            }

            if (mime.Contains("video"))
            {
                return "Video";
            }

            if (mime.Contains("image") || mime.Contains("drawing"))
            {
                return "Image";
            }

            if (mime.Contains("application") || mime.Contains("text") || mime.Contains("x-world") ||
                mime.Contains("message"))
            {
                return "File";
            }

            return "Forbidden";
        }
 
        public static void DisplayReportResult(dynamic respond)
        {
            if (Settings.SetApisReportMode)
            {
                string errorText;
                if (respond is ErrorObject errorMessage)
                {
                    errorText = errorMessage.Error.ErrorText;

                    if (errorText.Contains("Invalid or expired access_token") || errorText.Contains("No session sent") || errorText.Contains("Not authorized"))
                    {
                        ApiRequest.Logout();
                    }
                }
                else if (respond is MessageObject result)
                    errorText = result.Message;
                else
                    errorText = respond.ToString();
                 
                MessageBox.Show(errorText, "ReportMode", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                DisplayReportResultTrack(new Exception(errorText));
            }

            throw new Exception();
        }

        public static void AddControlOnGrid(Grid grid, UserControl control)
        {
            try
            {
                if (grid.Children.Count > 0)
                {
                    grid.Children.Clear();

                    grid.Children.Add(control);
                }
                else
                {
                    grid.Children.Add(control);
                }
            }
            catch (IOException e)
            {
                DisplayReportResultTrack(e);
            }
        }


        #region MultiMedia

        public static class MultiMedia
        {
            
            // Functions Save Images
            public static async void save_img(string userid, string filename, string url)
            {
                try
                {
                    if (string.IsNullOrEmpty(url))
                        return;

                    string folderDestination = FilesDestination + userid + "\\";
                    if (Directory.Exists(folderDestination) == false)
                    {
                        Directory.CreateDirectory(folderDestination);
                    }

                    HttpClient client;
                    if (Settings.WebExceptionSecurity)
                    {
                        HttpClientHandler clientHandler = new HttpClientHandler();
                        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                        clientHandler.SslProtocols = SslProtocols.Tls | SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13 | SslProtocols.Default;

                        // Pass the handler to httpClient(from you are calling api)
                        client = new HttpClient(clientHandler);
                    }
                    else
                    {
                        client = new HttpClient();
                    }

                    string cache; 
                    if (filename.Contains("?cache="))
                    {
                        cache = filename.Split("?cache=").LastOrDefault(); 
                        filename = filename.Replace("?cache=" + cache, "");
                        url = url.Replace("?cache=" + cache, ""); 
                    }

                    if (!File.Exists(folderDestination + filename))
                    {
                        var s = await client.GetStreamAsync(new Uri(url));
                        if (s.CanRead)
                        {
                            if (File.Exists(folderDestination + filename)) return;
                            await using FileStream fs = new FileStream(folderDestination + filename, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                            await s.CopyToAsync(fs);
                        } 
                    } 
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                }
            }
             
            // Functions Get_image from folder
            public static string Get_image(string userid, string filename, string url)
            {
                try
                {
                    string folderDestination = FilesDestination + userid + "\\";
                    if (Directory.Exists(folderDestination) == false)
                    {
                        Directory.CreateDirectory(folderDestination);
                    }
                    if (Directory.Exists(folderDestination))
                    {
                        // Get the file name from the path.
                        if (File.Exists(folderDestination + filename))
                        {
                            return folderDestination + filename;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(url))
                                return "";

                            Task.Factory.StartNew(() => //This code runs on a new thread, control is returned to the caller on the UI thread.
                            {
                                save_img(userid, filename, url);
                            });

                            return url;
                        }
                    }
                    else
                    {
                        return url;
                    }
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                    return url;
                }
            }

             // Functions Get_image from folder
            public static async void SaveFile(string userid, string filename,string folder , string url)
            {
                try
                {
                    if (string.IsNullOrEmpty(url))
                        return;

                    string folderDestination = FilesDestination + userid + "\\" + folder + "\\";

                    if (!Directory.Exists(folderDestination))
                        Directory.CreateDirectory(folderDestination);

                    HttpClient client;
                    if (Settings.WebExceptionSecurity)
                    {
                        HttpClientHandler clientHandler = new HttpClientHandler();
                        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                        clientHandler.SslProtocols = SslProtocols.Tls | SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13 | SslProtocols.Default;

                        // Pass the handler to httpClient(from you are calling api)
                        client = new HttpClient(clientHandler);
                    }
                    else
                    {
                        client = new HttpClient();
                    }

                    string cache;
                    if (filename.Contains("?cache="))
                    {
                        cache = filename.Split("?cache=").LastOrDefault();
                        filename = filename.Replace("?cache=" + cache, "");
                        url = url.Replace("?cache=" + cache, "");
                    }

                    if (!File.Exists(folderDestination + filename))
                    {
                        var s = await client.GetStreamAsync(new Uri(url));
                        if (s.CanRead)
                        {
                            if (File.Exists(folderDestination + filename)) return;
                            await using FileStream fs = new FileStream(folderDestination + filename, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                            await s.CopyToAsync(fs);
                        }
                    }
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                }
            }
             
            // Functions Get sound from folder
            public static string GetFile(string userid, string filename,string folder , string url)
            {
                try
                {
                    string folderDestination = FilesDestination + userid + "\\" + folder + "\\";
                    if (!Directory.Exists(folderDestination))
                    {
                        Directory.CreateDirectory(folderDestination);
                    }
                    if (Directory.Exists(folderDestination))
                    {
                        // Get the file name from the path.
                        if (File.Exists(folderDestination + filename))
                        {
                            return folderDestination + filename;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(url))
                                return "";

                            Task.Factory.StartNew(() => //This code runs on a new thread, control is returned to the caller on the UI thread.
                            {
                                SaveFile(userid, filename, folder, url);
                            });

                            return url;
                        }
                    }
                    else
                    {
                        return url;
                    }
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                    return url;
                }
            }
             
            // Functions Rename vidoe
            public static string Rename_Video(string filename)
            {
                string fileName = filename.Split('.').Last();
                int unixTimestamp = (int)(DateTime.UtcNow.Subtract(DateTime.UtcNow)).TotalMilliseconds;
                string dataTime = Convert.ToString(unixTimestamp);
                string newFileName = "V_" + FunString.RandomString(8) + "." + fileName;

                return newFileName;
            }
             
            // Functions open from folder Images messages
            public static void open_img(string userid, string filename, string url)
            {
                try
                {
                    string folderDestination = FilesDestination + userid + "\\" + "images\\";
                    var file = folderDestination + filename;
                    string fileName = file.Split('\\').Last();

                    if (File.Exists(file))
                    { 
                        var p = new Process();
                        p.StartInfo = new ProcessStartInfo(file)
                        {
                            UseShellExecute = true
                        };
                        p.Start(); 
                    }
                    else
                    {
                        GetFile(userid, filename, "images", url);
                    }
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                }
            }
             
            // Functions Save or get from folder sticker messages
            public static string Get_Sticker_messages(string filename, string url, MessagesDataFody selectedgroup)
            {
                try
                {
                    string folderDestination = MainDestination + "\\" + "sticker\\";
                    if (Directory.Exists(folderDestination) == false)
                    {
                        Directory.CreateDirectory(folderDestination);
                    }
                    // Get the file name from the path.
                    if (File.Exists(folderDestination + filename))
                    {
                        return folderDestination + filename;
                    }
                    else
                    { 
                        if (string.IsNullOrEmpty(url))
                            return "";

                        HttpClient client;
                        if (Settings.WebExceptionSecurity)
                        {
                            HttpClientHandler clientHandler = new HttpClientHandler();
                            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                            clientHandler.SslProtocols = SslProtocols.Tls | SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13 | SslProtocols.Default;

                            // Pass the handler to httpClient(from you are calling api)
                            client = new HttpClient(clientHandler);
                        }
                        else
                        {
                            client = new HttpClient();
                        }

                        string cache;
                        if (filename.Contains("?cache="))
                        {
                            cache = filename.Split("?cache=").LastOrDefault();
                            filename = filename.Replace("?cache=" + cache, "");
                            url = url.Replace("?cache=" + cache, "");
                        }

                        if (!File.Exists(folderDestination + filename))
                        {
                            Task.Run(async () =>
                            {
                                var s = await client.GetStreamAsync(new Uri(url));
                                if (s.CanRead)
                                {
                                    if (File.Exists(folderDestination + filename)) return;
                                    await using FileStream fs = new FileStream(folderDestination + filename, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                                    await s.CopyToAsync(fs);

                                    if (filename.Contains(".png"))
                                    {
                                        selectedgroup.Media = folderDestination + filename;
                                    }
                                    else
                                    {
                                        selectedgroup.MediaFileName = folderDestination + filename;
                                    }
                                    SqLiteDatabase database = new SqLiteDatabase();
                                    database.Insert_Or_Update_To_one_MessagesTable(selectedgroup);
                                } 
                            }); 
                        } 
                    }
                    return url;
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                    return url;
                }
            }

            //We would like to create thumbnails one after the other; so we will use a lock to give 
            //access to only one path of execution to this method
            public static object importMediaLock = new object();
            public static string ImportMediaThreadProc(string mediaFile, string id)
            {
                try
                {
                    MediaPlayer player = new MediaPlayer { Volume = 0, ScrubbingEnabled = true };
                    player.Open(new Uri(mediaFile));
                    player.Pause();
                    player.Position = TimeSpan.FromMilliseconds(4000);
                    //We need to give MediaPlayer some time to load. The efficiency of the MediaPlayer depends                 
                    //upon the capabilities of the machine it is running on
                    Thread.Sleep(2000);

                    //120 = thumbnail width, 90 = thumbnail height and 96x96 = horizontal x vertical DPI
                    //An in actual application, you wouldn's probably use hard coded values!
                    RenderTargetBitmap rtb = new RenderTargetBitmap(120, 90, 96, 96, PixelFormats.Pbgra32);
                    DrawingVisual dv = new DrawingVisual();
                    using (DrawingContext dc = dv.RenderOpen())
                    {
                        dc.DrawVideo(player, new Rect(0, 0, 120, 90));
                    }
                    rtb.Render(dv);
                    Duration duration = player.NaturalDuration;
                    int videoLength = 0;
                    if (duration.HasTimeSpan)
                    {
                        videoLength = (int)duration.TimeSpan.TotalSeconds;
                    }
                    BitmapFrame frame = BitmapFrame.Create(rtb).GetCurrentValueAsFrozen() as BitmapFrame;
                    BitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(frame as BitmapFrame);

                    //We cannot create the thumbnail here, as we are not in the UI thread right now
                    //It is the responsibility of the calee to close the MemoryStream
                    //We will instead call a method which will do its stuff on the UI thread!
                    MemoryStream memoryStream = new MemoryStream();
                    encoder.Save(memoryStream);
                    var path = CreateMediaItem(memoryStream, id, mediaFile, videoLength);
                    player.Close();
                    return path;
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                    return mediaFile;
                }
            }

            public static string CreateMediaItem(MemoryStream ms, string id, string name, int duration)
            {
                string path = name;
                try
                {
                    ms.Position = 0;

                    //BitmapImage bitmapImage = new BitmapImage();
                    //bitmapImage.BeginInit();
                    //bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    //bitmapImage.StreamSource = ms;
                    //bitmapImage.EndInit();
                    //var Thumbnail = bitmapImage;

                    //var Duration = duration;

                    var Name = System.IO.Path.GetFileName(name);
                    //The protocol we defined expects us to close the MemoryStream here.
                    //Otherwise our memory consumption would not be optimized. GC can take its time; so we better do it ourselves!

                    string folderDestination = FilesDestination + id + "\\" + "video\\";
                    string fileName = Name.Split('.').First();

                    path = folderDestination + fileName + ".jpg";

                    if (Directory.Exists(folderDestination) == false)
                    {
                        Directory.CreateDirectory(folderDestination);
                    }

                    FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
                    ms.WriteTo(file);
                    file.Close();

                    ms.Close();

                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                }
                return path;
            }

            public static string Get_ThumbnailVideo_messages(string url, string id, MessagesDataFody selectedgroup)
            {
                try
                {
                    string folderDestination = FilesDestination + id + "\\" + "video\\";
                    if (Directory.Exists(folderDestination) == false)
                    {
                        Directory.CreateDirectory(folderDestination);
                    }
                     
                    var filename = url.Split('/').Last().Split('.').First() + ".jpg";

                    // Get the file name from the path.
                    if (File.Exists(folderDestination + filename))
                    {
                        return selectedgroup.ImageVideo = folderDestination + filename;
                    }
                    else
                    {
                        return selectedgroup.ImageVideo = ImportMediaThreadProc(url, id); 
                    } 
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                    return selectedgroup.ImageVideo = url;
                }
            }

            public static async Task<string> GetDuration(string mediaFile)
            {
                if (string.IsNullOrEmpty(mediaFile))
                    return "0";

                try
                {
                    StorageFile videoFile = await StorageFile.GetFileFromPathAsync(mediaFile);

                    var typeView = Check_FileExtension(mediaFile);
                    if (typeView == "Audio")
                    {
                        var x = await videoFile.Properties.GetMusicPropertiesAsync();
                        Duration videoDuration = x.Duration;
                        return videoDuration.TimeSpan.TotalSeconds.ToString(CultureInfo.InvariantCulture);
                    }
                    if (typeView == "Video")
                    {
                        var x = await videoFile.Properties.GetVideoPropertiesAsync();
                        Duration videoDuration = x.Duration;
                        return videoDuration.TimeSpan.TotalSeconds.ToString(CultureInfo.InvariantCulture);
                    }
                    return "0";
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                    return "0";
                }
            }

        }

        #endregion

        #region String 

        public static class FunString
        {
            //========================= Variables =========================
            private static readonly Random Random = new Random();

            //========================= Functions =========================

            //creat new Random String Session 
            public static string RandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXdsdaawerthklmnbvcxer46gfdsYZ0123456789";
                return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
            }

            //creat new Random Color
            public static string RandomColor()
            {
                string color = "";
                int b;
                b = Random.Next(1, 11);
                color = b switch
                {
                    1 => "#c62828",
                    2 => "#AD1457",
                    3 => "#6A1B9A",
                    4 => "#4527A0",
                    5 => "#283593",
                    6 => "#1565C0",
                    7 => "#00838F",
                    8 => "#2E7D32",
                    9 => "#9E9D24",
                    10 => "#FF8F00",
                    11 => "#D84315",
                    _ => color
                };

                return color;
            }

            public static string GetoLettersfromString(string key)
            {
                try
                {
                    if (!string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
                        return "";

                    var string1 = key.Split(' ').First();
                    var string2 = key.Split(' ').Last();

                    if (string1 != string2)
                    {
                        string substring1 = "", substring2 = "";
                        if (string1.Length >= 1)
                            substring1 = string1.Substring(0, 1);

                        if (string2.Length >= 1)
                            substring2 = string2.Substring(0, 1);
                         
                        var result = substring1 + substring2;
                        return result.ToUpper();
                    }
                    else
                    {
                        string substring1 = "";
                        if (string1.Length >= 2)
                            substring1 = string1.Substring(0, 2);

                        var result = substring1;
                        return result.ToUpper();
                    }
                }
                catch (Exception exception)
                {
                    DisplayReportResultTrack(exception);
                    return "";
                }
            }

            public static string Format_byte_size(string filepath)
            {
                try
                {
                    /*
                    * var size = new FileInfo(filepath).Length;
                    * double totalSize = size / 1024.0F / 1024.0F;
                    * string sizeFile = totalSize.ToString("0.### KB"); 
                    */

                    string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                    double len = new FileInfo(filepath).Length;
                    int order = 0;
                    while (len >= 1024 && order < sizes.Length - 1)
                    {
                        order++;
                        len /= 1024;
                    }

                    // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
                    // show a single decimal place, and no space.
                    string result = $"{len:0.##} {sizes[order]}";
                    return result;
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                    return "0B";
                }
            }

            // Functions convert color RGB to HEX
            public static string HexFromRgb(int r, int g, int b)
            {
                string hex = ColorTranslator.FromHtml(string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b)).Name.Remove(0, 2);
                return "#" + hex;
            }

            public static string UppercaseFirst(string s)
            {
                // Check for empty string.
                if (string.IsNullOrEmpty(s))
                {
                    return string.Empty;
                }

                // Return char and concat substring.
                return char.ToUpper(s[0]) + s.Substring(1);
            }

            public static string TrimTo(string str, int maxLength)
            {
                try
                {
                    if (str.Length <= maxLength)
                    {
                        return str;
                    }

                    switch (str.Length)
                    {
                        case > 35:
                            {
                                var remove = str.Remove(0, 10);
                                return remove;
                            }
                    }

                    switch (str.Length)
                    {
                        case > 65:
                            {
                                var remove = str.Remove(0, 30);
                                return remove;
                            }
                    }

                    switch (str.Length)
                    {
                        case > 85:
                            {
                                var remove = str.Remove(0, 50);
                                return remove;
                            }
                    }

                    switch (str.Length)
                    {
                        case > 105:
                            {
                                var remove = str.Remove(0, 70);
                                return remove;
                            }
                        default:
                            return str.Substring(maxLength - 17, maxLength);
                    }
                }
                catch (Exception exception)
                {
                    DisplayReportResultTrack(exception);
                    return str.Substring(maxLength - 17, maxLength);
                }
            }

            //SubString Cut Of
            public static string SubStringCutOf(string s, int x)
            {
                try
                {
                    if (!string.IsNullOrEmpty(s) && s.Length > x)
                    {
                        string substring = s.Substring(0, x);
                        return substring + "...";
                    }

                    return s;
                }
                catch (Exception exception)
                {
                    DisplayReportResultTrack(exception);
                    return s;
                }
            }

            //Null Remover >> return Empty
            public static string StringNullRemover(string s)
            {
                try
                {
                    if (string.IsNullOrEmpty(s))
                    {
                        s = "Empty";
                    }

                    return s;
                }
                catch (Exception exception)
                {
                    DisplayReportResultTrack(exception);
                    return s;
                }
            }

            //De code
            public static string DecodeString(string content)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(content) || string.IsNullOrEmpty(content))
                        return "";

                    //const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";
                    const string stripFormatting = @"<[^>]*(>|$)";
                    //const string stripFormatting = @"<[^>]*(>|$)(\W|\n|\r)";
                    const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";
                    var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
                    var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
                    //var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

                    StringBuilder builder = new StringBuilder(content);
                    builder.Replace(":)", "\ud83d\ude0a")
                       .Replace(";)", "\ud83d\ude09")
                       .Replace("0)", "\ud83d\ude07")
                       .Replace("(<", "\ud83d\ude02")
                       .Replace(":D", "\ud83d\ude01")
                       .Replace("*_*", "\ud83d\ude0d")
                       .Replace("(<", "\ud83d\ude02")
                       .Replace("<3", "\ud83d\u2764")
                       .Replace("/_)", "\ud83d\ude0f")
                       .Replace("-_-", "\ud83d\ude11")
                       .Replace(":-/", "\ud83d\ude15")
                       .Replace(":*", "\ud83d\ude18")
                       .Replace(":_p", "\ud83d\ude1b")
                       .Replace(":p", "\ud83d\ude1c")
                       .Replace("x(", "\ud83d\ude20")
                       .Replace("X(", "\ud83d\ude21")
                       .Replace(":_(", "\ud83d\ude22")
                       .Replace("<5", "\ud83d\u2B50")
                       .Replace(":0", "\ud83d\ude31")
                       .Replace("B)", "\ud83d\ude0e")
                       .Replace("o(", "\ud83d\ude27")
                       .Replace("</3", "\uD83D\uDC94")
                       .Replace(":o", "\ud83d\ude26")
                       .Replace("o(", "\ud83d\ude27")
                       .Replace(":__(", "\ud83d\ude2d")
                       .Replace("!_", "\uD83D\u2757")
                       .Replace("<br> ", "\n")
                       .Replace("<br />", "\n")
                       .Replace("<br/>", "\n")
                       .Replace("[/a]", "/")
                       .Replace("[a]", "")
                       .Replace("%3A", ":")
                       .Replace("%2F", "/")
                       .Replace("%3F", "?")
                       .Replace("%3D", "=")
                       .Replace("<a href=", "")
                       .Replace("target=", "")
                       .Replace("_blank", "")
                       //.Replace(@"""", "")
                       .Replace("</a>", "")
                       .Replace("class=hash", "")
                       .Replace("rel=nofollow>", "")
                       .Replace("<p>", "")
                       .Replace("</p>", "")
                       .Replace("</body>", "")
                       .Replace("<body>", "")
                       .Replace("<div>", "")
                       .Replace("<div ", "")
                       .Replace("</div>", "")
                       .Replace("&#039;", "'")
                       .Replace("\'", "'")
                       .Replace("\\'", "'")
                       .Replace("&amp;", "&")
                       .Replace("&lt;", "<")
                       .Replace("&gt;", ">")
                       .Replace("&le;", "≤")
                       .Replace("&ge;", "≥")
                       .Replace("<iframe", "")
                       .Replace("</iframe>", "")
                       .Replace("<table", "")
                       .Replace("<ul>", "")
                       .Replace("<li>", "")
                       .Replace("&nbsp;", "")
                       .Replace("&amp;nbsp;&lt;/p&gt;&lt;p&gt;", "\r\n")
                       .Replace("&amp;", "&")
                       .Replace("&quot;", "")
                       .Replace("&apos;", "")
                       .Replace("&cent;", "¢")
                       .Replace("&pound;", "£")
                       .Replace("&yen;", "¥")
                       .Replace("&euro;", "€")
                       .Replace("&copy;", "©")
                       .Replace("&reg;", "®")
                       .Replace("<b>", "")
                       .Replace("<u>", "")
                       .Replace("<i>", "")
                       .Replace("</i>", "")
                       .Replace("</u>", "")
                       .Replace("</b>", "")
                       .Replace("<br>", "\n")
                       .Replace("</li>", "")
                       .Replace("</ul>", "")
                       .Replace("</table>", " ")
                       .Replace("a&#768;", "")
                       .Replace("a&#769;", "")
                       .Replace("a&#770;", "")
                       .Replace("a&#771;", "")
                       .Replace("O&#768;", "")
                       .Replace("O&#769;", "")
                       .Replace("O&#770;", "")
                       .Replace("O&#771;", "")
                       .Replace("</table>", "")
                       .Replace("&bull;", "•")
                       .Replace("&hellip;", "…")
                       .Replace("&prime;", "′")
                       .Replace("&Prime;", "″")
                       .Replace("&oline;", "‾")
                       .Replace("&frasl;", "⁄")
                       .Replace("&weierp;", "℘")
                       .Replace("&image;", "ℑ")
                       .Replace("&real;", "ℜ")
                       .Replace("&trade;", "™")
                       .Replace("&alefsym;", "ℵ")
                       .Replace("&larr;", "←")
                       .Replace("&uarr;", "↑")
                       .Replace("&rarr;", "→")
                       .Replace("&darr;", "↓")
                       .Replace("&barr;", "↔")
                       .Replace("&crarr;", "↵")
                       .Replace("&lArr;", "⇐")
                       .Replace("&uArr;", "⇑")
                       .Replace("&rArr;", "⇒")
                       .Replace("&dArr;", "⇓")
                       .Replace("&hArr;", "⇔")
                       .Replace("&forall;", "∀")
                       .Replace("&part;", "∂")
                       .Replace("&exist;", "∃")
                       .Replace("&empty;", "∅")
                       .Replace("&nabla;", "∇")
                       .Replace("&isin;", "∈")
                       .Replace("&notin;", "∉")
                       .Replace("&ni;", "∋")
                       .Replace("&prod;", "∏")
                       .Replace("&sum;", "∑")
                       .Replace("&minus;", "−")
                       .Replace("&lowast", "∗")
                       .Replace("&radic;", "√")
                       .Replace("&prop;", "∝")
                       .Replace("&infin;", "∞")
                       .Replace("&OEig;", "Œ")
                       .Replace("&oelig;", "œ")
                       .Replace("&Yuml;", "Ÿ")
                       .Replace("&spades;", "♠")
                       .Replace("&clubs;", "♣")
                       .Replace("&hearts;", "♥")
                       .Replace("&diams;", "♦")
                       .Replace("&thetasym;", "ϑ")
                       .Replace("&upsih;", "ϒ")
                       .Replace("&piv;", "ϖ")
                       .Replace("&Scaron;", "Š")
                       .Replace("&scaron;", "š")
                       .Replace("&ang;", "∠")
                       .Replace("&and;", "∧")
                       .Replace("&or;", "∨")
                       .Replace("&cap;", "∩")
                       .Replace("&cup;", "∪")
                       .Replace("&int;", "∫")
                       .Replace("&there4;", "∴")
                       .Replace("&sim;", "∼")
                       .Replace("&cong;", "≅")
                       .Replace("&asymp;", "≈")
                       .Replace("&ne;", "≠")
                       .Replace("&equiv;", "≡")
                       .Replace("&le;", "≤")
                       .Replace("&ge;", "≥")
                       .Replace("&sub;", "⊂")
                       .Replace("&sup;", "⊃")
                       .Replace("&nsub;", "⊄")
                       .Replace("&sube;", "⊆")
                       .Replace("&supe;", "⊇")
                       .Replace("&oplus;", "⊕")
                       .Replace("&otimes;", "⊗")
                       .Replace("&perp;", "⊥")
                       .Replace("&sdot;", "⋅")
                       .Replace("&lcell;", "⌈")
                       .Replace("&rcell;", "⌉")
                       .Replace("&lfloor;", "⌊")
                       .Replace("&rfloor;", "⌋")
                       .Replace("&lang;", "⟨")
                       .Replace("&rang;", "⟩")
                       .Replace("&loz;", "◊")
                       .Replace("\u0024", "$")
                       .Replace("\u20AC", "€")
                       .Replace("\u00A3", "£")
                       .Replace("\u00A5", "¥")
                       .Replace("\u00A2", "¢")
                       .Replace("\u20B9", "₹")
                       .Replace("\u20A8", "₨")
                       .Replace("\u20B1", "₱")
                       .Replace("\u20A9", "₩")
                       .Replace("\u0E3F", "฿")
                       .Replace("\u20AB", "₫")
                       .Replace("\u20AA", "₪")
                       .Replace("&#36;", "$")
                       .Replace("&#8364;", "€")
                       .Replace("&#163;", "£")
                       .Replace("&#165;", "¥")
                       .Replace("&#162;", "¢")
                       .Replace("&#8377;", "₹")
                       .Replace("&#8360;", "₨")
                       .Replace("&#8369;", "₱")
                       .Replace("&#8361;", "₩")
                       .Replace("&#3647;", "฿")
                       .Replace("&#8363;", "₫")
                       .Replace("&#8362;", "₪")
                       .Replace("</table>", " ");

                    var text = builder.ToString();

                    //Decode html specific characters
                    text = WebUtility.HtmlDecode(text);

                    if (!Settings.ShowTextWithSpace)
                    {
                        //Remove tag whitespace/line breaks
                        //text = tagWhiteSpaceRegex.Replace(text, "><");

                        text = Regex.Replace(text, @"^\s*$\n", string.Empty, RegexOptions.Multiline).TrimEnd();

                        //Replace <br /> with line breaks
                        text = lineBreakRegex.Replace(text, Environment.NewLine);

                        //Strip formatting
                        text = stripFormattingRegex.Replace(text, string.Empty);
                    }

                    return text;
                }
                catch (Exception exception)
                {
                    DisplayReportResultTrack(exception);
                    return "";
                }
            }

            //String format numbers to millions, thousands with rounding
            public static string FormatPriceValue(long num)
            {
                try
                {
                    return num switch
                    {
                        >= 100000000 => ((num >= 10050000 ? num - 500000 : num) / 1000000D).ToString("#M"),
                        >= 10000000 => ((num >= 10500000 ? num - 50000 : num) / 1000000D).ToString("0.#M"),
                        >= 1000000 => ((num >= 1005000 ? num - 5000 : num) / 1000000D).ToString("0.##M"),
                        >= 100000 => ((num >= 100500 ? num - 500 : num) / 1000D).ToString("0.k"),
                        >= 10000 => ((num >= 10550 ? num - 50 : num) / 1000D).ToString("0.#k"),
                        _ => num >= 1000
                            ? ((num >= 1005 ? num - 5 : num) / 1000D).ToString("0.##k")
                            : num.ToString("#,0")
                    };
                }
                catch (Exception exception)
                {
                    DisplayReportResultTrack(exception);
                    return num.ToString();
                }
            }

            public static bool IsEmailValid(string emailAddress)
            {
                try
                {
                    if (string.IsNullOrEmpty(emailAddress) || string.IsNullOrWhiteSpace(emailAddress))
                        return false;

                    MailAddress m = new MailAddress(emailAddress);
                    Console.WriteLine(m);
                    return true;
                }
                catch (FormatException)
                {
                    return false;
                }
            }

            public static bool IsUrlValid(string url)
            {
                try
                {
                    string pattern =
                        @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
                    Regex reg = new Regex(pattern, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

                    Match m = reg.Match(url);
                    while (m.Success)
                    {
                        //do things with your matching text 
                        m.NextMatch();
                        break;
                    }

                    if (reg.IsMatch(url))
                    {
                        //var ss = "http://" + m.Value;
                        return true;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    DisplayReportResultTrack(exception);
                    return false;
                }
            }

            public static bool IsPhoneNumber(string number)
            {
                return Regex.Match(number, @"^(\+?)([0-9]{9,20}?)$").Success;
            }


            // Functions convert color RGB to HEX
            public static string ConvertColorRgBtoHex(string color)
            {
                //to rgba => string.Format("rgba({0}, {1}, {2}, {3});", color_red, color_green, color_blue, color_alpha);
                try
                {
                    if (color.Contains("rgb"))
                    {
                        var regex = new Regex(@"([0-9]+)");
                        string colorData = color;

                        var matches = regex.Matches(colorData);

                        var colorRed = Convert.ToInt32(matches[0].ToString());
                        var colorGreen = Convert.ToInt32(matches[1].ToString());
                        var colorBlue = Convert.ToInt32(matches[2].ToString());
                        var colorAlpha = Convert.ToInt16(matches[3].ToString());
                        var hex = $"#{colorRed:X2}{colorGreen:X2}{colorBlue:X2}";
                        Console.WriteLine(colorAlpha);
                        return hex;
                    }

                    return Settings.MainColor;
                }
                catch (Exception exception)
                {
                    DisplayReportResultTrack(exception);
                    return Settings.MainColor;
                }
            }

            public static bool OnlyHexInString(string color)
            {
                try
                {
                    if (color.Contains("rgba"))
                    {
                        var regex = new Regex(@"([0-9]+)");
                        string colorData = color;

                        var matches = regex.Matches(colorData);

                        var colorRed = Convert.ToInt32(matches[0].ToString());
                        var colorGreen = Convert.ToInt32(matches[1].ToString());
                        var colorBlue = Convert.ToInt32(matches[2].ToString());
                        var colorAlpha = Convert.ToInt16(matches[3].ToString());
                        var hex = $"#{colorAlpha:X2}{colorRed:X2}{colorGreen:X2}{colorBlue:X2}";
                        Console.WriteLine(hex);
                        return true;
                    }

                    if (color.Contains("rgb"))
                    {
                        var regex = new Regex(@"([0-9]+)");
                        string colorData = color;

                        var matches = regex.Matches(colorData);
                        var colorRed = Convert.ToInt32(matches[0].ToString());
                        var colorGreen = Convert.ToInt32(matches[1].ToString());
                        var colorBlue = Convert.ToInt32(matches[2].ToString());
                        var colorAlpha = Convert.ToInt16(00);
                        var hex = $"#{colorAlpha:X2}{colorRed:X2}{colorGreen:X2}{colorBlue:X2}";
                        Console.WriteLine(hex);
                        return true;
                    }

                    var rxColor = new Regex("^#(?:[0-9a-fA-F]{3}){1,2}$");
                    var rxColor2 = new Regex("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3}|[0-9]{3}|[0-9]{6})$");
                    var rxColor3 = new Regex(@"\A\b[0-9a-fA-F]+\b\Z");
                    var rxColor4 =
                        new Regex(
                            @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z"); // For C-style hex notation (0xFF) you can use @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z"

                    if (rxColor.IsMatch(color) || rxColor2.IsMatch(color) || rxColor3.IsMatch(color) ||
                        rxColor4.IsMatch(color))
                    {
                        return true;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    DisplayReportResultTrack(exception);
                    return false;
                }
            }

            public static string Check_Regex(string text)
            {
                try
                {
                    var rxEmail = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                    var rxWebsite =
                        new Regex(
                            @"^(http|https|ftp|www)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*$");
                    var rxHashtag = new Regex(@"(?<=#)\w+");
                    var rxMention = new Regex("@(?<name>[^\\s]+)");
                    var rxNumber1 = new Regex(@"^\d$");
                    var rxNumber2 = new Regex("[0-9]");
                    var resultEmail = IsEmailValid(text);
                    var resultWeb = IsUrlValid(text);

                    if (rxEmail.IsMatch(text) || resultEmail)
                    {
                        return "Email";
                    }

                    if (rxWebsite.IsMatch(text) || resultWeb)
                    {
                        return "Website";
                    }

                    if (rxHashtag.IsMatch(text))
                    {
                        return "Hashtag";
                    }

                    if (rxMention.IsMatch(text))
                    {
                        //var results = Rx_Mention.Matches(text).Cast<Match>().Select(m => m.Groups["name"].Value).ToArray();

                        return "Mention";
                    }

                    if (rxNumber1.IsMatch(text) || rxNumber2.IsMatch(text))
                    {
                        return "Number";
                    }

                    return text;
                }
                catch (Exception exception)
                {
                    DisplayReportResultTrack(exception);
                    return text;
                }
            }


            
        }

        #endregion

        #region Time

        public static class Time
        {
            public static string LblJustNow = LocalResources.label_Online;
            public static string LblHours = LocalResources.label3_hours;
            public static string LblDays = LocalResources.label3_days;
            public static string LblMonth = LocalResources.label3_month;
            public static string LblMinutes = LocalResources.label3_minutes;
            public static string LblSeconds = LocalResources.label3_seconds;
            public static string LblYear = LocalResources.label3_year;
            public static string LblAboutMinute = LocalResources.label3_about_minute;
            public static string LblAboutHour = LocalResources.label3_about_hour;
            public static string LblYesterday = LocalResources.label3_yesterday;
            public static string LblAboutMonth = LocalResources.label3_about_month;
            public static string LblAboutYear = LocalResources.label3_about_year;

            //Split String Duration (00:00:00)
            public static string SplitStringDuration(string duration)
            {
                try
                {
                    string[] durationsplit = duration.Split(':');
                    switch (durationsplit.Length)
                    {
                        case 3 when durationsplit[0] == "00":
                            {
                                string newDuration = durationsplit[1] + ":" + durationsplit[2];
                                return newDuration;
                            }
                        case 3:
                            return duration;
                        default:
                            return duration;
                    }
                }
                catch (Exception exception)
                {
                    DisplayReportResultTrack(exception);
                    return duration;
                }
            }

            // Functions Get datatime today
            public static string GetDataTime()
            {
                DateTime today = DateTime.Now;
                TimeSpan duration = new TimeSpan(36, 0, 0, 0);
                DateTime answer = today.Add(duration);

                return answer.ToString(CultureInfo.InvariantCulture);
            }

            public static string TimeAgo(long time, bool withReplace)
            {
                try
                {
                    DateTime dateTime = UnixTimeStampToDateTime(time);
                    string result;
                    var timeSpan = DateTime.Now.Subtract(dateTime);

                    if (timeSpan <= TimeSpan.FromSeconds(60))
                    {
                        //result = $"{timeSpan.Seconds} " + Lbl_seconds;
                        result = LblJustNow;
                    }
                    else if (timeSpan <= TimeSpan.FromMinutes(60))
                    {
                        result = timeSpan.Minutes > 1 ? $"{timeSpan.Minutes} " + LblMinutes : LblAboutMinute;
                    }
                    else if (timeSpan <= TimeSpan.FromHours(24))
                    {
                        result = timeSpan.Hours > 1 ? $"{timeSpan.Hours} " + LblHours : LblAboutHour;
                    }
                    else if (timeSpan <= TimeSpan.FromDays(30))
                    {
                        result = timeSpan.Days > 1 ? $"{timeSpan.Days} " + LblDays : LblYesterday;
                    }
                    else if (timeSpan <= TimeSpan.FromDays(365))
                    {
                        result = timeSpan.Days > 30 ? $"{timeSpan.Days / 30} " + LblMonth : LblAboutMonth;
                    }
                    else
                    {
                        result = timeSpan.Days > 365 ? $"{timeSpan.Days / 365} " + LblYear : LblAboutYear;
                    }

                    return withReplace ? ReplaceTime(result) : result;
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                    return time.ToString();
                }
            }

            /// <summary>
            /// dataFmt = "{0,-30}{1}"
            /// timeFmt = "{0,-30}{1:MM-dd-yyyy HH:mm}"
            /// </summary>
            /// <param name="time"></param>
            /// <returns></returns>
            public static string TimeAgo(long time)
            {
                try
                {
                    DateTime dateTime = UnixTimeStampToDateTime(time);
                    string result;
                    var timeSpan = DateTime.Now.Subtract(dateTime);

                    if (timeSpan <= TimeSpan.FromSeconds(60))
                    {
                        result = dateTime.ToShortTimeString();
                    }
                    else if (timeSpan <= TimeSpan.FromMinutes(60))
                    {
                        result = dateTime.ToShortTimeString();
                    }
                    else if (timeSpan <= TimeSpan.FromHours(24))
                    {
                        result = dateTime.ToShortTimeString();
                    }
                    else if (timeSpan <= TimeSpan.FromDays(30))
                    {
                        result = dateTime.ToShortDateString();
                    }
                    else if (timeSpan <= TimeSpan.FromDays(365))
                    {
                        result = dateTime.ToShortDateString();
                    }
                    else
                    {
                        result = dateTime.ToShortDateString();
                    }

                    return result;
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                    return time.ToString();
                }
            }

            public static string TimeAgo(DateTime dateTime, bool withReplace)
            {
                try
                {
                    string result;
                    var timeSpan = DateTime.Now.Subtract(dateTime);

                    if (timeSpan <= TimeSpan.FromSeconds(60))
                    {
                        //result = $"{timeSpan.Seconds} " + Lbl_seconds;
                        result = LblJustNow;
                    }
                    else if (timeSpan <= TimeSpan.FromMinutes(60))
                    {
                        result = timeSpan.Minutes > 1 ? $"{timeSpan.Minutes} " + LblMinutes : LblAboutMinute;
                    }
                    else if (timeSpan <= TimeSpan.FromHours(24))
                    {
                        result = timeSpan.Hours > 1 ? $"{timeSpan.Hours} " + LblHours : LblAboutHour;
                    }
                    else if (timeSpan <= TimeSpan.FromDays(30))
                    {
                        result = timeSpan.Days > 1 ? $"{timeSpan.Days} " + LblDays : LblYesterday;
                    }
                    else if (timeSpan <= TimeSpan.FromDays(365))
                    {
                        result = timeSpan.Days > 30 ? $"{timeSpan.Days / 30} " + LblMonth : LblAboutMonth;
                    }
                    else
                    {
                        result = timeSpan.Days > 365 ? $"{timeSpan.Days / 365} " + LblYear : LblAboutYear;
                    }

                    return withReplace ? ReplaceTime(result) : result;
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                    return dateTime.ToShortTimeString();
                }
            }

            //Functions Replace Time
            public static string ReplaceTime(string time, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            {
                if (time == null)
                    return "";

                time = time.ToLower();
                if (time.Contains("hours ago") || time.Contains("hour ago"))
                {
                    time = time.Replace("hours ago", LblHours);
                    time = time.Replace("hour ago", LblHours);
                }
                else if (time.Contains("days ago") || time.Contains("day ago"))
                {
                    time = time.Replace("days ago", LblDays).Replace("day ago", LblDays);
                }
                else if (time.Contains("month ago") || time.Contains("months ago"))
                {
                    time = time.Replace("months ago", LblMonth);
                    time = time.Replace("month ago", LblMonth);
                }
                else if (time.Contains("minutes ago") || time.Contains("minute ago"))
                {
                    time = time.Replace("minutes ago", LblMinutes);
                    time = time.Replace("minute ago", LblMinutes);
                }
                else if (time.Contains("seconds ago") || time.Contains("second ago"))
                {
                    time = time.Replace("seconds ago", LblSeconds);
                    time = time.Replace("second ago", LblSeconds);
                }
                else if (time.Contains("year ago") || time.Contains("years ago"))
                {
                    time = time.Replace("year ago", LblYear);
                    time = time.Replace("years ago", LblYear);
                }
                else if (time.Contains("yesterday"))
                {
                    time = time.Replace("yesterday", LblYesterday);
                }

                return time;
            }

            //convert a Unix timestamp to DateTime 
            public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
            {
                // Unix timestamp is seconds past epoch
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                return dtDateTime;
            }

            private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            public static long CurrentTimeMillis()
            {
                return (long)(DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
            }

            /// <summary>
            /// false =>> HasAgeRequirement(new DateTime(1994, 10, 10));
            /// true =>> HasAgeRequirement(new DateTime(2006, 10, 10)); less than 18
            /// </summary>
            /// <param name="bornIn"></param>
            /// <returns></returns>
            public static bool HasAgeRequirement(DateTime bornIn)
            {
                return bornIn.AddYears(18) >= DateTime.Now;
            }
             
            public static void ConvertDateToStringHeader(DateTime date)
            {
                try
                {
                    string formattedDate;

                    // Uses the default calendar of the InvariantCulture.
                    var myCal = CultureInfo.InvariantCulture.Calendar;

                    int time_dd = myCal.GetDayOfMonth(date);
                    int time_MM = myCal.GetMonthsInYear(date.Year);
                    DateTime now = DateTime.Now;

                    string nowMonth = now.ToString("MM");
                    string nowDay = now.ToString("dd");

                    int c_dd = int.Parse(nowDay);
                    int c_MM = int.Parse(nowMonth);
                    if (time_MM == c_MM)
                    {
                        if (time_dd == c_dd)
                        {
                            formattedDate = "TODAY";
                        }
                        else if (time_dd == c_dd - 1)
                        {
                            formattedDate = "YESTERDAY";
                        }
                        else
                        {
                            formattedDate = date.ToString("D");
                        }
                    }
                    else
                    {
                        formattedDate = date.ToString("D");
                    }

                    Trace.WriteLine("formattedDate : " + formattedDate);
                    Trace.WriteLine("=========================");
                }
                catch (Exception exception)
                {
                    DisplayReportResultTrack(exception);
                }
            }


            #region To days

            public static double ConvertMillisecondsToDays(double milliseconds)
            {
                return TimeSpan.FromMilliseconds(milliseconds).TotalDays;
            }

            public static double ConvertSecondsToDays(double seconds)
            {
                return TimeSpan.FromSeconds(seconds).TotalDays;
            }

            public static double ConvertMinutesToDays(double minutes)
            {
                return TimeSpan.FromMinutes(minutes).TotalDays;
            }

            public static double ConvertHoursToDays(double hours)
            {
                return TimeSpan.FromHours(hours).TotalDays;
            }

            #endregion

            #region To hours

            public static double ConvertMillisecondsToHours(double milliseconds)
            {
                return TimeSpan.FromMilliseconds(milliseconds).TotalHours;
            }

            public static double ConvertSecondsToHours(double seconds)
            {
                return TimeSpan.FromSeconds(seconds).TotalHours;
            }

            public static double ConvertMinutesToHours(double minutes)
            {
                return TimeSpan.FromMinutes(minutes).TotalHours;
            }

            public static double ConvertDaysToHours(double days)
            {
                return TimeSpan.FromHours(days).TotalHours;
            }

            #endregion

            #region To minutes

            public static double ConvertMillisecondsToMinutes(double milliseconds)
            {
                return TimeSpan.FromMilliseconds(milliseconds).Minutes;
            }

            public static double ConvertSecondsToMinutes(double seconds)
            {
                return TimeSpan.FromSeconds(seconds).TotalMinutes;
            }

            public static double ConvertHoursToMinutes(double hours)
            {
                return TimeSpan.FromHours(hours).TotalMinutes;
            }

            public static double ConvertDaysToMinutes(double days)
            {
                return TimeSpan.FromDays(days).TotalMinutes;
            }

            #endregion

            #region To seconds

            public static double ConvertMillisecondsToSeconds(double milliseconds)
            {
                return TimeSpan.FromMilliseconds(milliseconds).Seconds;
            }

            public static double ConvertMinutesToSeconds(double minutes)
            {
                return TimeSpan.FromMinutes(minutes).TotalSeconds;
            }

            public static double ConvertHoursToSeconds(double hours)
            {
                return TimeSpan.FromHours(hours).TotalSeconds;
            }

            public static double ConvertDaysToSeconds(double days)
            {
                return TimeSpan.FromDays(days).TotalSeconds;
            }

            #endregion

            #region To milliseconds

            public static double ConvertSecondsToMilliseconds(double seconds)
            {
                return TimeSpan.FromSeconds(seconds).TotalMilliseconds;
            }

            public static double ConvertMinutesToMilliseconds(double minutes)
            {
                return TimeSpan.FromMinutes(minutes).TotalMilliseconds;
            }

            public static double ConvertHoursToMilliseconds(double hours)
            {
                return TimeSpan.FromHours(hours).TotalMilliseconds;
            }

            public static double ConvertDaysToMilliseconds(double days)
            {
                return TimeSpan.FromDays(days).TotalMilliseconds;
            }

            #endregion
        }

        #endregion

        #region IPath URL

        public static class Path
        {
            //Disk
            public static string FolderDiskMyApp = FilesDestination;

            public static void Chack_MyFolder(string id = "")
            {
                try
                {
                    var folderDiskImage = FolderDiskMyApp + "\\" + id + "\\images\\";
                    var folderDiskVideo = FolderDiskMyApp + "\\" + id + "\\video\\";
                    var folderDiskSound = FolderDiskMyApp + "\\" + id + "\\sound\\";
                    var folderDiskFile = FolderDiskMyApp + "\\" + id + "\\file\\";
                    var folderDiskGif = FolderDiskMyApp + "\\" + id + "\\gif\\";
                    var folderDiskSticker = FolderDiskMyApp + "\\" + id + "\\sticker\\";

                    if (!Directory.Exists(FolderDiskMyApp))
                        Directory.CreateDirectory(FolderDiskMyApp);

                    if (!Directory.Exists( folderDiskImage))
                        Directory.CreateDirectory( folderDiskImage);

                    if (!Directory.Exists( folderDiskVideo))
                        Directory.CreateDirectory( folderDiskVideo);

                    if (!Directory.Exists( folderDiskFile))
                        Directory.CreateDirectory( folderDiskFile);

                    if (!Directory.Exists( folderDiskSound))
                        Directory.CreateDirectory( folderDiskSound);

                    if (!Directory.Exists( folderDiskGif))
                        Directory.CreateDirectory( folderDiskGif);

                    if (!Directory.Exists( folderDiskSticker))
                        Directory.CreateDirectory( folderDiskSticker);
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                }
            }

            public static void DeleteAll_MyFolder()
            {
                try
                { 
                    if (Directory.Exists(FolderDiskMyApp))
                        Directory.Delete(FolderDiskMyApp, true); 
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                }
            }

            public static void DeleteAll_MyFolderDisk()
            {
                try
                { 
                    if (Directory.Exists(FolderDiskMyApp))
                        Directory.Delete(FolderDiskMyApp, true);
                }
                catch (Exception e)
                {
                    DisplayReportResultTrack(e);
                }
            }

            // Functions  Delete data File  user where block
            public static void Delete_dataFile_user(string userid)
            {
                try
                {
                    string folderDestination = FilesDestination + userid;

                    DirectoryInfo di = new DirectoryInfo(folderDestination);

                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                }
                catch (Exception ex)
                {
                    DisplayReportResultTrack(ex);
                }
            }

            // Functions  Delete data File and Folder user and Messages Table
            public static void ClearFolder()
            {
                try
                {
                    string folderDestination = FilesDestination;

                    DirectoryInfo di = new DirectoryInfo(folderDestination);

                    string[] folder = Directory.GetDirectories(folderDestination);

                    foreach (string dir in folder)
                    {
                        DirectoryInfo sub = new DirectoryInfo(dir);

                        foreach (DirectoryInfo s in sub.GetDirectories())
                        {
                            s.Delete(true);
                        }
                    }

                    SqLiteDatabase database = new SqLiteDatabase();
                    database.ClearAll_Messages(); 
                }

                catch (Exception ex)
                {
                    DisplayReportResultTrack(ex);
                }
            }


        }


        #endregion

    }
}