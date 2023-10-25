using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WoWonderDesktop.Helpers.Utils;
using WoWonderDesktop.language;

namespace WoWonderDesktop.Forms
{
    /// <summary>
    /// Interaction logic for Video_MediaPlayer_Window.xaml
    /// </summary>
    public partial class VideoMediaPlayerWindow : Window
    {
        private readonly DispatcherTimer Timer = new DispatcherTimer();

        public VideoMediaPlayerWindow(string path)
        {
            try
            {
                InitializeComponent();
                Title = LocalResources.label5_VideoPlayer;

                Vidoe_MediaElement.Source = new Uri(path);
                Vidoe_MediaElement.Play();

                Timer = new DispatcherTimer();
                Timer.Interval = TimeSpan.FromSeconds(1);
                Timer.Tick += timer_Tick;
                Timer.Start();
                switch (Settings.FlowDirectionRightToLeft)
                {
                    case true:
                        FlowDirection = FlowDirection.RightToLeft;
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Vidoe_MediaElement.Source != null)
                {
                    switch (Vidoe_MediaElement.NaturalDuration.HasTimeSpan)
                    {
                        case true when Convert.ToInt32(Vidoe_MediaElement.NaturalDuration.TimeSpan.TotalSeconds) != SliderVideo.Value:
                            lblStatus.Content = string.Format("{0} / {1}", Vidoe_MediaElement.Position.ToString(@"mm\:ss"), Vidoe_MediaElement.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                            SliderVideo.Value = Vidoe_MediaElement.Position.TotalSeconds;
                            SliderVideo.Maximum = Convert.ToInt32(Vidoe_MediaElement.NaturalDuration.TimeSpan.TotalSeconds);
                            break;
                        case true:
                            btnPlay.Visibility = Visibility.Visible;
                            btnPause.Visibility = Visibility.Collapsed;
                            SliderVideo.Value = 0;
                            Vidoe_MediaElement.Stop();
                            Timer.Stop();
                            break;
                    }
                }
                else
                    lblStatus.Content = LocalResources.label2_No_file_selected + "...";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnPlay_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                btnPlay.Visibility = Visibility.Collapsed;
                btnPause.Visibility = Visibility.Visible;

                switch (SliderVideo.Value > 0)
                {
                    case true:
                        Vidoe_MediaElement.Play();
                        Timer.Start();
                        break;
                    default:
                        Vidoe_MediaElement.Position = TimeSpan.FromSeconds(0);
                        Vidoe_MediaElement.Play();
                        Timer.Start();
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnPause_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (Vidoe_MediaElement.CanPause)
                {
                    case true:
                        Vidoe_MediaElement.Pause();
                        break;
                }

                btnPlay.Visibility = Visibility.Visible;
                btnPause.Visibility = Visibility.Collapsed;
                Timer.Stop();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnFullScreenExpand_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                btnFullScreenExpand.Visibility = Visibility.Collapsed;
                btnFullScreenCompress.Visibility = Visibility.Visible;

                Vidoe_MediaElement.Width = Double.NaN;
                Vidoe_MediaElement.Height = Double.NaN;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnFullScreenCompress_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                btnFullScreenExpand.Visibility = Visibility.Visible;
                btnFullScreenCompress.Visibility = Visibility.Collapsed;

                Vidoe_MediaElement.Width = 500;
                Vidoe_MediaElement.Height = 450;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Timer.Stop();
                Vidoe_MediaElement.Stop();
                Close();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SliderVideo_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                var mi = (Slider)sender;
                mi.Maximum = Vidoe_MediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Thumb_OnDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            try
            {
                var mi = (Slider)sender;
                TimeSpan ts = new TimeSpan(0, 0, 0, (int)mi.Value);
                Vidoe_MediaElement.Position = ts;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnRpeat_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Timer.Stop();
                Vidoe_MediaElement.Position = TimeSpan.Zero;
                Vidoe_MediaElement.Play();
                Timer.Start();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Vidoe_MediaElement_OnMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                MessageBox.Show(e.ErrorException.Message);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}
