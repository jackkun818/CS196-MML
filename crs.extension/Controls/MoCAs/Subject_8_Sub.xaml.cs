using crs.extension.Models;
using HandyControl.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace crs.extension.Controls.MoCAs
{
    /// <summary>
    /// Subject_8_Sub.xaml Interaction logic
    /// </summary>
    public partial class Subject_8_Sub : UserControl
    {
        static ConcurrentDictionary<int, string> voiceDict = new ConcurrentDictionary<int, string>
        {
            [0] = "Please name the animal as much as possible within 1 minute",
        };

        int second = -1;

        DispatcherTimer _dispatcherTimer;

        public Subject_8_Sub()
        {
            InitializeComponent();
            this.Loaded += Subject_Sub_Loaded;
            this.Unloaded += Subject_Sub_Unloaded;
        }

        private void Subject_Sub_Unloaded(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Tick -= _dispatcherTimer_Tick;
            _dispatcherTimer.Stop();

            Crs_WaveOutToolkit.AudioCancel();
        }

        private async void Subject_Sub_Loaded(object sender, RoutedEventArgs e)
        {
            // Play voice prompts, start timer operations, etc.

            _dispatcherTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(1000.0)
            };
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;

            Crs_WaveOutToolkit.AudioCancel();
            _ = Crs_WaveInToolkit.RecordAudioCompleteAsync(true);

            if (voiceDict.TryGetValue(0, out var voiceText))
            {
                var result = await Crs_WaveOutToolkit.TextToSpeechAsync(voiceText);
                if (result.status)
                {
                    Crs_WaveOutToolkit.AudioPlay(result.voiceBuffer);
                }
            }
        }

        private async void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            second--;
            CountdownTime = DateTime.MinValue.AddSeconds(second);

            if (second <= 0)
            {
                (sender as DispatcherTimer)?.Stop();
                await Crs_WaveInToolkit.RecordAudioCompleteAsync();
                return;
            }
        }

        private void Crs_AudioButton_Click(object sender, RoutedEventArgs e)
        {
            var toggleButton = e.OriginalSource as ToggleButton;
            if ((bool)toggleButton.IsChecked)
            {
                second = 59;
                CountdownTime = DateTime.MinValue.AddSeconds(second);
                _dispatcherTimer.Start();
                return;
            }

            _dispatcherTimer.Stop();
            CountdownTime = null;
        }

        public DateTime? CountdownTime
        {
            get { return (DateTime?)GetValue(CountdownTimeProperty); }
            set { SetValue(CountdownTimeProperty, value); }
        }

        public static readonly DependencyProperty CountdownTimeProperty =
            DependencyProperty.Register("CountdownTime", typeof(DateTime?), typeof(Subject_8_Sub), new PropertyMetadata(null));
    }
}
