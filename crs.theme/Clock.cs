using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace crs.theme
{
    public class Clock : TextBlock
    {
        private readonly DispatcherTimer _dispatcherTimer;

        private bool _isDisposed;

        public Clock()
        {
            _dispatcherTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(200.0)
            };

            base.IsVisibleChanged += FlipClock_IsVisibleChanged;
        }

        ~Clock()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                base.IsVisibleChanged -= FlipClock_IsVisibleChanged;
                _dispatcherTimer.Stop();

                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private void FlipClock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (base.IsVisible)
            {
                _dispatcherTimer.Tick += DispatcherTimer_Tick;
                _dispatcherTimer.Start();
            }
            else
            {
                _dispatcherTimer.Stop();
                _dispatcherTimer.Tick -= DispatcherTimer_Tick;
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            DisplayTime = DateTime.Now;
        }


        public DateTime DisplayTime
        {
            get { return (DateTime)GetValue(DisplayTimeProperty); }
            set { SetValue(DisplayTimeProperty, value); }
        }

        public static readonly DependencyProperty DisplayTimeProperty =
            DependencyProperty.Register("DisplayTime", typeof(DateTime), typeof(Clock), new PropertyMetadata(default(DateTime)));
    }
}
