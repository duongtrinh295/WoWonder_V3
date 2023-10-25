using System;
using System.Timers;
using System.Windows.Threading;
using CefSharp; 
using CefSharp.Handler;

namespace WoWonderDesktop.CefB
{
    /// <summary>
    /// EXPERIMENTAL - this implementation is very simplistic and not ready for production use
    /// See the following link for the CEF reference implementation.
    /// https://bitbucket.org/chromiumembedded/cef/commits/1ff26aa02a656b3bc9f0712591c92849c5909e04?at=2785
    /// </summary>
    public class WpfBrowserProcessHandler : BrowserProcessHandler
    {
        private Timer timer;
        private Dispatcher dispatcher;

        /// <summary>
        /// The interval between calls to Cef.DoMessageLoopWork
        /// </summary>
        protected const int SixtyTimesPerSecond = 1000 / 60;  // 60fps
        /// <summary>
        /// The maximum number of milliseconds we're willing to wait between calls to OnScheduleMessagePumpWork().
        /// </summary>
        protected const int ThirtyTimesPerSecond = 1000 / 30;  //30fps

        public WpfBrowserProcessHandler(Dispatcher dispatcher)
        {
            timer = new Timer { Interval = ThirtyTimesPerSecond, AutoReset = true };
            timer.Start();
            timer.Elapsed += TimerTick;

            this.dispatcher = dispatcher;
            this.dispatcher.ShutdownStarted += DispatcherShutdownStarted;
        }

        private void DispatcherShutdownStarted(object sender, EventArgs e)
        {
            //If the dispatcher is shutting down then we stop the timer
            if (timer != null)
            {
                timer.Stop();
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            //Basically execute Cef.DoMessageLoopWork 30 times per second, on the UI Thread
            dispatcher.BeginInvoke((Action)(() => Cef.DoMessageLoopWork()), DispatcherPriority.Render);
        }

        protected override void OnScheduleMessagePumpWork(long delay)
        {
            //If the delay is greater than the Maximum then use ThirtyTimesPerSecond
            //instead - we do this to achieve a minimum number of FPS
            if (delay > ThirtyTimesPerSecond)
            {
                delay = ThirtyTimesPerSecond;
            }

            //When delay <= 0 we'll execute Cef.DoMessageLoopWork immediately
            // if it's greater than we'll just let the Timer which fires 30 times per second
            // care of the call
            if (delay <= 0)
            {
                dispatcher.BeginInvoke((Action)(() => Cef.DoMessageLoopWork()), DispatcherPriority.Normal);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (dispatcher != null)
                {
                    dispatcher.ShutdownStarted -= DispatcherShutdownStarted;
                    dispatcher = null;
                }

                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                    timer = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
