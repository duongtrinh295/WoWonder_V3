using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WoWonderDesktop.Controls
{
    /// <summary>
    /// Interaction logic for RoundProfileButton.xaml
    /// </summary>
    public partial class RoundProfileButton : UserControl
    {
        public RoundProfileButton()
        {
            InitializeComponent();
        }
        public SolidColorBrush StrokeBrush
        {
            get => (SolidColorBrush)GetValue(StrokeBrushProperty);
            set => SetValue(StrokeBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeBrushProperty = DependencyProperty.Register("StrokeBrush", typeof(SolidColorBrush), typeof(RoundProfileButton));

        public bool IsOnline
        {
            get => (bool)GetValue(IsOnlineProperty);
            set => SetValue(IsOnlineProperty, value);
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOnlineProperty = DependencyProperty.Register("IsOnline", typeof(bool), typeof(RoundProfileButton));

        public Visibility IsOnlineVisibility
        {
            get => (Visibility)GetValue(IsOnlineVisibilityProperty);
            set => SetValue(IsOnlineVisibilityProperty, value);
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOnlineVisibilityProperty = DependencyProperty.Register("IsOnlineVisibility", typeof(Visibility), typeof(RoundProfileButton));

        public ImageSource ProfileImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ProfileImageSource", typeof(ImageSource), typeof(RoundProfileButton));
         
        //Event
        public static readonly RoutedEvent TapEvent = EventManager.RegisterRoutedEvent("Tap", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RoundProfileButton));

        // Provide CLR accessors for the event
        public event RoutedEventHandler Tap
        {
            add { AddHandler(TapEvent, value); }
            remove { RemoveHandler(TapEvent, value); }
        }

        // This method raises the Tap event
        void RaiseTapEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(TapEvent);
            RaiseEvent(newEventArgs);
        }
       
        // For demonstration purposes we raise the event when the MyButtonSimple is clicked
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            RaiseTapEvent();
        }
    }
} 
