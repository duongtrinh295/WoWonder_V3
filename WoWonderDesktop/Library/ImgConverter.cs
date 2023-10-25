using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WoWonderDesktop.Library
{
    [ValueConversion(typeof(string), typeof(string))]
    public class ImgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string uri)
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    
                    if (uri.Contains("d-avatar"))
                    {
                        image.UriSource = new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/no_profile_image_circle.png");
                    }
                    else if (uri.Contains("d-cover"))
                    {
                        image.UriSource = new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Images/Cover_image.png");
                    }
                    else
                    {
                        image.UriSource = new Uri(uri);
                    }
                     
                    //image.DecodePixelWidth = 1920; // should be enough, but you can experiment
                    image.EndInit();
                    return image;
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
