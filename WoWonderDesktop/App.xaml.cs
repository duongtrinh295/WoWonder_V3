using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WoWonderClient;
using WoWonderDesktop.CefB;
using WoWonderDesktop.Forms;
using WoWonderDesktop.Helpers.Controls;
using WoWonderDesktop.Helpers.Model;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;
using WoWonderDesktop.SQLiteDB;

namespace WoWonderDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs args)
        {
            try
            {
                base.OnStartup(args);

                AppDomain.CurrentDomain.AssemblyResolve += Resolver;

                if (Settings.WebExceptionSecurity)
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.SystemDefault;
                }

                InitializeWoWonder.Initialize(Settings.TripleDesAppServiceProvider, Assembly.GetExecutingAssembly().GetName(true).Name, Settings.WebExceptionSecurity, Settings.SetApisReportMode);
                 
                CefManager.Initialize();
                 
                SqLiteDatabase database = new SqLiteDatabase();
                database.CheckTablesStatus();

                ClassMapper.SetMappers();
                
                try
                {
                    var dataLang = database.GetSettingsApp();
                    if (!string.IsNullOrEmpty(dataLang?.LangResources))
                    {
                        if (dataLang.LangResources.Contains("ar"))
                            Settings.FlowDirectionRightToLeft = true;

                        SetCulture(dataLang.LangResources);
                    }
                    else
                    { 
                        if (!string.IsNullOrEmpty(Settings.LangResources) || !string.IsNullOrWhiteSpace(Settings.LangResources))
                        {
                            //language codes operating system windows
                            string langOsWindows2 = Settings.LangResources;
                            string nameLocalResources = @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "/language/LocalResources." + langOsWindows2 + ".resx";

                            if (langOsWindows2 is "ar" or "Arabic" or "ar-AR") 
                                Settings.FlowDirectionRightToLeft = true;

                            SetCulture(langOsWindows2);
                        }
                        //else if (!string.IsNullOrEmpty(dataLang?.DefualtLang))
                        //{
                        //    string lang = "en-US";
                        //    if (dataLang.DefualtLang.Contains("english"))
                        //        lang = "en-US";
                        //    else if (dataLang.DefualtLang.Contains("arabic"))
                        //        lang = "ar";

                        //    if (lang == "ar") Settings.FlowDirectionRightToLeft = true;

                        //    SetCulture(lang);

                        //    dataLang.LangResources = lang;
                        //}
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }

                var user = database.Get_data_Login();
                if (user != null)
                {
                    if (user.Status == "Active")
                    {
                        WoWonderClient.Current.AccessToken = UserDetails.AccessToken = user.AccessToken;
                        UserDetails.UserId = user.UserId;

                        // INSERT DATA PROFILE USER TO LIST
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => ApiRequest.Get_MyProfileData_Api(UserDetails.UserId)});

                        // Create, assign, and show the main window.
                        ChatWindow win = new ChatWindow();
                        MainWindow = win;
                        win.Show();
                    }
                    else if (user.Status == "Pending")
                    {
                        // Create, assign, and show the main window.
                        LoginWindow win = new LoginWindow();
                        MainWindow = win;
                        win.Show();
                    }
                }
                else
                {
                    // Create, assign, and show the main window.
                    LoginWindow win = new LoginWindow();
                    MainWindow = win;
                    win.Show();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Will attempt to load missing assembly from either x86 or x64 subdir
        // Required by CefSharp to load the unmanaged dependencies when running using AnyCPU
        private static Assembly Resolver(object sender, ResolveEventArgs args)
        {
            try
            {
                if (args.Name.StartsWith("CefSharp"))
                {
                    string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                    string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, Environment.Is64BitProcess ? "x64" : "x86", assemblyName);

                    return File.Exists(archSpecificPath) ? Assembly.LoadFile(archSpecificPath) : null;
                }

                return null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        private static void SetCulture(string language)
        {
            try
            {
                CultureInfo myCulture = new CultureInfo(language);
                LocalResources.Culture = myCulture;
                CultureInfo.DefaultThreadCurrentCulture = myCulture;
                Thread.CurrentThread.CurrentCulture = myCulture;
                Thread.CurrentThread.CurrentUICulture = myCulture;

                //new ChineseLunisolarCalendar();
                //new HebrewCalendar();
                //new HijriCalendar();
                //new JapaneseCalendar();
                //new JapaneseLunisolarCalendar();
                //new KoreanCalendar();
                //new KoreanLunisolarCalendar();
                //new PersianCalendar();
                //new TaiwanCalendar();
                //new TaiwanLunisolarCalendar();
                //new ThaiBuddhistCalendar();
                //new UmAlQuraCalendar();

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
    }
}
