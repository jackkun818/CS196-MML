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

namespace crs.extension.Controls.MoCAs
{
    /// <summary>
    /// Subject_5_Sub.xaml Interaction logic
    /// </summary>
    public partial class Subject_5_Sub : UserControl
    {
        static ConcurrentDictionary<int, string> voiceDict = new ConcurrentDictionary<int, string>
        {
            [0] = "Below I want to read out a series of numbers. Please listen carefully. Whenever I read 1, you clap your hands. When I read other numbers, don’t press: five. two. one. three. Nine. Four. one. one. eight. zero. six. two. one. five. one. Nine. Four. five. one. one. one. Four. one. Nine. zero. five. one. one. two",
        };

        public Subject_5_Sub()
        {
            InitializeComponent();
            this.Loaded += Subject_Sub_Loaded;
            this.Unloaded += Subject_Sub_Unloaded;
        }

        private void Subject_Sub_Unloaded(object sender, RoutedEventArgs e)
        {
            Crs_WaveOutToolkit.AudioCancel();
        }

        private async void Subject_Sub_Loaded(object sender, RoutedEventArgs e)
        {            
            // Play voice prompts, start timer operations, etc.

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
    }
}
