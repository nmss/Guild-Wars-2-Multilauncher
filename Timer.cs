using System;
using System.Windows.Threading;

namespace gw2_launcher
{
    class Timer
    {
        public static void delay(EventHandler eventHandler, TimeSpan timeSpan)
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += eventHandler;
            dispatcherTimer.Interval = timeSpan;
            dispatcherTimer.Start();
        }
    }
}
